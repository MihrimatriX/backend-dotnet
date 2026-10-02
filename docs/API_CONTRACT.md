# Ortak API Sözleşmesi (backend-dotnet ⇄ backend-spring)

Bu belge **backend-dotnet** ve **backend-spring** depolarında birebir aynıdır. İki backend ayrı
projelerdir ama aynı HTTP sözleşmesini uygular; vitrin (ör. `temp-shop-net`) yalnızca
`NEXT_PUBLIC_API_URL` değiştirilerek hangisine bağlanacağını seçer.

- .NET varsayılan adresi: `http://localhost:5000`
- Spring varsayılan adresi: `http://localhost:8081`

Uyumluluk her iki depoda da bulunan `scripts/contract-smoke.mjs` ile doğrulanır (bkz. §7).
Sözleşmede bir değişiklik yapılırsa **iki depoda da** aynı commit içinde uygulanmalıdır.

---

## 1. Genel kurallar

| Konu | Kural |
|---|---|
| Kök yol | Tüm uçlar `/api/` altında, **küçük harf** ve tekil kaynak adıyla: `auth, product, category, subcategory, campaign, cart, favorite, address, paymentmethod, order, review, notification, security, settings, helpsupport, health, metrics, test` |
| JSON | `camelCase`. Nesne içindeki `null` alanlar yazılır (`"description": null`). Bilinmeyen istek alanları yok sayılır. İstek alan adları büyük/küçük harf duyarsızdır. |
| Mantıksal alanlar | `is` önekiyle: `isActive`, `isDefault`, `isRead`, `isAvailable`, `isEmailVerified`, `isVerified`, `isHelpful`, `isSuccessful`, `isPublished`, `isFromSupport` |
| Tarih | ISO-8601, **UTC, `Z` sonekli**: `"2026-10-02T10:54:48.689Z"`. İstekte `Z` veya ofsetli değer kabul edilir ve UTC'ye çevrilir. |
| Para | JSON sayı, 2 ondalık (TRY). |
| Kimlikler | JSON sayı (`id`, `userId`, `productId` …). |
| Durum/tip değerleri | Düz string (enum yok): sipariş durumu `Pending, Processing, Shipped, Delivered, Cancelled, ReturnRequested, Returned`. |
| Sayfalama | Yalnızca `GET /api/product` sayfalı nesne döner (§1.3). Diğer liste uçları `pageNumber` (1 tabanlı) / `pageSize` alır ve **düz dizi** döner. |

### 1.1 Zarf (`BaseResponse`)

Tüm `/api/**` JSON cevapları (aşağıdaki "zarfsız" uçlar hariç) bu biçimdedir:

```json
{
  "success": true,
  "message": "Products retrieved successfully",
  "data": { },
  "error": "…",
  "errorCode": "…",
  "errors": { "email": ["Invalid email format"] },
  "traceId": "…"
}
```

- `success` ve `message` her zaman vardır. `data`, `error`, `errorCode`, `errors`, `traceId` **null ise yazılmaz**.
- **Başarılı her cevapta `data` doludur.** Silme/eylem uçları `data` olarak bilgi metni (`"Address deleted successfully"`) veya `true` döner. (Vitrin `unwrap()` `data` yoksa hata sayar.)
- Hata cevaplarında `success:false`, Türkçe/İngilizce okunur bir `message` ve **makine kodu `errorCode`** bulunur. Çerçeve seviyesindeki hatalarda (401/403/404/429/500/doğrulama) `traceId` = `X-Correlation-Id` değeridir.
- `error` alanı yalnızca geliştirme ortamında teknik ayrıntı (exception mesajı) taşır; istemci buna dayanmamalıdır.

Zarfsız uçlar: `/api/test/*`, `/api/health*`, `/api/metrics*`, `/health`, `/actuator/*`, Swagger.

### 1.2 HTTP durum kodları ve ortak hata kodları

| HTTP | `errorCode` | Ne zaman | `message` |
|---|---|---|---|
| 200 | – | Okuma/güncelleme/eylem başarılı | |
| 201 | – | Kaynak oluşturuldu (`POST /api/address`, `/api/paymentmethod`, `/api/order`, `/api/review`, `/api/category`, `/api/subcategory`, `/api/product`, `/api/campaign`, `/api/notification`, `/api/helpsupport/articles`, `/api/helpsupport/tickets`, `/api/auth/register`) | |
| 400 | `VALIDATION_ERROR` | Bean Validation / DataAnnotations hatası. `errors` = `{ alanAdi(camelCase): [mesajlar] }` | Alan adına göre alfabetik sıradaki ilk alanın ilk mesajı (deterministik) |
| 400 | `BAD_REQUEST` | Okunamayan JSON, tip uyuşmazlığı, eksik parametre | `"Geçersiz istek."` |
| 400 | (alan kodu) | İş kuralı ihlali (aşağıdaki tablolar) | |
| 401 | `UNAUTHORIZED` | Token yok / geçersiz / süresi dolmuş / iptal edilmiş | `"Kimlik doğrulama gerekli."` |
| 401 | `INVALID_CREDENTIALS` | Hatalı e-posta/şifre | `"Invalid email or password"` |
| 403 | `FORBIDDEN` | Rol yetersiz veya başka kullanıcının kaynağı | `"Bu işlem için yetkiniz yok."` (veya kaynağa özel metin) |
| 404 | `NOT_FOUND` veya `<KAYNAK>_NOT_FOUND` | Kayıt veya rota yok | |
| 409 | `EMAIL_TAKEN`, `CONFLICT`, `IDEMPOTENCY_CONFLICT` | Çakışma | |
| 429 | `RATE_LIMITED` | IP başına dakikalık limit aşıldı (`Retry-After` başlığı) | `"Çok fazla istek. Lütfen biraz sonra tekrar deneyin."` |
| 500 | `INTERNAL_ERROR` | Beklenmeyen hata | `"Beklenmeyen bir hata oluştu. Destek için traceId değerini iletin."` |

Kaynak bulunamadı kodları: `PRODUCT_NOT_FOUND, CATEGORY_NOT_FOUND, SUBCATEGORY_NOT_FOUND, CAMPAIGN_NOT_FOUND, ADDRESS_NOT_FOUND, PAYMENT_METHOD_NOT_FOUND, ORDER_NOT_FOUND, REVIEW_NOT_FOUND, NOTIFICATION_NOT_FOUND, ARTICLE_NOT_FOUND, USER_NOT_FOUND`.

### 1.3 Sayfalı sonuç (`PagedResult<T>`)

```json
{ "items": [], "totalCount": 0, "pageNumber": 1, "pageSize": 12,
  "totalPages": 0, "hasPreviousPage": false, "hasNextPage": false }
```

`pageNumber` 1 tabanlıdır; `pageNumber < 1` → 1, `pageSize` 1..100 aralığına kırpılır.

### 1.4 Ortak başlıklar

- **Korelasyon:** `X-Correlation-Id` (istekte gelirse — en fazla 128 karakter, `[A-Za-z0-9._:-]` — aynen kullanılır, yoksa üretilir) cevapta geri döner; CORS ile `Expose`edilir.
- **Güvenlik başlıkları:** `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `X-XSS-Protection: 1; mode=block`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy: geolocation=(), microphone=(), camera=()`; HSTS yalnızca HTTPS isteklerinde.
- **CORS:** izinli origin listesi yapılandırmadan (`Cors:AllowedOrigins` / `ALLOWED_ORIGINS`), varsayılan `http://localhost:3000, http://localhost:5173, http://127.0.0.1:5173, http://localhost:8080`; metotlar `GET, POST, PUT, DELETE, OPTIONS`; tüm başlıklar; credentials açık.
- **Hız limiti:** IP başına sabit pencere, varsayılan **dakikada 300 istek** (yapılandırılabilir; 0 = kapalı). `/health`, `/actuator/*`, `/api/health*`, `/api/metrics*`, Swagger muaftır.

---

## 2. Kimlik doğrulama

- `Authorization: Bearer <JWT>`; HS256; varsayılan geçerlilik 24 saat.
- **Claim seti (ikisinde aynı):**

| Claim | Değer |
|---|---|
| `sub` | Kullanıcı id'si (string, ör. `"10"`) |
| `email` | E-posta |
| `role` | `"Admin"` veya `"User"` |
| `jti` | Rastgele token kimliği |
| `iat`, `exp` | Unix saniye |
| `iss` | `EcommerceBackend` (yapılandırılabilir) |
| `aud` | `EcommerceUsers` (yapılandırılabilir) |

- Varsayılan geliştirme anahtarı ikisinde aynıdır (`mySecretKeyThatIsAtLeast256BitsLongForJWTTokenSecurity`); üretimde ortam değişkeniyle değiştirilmelidir (.NET `Jwt__Key`, Spring `JWT_SECRET`).
- **Rol:** e-posta, yapılandırmadaki yönetici listesinde ise `Admin` (.NET `Auth:AdminEmails`, Spring `app.auth.admin-emails`; varsayılan `admin@example.com, manager@shop.demo`), değilse `User`. Kayıt ve giriş aynı kuralı kullanır.
- Token doğrulanırken kullanıcının hâlâ aktif olduğu ve token'ın `logout-all-devices` ile iptal edilmediği kontrol edilir (§5.6).
- E-postalar **küçük harfe çevrilip kırpılarak** saklanır ve karşılaştırılır.

### 2.1 Demo hesapları (her iki backend de başlangıçta idempotent olarak oluşturur)

| E-posta | Şifre | Rol |
|---|---|---|
| admin@example.com | admin123 | Admin |
| manager@shop.demo | Manager123! | Admin |
| user1@example.com … user4@example.com | user123 | User |
| support@shop.demo | Support123! | User |
| demo.buyer@shop.local | Buyer123! | User |
| staff@shop.demo | Staff123! | User |

---

## 3. Veri şekilleri (DTO)

> Alan adları tam olarak budur. `?` = null olabilir.

**AuthResponse** `{ token, type:"Bearer", userId, email, firstName, lastName, isEmailVerified, role }`

**Product** `{ id, productName, unitPrice, unitInStock, quantityPerUnit, categoryId, categoryName?, subCategoryId?, subCategoryName?, description?, imageUrl?, discount, isActive, averageRating, totalReviews, createdAt, updatedAt }`
- `unitPrice` indirimsiz liste fiyatıdır; `discount` yüzde (0–100). Satış fiyatı = `round(unitPrice × (1 − discount/100), 2)` (yarım değerler yukarı).
- `averageRating` (1 ondalık, yorum yoksa 0) ve `totalReviews` aktif yorumlardan hesaplanır.

**Category** `{ id, categoryName, description?, imageUrl?, isActive, createdAt, updatedAt }`

**SubCategory** `{ id, subCategoryName, description?, imageUrl?, categoryId, categoryName, isActive, createdAt, updatedAt }`

**Campaign** `{ id, title, subtitle?, description?, discount, imageUrl?, backgroundColor?, timeLeft?, buttonText?, buttonHref?, isActive, startDate, endDate, createdAt, updatedAt }`

**Cart** `{ userId, items: CartItem[], totalItems, totalAmount, shippingFee, grandTotal, freeShippingRemainingTry? }`
**CartItem** `{ productId, productName, productImageUrl?, unitPrice, quantity, totalPrice, isAvailable }`

**Favorite** `{ id, userId, productId, productName, productImageUrl?, productPrice, productDiscount?, productCategory?, productInStock, createdAt }`

**Address** `{ id, userId, title, fullAddress, city, district, postalCode, country, isDefault, phoneNumber?, createdAt, updatedAt }`

**PaymentMethod** `{ id, userId, type, cardHolderName, cardNumber, expiryMonth, expiryYear, bankName?, accountNumber?, accountHolderName?, isDefault, isActive, createdAt, updatedAt }`
- `cardNumber` her zaman maskelidir: `"**** **** **** 1111"` (gerçek son 4 hane). Tam kart numarası ve CVV **saklanmaz**. `accountNumber` → `"****1234"`.

**Order** `{ id, orderNumber, userId, userName, userEmail, items: OrderItem[], subtotalAmount, shippingFee, totalAmount, status, notes?, shippingAddress: Address?, billingAddress: Address?, paymentMethod: PaymentMethod?, trackingNumber?, carrier?, shippedAt?, deliveredAt?, estimatedDeliveryAt?, cancelReason?, returnReason?, returnRequestedAt?, demoNextAction?, createdAt, updatedAt }`
**OrderItem** `{ id, productId, productName, productImageUrl?, quantity, unitPrice, totalPrice }` (`unitPrice` = sipariş anındaki indirimli fiyat)
- `orderNumber` = `ORD-yyyyMMdd-XXXXXXXX` (8 büyük harf hex). `totalAmount` kargo dahildir; `subtotalAmount` ürün toplamı.

**Review** `{ id, userId, productId, rating, title?, comment?, isVerified, isHelpful, userName?, productName?, createdAt, updatedAt }`
**ProductReviewSummary** `{ productId, averageRating, totalReviews, rating1Count, rating2Count, rating3Count, rating4Count, rating5Count }`

**Notification** `{ id, userId, title, message, type, actionUrl?, isRead, readAt?, isActive, createdAt, updatedAt }`
**NotificationSummary** `{ totalNotifications, unreadNotifications, recentNotifications: Notification[] (en yeni 5) }`

**Security** `{ userId, email, isEmailVerified, lastPasswordChange?, twoFactorEnabled, lastLoginAt?, lastLoginIp?, recentLogins: LoginHistory[] (en yeni 5) }`
**LoginHistory** `{ id, loginAt, ipAddress?, userAgent?, location?, isSuccessful }`
**SecuritySettings** `{ emailNotifications, smsNotifications, loginAlerts, twoFactorRequired, sessionTimeout }`

**UserSettings** `{ userId, language, timezone, currency, emailNotifications, smsNotifications, pushNotifications, marketingEmails, orderUpdates, priceAlerts, stockNotifications, theme, itemsPerPage, autoSaveCart, showProductRecommendations, enableLocationServices, createdAt, updatedAt }`
- Varsayılanlar: `tr, Europe/Istanbul, TRY, true, false, true, false, true, true, true, light, 20, true, true, false`.

**PrivacySettings** `{ userId, profileVisibility, showEmail, showPhone, allowDataCollection, allowAnalytics, allowCookies, allowMarketing, dataSharing, createdAt, updatedAt }`
- Varsayılanlar: `true, false, false, true, true, true, false, false`.

**HelpArticle** `{ id, title, content, category, tags: string[], viewCount, isPublished, createdAt, updatedAt }`
**Faq** `{ id, question, answer, category, viewCount, isPublished, createdAt, updatedAt }`
**SupportTicket** `{ id, userId, userName, subject, description, category, priority, status, assignedTo?, createdAt, updatedAt, messages: SupportMessage[] }`
**SupportMessage** `{ id, ticketId, userId, userName, message, isFromSupport, createdAt }`

---

## 4. Uç noktalar

Yetki sütunu: **Anonim**, **Kullanıcı** (geçerli JWT), **Admin** (`role=Admin`). "Sahip" = kaynak çağıran kullanıcıya ait olmalı; değilse 403 `FORBIDDEN`.

### 4.1 Auth — `/api/auth`

| Metot | Yol | Yetki | Gövde | Başarı | Hatalar |
|---|---|---|---|---|---|
| POST | `/register` | Anonim | `{email, password(≥6), firstName, lastName, phoneNumber?, address?, city?, postalCode?}` | 201 `AuthResponse`, msg `User registered successfully` | 409 `EMAIL_TAKEN` "Email is already taken"; 400 `VALIDATION_ERROR` |
| POST | `/login` | Anonim | `{email, password}` | 200 `AuthResponse`, msg `Login successful` | 401 `INVALID_CREDENTIALS`; 400 `VALIDATION_ERROR` |
| POST | `/logout` | Anonim/Kullanıcı | – | 200 data `"Logout successful"` | – |

Giriş denemeleri (başarılı ve, kullanıcı varsa, başarısız) `LoginHistory`'ye IP ve User-Agent ile yazılır.

### 4.2 Ürün — `/api/product`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/` | Anonim | Sorgu: `categoryId?, subCategoryId?, minPrice?, maxPrice?, searchTerm?, sortBy=Id, sortOrder=asc, pageNumber=1, pageSize=12` → `PagedResult<Product>`. Yalnızca aktif ürünler. `searchTerm` ad/açıklamada büyük-küçük harf duyarsız arar. `sortBy` (harf duyarsız): `Id`, `ProductName`/`name`, `UnitPrice`/`price`, `CreatedAt`/`createdat`, `Discount`, `UnitInStock`/`stock`; bilinmeyen değer → `Id`. `sortOrder`: `asc`/`desc`. Fiyat filtreleri liste fiyatına uygulanır. |
| GET | `/{id}` | Anonim | 200 `Product`; yok/pasif → 404 `PRODUCT_NOT_FOUND` "Product not found" |
| GET | `/category/{categoryId}` | Anonim | `Product[]` |
| GET | `/search?q=` | Anonim | `Product[]` (`q` zorunlu) |
| GET | `/featured` | Anonim | `Product[]` — indirim > %20 **veya** son 7 günde eklenmiş; en fazla 20, en yeni önce |
| GET | `/discounted` | Anonim | `Product[]` — indirim > 0; en fazla 20, indirim oranı yüksek olan önce |
| POST | `/` | Admin | `Product` gövdesi (`productName, unitPrice>0, unitInStock≥0, quantityPerUnit, categoryId, subCategoryId?, description?, imageUrl?, discount 0..100, isActive=true`) → 201. Kategori yok → 400 `CATEGORY_NOT_FOUND` "Category not found" |
| PUT | `/{id}` | Admin | Aynı gövde → 200. Ürün yok → 404 `PRODUCT_NOT_FOUND` |
| DELETE | `/{id}` | Admin | Yumuşak silme → 200 data `"Product deleted successfully"`; yok → 404 |

### 4.3 Kategori — `/api/category`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/` | Anonim | Aktif kategoriler, ada göre sıralı |
| GET | `/{id}` | Anonim | 404 `CATEGORY_NOT_FOUND` "Category not found" |
| POST | `/` | Admin | `{categoryName, description?, imageUrl?, isActive=true}` → 201. Aynı ad (harf duyarsız) varsa 400 `CATEGORY_EXISTS` "Category name already exists" |
| PUT | `/{id}` | Admin | `{categoryName, description?, imageUrl?, isActive?}` (`isActive` gönderilmezse değişmez). Pasif kategoriler de güncellenebilir → 200 |
| DELETE | `/{id}` | Admin | Yumuşak silme → 200 data `"Category deleted successfully"` |

### 4.4 Alt kategori — `/api/subcategory`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/` | Anonim | Aktif alt kategoriler (kategori adı, sonra alt kategori adı sıralı) |
| GET | `/category/{categoryId}` | Anonim | Kategorinin aktif alt kategorileri |
| GET | `/{id}` | Anonim | 404 `SUBCATEGORY_NOT_FOUND` |
| POST | `/` | Admin | `{subCategoryName, description?, imageUrl?, categoryId, isActive=true}` → 201; kategori yok → 400 `CATEGORY_NOT_FOUND` |
| PUT | `/{id}` | Admin | `{id?, subCategoryName, description?, imageUrl?, categoryId, isActive?}`; gövdedeki `id` gönderilmişse rota ile aynı olmalı (400 `ID_MISMATCH`) → 200 (pasife alma dahil) |
| DELETE | `/{id}` | Admin | 200 data `true` |

### 4.5 Kampanya — `/api/campaign`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/` | Anonim | Aktif kampanyalar, en yeni önce |
| GET | `/active` | Anonim | Aktif ve `startDate ≤ şimdi(UTC) ≤ endDate` |
| GET | `/{id}` | Anonim | 404 `CAMPAIGN_NOT_FOUND` |
| POST | `/` | Admin | `Campaign` alanları (`title` 2..200 zorunlu, `discount` 0..100, `startDate`, `endDate` zorunlu, `endDate ≥ startDate` değilse 400 `INVALID_DATE_RANGE`) → 201 |
| PUT | `/{id}` | Admin | Aynı alanlar; `isActive` gönderilmezse değişmez → 200 |
| DELETE | `/{id}` | Admin | Yumuşak silme → 200 data `"Campaign deleted successfully"` |

### 4.6 Sepet — `/api/cart` (Kullanıcı)

| Metot | Yol | Gövde | Açıklama |
|---|---|---|---|
| GET | `/` | – | `Cart` (msg `Sepet getirildi`) |
| POST | `/add` | `{productId, quantity}` | Miktar mevcut satıra **eklenir**, stokla sınırlanır → 200 `Cart`. `quantity ≤ 0` → 400 `INVALID_QUANTITY` "Adet 0'dan büyük olmalıdır."; ürün yok/pasif → 400 `PRODUCT_NOT_FOUND` "Ürün bulunamadı veya satışta değil."; stok 0 → 400 `OUT_OF_STOCK` "Bu ürün stokta yok." |
| PUT | `/update` | `{productId, quantity}` | Miktarı **ayarlar** → 200 `Cart`. `quantity ≤ 0` satırı siler. Stok yetersiz → 400 `INSUFFICIENT_STOCK` "Stokta yeterli ürün yok. Miktarı düşürün."; sepette yok → 400 `NOT_IN_CART` "Bu ürün sepetinizde yok."; ürün yok → 400 `PRODUCT_NOT_FOUND` |
| DELETE | `/remove/{productId}` | – | İdempotent → 200 data `true` |
| DELETE | `/clear` | – | 200 data `true` |
| GET | `/count` | – | 200 data = toplam adet (int) |
| GET | `/total` | – | 200 data = `grandTotal` |

### 4.7 Favori — `/api/favorite` (Kullanıcı)

| Metot | Yol | Açıklama |
|---|---|---|
| GET | `/` | `Favorite[]` en yeni önce |
| POST | `/add` | `{productId}` → 200 `Favorite` (ürün bilgileri dolu). Ürün yok/pasif → 400 `PRODUCT_NOT_FOUND` "Product not found or inactive"; zaten favoride → 400 `ALREADY_FAVORITE` "Product already in favorites" |
| DELETE | `/remove/{productId}` | 200 data `"Product removed from favorites"`; favoride değil → 400 `NOT_FAVORITE` "Product not found in favorites" |
| GET | `/check/{productId}` | 200 data `true/false` |
| DELETE | `/clear` | 200 data `"Favorites cleared successfully"` |

### 4.8 Adres — `/api/address` (Kullanıcı, Sahip)

| Metot | Yol | Açıklama |
|---|---|---|
| GET | `/user/{userId}` | Kendi adresleri (varsayılan önce, sonra en yeni). Başka kullanıcı → 403 `FORBIDDEN` "You can only access your own addresses" |
| GET | `/{id}` | 404 `ADDRESS_NOT_FOUND` "Address not found" (başkasınınki de 404) |
| POST | `/` | `{title, fullAddress, city, district, postalCode, country="Turkey", isDefault=false, phoneNumber?}` → 201. Kullanıcının ilk adresi otomatik varsayılan olur; `isDefault:true` diğerlerini kaldırır. |
| PUT | `/{id}` | Aynı alanlar (`country` boşsa "Turkey", `isDefault` gönderilmezse değişmez) → 200 |
| DELETE | `/{id}` | Yumuşak silme → 200 data `"Address deleted successfully"`; varsayılan silinirse kalan en yeni adres varsayılan olur |
| PUT | `/{id}/default` | 200 `Address` |

### 4.9 Ödeme yöntemi — `/api/paymentmethod` (Kullanıcı, Sahip)

Adres ile aynı rota kalıbı: `GET /user/{userId}`, `GET /{id}`, `POST /`, `PUT /{id}`, `DELETE /{id}`, `PUT /{id}/default`.
- Gövde: `{type, cardHolderName, cardNumber, expiryMonth 1..12, expiryYear, cvv?, bankName?, accountNumber?, accountHolderName?, isDefault=false}`.
- `cardNumber` boşluk/tire temizlendikten sonra 12–19 rakam olmalı → değilse 400 `INVALID_CARD_NUMBER` "Geçersiz kart numarası.". Son kullanma tarihi geçmişse 400 `CARD_EXPIRED` "Kartın son kullanma tarihi geçmiş.".
- `PUT` sırasında `cardNumber` maskeli (`*` içeren) gelirse mevcut numara korunur.
- Başkasının listesi → 403 "You can only access your own payment methods"; bulunamadı → 404 `PAYMENT_METHOD_NOT_FOUND` "Payment method not found".

### 4.10 Sipariş — `/api/order`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/` | Kullanıcı | Kendi siparişleri, en yeni önce |
| GET | `/{id}` | Kullanıcı (Sahip) | 404 `ORDER_NOT_FOUND` "Sipariş bulunamadı" |
| POST | `/` | Kullanıcı | Bkz. §5.2. 201 msg `Siparişiniz alındı`. İsteğe bağlı `Idempotency-Key` başlığı. |
| PUT | `/{id}/cancel` | Kullanıcı (Sahip) | İsteğe bağlı gövde `{reason?}` (≤500). Yalnızca `Pending`/`Processing` → stok iade, durum `Cancelled` → 200 data `"Sipariş iptal edildi"`. Aksi → 400 `CANCEL_NOT_ALLOWED` "Bu sipariş iptal edilemez." |
| POST | `/{id}/return-request` | Kullanıcı (Sahip) | `{reason}` zorunlu (≤500). Yalnızca `Delivered` → `ReturnRequested` → 200 `Order`, msg `İade talebiniz alındı`. Aksi → 400 `RETURN_NOT_ALLOWED` "Yalnızca teslim edilmiş siparişler için iade talebi oluşturulabilir." |
| POST | `/{id}/demo/advance-fulfillment` | Kullanıcı (Sahip) | Demo lojistik: `Pending→Processing→Shipped→Delivered`. Kapalıysa 404 `DEMO_FULFILLMENT_DISABLED`; ilerletilecek adım yoksa 400 `DEMO_ADVANCE_INVALID_STATE` |
| PUT | `/{id}/status` | Admin | `{status, notes?}` — bkz. §5.4. 200 `Order`, msg `Sipariş durumu güncellendi`. Geçersiz durum → 400 `INVALID_STATUS`; yok → 404 `ORDER_NOT_FOUND` |
| GET | `/admin` | Admin | Tüm siparişler, en yeni önce; `pageNumber=1, pageSize=20` uygulanır (düz dizi) |

### 4.11 Yorum — `/api/review`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/product/{productId}` | Anonim | Aktif yorumlar, en yeni önce |
| GET | `/product/{productId}/summary` | Anonim | `ProductReviewSummary` (yorum yoksa sıfırlar) |
| GET | `/{id}` | Anonim | 404 `REVIEW_NOT_FOUND` "Review not found" |
| GET | `/admin` | Admin | Tüm aktif yorumlar, `pageNumber=1, pageSize=50` |
| POST | `/` | Kullanıcı | `{productId, rating 1..5, title?(≤200), comment?(≤1000)}`; kullanıcı token'dan alınır → 201 `Review` (`userName`, `productName` dolu). `isVerified` = kullanıcının bu ürünü içeren iptal edilmemiş bir siparişi varsa `true`. Ürün yok → 400 `PRODUCT_NOT_FOUND` "Product not found"; zaten yorum var → 400 `REVIEW_EXISTS` "You have already reviewed this product. Edit or remove your existing review." |
| PUT | `/{id}` | Kullanıcı (Sahip veya Admin) | Kısmi `{rating?, title?, comment?}` → 200; başkasının → 403 `FORBIDDEN` "You can only edit your own reviews" |
| DELETE | `/{id}` | Kullanıcı (Sahip veya Admin) | Yumuşak silme → 200 data `"Review deleted successfully"`; başkasının → 403 "You can only delete your own reviews" |

### 4.12 Bildirim — `/api/notification`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/user/{userId}` | Kullanıcı (Sahip) | `pageNumber=1, pageSize=10`; başkası → 403 "You can only access your own notifications" |
| GET | `/summary` | Kullanıcı | `NotificationSummary` |
| GET | `/{id}` | Kullanıcı (Sahip) | 404 `NOTIFICATION_NOT_FOUND` |
| PUT | `/{id}` | Kullanıcı (Sahip) | `{isRead}`; okundu yapılırsa `readAt` = şimdi, okunmadı yapılırsa `readAt` = null → 200 `Notification` |
| PUT | `/mark-all-read` | Kullanıcı | 200 data `"All notifications marked as read"` |
| DELETE | `/{id}` | Kullanıcı (Sahip) | 200 data `"Notification deleted successfully"` |
| POST | `/` | Admin | `{userId, title, message, type, actionUrl?}` → 201; kullanıcı yok → 400 `USER_NOT_FOUND` |
| GET | `/admin/user/{userId}` | Admin | `pageNumber=1, pageSize=30` |

Sistem bildirimleri (Type `Order`, actionUrl `/orders/{id}`):
- Sipariş oluşturulunca: "Sipariş alındı" / `#{orderNumber} siparişiniz oluşturuldu. Tutar: {totalAmount:F2}`
- Durum değişince: Processing "Sipariş hazırlanıyor", Shipped "Sipariş kargoya verildi", Delivered "Sipariş teslim edildi", Cancelled "Sipariş iptal edildi", ReturnRequested "İade talebi alındı", Returned "İade tamamlandı" (mesaj: `#{orderNumber} siparişiniz …`).

### 4.13 Güvenlik — `/api/security` (Kullanıcı)

| Metot | Yol | Açıklama |
|---|---|---|
| GET | `/info` | `Security`. `lastLoginAt/lastLoginIp` son başarılı girişten; `lastPasswordChange` = kullanıcının son güncellenme zamanı; `twoFactorEnabled` false |
| GET | `/login-history` | `pageNumber=1, pageSize=10` → `LoginHistory[]` |
| GET | `/settings` | `SecuritySettings` (kalıcı; ilk okumada varsayılanlar `true,false,true,false,30`) |
| PUT | `/settings` | `SecuritySettings` gövdesi kaydedilir → 200 `SecuritySettings` |
| POST | `/change-password` | `{currentPassword, newPassword(6..100), confirmPassword}` → 200 data `"Password changed successfully"`. Eşleşmezse 400 `VALIDATION_ERROR` "New password and confirm password do not match"; mevcut şifre hatalı → 400 `INVALID_PASSWORD` "Current password is incorrect" |
| POST | `/update-email` | `{newEmail, currentPassword}` → 200 data `"Email updated successfully"`; şifre hatalı → 400 `INVALID_PASSWORD`; e-posta kullanımda → 400 `EMAIL_TAKEN` "Email address is already in use". `isEmailVerified` false olur. |
| POST | `/logout-all-devices` | Çağıran token **hariç** kullanıcının diğer tüm token'larını geçersiz kılar → 200 data `"All devices logged out successfully"` |
| POST | `/enable-2fa`, `/disable-2fa` | Şifre doğrulayan taslak uçlar (`{password}`, `{password, verificationCode}`); 2FA henüz uygulanmadı |

### 4.14 Ayarlar — `/api/settings` (Kullanıcı; her zaman çağıranın ayarları)

| Metot | Yol | Açıklama |
|---|---|---|
| GET | `/user` | `UserSettings` (yoksa varsayılanlarla oluşturulur) |
| PUT | `/user` | Kısmi güncelleme (gönderilmeyen alan değişmez) → 200 `UserSettings` |
| GET | `/privacy` | `PrivacySettings` (yoksa oluşturulur) |
| PUT | `/privacy` | Kısmi güncelleme → 200 `PrivacySettings` |
| POST | `/reset` | İkisini de varsayılana döndürür → 200 data `"Settings reset to defaults successfully"` |
| GET | `/export` | data = **JSON metni**: `{"userSettings":{…},"privacySettings":{…},"exportDate":"…Z"}` (camelCase) |
| POST | `/import` | Gövde: export metnini içeren **JSON string** (`"{\"userSettings\":…}"`) veya doğrudan aynı nesne. Ayarları uygular → 200 data `"Settings imported successfully"`; okunamazsa 400 `INVALID_IMPORT` |

### 4.15 Yardım & destek — `/api/helpsupport`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/articles` | Anonim | `category?, pageNumber=1, pageSize=10`; yayımlanmış makaleler, en yeni önce (`tags` dolu) |
| GET | `/articles/{id}` | Anonim | `viewCount` artar; 404 `ARTICLE_NOT_FOUND` |
| POST | `/articles` | Admin | `{title, content, category, tags[]=[], isPublished=true}` → 201 |
| GET | `/faqs` | Anonim | `category?, pageNumber=1, pageSize=10` |
| POST | `/contact` | Anonim | `{name, email, phone?, subject, message, category="General"}` → 200 data `"Thank you for your message. We will get back to you soon."` (loglanır) |
| GET | `/tickets` | Kullanıcı | Kendi talepleri (`pageNumber=1, pageSize=10`) |
| POST | `/tickets` | Kullanıcı | `{subject, description, category, priority="Medium"}` → 201 `SupportTicket` (`status:"Open"`, `messages:[]`) |
| GET | `/tickets/admin` | Admin | Tüm talepler, `pageNumber=1, pageSize=20` |

### 4.16 Durum, metrik ve test (zarfsız, Anonim)

| Metot | Yol | Cevap |
|---|---|---|
| GET | `/api/test/hello` | `{message, status:"success", timestamp, framework, version, backend}` — `backend` = `"dotnet"` / `"spring"` |
| GET | `/api/test/health` | `{status:"UP", service, timestamp, environment, backend}` |
| GET | `/api/test/info` | `{service, version, framework, environment, timestamp, uptime, machine, os, backend}` |
| GET | `/api/test/ping` | `{message:"pong", timestamp}` |
| GET | `/api/health` | `{status:"Healthy"/"Degraded"/"Unhealthy", totalDuration, entries:[{name, status, duration, description?, data:{}, exception?}], timestamp}` — sağlıksızsa 503 |
| GET | `/api/health/ready`, `/api/health/live` | `{status:"Ready"}` / `{status:"Alive"}` + `timestamp` |
| GET | `/api/metrics/custom` | `{httpRequests:{total, description}, httpRequestDuration:{description}, activeConnections:{value, description}, orders:{total, description}, products:{inStock, description}, timestamp}` (sipariş/ürün sayıları veritabanından) |
| GET | `/api/metrics`, `/api/metrics/prometheus` | Prometheus metin biçimi |
| GET | `/health` | `OK` (text) |
| GET | `/actuator/health` | `{"status":"UP"}` |

`service`: `ecommerce-backend-dotnet` / `ecommerce-backend-spring`.

---

## 5. İş kuralları

### 5.1 Fiyat ve kargo (Checkout ayarları)
- Satış fiyatı: `round(unitPrice × (1 − discount/100), 2)`.
- Ücretsiz kargo eşiği `150.00` TRY, standart kargo `34.99` TRY (yapılandırılabilir: .NET `Checkout:*`, Spring `app.checkout.*`).
- Sepet: `totalAmount` = Σ satır `totalPrice`; boş sepette `shippingFee = 0`, `grandTotal = 0`, `freeShippingRemainingTry = 150`. Ara toplam ≥ eşik → kargo 0 ve `freeShippingRemainingTry = null`; değilse kargo 34.99 ve kalan = eşik − ara toplam.
- Sepet satırı: `unitPrice` = satış fiyatı; `totalPrice` = `unitPrice × min(quantity, stok)`; `isAvailable` = stok > 0 ve stok ≥ quantity. Pasif ürünler sepette gösterilmez.

### 5.2 Sipariş oluşturma (`POST /api/order`)
Gövde: `{shippingAddressId, paymentMethodId, items:[{productId, quantity≥1}] (en az 1), notes?}`

Sırasıyla:
1. `Idempotency-Key` varsa (kırpılır, küçük harfe çevrilir; 8–128 karakter, aksi 400 `IDEMPOTENCY_KEY_INVALID`): aynı kullanıcı + anahtar ile daha önce sipariş oluşturulduysa o sipariş **200** ve msg `Idempotent replay — same order as first request` ile döner.
2. Kalemler `productId`'ye göre birleştirilir; boşsa 400 `EMPTY_ORDER` "Sepette ürün yok.".
3. Adres kullanıcıya ait ve aktif olmalı → aksi 400 `INVALID_ADDRESS` "Teslimat adresi geçersiz veya size ait değil.".
4. Ödeme yöntemi kullanıcıya ait ve aktif olmalı → aksi 400 `INVALID_PAYMENT` "Ödeme yöntemi geçersiz veya size ait değil.".
5. Sunucu sepeti doluysa: sepette uygun olmayan satır varsa 400 `CART_UNAVAILABLE`; istenen kalemler sepetle birebir (ürün + adet) eşleşmiyorsa 400 `CART_MISMATCH` "Sepet ile ödeme özeti uyuşmuyor. Sayfayı yenileyip tekrar deneyin.". Sepet boşsa bu kontrol atlanır.
6. Tek transaction içinde: her ürün aktif olmalı ("Bir ürün artık satışta değil veya bulunamadı.") ve stok yetmeli ("Yetersiz stok: {ad}. Miktarı azaltın veya sepetten çıkarın.") → aksi 400 `CHECKOUT_FAILED`; kargo hesaplanır; ödeme simülasyonu (kart süresi geçmişse "Kartın son kullanma tarihi geçmiş. Lütfen başka bir kart seçin." → 400 `CHECKOUT_FAILED`); stok düşülür (iyimser kilit; çakışmada 409 `CONFLICT`); sipariş `Pending` olarak kaydedilir; `billingAddress = shippingAddress`; sepet temizlenir; "Sipariş alındı" bildirimi oluşturulur.
7. 201 + `Order`.

### 5.3 Müşteri iptali / iadesi
- İptal: yalnızca `Pending`, `Processing`. Stok iade edilir, `cancelReason` saklanır.
- İade talebi: yalnızca `Delivered`; `returnReason`, `returnRequestedAt` saklanır.

### 5.4 Yönetici durum güncellemesi (`PUT /api/order/{id}/status`)
- İzinli değerler (harf duyarsız, kanonik yazımla saklanır): `Pending, Processing, Shipped, Delivered, Cancelled, ReturnRequested, Returned`; aksi 400 `INVALID_STATUS` "Geçersiz durum. Kullanın: Pending, Processing, Shipped, Delivered, Cancelled, ReturnRequested, Returned.".
- `Cancelled`'a `Pending`/`Processing`'den geçişte ve `Returned`'a geçişte stok iade edilir.
- `Shipped`: `shippedAt`=şimdi, `carrier` boşsa "Yurtiçi Kargo", `trackingNumber` boşsa `TR` + 10 hane, `estimatedDeliveryAt` = şimdi + 3 gün. `Delivered`: `deliveredAt`=şimdi.
- Durum değiştiyse kullanıcıya bildirim gider (§4.12).

### 5.5 Demo lojistik
- Açık/kapalı: .NET `Checkout:DemoFulfillmentEnabled`, Spring `app.ecommerce.demo-fulfillment-enabled` (geliştirmede açık, üretimde kapalı).
- Açıksa `Pending/Processing/Shipped` siparişlerde `demoNextAction = "DEMO_ADVANCE_FULFILLMENT"`, aksi `null`.

### 5.6 Oturum iptali (`logout-all-devices`)
- Kullanıcıya iptal zamanı (`tokensRevokedAt`, saniye hassasiyetinde) ve çağıran token'ın `jti` değeri (`revokeExceptJti`) yazılır. Doğrulamada token'ın `iat` değeri bu zamana **eşit veya daha eski** ise ve `jti` istisna değilse 401 `UNAUTHORIZED`. Pasif kullanıcıların token'ları da 401 alır.

---

## 6. Veritabanı ve ortamlar (özet)

| | .NET | Spring |
|---|---|---|
| Geliştirme | SQLite (`ecommerce.db`, `EnsureCreated` + şema tamamlayıcı) | H2 bellek içi (`create-drop`) |
| Üretim/Docker | PostgreSQL + EF Core migration | PostgreSQL + Flyway |
| Önbellek | Redis (opsiyonel) | Redis (opsiyonel) |
| Mesajlaşma | MassTransit (InMemory/RabbitMQ) + outbox | RabbitMQ (opsiyonel) |

---

## 7. Uyumluluk testi

```bash
# Her iki backend ayaktayken:
node scripts/contract-smoke.mjs run http://localhost:5000 out-dotnet.json
node scripts/contract-smoke.mjs run http://localhost:8081 out-spring.json
node scripts/contract-smoke.mjs compare out-dotnet.json out-spring.json
```

`run` her çağrının durum kodunu, `success`/`errorCode` değerlerini ve JSON şeklini kaydeder; `compare`
iki backend arasındaki her farkı listeler ve fark yoksa 0 ile çıkar.
