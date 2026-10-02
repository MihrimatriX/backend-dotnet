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

        // Sözleşme uyumu (ContractAlignment migration'ının SQLite karşılığı) — eski dosyalara eksik kolonları ekler.
        await AddColumnIfMissingAsync(db, "users", "tokens_revoked_at", "TEXT NULL", ct);
        await AddColumnIfMissingAsync(db, "users", "revoke_except_jti", "TEXT NULL", ct);
        foreach (var column in new[]
                 {
                     "idempotency_key", "tracking_number", "carrier", "estimated_delivery_at", "cancel_reason",
                     "return_reason", "return_requested_at",
                 })
        {
            await AddColumnIfMissingAsync(db, "orders", column, "TEXT NULL", ct);
        }
    }

    private static async Task AddColumnIfMissingAsync(
        ApplicationDbContext db,
        string table,
        string column,
        string definition,
        CancellationToken ct)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = '{column}'";
        var exists = Convert.ToInt64(await command.ExecuteScalarAsync(ct)) > 0;
        if (exists)
            return;

        // Tablo/kolon adları kod içi sabitlerdir (kullanıcı girdisi değil).
        command.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
        await command.ExecuteNonQueryAsync(ct);
    }
}
