using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EcommerceBackend.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IAddressRepository addressRepository,
            IPaymentMethodRepository paymentMethodRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _addressRepository = addressRepository;
            _paymentMethodRepository = paymentMethodRepository;
        }

        public async Task<BaseResponseDto<List<OrderDto>>> GetUserOrdersAsync(int userId)
        {
            try
            {
                var orders = await _orderRepository.GetUserOrdersAsync(userId);
                var orderDtos = orders.Select(ConvertToDto).ToList();

                return BaseResponseDto<List<OrderDto>>.SuccessResult("Orders retrieved successfully", orderDtos);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<OrderDto>>.ErrorResult($"Error retrieving orders: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<OrderDto>> GetOrderByIdAsync(int orderId, int userId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null || order.UserId != userId)
                {
                    return BaseResponseDto<OrderDto>.ErrorResult("Order not found");
                }

                return BaseResponseDto<OrderDto>.SuccessResult("Order retrieved successfully", ConvertToDto(order));
            }
            catch (Exception ex)
            {
                return BaseResponseDto<OrderDto>.ErrorResult($"Error retrieving order: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<OrderDto>> CreateOrderAsync(int userId, CreateOrderDto createOrderDto)
        {
            try
            {
                // Validate shipping address
                var shippingAddress = await _addressRepository.GetByIdAsync(createOrderDto.ShippingAddressId);
                if (shippingAddress == null || shippingAddress.UserId != userId)
                {
                    return BaseResponseDto<OrderDto>.ErrorResult("Invalid shipping address");
                }

                // Validate payment method
                var paymentMethod = await _paymentMethodRepository.GetByIdAsync(createOrderDto.PaymentMethodId);
                if (paymentMethod == null || paymentMethod.UserId != userId)
                {
                    return BaseResponseDto<OrderDto>.ErrorResult("Invalid payment method");
                }

                // Validate products and calculate total
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in createOrderDto.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null || !product.IsActive)
                    {
                        return BaseResponseDto<OrderDto>.ErrorResult($"Product {item.ProductId} not found or inactive");
                    }

                    if (product.UnitInStock < item.Quantity)
                    {
                        return BaseResponseDto<OrderDto>.ErrorResult($"Insufficient stock for product {product.ProductName}");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.UnitPrice
                    };

                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;

                    // Update stock
                    product.UnitInStock -= item.Quantity;
                    await _productRepository.UpdateAsync(product.Id, product);
                }

                // Create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    UserId = userId,
                    OrderItems = orderItems,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    ShippingAddressId = createOrderDto.ShippingAddressId,
                    BillingAddressId = createOrderDto.ShippingAddressId, // Same as shipping for now
                    PaymentMethodId = createOrderDto.PaymentMethodId,
                    Notes = createOrderDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdOrder = await _orderRepository.CreateAsync(order);
                return BaseResponseDto<OrderDto>.SuccessResult("Order created successfully", ConvertToDto(createdOrder));
            }
            catch (Exception ex)
            {
                return BaseResponseDto<OrderDto>.ErrorResult($"Error creating order: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<OrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return BaseResponseDto<OrderDto>.ErrorResult("Order not found");
                }

                order.Status = updateOrderStatusDto.Status;
                order.UpdatedAt = DateTime.UtcNow;

                var updatedOrder = await _orderRepository.UpdateAsync(order);
                return BaseResponseDto<OrderDto>.SuccessResult("Order status updated successfully", ConvertToDto(updatedOrder));
            }
            catch (Exception ex)
            {
                return BaseResponseDto<OrderDto>.ErrorResult($"Error updating order status: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> CancelOrderAsync(int orderId, int userId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null || order.UserId != userId)
                {
                    return BaseResponseDto<string>.ErrorResult("Order not found");
                }

                if (order.Status == "Delivered" || order.Status == "Cancelled")
                {
                    return BaseResponseDto<string>.ErrorResult("Cannot cancel this order");
                }

                // Restore stock
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.UnitInStock += item.Quantity;
                        await _productRepository.UpdateAsync(product.Id, product);
                    }
                }

                order.Status = "Cancelled";
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);
                return BaseResponseDto<string>.SuccessResult("Order cancelled successfully", "Order cancelled successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error cancelling order: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<List<OrderDto>>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var orders = await _orderRepository.GetAllOrdersAsync();
                var orderDtos = orders.Select(ConvertToDto).ToList();

                return BaseResponseDto<List<OrderDto>>.SuccessResult("Orders retrieved successfully", orderDtos);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<OrderDto>>.ErrorResult($"Error retrieving orders: {ex.Message}");
            }
        }

        private OrderDto ConvertToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                UserName = order.User?.FirstName + " " + order.User?.LastName ?? "",
                UserEmail = order.User?.Email ?? "",
                Items = order.OrderItems?.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.ProductName ?? "",
                    ProductImageUrl = item.Product?.ImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList() ?? new List<OrderItemDto>(),
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress != null ? new AddressDto
                {
                    Id = order.ShippingAddress.Id,
                    Title = order.ShippingAddress.Title,
                    FullAddress = order.ShippingAddress.FullAddress,
                    City = order.ShippingAddress.City,
                    District = order.ShippingAddress.District,
                    PostalCode = order.ShippingAddress.PostalCode,
                    PhoneNumber = order.ShippingAddress.PhoneNumber,
                    IsDefault = order.ShippingAddress.IsDefault
                } : new AddressDto(),
                BillingAddress = order.BillingAddress != null ? new AddressDto
                {
                    Id = order.BillingAddress.Id,
                    Title = order.BillingAddress.Title,
                    FullAddress = order.BillingAddress.FullAddress,
                    City = order.BillingAddress.City,
                    District = order.BillingAddress.District,
                    PostalCode = order.BillingAddress.PostalCode,
                    PhoneNumber = order.BillingAddress.PhoneNumber,
                    IsDefault = order.BillingAddress.IsDefault
                } : new AddressDto(),
                PaymentMethod = order.PaymentMethod != null ? new PaymentMethodDto
                {
                    Id = order.PaymentMethod.Id,
                    Type = order.PaymentMethod.Type,
                    CardNumber = order.PaymentMethod.CardNumber,
                    CardHolderName = order.PaymentMethod.CardHolderName,
                    ExpiryMonth = order.PaymentMethod.ExpiryMonth,
                    ExpiryYear = order.PaymentMethod.ExpiryYear,
                    IsDefault = order.PaymentMethod.IsDefault,
                    BankName = order.PaymentMethod.BankName,
                    AccountNumber = order.PaymentMethod.AccountNumber,
                    AccountHolderName = order.PaymentMethod.AccountHolderName
                } : null,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}