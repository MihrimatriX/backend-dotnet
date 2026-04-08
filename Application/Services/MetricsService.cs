using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MetricsService> _logger;

        public MetricsService(ApplicationDbContext context, ILogger<MetricsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BaseResponseDto<object>> GetDashboardMetricsAsync()
        {
            try
            {
                var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
                var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
                var totalOrders = await _context.Orders.CountAsync(o => o.IsActive);
                var totalCategories = await _context.Categories.CountAsync(c => c.IsActive);
                var totalReviews = await _context.Reviews.CountAsync(r => r.IsActive);
                var totalCampaigns = await _context.Campaigns.CountAsync(c => c.IsActive);

                var totalSales = await _context.Orders
                    .Where(o => o.IsActive)
                    .SumAsync(o => o.TotalAmount);

                var metrics = new
                {
                    TotalProducts = totalProducts,
                    TotalUsers = totalUsers,
                    TotalOrders = totalOrders,
                    TotalCategories = totalCategories,
                    TotalReviews = totalReviews,
                    TotalCampaigns = totalCampaigns,
                    TotalSales = totalSales,
                    LastUpdated = DateTime.UtcNow
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = metrics,
                    Message = "Dashboard metrics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard metrics");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error retrieving dashboard metrics"
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetProductMetricsAsync()
        {
            try
            {
                var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
                var activeProducts = await _context.Products.CountAsync(p => p.IsActive && p.UnitInStock > 0);
                var outOfStockProducts = await _context.Products.CountAsync(p => p.IsActive && p.UnitInStock == 0);
                var averagePrice = await _context.Products
                    .Where(p => p.IsActive)
                    .AverageAsync(p => p.UnitPrice);

                var topCategories = await _context.Categories
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        CategoryName = c.CategoryName,
                        ProductCount = c.Products.Count(p => p.IsActive)
                    })
                    .OrderByDescending(x => x.ProductCount)
                    .Take(5)
                    .ToListAsync();

                var metrics = new
                {
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    OutOfStockProducts = outOfStockProducts,
                    AveragePrice = Math.Round(averagePrice, 2),
                    TopCategories = topCategories,
                    LastUpdated = DateTime.UtcNow
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = metrics,
                    Message = "Product metrics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product metrics");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error retrieving product metrics"
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetOrderMetricsAsync()
        {
            try
            {
                var totalOrders = await _context.Orders.CountAsync(o => o.IsActive);
                var pendingOrders = await _context.Orders.CountAsync(o => o.IsActive && o.Status == "Pending");
                var completedOrders = await _context.Orders.CountAsync(o => o.IsActive && o.Status == "Completed");
                var cancelledOrders = await _context.Orders.CountAsync(o => o.IsActive && o.Status == "Cancelled");

                var totalRevenue = await _context.Orders
                    .Where(o => o.IsActive && o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount);

                var averageOrderValue = await _context.Orders
                    .Where(o => o.IsActive && o.Status == "Completed")
                    .AverageAsync(o => o.TotalAmount);

                var recentOrders = await _context.Orders
                    .Where(o => o.IsActive)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.TotalAmount,
                        o.Status,
                        o.CreatedAt
                    })
                    .ToListAsync();

                var metrics = new
                {
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    CompletedOrders = completedOrders,
                    CancelledOrders = cancelledOrders,
                    TotalRevenue = Math.Round(totalRevenue, 2),
                    AverageOrderValue = Math.Round(averageOrderValue, 2),
                    RecentOrders = recentOrders,
                    LastUpdated = DateTime.UtcNow
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = metrics,
                    Message = "Order metrics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order metrics");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error retrieving order metrics"
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetUserMetricsAsync()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
                var verifiedUsers = await _context.Users.CountAsync(u => u.IsActive && u.IsEmailVerified);
                var unverifiedUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsEmailVerified);

                var recentUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.IsEmailVerified,
                        u.CreatedAt
                    })
                    .ToListAsync();

                var userRegistrationsByMonth = await _context.Users
                    .Where(u => u.IsActive && u.CreatedAt >= DateTime.UtcNow.AddMonths(-12))
                    .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

                var metrics = new
                {
                    TotalUsers = totalUsers,
                    VerifiedUsers = verifiedUsers,
                    UnverifiedUsers = unverifiedUsers,
                    RecentUsers = recentUsers,
                    UserRegistrationsByMonth = userRegistrationsByMonth,
                    LastUpdated = DateTime.UtcNow
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = metrics,
                    Message = "User metrics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user metrics");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error retrieving user metrics"
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetSalesMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var salesData = await _context.Orders
                    .Where(o => o.IsActive && o.Status == "Completed" && o.CreatedAt >= start && o.CreatedAt <= end)
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var totalSales = salesData.Sum(x => x.TotalSales);
                var totalOrders = salesData.Sum(x => x.OrderCount);
                var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

                var metrics = new
                {
                    StartDate = start,
                    EndDate = end,
                    TotalSales = Math.Round(totalSales, 2),
                    TotalOrders = totalOrders,
                    AverageOrderValue = Math.Round(averageOrderValue, 2),
                    SalesData = salesData,
                    LastUpdated = DateTime.UtcNow
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = metrics,
                    Message = "Sales metrics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales metrics");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error retrieving sales metrics"
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetCategoryMetricsAsync()
        {
            try
            {
                var categoryStats = await _context.Categories
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        CategoryName = c.CategoryName,
                        ProductCount = c.Products.Count(p => p.IsActive),
                        TotalStock = c.Products.Where(p => p.IsActive).Sum(p => p.UnitInStock),
                        AveragePrice = c.Products.Where(p => p.IsActive).Average(p => p.UnitPrice)
                    })
                    .OrderByDescending(x => x.ProductCount)
                    .ToListAsync();

                var metrics = new
                {
                    CategoryStats = categoryStats,
                    TotalCategories = categoryStats.Count,
                    LastUpdated = DateTime.UtcNow
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = metrics,
                    Message = "Category metrics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category metrics");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error retrieving category metrics"
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetReviewMetricsAsync()
        {
            try
            {
                var totalReviews = await _context.Reviews.CountAsync(r => r.IsActive);
                var averageRating = await _context.Reviews
                    .Where(r => r.IsActive)
                    .AverageAsync(r => r.Rating);

                var ratingDistribution = await _context.Reviews
                    .Where(r => r.IsActive)
                    .GroupBy(r => r.Rating)
                    .Select(g => new
                    {
                        Rating = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Rating)
                    .ToListAsync();

                var recentReviews = await _context.Reviews
                    .Where(r => r.IsActive)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .Select(r => new
                    {
                        r.Id,
                        r.Rating,
                        r.Title,
                        r.Comment,
                        r.CreatedAt,
                        ProductName = r.Product.ProductName
                    })
                    .ToListAsync();

                var metrics = new
                {
                    TotalReviews = totalReviews,
                    AverageRating = Math.Round(averageRating, 2),
                    RatingDistribution = ratingDistribution,
                    RecentReviews = recentReviews,
                    LastUpdated = DateTime.UtcNow
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = metrics,
                    Message = "Review metrics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review metrics");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error retrieving review metrics"
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetCampaignMetricsAsync()
        {
            try
            {
                var totalCampaigns = await _context.Campaigns.CountAsync(c => c.IsActive);
                var activeCampaigns = await _context.Campaigns.CountAsync(c => c.IsActive);
                var inactiveCampaigns = await _context.Campaigns.CountAsync(c => !c.IsActive);

                var campaignStats = await _context.Campaigns
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        c.Id,
                        c.Title,
                        c.Discount,
                        c.IsActive,
                        c.CreatedAt
                    })
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var metrics = new
                {
                    TotalCampaigns = totalCampaigns,
                    ActiveCampaigns = activeCampaigns,
                    InactiveCampaigns = inactiveCampaigns,
                    CampaignStats = campaignStats,
                    LastUpdated = DateTime.UtcNow
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = metrics,
                    Message = "Campaign metrics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaign metrics");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error retrieving campaign metrics"
                };
            }
        }
    }
}
