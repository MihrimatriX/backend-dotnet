using EcommerceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace EcommerceBackend.Infrastructure.Data;

/// <summary>
/// Katalog dışındaki tüm işlevsel tablolara örnek veri ekler (adres, ödeme, sipariş, bildirim, yardım, destek, sepet, favori vb.).
/// Tekrar çalıştırıldığında aynı işaretli kayıt varsa atlanır.
/// <c>seed-demo-force</c> veya <c>seed-demo --force</c> ile önceki demo satırları silinip seed yeniden uygulanır.
/// </summary>
public static class DemoDataSeeder
{
    private const string HelpMarkerTitle = "[DEMO-SEED] Yardım merkezi — hızlı başlangıç";
    private const string FaqMarkerQuestion = "[DEMO-SEED] Siparişimi nasıl iptal ederim?";
    private const string TicketMarkerSubject = "[DEMO-SEED] Kargo gecikmesi hakkında";
    private const string AddressTitleHome = "[DEMO] Ev adresi";
    private const string AddressTitleWork = "[DEMO] İş adresi";
    private const string NotificationTitlePrefix = "[DEMO-SEED]";
    private const string SeedLoginIp = "203.0.113.50";

    public static async Task SeedAsync(ApplicationDbContext context, bool force = false, CancellationToken cancellationToken = default)
    {
        if (force)
        {
            await ClearPreviousDemoSeedAsync(context, cancellationToken);
        }
        else if (await context.HelpArticles.AnyAsync(h => h.Title == HelpMarkerTitle, cancellationToken))
        {
            Log.Information("Demo seed atlandı: daha önce uygulanmış ({Marker}). Tekrar için: seed-demo --force", HelpMarkerTitle);
            return;
        }

        var users = await context.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(cancellationToken);
        if (users.Count == 0)
        {
            Log.Warning("Demo seed: kullanıcı yok; önce uygulama ile temel DataSeeder çalışmalı.");
            return;
        }

        var products = await context.Products.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.Id).Take(200).ToListAsync(cancellationToken);
        if (products.Count == 0)
        {
            Log.Warning("Demo seed: aktif ürün yok.");
            return;
        }

        var rnd = new Random(42);

        await SeedHelpAndFaqAsync(context, cancellationToken);
        await SeedUserSettingsAndPrivacyAsync(context, users, cancellationToken);
        await SeedAddressesAndPaymentsAsync(context, users, rnd, cancellationToken);
        await SeedLoginHistoryAsync(context, users, rnd, cancellationToken);
        await SeedNotificationsAsync(context, users, rnd, cancellationToken);
        await SeedFavoritesAsync(context, users, products, rnd, cancellationToken);
        await SeedCartItemsAsync(context, users, products, rnd, cancellationToken);
        await SeedOrdersAsync(context, users, products, rnd, cancellationToken);
        await SeedSupportAsync(context, users, cancellationToken);

        Log.Information("Demo seed tamamlandı.");
    }

    /// <summary>
    /// <c>--force</c> ile yeniden seed öncesi, demo ile oluşturulduğu bilinen satırları kaldırır.
    /// </summary>
    private static async Task ClearPreviousDemoSeedAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var help = await context.HelpArticles
            .Where(h => h.Title.StartsWith("[DEMO-SEED]"))
            .ToListAsync(cancellationToken);
        if (help.Count > 0)
            context.HelpArticles.RemoveRange(help);

        var faqs = await context.Faqs
            .Where(f => f.Question.StartsWith("[DEMO-SEED]"))
            .ToListAsync(cancellationToken);
        if (faqs.Count > 0)
            context.Faqs.RemoveRange(faqs);

        var demoTicketIds = await context.SupportTickets
            .Where(t => t.Subject == TicketMarkerSubject)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
        if (demoTicketIds.Count > 0)
        {
            var messages = await context.SupportMessages
                .Where(m => demoTicketIds.Contains(m.TicketId))
                .ToListAsync(cancellationToken);
            context.SupportMessages.RemoveRange(messages);

            var tickets = await context.SupportTickets
                .Where(t => demoTicketIds.Contains(t.Id))
                .ToListAsync(cancellationToken);
            context.SupportTickets.RemoveRange(tickets);
        }

        var demoOrders = await context.Orders
            .Where(o => o.OrderNumber.StartsWith("DEMO-") || o.Notes == "Demo seed siparişi.")
            .ToListAsync(cancellationToken);
        if (demoOrders.Count > 0)
        {
            var orderIds = demoOrders.ConvertAll(o => o.Id);
            var items = await context.OrderItems
                .Where(i => orderIds.Contains(i.OrderId))
                .ToListAsync(cancellationToken);
            context.OrderItems.RemoveRange(items);
            context.Orders.RemoveRange(demoOrders);
        }

        var logins = await context.LoginHistories
            .Where(l => l.IpAddress == SeedLoginIp)
            .ToListAsync(cancellationToken);
        if (logins.Count > 0)
            context.LoginHistories.RemoveRange(logins);

        var notifs = await context.Notifications
            .Where(n => n.Title.StartsWith(NotificationTitlePrefix))
            .ToListAsync(cancellationToken);
        if (notifs.Count > 0)
            context.Notifications.RemoveRange(notifs);

        await context.SaveChangesAsync(cancellationToken);
        Log.Information("Force demo seed: önceki demo kayıtları (yardım, sipariş, bildirim vb.) temizlendi.");
    }

    private static async Task SeedHelpAndFaqAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var articles = new List<HelpArticle>
        {
            new()
            {
                Title = HelpMarkerTitle,
                Content = "Hesabınızdan siparişlerinizi takip edebilir, iade talebi oluşturabilir ve adreslerinizi yönetebilirsiniz.",
                Category = "Hesap",
                Tags = "demo, başlangıç, hesap",
                ViewCount = 42,
                IsPublished = true
            },
            new()
            {
                Title = "[DEMO-SEED] Kargo ve teslimat süreleri",
                Content = "Standart kargo 2–4 iş günü, hızlı kargo 1–2 iş günü içinde teslim edilir. Tatil günleri süreye dahil değildir.",
                Category = "Kargo",
                Tags = "kargo, teslimat",
                ViewCount = 128,
                IsPublished = true
            },
            new()
            {
                Title = "[DEMO-SEED] Ödeme yöntemleri",
                Content = "Kredi kartı, banka kartı ve havale ile ödeme kabul edilir. Taksit seçenekleri bankanıza göre değişir.",
                Category = "Ödeme",
                Tags = "ödeme, kart",
                ViewCount = 90,
                IsPublished = true
            }
        };

        var faqs = new List<Faq>
        {
            new()
            {
                Question = FaqMarkerQuestion,
                Answer = "Hesabım > Siparişlerim üzerinden iptal edilebilir siparişlerde 'İptal' düğmesini kullanın. Kargoya verilmiş siparişlerde iade süreci geçerlidir.",
                Category = "Sipariş",
                ViewCount = 15,
                IsPublished = true
            },
            new()
            {
                Question = "[DEMO-SEED] Ücretsiz kargo var mı?",
                Answer = "Belirli tutarın üzerindeki siparişlerde ücretsiz kargo kampanyalarımız olabilir; ödeme adımında güncel koşullar gösterilir.",
                Category = "Kargo",
                ViewCount = 33,
                IsPublished = true
            },
            new()
            {
                Question = "[DEMO-SEED] İade süresi ne kadar?",
                Answer = "Ürün tesliminden itibaren 14 gün içinde cayma hakkınızı kullanabilirsiniz (istisnalar ürün sayfasında belirtilir).",
                Category = "İade",
                ViewCount = 21,
                IsPublished = true
            }
        };

        await context.HelpArticles.AddRangeAsync(articles, cancellationToken);
        await context.Faqs.AddRangeAsync(faqs, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedUserSettingsAndPrivacyAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> users,
        CancellationToken cancellationToken)
    {
        foreach (var u in users)
        {
            if (!await context.UserSettings.AnyAsync(s => s.UserId == u.Id, cancellationToken))
            {
                context.UserSettings.Add(new UserSettings
                {
                    UserId = u.Id,
                    Language = u.Id % 2 == 0 ? "tr" : "en",
                    Timezone = "Europe/Istanbul",
                    Currency = "TRY",
                    EmailNotifications = true,
                    SmsNotifications = u.Id % 3 == 0,
                    PushNotifications = true,
                    MarketingEmails = false,
                    OrderUpdates = true,
                    PriceAlerts = true,
                    StockNotifications = true,
                    Theme = u.Id % 2 == 0 ? "light" : "dark",
                    ItemsPerPage = 20,
                    AutoSaveCart = true,
                    ShowProductRecommendations = true,
                    EnableLocationServices = false
                });
            }

            if (!await context.PrivacySettings.AnyAsync(s => s.UserId == u.Id, cancellationToken))
            {
                context.PrivacySettings.Add(new PrivacySettings
                {
                    UserId = u.Id,
                    ProfileVisibility = true,
                    ShowEmail = false,
                    ShowPhone = false,
                    AllowDataCollection = true,
                    AllowAnalytics = true,
                    AllowCookies = true,
                    AllowMarketing = false,
                    DataSharing = false
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAddressesAndPaymentsAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> users,
        Random rnd,
        CancellationToken cancellationToken)
    {
        foreach (var u in users)
        {
            if (await context.Addresses.AnyAsync(a => a.UserId == u.Id && a.Title == AddressTitleHome, cancellationToken))
                continue;

            var home = new Address
            {
                UserId = u.Id,
                Title = AddressTitleHome,
                FullAddress = $"Atatürk Bulvarı No:{10 + u.Id} Daire:2, Çankaya",
                City = u.City ?? "Ankara",
                District = "Çankaya",
                PostalCode = u.PostalCode ?? "06000",
                Country = "Turkey",
                IsDefault = true,
                PhoneNumber = u.PhoneNumber ?? "+90 555 000 0000"
            };
            var work = new Address
            {
                UserId = u.Id,
                Title = AddressTitleWork,
                FullAddress = $"Teknokent Sokak B Blok Kat:{u.Id % 5 + 1}",
                City = "İstanbul",
                District = "Sarıyer",
                PostalCode = "34467",
                Country = "Turkey",
                IsDefault = false,
                PhoneNumber = u.PhoneNumber
            };
            await context.Addresses.AddRangeAsync(new[] { home, work }, cancellationToken);

            var pmCard = new PaymentMethod
            {
                UserId = u.Id,
                Type = "CreditCard",
                CardHolderName = $"{u.FirstName} {u.LastName}",
                CardNumber = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"SEED_DEMO_PAN_{u.Id}_{Guid.NewGuid():N}")),
                ExpiryMonth = 6 + (u.Id % 6),
                ExpiryYear = 2028,
                Cvv = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("SEED")),
                IsDefault = true
            };
            await context.PaymentMethods.AddAsync(pmCard, cancellationToken);

            if (u.Id % 2 == 0)
            {
                await context.PaymentMethods.AddAsync(new PaymentMethod
                {
                    UserId = u.Id,
                    Type = "DebitCard",
                    CardHolderName = u.FirstName + " Ek Kart",
                    CardNumber = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"SEED_DEMO_PAN2_{u.Id}_{rnd.Next(1000, 9999)}")),
                    ExpiryMonth = 12,
                    ExpiryYear = 2029,
                    Cvv = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("SE2")),
                    IsDefault = false
                }, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedLoginHistoryAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> users,
        Random rnd,
        CancellationToken cancellationToken)
    {
        var entries = new List<LoginHistory>();
        foreach (var u in users)
        {
            for (var i = 0; i < 4 + rnd.Next(0, 4); i++)
            {
                var ok = rnd.Next(10) != 0;
                entries.Add(new LoginHistory
                {
                    UserId = u.Id,
                    LoginAt = DateTime.UtcNow.AddDays(-rnd.Next(1, 120)).AddHours(rnd.Next(0, 23)),
                    IpAddress = SeedLoginIp,
                    UserAgent = ok
                        ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                        : "curl/8.0 demo-fail",
                    Location = ok ? "Istanbul, TR" : null,
                    IsSuccessful = ok,
                    FailureReason = ok ? null : "Invalid password (demo)"
                });
            }
        }

        await context.LoginHistories.AddRangeAsync(entries, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedNotificationsAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> users,
        Random rnd,
        CancellationToken cancellationToken)
    {
        var types = new[] { "Order", "Product", "System", "Promotion" };
        var list = new List<Notification>();

        foreach (var u in users)
        {
            for (var n = 0; n < 3 + rnd.Next(0, 3); n++)
            {
                var type = types[rnd.Next(types.Length)];
                list.Add(new Notification
                {
                    UserId = u.Id,
                    Title = $"{NotificationTitlePrefix} {type} bildirimi #{n + 1}",
                    Message = type switch
                    {
                        "Order" => "Siparişiniz kargoya verildi. Takip numaranız hesabınızda görüntülenir.",
                        "Product" => "Favorilerinizden bir ürünün fiyatı düştü.",
                        "Promotion" => "Bu hafta seçili kategorilerde ek indirim.",
                        _ => "Hesap güvenliğiniz için şifrenizi düzenli yenileyin."
                    },
                    Type = type,
                    ActionUrl = type == "Order" ? "/account/orders" : "/products",
                    IsRead = rnd.Next(3) == 0,
                    ReadAt = rnd.Next(3) == 0 ? DateTime.UtcNow.AddDays(-1) : null
                });
            }
        }

        await context.Notifications.AddRangeAsync(list, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedFavoritesAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> users,
        IReadOnlyList<Product> products,
        Random rnd,
        CancellationToken cancellationToken)
    {
        foreach (var u in users)
        {
            var taken = new HashSet<int>();
            var target = Math.Min(5 + rnd.Next(0, 4), products.Count);
            var attempts = 0;
            while (taken.Count < target && attempts < products.Count * 4)
            {
                attempts++;
                var p = products[rnd.Next(products.Count)];
                if (taken.Contains(p.Id))
                    continue;

                if (await context.Favorites.AnyAsync(f => f.UserId == u.Id && f.ProductId == p.Id, cancellationToken))
                    continue;

                taken.Add(p.Id);
                context.Favorites.Add(new Favorite { UserId = u.Id, ProductId = p.Id });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCartItemsAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> users,
        IReadOnlyList<Product> products,
        Random rnd,
        CancellationToken cancellationToken)
    {
        foreach (var u in users.Where(x => !x.Email.Contains("admin", StringComparison.OrdinalIgnoreCase)))
        {
            var count = 1 + rnd.Next(0, 4);
            for (var i = 0; i < count; i++)
            {
                var p = products[rnd.Next(products.Count)];
                var existing = await context.CartItems.FirstOrDefaultAsync(
                    c => c.UserId == u.Id && c.ProductId == p.Id,
                    cancellationToken);
                if (existing != null)
                {
                    existing.Quantity = Math.Min(99, existing.Quantity + rnd.Next(1, 3));
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    context.CartItems.Add(new CartItem
                    {
                        UserId = u.Id,
                        ProductId = p.Id,
                        Quantity = rnd.Next(1, 5)
                    });
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedOrdersAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> users,
        IReadOnlyList<Product> products,
        Random rnd,
        CancellationToken cancellationToken)
    {
        var statuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
        var buyers = users.Where(u => !u.Email.Contains("admin", StringComparison.OrdinalIgnoreCase)).ToList();
        if (buyers.Count == 0)
            buyers = users.ToList();

        foreach (var u in buyers.Take(4))
        {
            var addrIds = await context.Addresses
                .Where(a => a.UserId == u.Id)
                .OrderBy(a => a.Id)
                .Select(a => a.Id)
                .Take(2)
                .ToListAsync(cancellationToken);
            if (addrIds.Count == 0)
                continue;

            var pmId = await context.PaymentMethods
                .Where(p => p.UserId == u.Id && p.IsDefault)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var status = statuses[rnd.Next(statuses.Length)];
            var orderNumber = $"DEMO-{u.Id}-{Guid.NewGuid():N}";
            if (orderNumber.Length > 50)
                orderNumber = orderNumber[..50];

            var order = new Order
            {
                UserId = u.Id,
                OrderNumber = orderNumber,
                Status = status,
                ShippingAddressId = addrIds[0],
                BillingAddressId = addrIds.Count > 1 ? addrIds[1] : addrIds[0],
                PaymentMethodId = pmId == 0 ? null : pmId,
                Notes = "Demo seed siparişi.",
                ShippedAt = status is "Shipped" or "Delivered"
                    ? DateTime.UtcNow.AddDays(-5)
                    : null,
                DeliveredAt = status == "Delivered"
                    ? DateTime.UtcNow.AddDays(-2)
                    : null
            };

            var lineCount = 1 + rnd.Next(0, 3);
            decimal total = 0;
            var lineProducts = new HashSet<int>();
            var items = new List<OrderItem>();
            for (var i = 0; i < lineCount && lineProducts.Count < products.Count; i++)
            {
                var p = products[rnd.Next(products.Count)];
                if (!lineProducts.Add(p.Id))
                {
                    i--;
                    continue;
                }

                var qty = rnd.Next(1, 4);
                var unit = Math.Round(p.UnitPrice * (1 - p.Discount / 100m), 2);
                total += unit * qty;
                items.Add(new OrderItem
                {
                    ProductId = p.Id,
                    Quantity = qty,
                    UnitPrice = unit,
                    Discount = 0
                });
            }

            order.TotalAmount = Math.Round(total, 2);
            await context.Orders.AddAsync(order, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            foreach (var it in items)
                it.OrderId = order.Id;

            await context.OrderItems.AddRangeAsync(items, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedSupportAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> users,
        CancellationToken cancellationToken)
    {
        var customer = users.FirstOrDefault(u => u.Email.Contains("user1", StringComparison.OrdinalIgnoreCase))
            ?? users.First();
        var supportAgent = users.FirstOrDefault(u => u.Email.Equals("admin@example.com", StringComparison.OrdinalIgnoreCase))
            ?? customer;

        var ticket = new SupportTicket
        {
            UserId = customer.Id,
            Subject = TicketMarkerSubject,
            Description = "Sipariş numaralı gönderim 3 gündür hareket etmiyor; bilgi rica ederim.",
            Category = "Kargo",
            Priority = "High",
            Status = "Open",
            AssignedTo = "support-demo@example.com"
        };
        await context.SupportTickets.AddAsync(ticket, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var messages = new[]
        {
            (customer.Id, false, "Merhaba, siparişim hâlâ dağıtımda görünmüyor."),
            (supportAgent.Id, true, "Merhaba, kargo firmasından güncelleme talep ettik; 24 saat içinde size döneceğiz.")
        };

        foreach (var (userId, fromSupport, text) in messages)
        {
            await context.SupportMessages.AddAsync(new SupportMessage
            {
                TicketId = ticket.Id,
                UserId = userId,
                Message = text,
                IsFromSupport = fromSupport
            }, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
