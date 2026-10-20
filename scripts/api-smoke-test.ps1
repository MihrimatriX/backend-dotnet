# Tüm public + JWT korumalı API uçlarını hızlı doğrular.
# Önkoşul: API çalışıyor (örn. docker compose up veya dotnet run).
#
# Kullanım:
#   .\scripts\api-smoke-test.ps1
#   .\scripts\api-smoke-test.ps1 -BaseUrl http://localhost:5000
#   .\scripts\api-smoke-test.ps1 -SkipWait
#
param(
    [string]$BaseUrl = 'http://localhost:5000',
    [switch]$SkipWait
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_repo-root.ps1"
Set-Location (Get-RepoRoot)

$script:Passed = 0
$script:Failed = 0
$script:SmokeUser1Id = $null

function Test-ApiCall {
    param(
        [string]$Label,
        [string]$Method = 'GET',
        [string]$Uri,
        [string]$Token = $null,
        $Body,
        [string]$RawBody,
        [int[]]$SuccessCodes = @(200, 201, 204)
    )

    $headers = @{ Accept = 'application/json' }
    if ($Token) { $headers['Authorization'] = "Bearer $Token" }

    $params = @{
        Uri             = $Uri
        Method          = $Method
        Headers         = $headers
        UseBasicParsing = $true
    }
    if ($PSBoundParameters.ContainsKey('RawBody') -and $null -ne $RawBody -and $RawBody -ne '') {
        $params['Body'] = $RawBody
        $params['ContentType'] = 'application/json; charset=utf-8'
    }
    elseif ($null -ne $Body) {
        $params['Body'] = ($Body | ConvertTo-Json -Depth 12 -Compress)
        $params['ContentType'] = 'application/json'
    }

    $code = 0
    $raw = $null
    try {
        $r = Invoke-WebRequest @params -ErrorAction Stop
        $code = [int]$r.StatusCode
        $raw = $r.Content
    }
    catch {
        $resp = $_.Exception.Response
        if ($resp) {
            $code = [int]$resp.StatusCode
            try {
                $stream = $resp.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($stream)
                $raw = $reader.ReadToEnd()
                $reader.Close()
            }
            catch { }
        }
        else {
            Write-Host "FAIL $Label — $($_.Exception.Message)" -ForegroundColor Red
            $script:Failed++
            return $null
        }
    }

    if ($SuccessCodes -contains $code) {
        Write-Host "OK   $Label ($code)" -ForegroundColor Green
        $script:Passed++
        if ($raw) {
            try { return ($raw | ConvertFrom-Json) } catch { return $null }
        }
        return $null
    }

    Write-Host "FAIL $Label (HTTP $code, beklenen: $($SuccessCodes -join ', '))" -ForegroundColor Red
    $script:Failed++
    return $null
}

function Get-Login {
    param([string]$Email, [string]$Password)
    $body = @{ email = $Email; password = $Password }
    $r = Test-ApiCall -Label "Auth/login $Email" -Method POST -Uri "$BaseUrl/api/Auth/login" -Body $body -SuccessCodes @(200)
    if (-not $r -or -not $r.success) { return $null }
    return @{ Token = [string]$r.data.token; UserId = [int]$r.data.userId }
}

if (-not $SkipWait) {
    Write-Host '[api-smoke-test] /health bekleniyor...' -ForegroundColor Cyan
    & "$PSScriptRoot\wait-api.ps1" -Url "$BaseUrl/health" -TimeoutSec 90
}

Write-Host "`n=== Genel / anonim ===" -ForegroundColor Cyan
Test-ApiCall -Label 'GET /' -Uri "$BaseUrl/"
Test-ApiCall -Label 'GET /health' -Uri "$BaseUrl/health"
Test-ApiCall -Label 'GET /api/health' -Uri "$BaseUrl/api/health"
Test-ApiCall -Label 'GET /api/health/ready' -Uri "$BaseUrl/api/health/ready"
Test-ApiCall -Label 'GET /api/health/live' -Uri "$BaseUrl/api/health/live"
Test-ApiCall -Label 'GET /swagger/v1/swagger.json' -Uri "$BaseUrl/swagger/v1/swagger.json"
Test-ApiCall -Label 'GET /api/Test/hello' -Uri "$BaseUrl/api/Test/hello"
Test-ApiCall -Label 'GET /api/Test/health' -Uri "$BaseUrl/api/Test/health"
Test-ApiCall -Label 'GET /api/Test/info' -Uri "$BaseUrl/api/Test/info"
Test-ApiCall -Label 'GET /api/Test/ping' -Uri "$BaseUrl/api/Test/ping"
Test-ApiCall -Label 'GET /api/Metrics' -Uri "$BaseUrl/api/Metrics" -SuccessCodes @(200)
Test-ApiCall -Label 'GET /api/Metrics/prometheus' -Uri "$BaseUrl/api/Metrics/prometheus" -SuccessCodes @(200)
Test-ApiCall -Label 'GET /api/Metrics/custom' -Uri "$BaseUrl/api/Metrics/custom"

$catList = Test-ApiCall -Label 'GET /api/Category' -Uri "$BaseUrl/api/Category"
$categoryId = 1
if ($catList.data -and $catList.data.Count -gt 0) { $categoryId = [int]$catList.data[0].id }

Test-ApiCall -Label "GET /api/Category/$categoryId" -Uri "$BaseUrl/api/Category/$categoryId"

$products = Test-ApiCall -Label 'GET /api/Product?pageSize=5' -Uri "$BaseUrl/api/Product?pageNumber=1&pageSize=5"
$productId = 1
if ($products.data.items -and $products.data.items.Count -gt 0) { $productId = [int]$products.data.items[0].id }
if (-not $productId) { $productId = 1 }

Test-ApiCall -Label "GET /api/Product/$productId" -Uri "$BaseUrl/api/Product/$productId"
Test-ApiCall -Label "GET /api/Product/category/$categoryId" -Uri "$BaseUrl/api/Product/category/$categoryId"
Test-ApiCall -Label 'GET /api/Product/search?q=test' -Uri "$BaseUrl/api/Product/search?q=test"
Test-ApiCall -Label 'GET /api/Product/featured' -Uri "$BaseUrl/api/Product/featured"
Test-ApiCall -Label 'GET /api/Product/discounted' -Uri "$BaseUrl/api/Product/discounted"

$campaignList = Test-ApiCall -Label 'GET /api/Campaign' -Uri "$BaseUrl/api/Campaign"
Test-ApiCall -Label 'GET /api/Campaign/active' -Uri "$BaseUrl/api/Campaign/active"
$campaignId = 1
if ($campaignList.data -and $campaignList.data.Count -gt 0) { $campaignId = [int]$campaignList.data[0].id }
Test-ApiCall -Label "GET /api/Campaign/$campaignId" -Uri "$BaseUrl/api/Campaign/$campaignId"

Test-ApiCall -Label 'GET /api/SubCategory' -Uri "$BaseUrl/api/SubCategory"
Test-ApiCall -Label "GET /api/SubCategory/category/$categoryId" -Uri "$BaseUrl/api/SubCategory/category/$categoryId"
Test-ApiCall -Label 'GET /api/SubCategory/1' -Uri "$BaseUrl/api/SubCategory/1" -SuccessCodes @(200, 404)

Test-ApiCall -Label "GET /api/Review/product/$productId" -Uri "$BaseUrl/api/Review/product/$productId"
Test-ApiCall -Label "GET /api/Review/product/$productId/summary" -Uri "$BaseUrl/api/Review/product/$productId/summary"
Test-ApiCall -Label 'GET /api/Review/1' -Uri "$BaseUrl/api/Review/1" -SuccessCodes @(200, 404)

Test-ApiCall -Label 'GET /api/HelpSupport/articles' -Uri "$BaseUrl/api/HelpSupport/articles"
Test-ApiCall -Label 'GET /api/HelpSupport/articles/1' -Uri "$BaseUrl/api/HelpSupport/articles/1" -SuccessCodes @(200, 404)
Test-ApiCall -Label 'GET /api/HelpSupport/faqs' -Uri "$BaseUrl/api/HelpSupport/faqs"
$contact = @{
    name    = 'Smoke Test'
    email   = 'smoke@example.com'
    subject = 'Test'
    message = 'Hello'
}
Test-ApiCall -Label 'POST /api/HelpSupport/contact' -Method POST -Uri "$BaseUrl/api/HelpSupport/contact" -Body $contact

$regEmail = "smoke_$([Guid]::NewGuid().ToString('N').Substring(0, 12))@example.com"
Test-ApiCall -Label 'POST /api/Auth/register' -Method POST -Uri "$BaseUrl/api/Auth/register" -Body @{
    email       = $regEmail
    password    = 'regpass1'
    firstName   = 'Smoke'
    lastName    = 'User'
    phoneNumber = '+905551112233'
    address     = 'Adres'
    city        = 'İstanbul'
    postalCode  = '34000'
} -SuccessCodes @(200, 201)

Test-ApiCall -Label 'POST /api/Auth/logout' -Method POST -Uri "$BaseUrl/api/Auth/logout" -Body @{}

Write-Host "`n=== user1@example.com (User) ===" -ForegroundColor Cyan
$u1 = Get-Login -Email 'user1@example.com' -Password 'user123'
if (-not $u1) { Write-Host 'user1 giriş başarısız; kullanıcı uçları atlanıyor.' -ForegroundColor Yellow }
else {
    $t1 = $u1.Token
    $id1 = $u1.UserId
    $script:SmokeUser1Id = $id1

    Test-ApiCall -Label 'GET /api/Cart' -Uri "$BaseUrl/api/Cart" -Token $t1
    Test-ApiCall -Label 'GET /api/Cart/total' -Uri "$BaseUrl/api/Cart/total" -Token $t1
    Test-ApiCall -Label 'GET /api/Cart/count' -Uri "$BaseUrl/api/Cart/count" -Token $t1
    Test-ApiCall -Label 'POST /api/Cart/add' -Method POST -Uri "$BaseUrl/api/Cart/add" -Token $t1 -Body @{ productId = $productId; quantity = 1 }
    Test-ApiCall -Label 'PUT /api/Cart/update' -Method PUT -Uri "$BaseUrl/api/Cart/update" -Token $t1 -Body @{ productId = $productId; quantity = 2 }
    Test-ApiCall -Label "DELETE /api/Cart/remove/$productId" -Method DELETE -Uri "$BaseUrl/api/Cart/remove/$productId" -Token $t1
    Test-ApiCall -Label 'DELETE /api/Cart/clear' -Method DELETE -Uri "$BaseUrl/api/Cart/clear" -Token $t1

    Test-ApiCall -Label 'GET /api/Favorite' -Uri "$BaseUrl/api/Favorite" -Token $t1
    Test-ApiCall -Label 'POST /api/Favorite/add' -Method POST -Uri "$BaseUrl/api/Favorite/add" -Token $t1 -Body @{ productId = $productId }
    Test-ApiCall -Label "GET /api/Favorite/check/$productId" -Uri "$BaseUrl/api/Favorite/check/$productId" -Token $t1
    Test-ApiCall -Label "DELETE /api/Favorite/remove/$productId" -Method DELETE -Uri "$BaseUrl/api/Favorite/remove/$productId" -Token $t1

    Test-ApiCall -Label "GET /api/Address/user/$id1" -Uri "$BaseUrl/api/Address/user/$id1" -Token $t1
    $addrBody = @{
        title       = 'Ev'
        fullAddress = 'Test cad. 1'
        city        = 'İstanbul'
        district    = 'Kadıköy'
        postalCode  = '34000'
        country     = 'Turkey'
        isDefault   = $true
        phoneNumber = '+905551112233'
    }
    $addrRes = Test-ApiCall -Label 'POST /api/Address' -Method POST -Uri "$BaseUrl/api/Address" -Token $t1 -Body $addrBody
    $addressId = 1
    if ($addrRes.data.id) { $addressId = [int]$addrRes.data.id }
    Test-ApiCall -Label "GET /api/Address/$addressId" -Uri "$BaseUrl/api/Address/$addressId" -Token $t1
    $updAddr = @{
        title       = 'Ev2'
        fullAddress = 'Test cad. 3'
        city        = 'İstanbul'
        district    = 'Kadıköy'
        postalCode  = '34000'
        country     = 'Turkey'
        isDefault   = $true
        phoneNumber = '+905551112233'
    }
    Test-ApiCall -Label "PUT /api/Address/$addressId" -Method PUT -Uri "$BaseUrl/api/Address/$addressId" -Token $t1 -Body $updAddr
    Test-ApiCall -Label "PUT /api/Address/$addressId/default" -Method PUT -Uri "$BaseUrl/api/Address/$addressId/default" -Token $t1

    $pmBody = @{
        type            = 'CreditCard'
        cardHolderName  = 'Test User'
        cardNumber      = '4111111111111111'
        expiryMonth     = 12
        expiryYear      = 2030
        cvv             = '123'
        isDefault       = $true
    }
    Test-ApiCall -Label "GET /api/PaymentMethod/user/$id1" -Uri "$BaseUrl/api/PaymentMethod/user/$id1" -Token $t1
    $pmRes = Test-ApiCall -Label 'POST /api/PaymentMethod' -Method POST -Uri "$BaseUrl/api/PaymentMethod" -Token $t1 -Body $pmBody
    $paymentMethodId = 1
    if ($pmRes.data.id) { $paymentMethodId = [int]$pmRes.data.id }
    Test-ApiCall -Label "GET /api/PaymentMethod/$paymentMethodId" -Uri "$BaseUrl/api/PaymentMethod/$paymentMethodId" -Token $t1

    Test-ApiCall -Label 'GET /api/Order' -Uri "$BaseUrl/api/Order" -Token $t1
    Test-ApiCall -Label 'GET /api/Order/1' -Uri "$BaseUrl/api/Order/1" -Token $t1 -SuccessCodes @(200, 404)
    $orderBody = @{
        shippingAddressId = $addressId
        paymentMethodId   = $paymentMethodId
        items             = @(@{ productId = $productId; quantity = 1 })
        notes             = 'smoke'
    }
    $ordRes = Test-ApiCall -Label 'POST /api/Order' -Method POST -Uri "$BaseUrl/api/Order" -Token $t1 -Body $orderBody -SuccessCodes @(200, 201, 400)
    $orderId = $null
    if ($ordRes.data.id) { $orderId = [int]$ordRes.data.id }
    if ($orderId) {
        Test-ApiCall -Label "GET /api/Order/$orderId" -Uri "$BaseUrl/api/Order/$orderId" -Token $t1
        Test-ApiCall -Label "PUT /api/Order/$orderId/cancel" -Method PUT -Uri "$BaseUrl/api/Order/$orderId/cancel" -Token $t1 -SuccessCodes @(200, 400)
    }

    Test-ApiCall -Label "GET /api/Notification/user/$id1" -Uri "$BaseUrl/api/Notification/user/$id1?pageSize=5" -Token $t1
    Test-ApiCall -Label 'GET /api/Notification/summary' -Uri "$BaseUrl/api/Notification/summary" -Token $t1
    Test-ApiCall -Label 'PUT /api/Notification/mark-all-read' -Method PUT -Uri "$BaseUrl/api/Notification/mark-all-read" -Token $t1

    Test-ApiCall -Label 'GET /api/Settings/user' -Uri "$BaseUrl/api/Settings/user" -Token $t1
    Test-ApiCall -Label 'PUT /api/Settings/user' -Method PUT -Uri "$BaseUrl/api/Settings/user" -Token $t1 -Body @{ language = 'tr'; theme = 'light' }
    Test-ApiCall -Label 'GET /api/Settings/privacy' -Uri "$BaseUrl/api/Settings/privacy" -Token $t1
    Test-ApiCall -Label 'PUT /api/Settings/privacy' -Method PUT -Uri "$BaseUrl/api/Settings/privacy" -Token $t1 -Body @{
        profileVisibility = $true
        showEmail         = $false
        allowCookies      = $true
    }
    Test-ApiCall -Label 'GET /api/Settings/export' -Uri "$BaseUrl/api/Settings/export" -Token $t1
    Test-ApiCall -Label 'POST /api/Settings/reset' -Method POST -Uri "$BaseUrl/api/Settings/reset" -Token $t1
    Test-ApiCall -Label 'POST /api/Settings/import (ham JSON string)' -Method POST -Uri "$BaseUrl/api/Settings/import" -Token $t1 -RawBody '""' -SuccessCodes @(200, 400)

    Test-ApiCall -Label 'GET /api/Security/info' -Uri "$BaseUrl/api/Security/info" -Token $t1
    Test-ApiCall -Label 'GET /api/Security/login-history' -Uri "$BaseUrl/api/Security/login-history" -Token $t1
    Test-ApiCall -Label 'GET /api/Security/settings' -Uri "$BaseUrl/api/Security/settings" -Token $t1
    $badPwd = @{ currentPassword = 'wrong'; newPassword = 'newpass12'; confirmPassword = 'newpass12' }
    Test-ApiCall -Label 'POST /api/Security/change-password (bilerek hatalı)' -Method POST -Uri "$BaseUrl/api/Security/change-password" -Token $t1 -Body $badPwd -SuccessCodes @(400)
    $secSet = @{ emailNotifications = $true; smsNotifications = $false; loginAlerts = $true; twoFactorRequired = $false; sessionTimeout = 30 }
    Test-ApiCall -Label 'PUT /api/Security/settings' -Method PUT -Uri "$BaseUrl/api/Security/settings" -Token $t1 -Body $secSet
    Test-ApiCall -Label 'POST /api/Security/logout-all-devices' -Method POST -Uri "$BaseUrl/api/Security/logout-all-devices" -Token $t1

    Test-ApiCall -Label 'GET /api/HelpSupport/tickets' -Uri "$BaseUrl/api/HelpSupport/tickets" -Token $t1
    $ticket = @{ subject = 'Smoke'; description = 'Test'; category = 'Genel'; priority = 'Low' }
    Test-ApiCall -Label 'POST /api/HelpSupport/tickets' -Method POST -Uri "$BaseUrl/api/HelpSupport/tickets" -Token $t1 -Body $ticket

    $rev = @{ productId = $productId; rating = 5; title = 'İyi'; comment = 'Smoke' }
    Test-ApiCall -Label 'POST /api/Review' -Method POST -Uri "$BaseUrl/api/Review" -Token $t1 -Body $rev -SuccessCodes @(200, 201, 400)
}

Write-Host "`n=== admin@example.com (Admin) ===" -ForegroundColor Cyan
$adm = Get-Login -Email 'admin@example.com' -Password 'admin123'
if (-not $adm) { Write-Host 'Admin giriş başarısız; Admin uçları atlanıyor.' -ForegroundColor Yellow }
else {
    $ta = $adm.Token
    Test-ApiCall -Label 'GET /api/Order/admin' -Uri "$BaseUrl/api/Order/admin?pageSize=5" -Token $ta
    Test-ApiCall -Label 'PUT /api/Order/1/status (yoksa 400)' -Method PUT -Uri "$BaseUrl/api/Order/1/status" -Token $ta -Body @{ status = 'Shipped'; notes = 'smoke' } -SuccessCodes @(200, 400, 404)

    $camp = @{
        title            = "Smoke-$(New-Guid)"
        subtitle         = 's'
        description      = 'd'
        discount         = 10
        startDate        = (Get-Date).ToUniversalTime().ToString('o')
        endDate          = (Get-Date).AddDays(30).ToUniversalTime().ToString('o')
        isActive         = $true
        buttonText       = 'Git'
        buttonHref       = '/'
        backgroundColor  = '#fff'
    }
    $cr = Test-ApiCall -Label 'POST /api/Campaign (Admin)' -Method POST -Uri "$BaseUrl/api/Campaign" -Token $ta -Body $camp -SuccessCodes @(200, 201)
    $newCampId = $null
    if ($cr.data.id) { $newCampId = [int]$cr.data.id }
    if ($newCampId) {
        $campUp = $camp.Clone()
        $campUp.title = "$($camp.title)-2"
        Test-ApiCall -Label "PUT /api/Campaign/$newCampId" -Method PUT -Uri "$BaseUrl/api/Campaign/$newCampId" -Token $ta -Body $campUp
        Test-ApiCall -Label "DELETE /api/Campaign/$newCampId" -Method DELETE -Uri "$BaseUrl/api/Campaign/$newCampId" -Token $ta -SuccessCodes @(200, 204, 404)
    }

    if ($script:SmokeUser1Id) {
        $n = @{
            userId    = $script:SmokeUser1Id
            title     = 'Smoke bildirim'
            message   = 'Test'
            type      = 'Info'
            actionUrl = '/orders'
        }
        Test-ApiCall -Label 'POST /api/Notification (Admin -> user1)' -Method POST -Uri "$BaseUrl/api/Notification" -Token $ta -Body $n
    }

    $article = @{ title = 'Smoke-help'; content = 'İçerik'; category = 'Genel'; tags = @('t'); isPublished = $true }
    Test-ApiCall -Label 'POST /api/HelpSupport/articles (Admin)' -Method POST -Uri "$BaseUrl/api/HelpSupport/articles" -Token $ta -Body $article -SuccessCodes @(200, 201)
}

Write-Host "`n=== Geçici Category + SubCategory (oluştur / sil) ===" -ForegroundColor Cyan
$adm2 = Get-Login -Email 'admin@example.com' -Password 'admin123'
if ($adm2) {
    $tok = $adm2.Token
    $guid = [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $newCat = @{ categoryName = "SmokeCat-$guid"; description = 't'; isActive = $true }
    $cRes = Test-ApiCall -Label 'POST /api/Category' -Method POST -Uri "$BaseUrl/api/Category" -Token $tok -Body $newCat -SuccessCodes @(200, 201)
    $newCatId = $null
    if ($cRes.data.id) { $newCatId = [int]$cRes.data.id }
    if ($newCatId) {
        Test-ApiCall -Label "GET /api/Category/$newCatId" -Uri "$BaseUrl/api/Category/$newCatId" -Token $tok
        $upCat = @{ categoryName = "SmokeCat-${guid}2"; description = 'u'; imageUrl = $null; isActive = $true }
        Test-ApiCall -Label "PUT /api/Category/$newCatId" -Method PUT -Uri "$BaseUrl/api/Category/$newCatId" -Token $tok -Body $upCat
        $sub = @{ subCategoryName = "Sub-$guid"; description = 's'; categoryId = $newCatId }
        $sRes = Test-ApiCall -Label 'POST /api/SubCategory' -Method POST -Uri "$BaseUrl/api/SubCategory" -Token $tok -Body $sub -SuccessCodes @(200, 201)
        $subId = $null
        if ($sRes.data.id) { $subId = [int]$sRes.data.id }
        if ($subId) {
            $su = @{ id = $subId; subCategoryName = "Sub-${guid}x"; description = 'x'; categoryId = $newCatId; isActive = $true; imageUrl = $null }
            Test-ApiCall -Label "PUT /api/SubCategory/$subId" -Method PUT -Uri "$BaseUrl/api/SubCategory/$subId" -Token $tok -Body $su
            Test-ApiCall -Label "DELETE /api/SubCategory/$subId" -Method DELETE -Uri "$BaseUrl/api/SubCategory/$subId" -Token $tok
        }
        Test-ApiCall -Label "DELETE /api/Category/$newCatId" -Method DELETE -Uri "$BaseUrl/api/Category/$newCatId" -Token $tok
    }
}

Write-Host "`n=== Product POST/PUT/DELETE (Admin token) ===" -ForegroundColor Cyan
$adm3 = Get-Login -Email 'admin@example.com' -Password 'admin123'
if ($adm3) {
    $tok3 = $adm3.Token
    $pg = [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $prod = @{
        productName     = "SmokeProd-$pg"
        unitPrice       = 9.99
        unitInStock     = 10
        quantityPerUnit = '1 adet'
        categoryId      = $categoryId
        description     = 'smoke'
        discount        = 0
        isActive        = $true
    }
    $pRes = Test-ApiCall -Label 'POST /api/Product' -Method POST -Uri "$BaseUrl/api/Product" -Token $tok3 -Body $prod -SuccessCodes @(200, 201)
    $newPid = $null
    if ($pRes.data.id) { $newPid = [int]$pRes.data.id }
    if ($newPid) {
        $prod2 = @{
            id              = $newPid
            productName     = "SmokeProd-${pg}x"
            unitPrice       = 9.99
            unitInStock     = 10
            quantityPerUnit = '1 adet'
            categoryId      = $categoryId
            description     = 'smoke'
            discount        = 0
            isActive        = $true
        }
        Test-ApiCall -Label "PUT /api/Product/$newPid" -Method PUT -Uri "$BaseUrl/api/Product/$newPid" -Token $tok3 -Body $prod2
        Test-ApiCall -Label "DELETE /api/Product/$newPid" -Method DELETE -Uri "$BaseUrl/api/Product/$newPid" -Token $tok3
    }
}

Write-Host "`n--- Özet: OK=$script:Passed  FAIL=$script:Failed ---" -ForegroundColor $(if ($script:Failed -eq 0) { 'Green' } else { 'Yellow' })
if ($script:Failed -gt 0) { exit 1 }
exit 0
