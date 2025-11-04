using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new();

        public DataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!await _context.Categories.AnyAsync())
            {
                await SeedCategoriesAsync();
            }

            if (!await _context.SubCategories.AnyAsync())
            {
                await SeedSubCategoriesAsync();
            }

            if (!await _context.Products.AnyAsync())
            {
                await SeedProductsAsync();
            }

            if (!await _context.Campaigns.AnyAsync())
            {
                await SeedCampaignsAsync();
            }

            if (!await _context.Users.AnyAsync())
            {
                await SeedUsersAsync();
            }

            if (!await _context.Reviews.AnyAsync())
            {
                await SeedReviewsAsync();
            }
        }

        private async Task SeedCategoriesAsync()
        {
            // Check if categories already exist
            if (await _context.Categories.AnyAsync())
            {
                return;
            }

            var categories = new List<Category>
            {
                new() { CategoryName = "Elektronik", Description = "Elektronik ürünler ve aksesuarlar", ImageUrl = "https://picsum.photos/200/200?random=1" },
                new() { CategoryName = "Giyim", Description = "Kadın, erkek ve çocuk giyim ürünleri", ImageUrl = "https://picsum.photos/200/200?random=2" },
                new() { CategoryName = "Ev & Yaşam", Description = "Ev dekorasyonu ve yaşam ürünleri", ImageUrl = "https://picsum.photos/200/200?random=3" },
                new() { CategoryName = "Spor & Outdoor", Description = "Spor malzemeleri ve outdoor ürünler", ImageUrl = "https://picsum.photos/200/200?random=4" },
                new() { CategoryName = "Anne & Bebek", Description = "Anne ve bebek ürünleri", ImageUrl = "https://picsum.photos/200/200?random=5" },
                new() { CategoryName = "Kozmetik & Bakım", Description = "Kozmetik ve kişisel bakım ürünleri", ImageUrl = "https://picsum.photos/200/200?random=6" },
                new() { CategoryName = "Süpermarket", Description = "Gıda ve temizlik ürünleri", ImageUrl = "https://picsum.photos/200/200?random=7" },
                new() { CategoryName = "Kitap & Müzik", Description = "Kitap, müzik ve medya ürünleri", ImageUrl = "https://picsum.photos/200/200?random=8" },
                new() { CategoryName = "Oto & Bahçe", Description = "Otomotiv ve bahçe ürünleri", ImageUrl = "https://picsum.photos/200/200?random=9" },
                new() { CategoryName = "Kırtasiye & Ofis", Description = "Kırtasiye ve ofis malzemeleri", ImageUrl = "https://picsum.photos/200/200?random=10" },
                new() { CategoryName = "Oyuncak & Hobi", Description = "Oyuncaklar ve hobi malzemeleri", ImageUrl = "https://picsum.photos/200/200?random=11" },
                new() { CategoryName = "Sağlık & Medikal", Description = "Sağlık ve medikal ürünler", ImageUrl = "https://picsum.photos/200/200?random=12" },
                new() { CategoryName = "Pet Shop", Description = "Evcil hayvan ürünleri", ImageUrl = "https://picsum.photos/200/200?random=13" },
                new() { CategoryName = "Moda & Aksesuar", Description = "Moda aksesuarları ve takılar", ImageUrl = "https://picsum.photos/200/200?random=14" },
                new() { CategoryName = "Beyaz Eşya", Description = "Beyaz eşya ve ev aletleri", ImageUrl = "https://picsum.photos/200/200?random=15" }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProductsAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            var products = new List<Product>();

            // Elektronik ürünleri
            var electronics = new[]
            {
                "iPhone 15 Pro Max", "Samsung Galaxy S24 Ultra", "MacBook Pro M3", "Dell XPS 13", "iPad Air",
                "Sony WH-1000XM5", "AirPods Pro", "Samsung 4K TV", "PlayStation 5", "Xbox Series X",
                "Nintendo Switch", "Canon EOS R5", "Sony A7 IV", "DJI Mavic 3", "Apple Watch Series 9",
                "Samsung Galaxy Watch", "Fitbit Versa 4", "Garmin Fenix 7", "GoPro Hero 12", "Insta360 X3",
                "iPhone 14 Pro", "Samsung Galaxy S23", "MacBook Air M2", "Dell Inspiron 15", "iPad Pro",
                "Sony WH-1000XM4", "AirPods Max", "LG OLED TV", "PlayStation 4", "Xbox One X",
                "Nintendo Switch Lite", "Canon EOS R6", "Sony A7 III", "DJI Mini 3", "Apple Watch Series 8",
                "Samsung Galaxy Watch 5", "Fitbit Sense", "Garmin Forerunner 955", "GoPro Hero 11", "Insta360 One RS",
                "iPhone 13 Pro", "Samsung Galaxy S22", "MacBook Pro M1", "Dell Latitude 14", "iPad Mini",
                "Sony WF-1000XM4", "AirPods 3", "Samsung QLED TV", "PlayStation 3", "Xbox 360",
                "Nintendo 3DS", "Canon EOS 90D", "Sony A6000", "DJI Air 2S", "Apple Watch SE",
                "Samsung Galaxy Watch 4", "Fitbit Charge 5", "Garmin Venu 2", "GoPro Hero 10", "Insta360 Go 2"
            };

            // Giyim ürünleri
            var clothing = new[]
            {
                "Nike Air Max 270", "Adidas Ultraboost 22", "Levi's 501 Jeans", "Zara Blazer", "H&M T-Shirt",
                "Uniqlo Hoodie", "Converse Chuck Taylor", "Vans Old Skool", "Puma Suede", "New Balance 574",
                "Tommy Hilfiger Polo", "Calvin Klein Underwear", "Champion Sweatshirt", "The North Face Jacket",
                "Columbia Hiking Boots", "Timberland Boots", "Ray-Ban Sunglasses", "Oakley Glasses", "Gucci Belt",
                "Louis Vuitton Bag"
            };

            // Ev & Yaşam ürünleri
            var home = new[]
            {
                "IKEA Sofa", "Zara Home Bedding", "West Elm Coffee Table", "Crate & Barrel Dining Set",
                "Pottery Barn Lamp", "Anthropologie Mirror", "Urban Outfitters Rug", "CB2 Vase",
                "Design Within Reach Chair", "Restoration Hardware Pillow", "Williams Sonoma Cookware",
                "Le Creuset Dutch Oven", "KitchenAid Mixer", "Vitamix Blender", "Breville Espresso Machine",
                "Dyson Vacuum", "Shark Steam Mop", "Roomba Robot Vacuum", "Philips Hue Bulbs", "Nest Thermostat"
            };

            // Spor ürünleri
            var sports = new[]
            {
                "Nike Basketball", "Adidas Football", "Wilson Tennis Racket", "Callaway Golf Clubs",
                "Under Armour Compression Shirt", "Lululemon Yoga Mat", "Peloton Bike", "Bowflex Dumbbells",
                "TRX Suspension Trainer", "Rogue Barbell", "Concept2 Rowing Machine", "NordicTrack Treadmill",
                "Garmin GPS Watch", "Polar Heart Rate Monitor", "Suunto Compass", "Black Diamond Headlamp",
                "Patagonia Backpack", "Arc'teryx Jacket", "Merrell Hiking Shoes", "Salomon Trail Running Shoes"
            };

            // Kitap ürünleri
            var books = new[]
            {
                "The Great Gatsby", "To Kill a Mockingbird", "1984", "Pride and Prejudice", "The Catcher in the Rye",
                "Lord of the Flies", "The Hobbit", "Harry Potter Series", "The Chronicles of Narnia", "Dune",
                "Foundation Series", "The Martian", "Ready Player One", "The Handmaid's Tale", "Brave New World",
                "Fahrenheit 451", "Animal Farm", "The Lord of the Rings", "Game of Thrones", "The Witcher"
            };

            // Kozmetik ürünleri
            var cosmetics = new[]
            {
                "MAC Lipstick", "NARS Foundation", "Urban Decay Eyeshadow", "Too Faced Mascara", "Fenty Beauty Highlighter",
                "Glossier Cloud Paint", "Charlotte Tilbury Blush", "Pat McGrath Lipstick", "Huda Beauty Palette",
                "Anastasia Brow Wiz", "Benefit Mascara", "Tarte Concealer", "IT Cosmetics CC Cream", "BareMinerals Powder",
                "Clinique Moisturizer", "Estée Lauder Serum", "Lancôme Eye Cream", "Dior Perfume", "Chanel No. 5",
                "Tom Ford Fragrance"
            };

            var allProducts = new[] { electronics, clothing, home, sports, books, cosmetics };

            for (int i = 0; i < allProducts.Length && i < categories.Count; i++)
            {
                var category = categories[i];
                var productNames = allProducts[i];

                foreach (var productName in productNames)
                {
                    products.Add(new Product
                    {
                        ProductName = productName,
                        UnitPrice = GenerateRandomPrice(),
                        UnitInStock = _random.Next(1, 101),
                        QuantityPerUnit = "1 adet",
                        CategoryId = category.Id,
                        Description = GenerateDescription(productName),
                        ImageUrl = $"https://picsum.photos/400/400?random={_random.Next(1, 1001)}",
                        Discount = _random.Next(0, 50),
                        IsActive = true
                    });
                }
            }

            // Her kategoriden çok daha fazla ürün ekle
            foreach (var category in categories)
            {
                for (int i = 0; i < 100; i++) // Her kategoriden 100 ürün
                {
                    products.Add(new Product
                    {
                        ProductName = $"{category.CategoryName} Ürün {i + 1}",
                        UnitPrice = GenerateRandomPrice(),
                        UnitInStock = _random.Next(1, 201),
                        QuantityPerUnit = "1 adet",
                        CategoryId = category.Id,
                        Description = $"Yüksek kaliteli {category.CategoryName.ToLower()} ürünü. Premium malzeme ve işçilik ile üretilmiştir.",
                        ImageUrl = $"https://picsum.photos/400/400?random={_random.Next(1, 10001)}",
                        Discount = _random.Next(0, 70),
                        IsActive = true
                    });
                }
            }

            // Ekstra çeşitlilik için daha fazla ürün
            var extraProducts = new[]
            {
                "Premium Bluetooth Kulaklık", "Akıllı Saat Pro", "Gaming Mouse", "Mekanik Klavye", "Webcam HD",
                "Mikrofon USB", "Hoparlör Bluetooth", "Powerbank 20000mAh", "Telefon Kılıfı", "Ekran Koruyucu",
                "Şarj Kablosu", "Adaptör USB-C", "SD Kart 128GB", "USB Flash 64GB", "Hard Disk 1TB",
                "SSD 500GB", "RAM 16GB", "İşlemci Intel i7", "Anakart Gaming", "Ekran Kartı RTX",
                "Kasa Gaming", "Güç Kaynağı 750W", "Soğutucu CPU", "Fan RGB", "LED Strip",
                "Kablo HDMI", "Splitter USB", "Hub USB", "Dock Station", "Stand Laptop",
                "Mouse Pad", "Desk Mat", "Monitör 27\"", "Klavye Mekanik", "Mouse Gaming",
                "Kulaklık Gaming", "Mikrofon Gaming", "Webcam 4K", "Tripod", "Işık Ring",
                "Green Screen", "Mikser Ses", "Hoparlör Studio", "Amfi", "Gitar Elektro",
                "Piyano Dijital", "Davul Elektronik", "Mikrofon Vokal", "Konsol Mixer", "Efekt Pedal"
            };

            foreach (var productName in extraProducts)
            {
                var randomCategory = categories[_random.Next(categories.Count)];
                products.Add(new Product
                {
                    ProductName = productName,
                    UnitPrice = GenerateRandomPrice(),
                    UnitInStock = _random.Next(1, 151),
                    QuantityPerUnit = "1 adet",
                    CategoryId = randomCategory.Id,
                    Description = $"Profesyonel kalitede {productName.ToLower()}. Uzun ömürlü ve güvenilir.",
                    ImageUrl = $"https://picsum.photos/400/400?random={_random.Next(1, 10001)}",
                    Discount = _random.Next(0, 60),
                    IsActive = true
                });
            }

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCampaignsAsync()
        {
            var campaigns = new List<Campaign>
            {
                new()
                {
                    Title = "Büyük İndirim Fırsatı",
                    Subtitle = "Tüm ürünlerde %50'ye varan indirimler",
                    Description = "Sınırlı süre için tüm kategorilerde büyük indirimler. Kaçırma!",
                    Discount = 50,
                    ImageUrl = "https://picsum.photos/800/400?random=1001",
                    BackgroundColor = "#FF6B6B",
                    TimeLeft = "3 gün kaldı",
                    ButtonText = "Alışverişe Başla",
                    ButtonHref = "/products",
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(3),
                    IsActive = true
                },
                new()
                {
                    Title = "Elektronik Günleri",
                    Subtitle = "Teknoloji ürünlerinde özel fiyatlar",
                    Description = "Telefon, laptop, tablet ve aksesuarlarda özel kampanya fiyatları",
                    Discount = 30,
                    ImageUrl = "https://picsum.photos/800/400?random=1002",
                    BackgroundColor = "#4ECDC4",
                    TimeLeft = "1 hafta kaldı",
                    ButtonText = "Elektronik Ürünleri Gör",
                    ButtonHref = "/categories/1",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new()
                {
                    Title = "Yaz Koleksiyonu",
                    Subtitle = "Yeni sezon giyim ürünleri",
                    Description = "Yaz ayları için hazırlanan yeni koleksiyon ürünleri",
                    Discount = 25,
                    ImageUrl = "https://picsum.photos/800/400?random=1003",
                    BackgroundColor = "#45B7D1",
                    TimeLeft = "2 hafta kaldı",
                    ButtonText = "Koleksiyonu İncele",
                    ButtonHref = "/categories/2",
                    StartDate = DateTime.UtcNow.AddDays(-2),
                    EndDate = DateTime.UtcNow.AddDays(12),
                    IsActive = true
                },
                new()
                {
                    Title = "Ev Dekorasyonu",
                    Subtitle = "Evinizi yenileyin",
                    Description = "Ev dekorasyonu ve yaşam ürünlerinde özel indirimler",
                    Discount = 40,
                    ImageUrl = "https://picsum.photos/800/400?random=1004",
                    BackgroundColor = "#96CEB4",
                    TimeLeft = "5 gün kaldı",
                    ButtonText = "Ev Ürünlerini Gör",
                    ButtonHref = "/categories/3",
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(4),
                    IsActive = true
                },
                new()
                {
                    Title = "Spor Malzemeleri",
                    Subtitle = "Sağlıklı yaşam için",
                    Description = "Fitness ve spor malzemelerinde büyük indirimler",
                    Discount = 35,
                    ImageUrl = "https://picsum.photos/800/400?random=1005",
                    BackgroundColor = "#FFEAA7",
                    TimeLeft = "1 hafta kaldı",
                    ButtonText = "Spor Ürünleri",
                    ButtonHref = "/categories/4",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                }
            };

            await _context.Campaigns.AddRangeAsync(campaigns);
            await _context.SaveChangesAsync();
        }

        private decimal GenerateRandomPrice()
        {
            return Math.Round((decimal)(10 + _random.NextDouble() * 990), 2);
        }

        private async Task SeedUsersAsync()
        {
            var users = new List<User>
            {
                new() 
                { 
                    Email = "admin@example.com", 
                    Password = "admin123", 
                    FirstName = "Admin", 
                    LastName = "User",
                    PhoneNumber = "+90 555 123 4567",
                    Address = "Admin Adresi",
                    City = "İstanbul",
                    PostalCode = "34000",
                    IsEmailVerified = true,
                    IsActive = true
                },
                new() 
                { 
                    Email = "user1@example.com", 
                    Password = "user123", 
                    FirstName = "Ahmet", 
                    LastName = "Yılmaz",
                    PhoneNumber = "+90 555 234 5678",
                    Address = "Kullanıcı Adresi 1",
                    City = "Ankara",
                    PostalCode = "06000",
                    IsEmailVerified = true,
                    IsActive = true
                },
                new() 
                { 
                    Email = "user2@example.com", 
                    Password = "user123", 
                    FirstName = "Ayşe", 
                    LastName = "Demir",
                    PhoneNumber = "+90 555 345 6789",
                    Address = "Kullanıcı Adresi 2",
                    City = "İzmir",
                    PostalCode = "35000",
                    IsEmailVerified = true,
                    IsActive = true
                },
                new() 
                { 
                    Email = "user3@example.com", 
                    Password = "user123", 
                    FirstName = "Mehmet", 
                    LastName = "Kaya",
                    PhoneNumber = "+90 555 456 7890",
                    Address = "Kullanıcı Adresi 3",
                    City = "Bursa",
                    PostalCode = "16000",
                    IsEmailVerified = false,
                    IsActive = true
                },
                new() 
                { 
                    Email = "user4@example.com", 
                    Password = "user123", 
                    FirstName = "Fatma", 
                    LastName = "Özkan",
                    PhoneNumber = "+90 555 567 8901",
                    Address = "Kullanıcı Adresi 4",
                    City = "Antalya",
                    PostalCode = "07000",
                    IsEmailVerified = true,
                    IsActive = true
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        private async Task SeedReviewsAsync()
        {
            var products = await _context.Products.Take(20).ToListAsync();
            var users = await _context.Users.ToListAsync();
            var reviews = new List<Review>();

            var reviewTitles = new[]
            {
                "Harika ürün!", "Çok memnun kaldım", "Beklentilerimi aştı", "Kaliteli ve dayanıklı",
                "Hızlı teslimat", "Fiyat performans mükemmel", "Tavsiye ederim", "Çok iyi",
                "Biraz pahalı ama değer", "Orta kalite", "İdare eder", "Beklediğim gibi değil",
                "Kargo sorunu yaşadım", "Ürün hasarlı geldi", "İade ettim", "Tekrar alırım",
                "Ailem çok beğendi", "Arkadaşlarıma tavsiye ettim", "Uzun süre kullandım", "Çok pratik"
            };

            var reviewComments = new[]
            {
                "Ürün gerçekten çok kaliteli. Beklentilerimi aştı. Hızlı kargo ve güvenli paketleme.",
                "Fiyatına göre çok iyi bir ürün. Kalitesi beklediğimden daha iyi çıktı.",
                "Kullanımı çok kolay. Ailem de çok beğendi. Tavsiye ederim.",
                "Ürün güzel ama biraz pahalı. Yine de kaliteli olduğu için memnunum.",
                "Orta kalite bir ürün. Fiyatına göre idare eder. Daha iyisi de var tabii.",
                "Beklediğim gibi değil. Kalitesi düşük geldi. İade etmeyi düşünüyorum.",
                "Kargo sürecinde sorun yaşadım ama ürün güzel. Satıcı ile iletişim iyiydi.",
                "Ürün hasarlı geldi. Müşteri hizmetleri hızlı çözüm sağladı.",
                "Çok memnun kaldım. Tekrar alacağım. Arkadaşlarıma da tavsiye ettim.",
                "Uzun süre kullandım. Dayanıklı ve pratik. Fiyatına değer."
            };

            foreach (var product in products)
            {
                // Her ürün için 3-8 arası yorum ekle
                var reviewCount = _random.Next(3, 9);
                
                for (int i = 0; i < reviewCount; i++)
                {
                    var user = users[_random.Next(users.Count)];
                    var rating = _random.Next(1, 6); // 1-5 arası puan
                    var title = reviewTitles[_random.Next(reviewTitles.Length)];
                    var comment = reviewComments[_random.Next(reviewComments.Length)];

                    reviews.Add(new Review
                    {
                        UserId = user.Id,
                        ProductId = product.Id,
                        Rating = rating,
                        Title = title,
                        Comment = comment,
                        IsVerified = _random.Next(2) == 1, // %50 doğrulanmış
                        IsHelpful = _random.Next(2) == 1, // %50 faydalı
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365)), // Son 1 yıl içinde
                        UpdatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365))
                    });
                }
            }

            await _context.Reviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();
        }

        private string GenerateDescription(string productName)
        {
            var descriptions = new[]
            {
                "Yüksek kaliteli ve dayanıklı ürün",
                "Modern tasarım ve kullanıcı dostu",
                "Profesyonel kalite standartlarında",
                "Uzun ömürlü ve güvenilir",
                "İnovatif teknoloji ile üretilmiş",
                "Çevre dostu malzemelerden yapılmış",
                "Kolay kullanım ve bakım",
                "Premium kalite ve performans",
                "Trend tasarım ve stil",
                "Müşteri memnuniyeti odaklı"
            };

            return $"{descriptions[_random.Next(descriptions.Length)]} {productName.ToLower()}.";
        }

        private async Task SeedSubCategoriesAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            var subCategories = new List<SubCategory>();

            foreach (var category in categories)
            {
                switch (category.CategoryName)
                {
                    case "Elektronik":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Telefon & Tablet", CategoryId = category.Id, Description = "Akıllı telefonlar ve tabletler", ImageUrl = "https://picsum.photos/200/200?random=101" },
                            new SubCategory { SubCategoryName = "Bilgisayar", CategoryId = category.Id, Description = "Laptop, masaüstü bilgisayar ve aksesuarları", ImageUrl = "https://picsum.photos/200/200?random=102" },
                            new SubCategory { SubCategoryName = "Ev Elektroniği", CategoryId = category.Id, Description = "TV, ses sistemleri ve ev elektroniği", ImageUrl = "https://picsum.photos/200/200?random=103" }
                        });
                        break;
                    case "Giyim":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Kadın Giyim", CategoryId = category.Id, Description = "Kadın giyim ürünleri", ImageUrl = "https://picsum.photos/200/200?random=104" },
                            new SubCategory { SubCategoryName = "Erkek Giyim", CategoryId = category.Id, Description = "Erkek giyim ürünleri", ImageUrl = "https://picsum.photos/200/200?random=105" },
                            new SubCategory { SubCategoryName = "Ayakkabı & Çanta", CategoryId = category.Id, Description = "Ayakkabı ve çanta ürünleri", ImageUrl = "https://picsum.photos/200/200?random=106" }
                        });
                        break;
                    case "Ev & Yaşam":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Mobilya", CategoryId = category.Id, Description = "Ev mobilyaları", ImageUrl = "https://picsum.photos/200/200?random=107" },
                            new SubCategory { SubCategoryName = "Dekorasyon", CategoryId = category.Id, Description = "Ev dekorasyon ürünleri", ImageUrl = "https://picsum.photos/200/200?random=108" },
                            new SubCategory { SubCategoryName = "Mutfak & Banyo", CategoryId = category.Id, Description = "Mutfak ve banyo ürünleri", ImageUrl = "https://picsum.photos/200/200?random=109" }
                        });
                        break;
                    case "Spor & Outdoor":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Fitness", CategoryId = category.Id, Description = "Fitness ve spor ekipmanları", ImageUrl = "https://picsum.photos/200/200?random=110" },
                            new SubCategory { SubCategoryName = "Outdoor", CategoryId = category.Id, Description = "Açık hava sporları", ImageUrl = "https://picsum.photos/200/200?random=111" },
                            new SubCategory { SubCategoryName = "Takım Sporları", CategoryId = category.Id, Description = "Takım sporları ekipmanları", ImageUrl = "https://picsum.photos/200/200?random=112" }
                        });
                        break;
                    case "Anne & Bebek":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Bebek Giyim", CategoryId = category.Id, Description = "Bebek giyim ürünleri", ImageUrl = "https://picsum.photos/200/200?random=113" },
                            new SubCategory { SubCategoryName = "Bebek Bakım", CategoryId = category.Id, Description = "Bebek bakım ürünleri", ImageUrl = "https://picsum.photos/200/200?random=114" },
                            new SubCategory { SubCategoryName = "Bebek Beslenme", CategoryId = category.Id, Description = "Bebek beslenme ürünleri", ImageUrl = "https://picsum.photos/200/200?random=115" }
                        });
                        break;
                    case "Kozmetik & Bakım":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Makyaj", CategoryId = category.Id, Description = "Makyaj ürünleri", ImageUrl = "https://picsum.photos/200/200?random=116" },
                            new SubCategory { SubCategoryName = "Cilt Bakımı", CategoryId = category.Id, Description = "Cilt bakım ürünleri", ImageUrl = "https://picsum.photos/200/200?random=117" },
                            new SubCategory { SubCategoryName = "Saç Bakımı", CategoryId = category.Id, Description = "Saç bakım ürünleri", ImageUrl = "https://picsum.photos/200/200?random=118" }
                        });
                        break;
                    case "Süpermarket":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Gıda", CategoryId = category.Id, Description = "Gıda ürünleri", ImageUrl = "https://picsum.photos/200/200?random=119" },
                            new SubCategory { SubCategoryName = "Temizlik", CategoryId = category.Id, Description = "Temizlik ürünleri", ImageUrl = "https://picsum.photos/200/200?random=120" },
                            new SubCategory { SubCategoryName = "Kişisel Bakım", CategoryId = category.Id, Description = "Kişisel bakım ürünleri", ImageUrl = "https://picsum.photos/200/200?random=121" }
                        });
                        break;
                    case "Kitap & Müzik":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Kitaplar", CategoryId = category.Id, Description = "Kitap ürünleri", ImageUrl = "https://picsum.photos/200/200?random=122" },
                            new SubCategory { SubCategoryName = "Müzik", CategoryId = category.Id, Description = "Müzik ürünleri", ImageUrl = "https://picsum.photos/200/200?random=123" },
                            new SubCategory { SubCategoryName = "Medya", CategoryId = category.Id, Description = "Medya ürünleri", ImageUrl = "https://picsum.photos/200/200?random=124" }
                        });
                        break;
                    case "Oto & Bahçe":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Otomotiv", CategoryId = category.Id, Description = "Otomotiv ürünleri", ImageUrl = "https://picsum.photos/200/200?random=125" },
                            new SubCategory { SubCategoryName = "Bahçe", CategoryId = category.Id, Description = "Bahçe ürünleri", ImageUrl = "https://picsum.photos/200/200?random=126" },
                            new SubCategory { SubCategoryName = "İnşaat & Yapı", CategoryId = category.Id, Description = "İnşaat ve yapı ürünleri", ImageUrl = "https://picsum.photos/200/200?random=127" }
                        });
                        break;
                    case "Kırtasiye & Ofis":
                        subCategories.AddRange(new[]
                        {
                            new SubCategory { SubCategoryName = "Kırtasiye", CategoryId = category.Id, Description = "Kırtasiye ürünleri", ImageUrl = "https://picsum.photos/200/200?random=128" },
                            new SubCategory { SubCategoryName = "Ofis", CategoryId = category.Id, Description = "Ofis ürünleri", ImageUrl = "https://picsum.photos/200/200?random=129" },
                            new SubCategory { SubCategoryName = "Okul", CategoryId = category.Id, Description = "Okul ürünleri", ImageUrl = "https://picsum.photos/200/200?random=130" }
                        });
                        break;
                }
            }

            await _context.SubCategories.AddRangeAsync(subCategories);
            await _context.SaveChangesAsync();
        }
    }
}
