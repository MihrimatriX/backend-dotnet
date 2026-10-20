using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.IntegrationTests.Support;
using Xunit;

namespace EcommerceBackend.IntegrationTests.Orders;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class OrderLifecycleTests
{
    private readonly HttpClient _client;

    public OrderLifecycleTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Authenticated_user_can_place_order_end_to_end()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "user1@example.com",
            Password = "user123"
        }, TestJson.Options);
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<BaseResponseDto<AuthResponseDto>>(TestJson.Options);
        Assert.True(auth?.Success);
        var token = auth!.Data!.Token;
        var userId = (int)auth.Data.UserId;

        using var authScope = new BearerTokenScope(_client, token);
        await _client.DeleteAsync("/api/cart/clear");

        var addressPayload = new CreateAddressDto
        {
            Title = "Ev",
            FullAddress = "Test Sokak No 1",
            City = "İstanbul",
            District = "Kadıköy",
            PostalCode = "34000",
            Country = "Turkey",
            IsDefault = true,
            PhoneNumber = "+905551112233"
        };
        var addressRes = await _client.PostAsJsonAsync("/api/address", addressPayload, TestJson.Options);
        addressRes.EnsureSuccessStatusCode();
        var addressEnv = await addressRes.Content.ReadFromJsonAsync<BaseResponseDto<AddressDto>>(TestJson.Options);
        Assert.True(addressEnv?.Success);
        var addressId = addressEnv!.Data!.Id;

        var paymentPayload = new CreatePaymentMethodDto
        {
            Type = "Card",
            CardHolderName = "Test User",
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2030,
            Cvv = "123",
            IsDefault = true
        };
        var payRes = await _client.PostAsJsonAsync("/api/paymentmethod", paymentPayload, TestJson.Options);
        payRes.EnsureSuccessStatusCode();
        var payEnv = await payRes.Content.ReadFromJsonAsync<BaseResponseDto<PaymentMethodDto>>(TestJson.Options);
        Assert.True(payEnv?.Success);
        var paymentMethodId = payEnv!.Data!.Id;

        var productsRes = await _client.GetAsync("/api/product?pageNumber=1&pageSize=50");
        productsRes.EnsureSuccessStatusCode();
        var productsEnv = await productsRes.Content.ReadFromJsonAsync<BaseResponseDto<PagedResultDto<ProductDto>>>(TestJson.Options);
        var inStock = productsEnv!.Data!.Items.FirstOrDefault(p => p.UnitInStock >= 1);
        Assert.NotNull(inStock);
        var productId = inStock!.Id;

        var addCart = await _client.PostAsJsonAsync("/api/cart/add", new { productId, quantity = 1 }, TestJson.Options);
        addCart.EnsureSuccessStatusCode();
        var addCartEnv = await addCart.Content.ReadFromJsonAsync<BaseResponseDto<CartDto>>(TestJson.Options);
        Assert.True(addCartEnv?.Success);

        var orderPayload = new CreateOrderDto
        {
            ShippingAddressId = addressId,
            PaymentMethodId = paymentMethodId,
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 1 }
            },
            Notes = "integration-test"
        };

        var orderRes = await _client.PostAsJsonAsync("/api/order", orderPayload, TestJson.Options);
        var orderRaw = await orderRes.Content.ReadAsStringAsync();
        Assert.True(
            orderRes.StatusCode == System.Net.HttpStatusCode.Created,
            $"Beklenen 201, gelen {(int)orderRes.StatusCode}. Gövde: {orderRaw}");

        var orderEnv = JsonSerializer.Deserialize<BaseResponseDto<OrderDto>>(orderRaw, TestJson.Options);
        Assert.True(orderEnv?.Success);
        Assert.NotNull(orderEnv!.Data);
        Assert.False(string.IsNullOrWhiteSpace(orderEnv.Data.OrderNumber));
        Assert.Equal(userId, orderEnv.Data.UserId);
        Assert.Single(orderEnv.Data.Items);
        Assert.Equal("Pending", orderEnv.Data.Status);
    }

    private sealed class BearerTokenScope : IDisposable
    {
        private readonly HttpClient _client;
        private readonly AuthenticationHeaderValue? _previous;

        public BearerTokenScope(HttpClient client, string token)
        {
            _client = client;
            _previous = client.DefaultRequestHeaders.Authorization;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void Dispose()
        {
            _client.DefaultRequestHeaders.Authorization = _previous;
        }
    }
}
