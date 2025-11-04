using EcommerceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<PrivacySettings> PrivacySettings { get; set; }
        public DbSet<HelpArticle> HelpArticles { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200).HasColumnName("product_name");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(12,2)").HasColumnName("unit_price");
                entity.Property(e => e.QuantityPerUnit).IsRequired().HasMaxLength(50).HasColumnName("quantity_per_unit");
                entity.Property(e => e.Description).HasMaxLength(1000).HasColumnName("description");
                entity.Property(e => e.Discount).HasDefaultValue(0).HasColumnName("discount");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.SubCategoryId).HasColumnName("sub_category_id");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                entity.Property(e => e.UnitInStock).HasColumnName("unit_in_stock");
                
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SubCategory)
                    .WithMany(sc => sc.Products)
                    .HasForeignKey(e => e.SubCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100).HasColumnName("category_name");
                entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("description");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                
                entity.HasIndex(e => e.CategoryName).IsUnique();
                
                entity.HasMany(e => e.SubCategories)
                    .WithOne(sc => sc.Category)
                    .HasForeignKey(sc => sc.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // SubCategory configuration
            modelBuilder.Entity<SubCategory>(entity =>
            {
                entity.ToTable("sub_categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.SubCategoryName).IsRequired().HasMaxLength(100).HasColumnName("sub_category_name");
                entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("description");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasIndex(e => new { e.SubCategoryName, e.CategoryId }).IsUnique();
            });
            
            // Campaign configuration
            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.ToTable("campaigns"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200).HasColumnName("title");
                entity.Property(e => e.Subtitle).HasMaxLength(300).HasColumnName("subtitle");
                entity.Property(e => e.Description).HasMaxLength(1000).HasColumnName("description");
                entity.Property(e => e.ButtonText).HasMaxLength(50).HasColumnName("button_text");
                entity.Property(e => e.Discount).HasDefaultValue(0).HasColumnName("discount");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            });
            
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255).HasColumnName("email");
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255).HasColumnName("password");
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100).HasColumnName("first_name");
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100).HasColumnName("last_name");
                entity.Property(e => e.PhoneNumber).HasMaxLength(20).HasColumnName("phone_number");
                entity.Property(e => e.Address).HasColumnName("address");
                entity.Property(e => e.City).HasColumnName("city");
                entity.Property(e => e.PostalCode).HasColumnName("postal_code");
                entity.Property(e => e.IsEmailVerified).HasDefaultValue(false).HasColumnName("is_email_verified");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                // entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at"); // Column doesn't exist in database
                
                entity.HasIndex(e => e.Email).IsUnique();
            });
            
            // Review configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Rating).IsRequired().HasColumnName("rating");
                entity.Property(e => e.Comment).HasMaxLength(1000).HasColumnName("comment");
                entity.Property(e => e.Title).HasMaxLength(200).HasColumnName("title");
                entity.Property(e => e.IsVerified).HasDefaultValue(false).HasColumnName("is_verified");
                entity.Property(e => e.IsHelpful).HasDefaultValue(false).HasColumnName("is_helpful");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50).HasColumnName("order_number");
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(12,2)").HasColumnName("total_amount");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasColumnName("status");
                entity.Property(e => e.ShippingAddressId).HasColumnName("shipping_address_id");
                entity.Property(e => e.BillingAddressId).HasColumnName("billing_address_id");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                
                // Ignore navigation properties that are not mapped to database
                entity.Ignore(e => e.ShippingAddress);
                entity.Ignore(e => e.BillingAddress);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasIndex(e => e.OrderNumber).IsUnique();
            });
            
            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Quantity).IsRequired().HasColumnName("quantity");
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(12,2)").HasColumnName("unit_price");
                entity.Property(e => e.Discount).HasDefaultValue(0).HasColumnType("decimal(12,2)").HasColumnName("discount");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Favorite configuration
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.ToTable("favorites"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            });
            
            // Address configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("addresses"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100).HasColumnName("title");
                entity.Property(e => e.FullAddress).IsRequired().HasMaxLength(500).HasColumnName("full_address");
                entity.Property(e => e.City).IsRequired().HasMaxLength(100).HasColumnName("city");
                entity.Property(e => e.District).IsRequired().HasMaxLength(100).HasColumnName("district");
                entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20).HasColumnName("postal_code");
                entity.Property(e => e.Country).HasMaxLength(100).HasColumnName("country");
                entity.Property(e => e.IsDefault).HasDefaultValue(false).HasColumnName("is_default");
                entity.Property(e => e.PhoneNumber).HasMaxLength(20).HasColumnName("phone_number");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // PaymentMethod configuration
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("payment_methods"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50).HasColumnName("type");
                entity.Property(e => e.CardHolderName).IsRequired().HasMaxLength(100).HasColumnName("card_holder_name");
                entity.Property(e => e.CardNumber).IsRequired().HasMaxLength(20).HasColumnName("card_number");
                entity.Property(e => e.ExpiryMonth).IsRequired().HasColumnName("expiry_month");
                entity.Property(e => e.ExpiryYear).IsRequired().HasColumnName("expiry_year");
                entity.Property(e => e.Cvv).HasMaxLength(10).HasColumnName("cvv");
                entity.Property(e => e.BankName).HasMaxLength(100).HasColumnName("bank_name");
                entity.Property(e => e.AccountNumber).HasMaxLength(50).HasColumnName("account_number");
                entity.Property(e => e.AccountHolderName).HasMaxLength(100).HasColumnName("account_holder_name");
                entity.Property(e => e.IsDefault).HasDefaultValue(false).HasColumnName("is_default");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200).HasColumnName("title");
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000).HasColumnName("message");
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50).HasColumnName("type");
                entity.Property(e => e.ActionUrl).HasMaxLength(100).HasColumnName("action_url");
                entity.Property(e => e.IsRead).HasDefaultValue(false).HasColumnName("is_read");
                entity.Property(e => e.ReadAt).HasColumnName("read_at");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // LoginHistory configuration
            modelBuilder.Entity<LoginHistory>(entity =>
            {
                entity.ToTable("login_histories"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.LoginAt).HasColumnName("login_at");
                entity.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
                entity.Property(e => e.UserAgent).HasMaxLength(500).HasColumnName("user_agent");
                entity.Property(e => e.Location).HasMaxLength(100).HasColumnName("location");
                entity.Property(e => e.IsSuccessful).HasDefaultValue(true).HasColumnName("is_successful");
                entity.Property(e => e.FailureReason).HasMaxLength(200).HasColumnName("failure_reason");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // UserSettings configuration
            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.ToTable("user_settings"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Language).HasMaxLength(10).HasColumnName("language");
                entity.Property(e => e.Timezone).HasMaxLength(50).HasColumnName("timezone");
                entity.Property(e => e.Currency).HasMaxLength(10).HasColumnName("currency");
                entity.Property(e => e.EmailNotifications).HasDefaultValue(true).HasColumnName("email_notifications");
                entity.Property(e => e.SmsNotifications).HasDefaultValue(false).HasColumnName("sms_notifications");
                entity.Property(e => e.PushNotifications).HasDefaultValue(true).HasColumnName("push_notifications");
                entity.Property(e => e.MarketingEmails).HasDefaultValue(false).HasColumnName("marketing_emails");
                entity.Property(e => e.OrderUpdates).HasDefaultValue(true).HasColumnName("order_updates");
                entity.Property(e => e.PriceAlerts).HasDefaultValue(true).HasColumnName("price_alerts");
                entity.Property(e => e.StockNotifications).HasDefaultValue(true).HasColumnName("stock_notifications");
                entity.Property(e => e.Theme).HasMaxLength(20).HasColumnName("theme");
                entity.Property(e => e.ItemsPerPage).HasDefaultValue(20).HasColumnName("items_per_page");
                entity.Property(e => e.AutoSaveCart).HasDefaultValue(true).HasColumnName("auto_save_cart");
                entity.Property(e => e.ShowProductRecommendations).HasDefaultValue(true).HasColumnName("show_product_recommendations");
                entity.Property(e => e.EnableLocationServices).HasDefaultValue(false).HasColumnName("enable_location_services");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // PrivacySettings configuration
            modelBuilder.Entity<PrivacySettings>(entity =>
            {
                entity.ToTable("privacy_settings"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ProfileVisibility).HasDefaultValue(true).HasColumnName("profile_visibility");
                entity.Property(e => e.ShowEmail).HasDefaultValue(false).HasColumnName("show_email");
                entity.Property(e => e.ShowPhone).HasDefaultValue(false).HasColumnName("show_phone");
                entity.Property(e => e.AllowDataCollection).HasDefaultValue(true).HasColumnName("allow_data_collection");
                entity.Property(e => e.AllowAnalytics).HasDefaultValue(true).HasColumnName("allow_analytics");
                entity.Property(e => e.AllowCookies).HasDefaultValue(true).HasColumnName("allow_cookies");
                entity.Property(e => e.AllowMarketing).HasDefaultValue(false).HasColumnName("allow_marketing");
                entity.Property(e => e.DataSharing).HasDefaultValue(false).HasColumnName("data_sharing");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // HelpArticle configuration
            modelBuilder.Entity<HelpArticle>(entity =>
            {
                entity.ToTable("help_articles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200).HasColumnName("title");
                entity.Property(e => e.Content).IsRequired().HasColumnName("content");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50).HasColumnName("category");
                entity.Property(e => e.Tags).HasColumnName("tags");
                entity.Property(e => e.ViewCount).HasDefaultValue(0).HasColumnName("view_count");
                entity.Property(e => e.IsPublished).HasDefaultValue(true).HasColumnName("is_published");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            });
            
            // SupportTicket configuration
            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.ToTable("support_tickets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200).HasColumnName("subject");
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000).HasColumnName("description");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50).HasColumnName("category");
                entity.Property(e => e.Priority).HasMaxLength(20).HasColumnName("priority");
                entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
                entity.Property(e => e.AssignedTo).HasMaxLength(100).HasColumnName("assigned_to");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // SupportMessage configuration
            modelBuilder.Entity<SupportMessage>(entity =>
            {
                entity.ToTable("support_messages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TicketId).HasColumnName("ticket_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000).HasColumnName("message");
                entity.Property(e => e.IsFromSupport).HasDefaultValue(false).HasColumnName("is_from_support");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
                
                entity.HasOne(e => e.Ticket)
                    .WithMany(t => t.Messages)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Faq configuration
            modelBuilder.Entity<Faq>(entity =>
            {
                entity.ToTable("faqs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Question).IsRequired().HasMaxLength(500).HasColumnName("question");
                entity.Property(e => e.Answer).IsRequired().HasColumnName("answer");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50).HasColumnName("category");
                entity.Property(e => e.ViewCount).HasDefaultValue(0).HasColumnName("view_count");
                entity.Property(e => e.IsPublished).HasDefaultValue(true).HasColumnName("is_published");
                entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            });
        }
    }
}
