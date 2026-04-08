using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceBackend.Infrastructure.Data;

/// <summary>
/// dotnet ef için Npgsql modeli; gerçek sunucu gerekmez (yalnızca migration üretimi).
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=127.0.0.1;Database=_ef_design;Username=_;Password=_");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
