using System.Net.Http.Json;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.IntegrationTests.Support;
using Xunit;

namespace EcommerceBackend.IntegrationTests.Catalog;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class CatalogApiTests
{
    private readonly HttpClient _client;

    public CatalogApiTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Featured_products_returns_seeded_data()
    {
        var response = await _client.GetAsync("/api/product/featured");
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<BaseResponseDto<List<ProductDto>>>(TestJson.Options);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        Assert.NotEmpty(envelope.Data);
    }

    [Fact]
    public async Task Product_paged_list_returns_items_and_total()
    {
        var response = await _client.GetAsync("/api/product?pageNumber=1&pageSize=5");
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<BaseResponseDto<PagedResultDto<ProductDto>>>(TestJson.Options);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        Assert.NotEmpty(envelope.Data.Items);
        Assert.True(envelope.Data.TotalCount >= envelope.Data.Items.Count);
    }

    [Fact]
    public async Task Featured_twice_second_call_still_ok_redis_warm_path()
    {
        await _client.GetAsync("/api/product/featured");
        var response = await _client.GetAsync("/api/product/featured");
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<BaseResponseDto<List<ProductDto>>>(TestJson.Options);
        Assert.True(envelope?.Success == true);
        Assert.NotEmpty(envelope!.Data!);
    }
}
