namespace EcommerceBackend.Application.Options;

public class CatalogCacheOptions
{
    public const string SectionName = "CatalogCache";

    public int FeaturedTtlMinutes { get; set; } = 5;

    public int DiscountedTtlMinutes { get; set; } = 5;
}
