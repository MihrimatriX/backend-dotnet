namespace EcommerceBackend.Application.Common;

/// <summary>
/// Liste uçları için ortak sayfalama kuralı (§1.3): <c>pageNumber &lt; 1</c> → 1, <c>pageSize</c> 1..100.
/// </summary>
public readonly record struct Paging(int PageNumber, int PageSize)
{
    public const int MaxPageSize = 100;

    public int Skip => (PageNumber - 1) * PageSize;

    public static Paging Normalize(int pageNumber, int pageSize) =>
        new(Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, MaxPageSize));
}
