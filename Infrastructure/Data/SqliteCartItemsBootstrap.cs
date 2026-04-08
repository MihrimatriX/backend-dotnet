using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Data;

/// <summary>
/// SQLite geliştirme ortamında <see cref="ApplicationDbContext"/> modeline sonradan eklenen
/// <c>cart_items</c> tablosunu, mevcut <c>EnsureCreated</c> veritabanlarına uygular.
/// PostgreSQL için normal EF migration kullanılır.
/// </summary>
public static class SqliteCartItemsBootstrap
{
    public static async Task EnsureAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS cart_items (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL REFERENCES users (id) ON DELETE CASCADE,
                product_id INTEGER NOT NULL REFERENCES products (id) ON DELETE RESTRICT,
                quantity INTEGER NOT NULL,
                created_at TEXT NOT NULL DEFAULT (datetime('now')),
                updated_at TEXT NOT NULL DEFAULT (datetime('now')),
                is_active INTEGER NOT NULL DEFAULT 1
            );
            """,
            cancellationToken: ct);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS IX_cart_items_user_id_product_id
            ON cart_items (user_id, product_id);
            """,
            cancellationToken: ct);
    }
}
