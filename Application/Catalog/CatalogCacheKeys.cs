namespace EcommerceBackend.Application.Catalog;

public static class CatalogCacheKeys
{
    // v2: ProductDto alt kategori, puan ve tarih alanlarını içerir; eski şekildeki önbellek kullanılmaz.
    public const string FeaturedProducts = "catalog:products:featured:v2";

    public const string DiscountedProducts = "catalog:products:discounted:v2";
}
