#!/usr/bin/env node
// Ortak API sözleşmesi duman testi — backend-dotnet ve backend-spring'de birebir aynı dosya.
// docs/API_CONTRACT.md'deki uçları vitrinin (temp-shop-net) kullandığı sırayla dener ve
//  - beklenen HTTP kodunu,
//  - başarılı cevaplarda `data` bulunmasını,
//  - tarihlerin UTC `Z` biçiminde olmasını
// doğrular. Ayrıca her cevabın JSON şeklini kaydeder; iki backend'in çıktısı `compare` ile
// karşılaştırılır.
//
//   node scripts/contract-smoke.mjs run http://localhost:5000 out-dotnet.json
//   node scripts/contract-smoke.mjs run http://localhost:8081 out-spring.json
//   node scripts/contract-smoke.mjs compare out-dotnet.json out-spring.json
//
// Gereken seed: admin@example.com / admin123 (Admin) ve en az bir aktif, stoklu ürün.

import { readFileSync, writeFileSync } from "node:fs";

const [, , mode, ...args] = process.argv;
const ISO_DATE = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/;

function shapeOf(v) {
  if (v === null || v === undefined) return "null";
  if (Array.isArray(v)) {
    if (v.length === 0) return ["empty"];
    let merged = shapeOf(v[0]);
    for (const x of v.slice(1, 20)) merged = mergeShapes(merged, shapeOf(x));
    return [merged];
  }
  if (typeof v === "object") {
    const o = {};
    for (const k of Object.keys(v).sort()) o[k] = shapeOf(v[k]);
    return o;
  }
  return typeof v;
}

function mergeShapes(a, b) {
  if (a === "null") return b;
  if (b === "null") return a;
  if (Array.isArray(a) && Array.isArray(b)) {
    if (a[0] === "empty") return b;
    if (b[0] === "empty") return a;
    return [mergeShapes(a[0], b[0])];
  }
  if (a && b && typeof a === "object" && typeof b === "object" && !Array.isArray(a) && !Array.isArray(b)) {
    const o = { ...a };
    for (const k of Object.keys(b)) o[k] = k in o ? mergeShapes(o[k], b[k]) : b[k];
    return o;
  }
  return a;
}

function badDates(v, path = "$", out = []) {
  if (typeof v === "string") {
    if (ISO_DATE.test(v) && !v.endsWith("Z")) out.push(`${path}=${v}`);
  } else if (Array.isArray(v)) {
    v.slice(0, 5).forEach((x, i) => badDates(x, `${path}[${i}]`, out));
  } else if (v && typeof v === "object") {
    for (const [k, x] of Object.entries(v)) badDates(x, `${path}.${k}`, out);
  }
  return out;
}

function decodeJwt(token) {
  try {
    return JSON.parse(Buffer.from(token.split(".")[1], "base64url").toString("utf8"));
  } catch {
    return {};
  }
}

async function run(base, outFile) {
  base = base.replace(/\/$/, "");
  const results = [];
  const stamp = Date.now();

  async function call(name, method, path, { token, body, raw, expect = 200, headers: extra } = {}) {
    const headers = { Accept: "application/json", ...extra };
    if (body !== undefined) headers["Content-Type"] = "application/json";
    if (token) headers.Authorization = `Bearer ${token}`;
    let status = 0;
    let json = null;
    let text = "";
    try {
      const res = await fetch(base + path, {
        method,
        headers,
        body: body === undefined ? undefined : raw ? body : JSON.stringify(body),
      });
      status = res.status;
      text = await res.text();
      try {
        json = text ? JSON.parse(text) : null;
      } catch {
        json = null;
      }
    } catch (e) {
      text = String(e);
    }
    const enveloped = json && typeof json === "object" && !Array.isArray(json) && "success" in json;
    const problems = [];
    const expected = Array.isArray(expect) ? expect : [expect];
    if (!expected.includes(status)) problems.push(`HTTP ${status}, beklenen ${expected.join("/")}`);
    if (enveloped && status >= 200 && status < 300) {
      if (json.success !== true) problems.push("success=false");
      if (json.data === undefined || json.data === null) problems.push("data yok");
    }
    if (enveloped && status >= 400 && !json.errorCode) problems.push("errorCode yok");
    if (status >= 400 && !enveloped && path.startsWith("/api/")) problems.push("hata zarfı yok");
    const dates = json ? badDates(json) : [];
    if (dates.length) problems.push(`Z'siz tarih: ${dates[0]}`);
    const entry = {
      name,
      method,
      path: path.replace(/\/\d+(?=\/|$|\?)/g, "/{id}").replace(/=\d+/g, "={n}"),
      status,
      success: enveloped ? json.success : undefined,
      errorCode: enveloped ? json.errorCode : undefined,
      message: enveloped ? json.message : undefined,
      problems,
      shape: json === null ? (text ? "non-json" : "empty") : shapeOf(json),
    };
    results.push(entry);
    const flag = problems.length ? "✗" : "✓";
    const extraInfo = entry.errorCode ? ` [${entry.errorCode}]` : "";
    console.log(`${flag} ${String(status).padEnd(3)} ${method.padEnd(6)} ${path}${extraInfo}${problems.length ? "  ← " + problems.join("; ") : ""}`);
    return json;
  }

  function check(name, ok, detail) {
    results.push({ name, method: "CHECK", path: "", status: ok ? 200 : 0, problems: ok ? [] : [detail], shape: "check" });
    console.log(`${ok ? "✓" : "✗"} CHECK  ${name}${ok ? "" : "  ← " + detail}`);
  }

  // ---- Altyapı (zarfsız)
  await call("test.hello", "GET", "/api/test/hello");
  await call("test.health", "GET", "/api/test/health");
  await call("test.info", "GET", "/api/test/info");
  await call("test.ping", "GET", "/api/test/ping");
  await call("health", "GET", "/api/health", { expect: [200, 503] });
  await call("health.ready", "GET", "/api/health/ready");
  await call("health.live", "GET", "/api/health/live");
  await call("metrics.custom", "GET", "/api/metrics/custom");
  await call("metrics.prometheus", "GET", "/api/metrics/prometheus");

  // ---- Kimlik
  const email = `contract.${stamp}@example.com`;
  const password = "Contract123!";
  const reg = await call("auth.register", "POST", "/api/auth/register", {
    body: { email: `  ${email.toUpperCase()} `, password, firstName: "Sözleşme", lastName: "Test", phoneNumber: "05550000000" },
    expect: 201,
  });
  check("auth.register.emailNormalized", reg?.data?.email === email, `email=${reg?.data?.email}`);
  await call("auth.register.duplicate", "POST", "/api/auth/register", {
    body: { email, password, firstName: "Sözleşme", lastName: "Test" },
    expect: 409,
  });
  await call("auth.register.invalid", "POST", "/api/auth/register", { body: { email: "bad", password: "1" }, expect: 400 });
  await call("auth.login.malformed", "POST", "/api/auth/login", { body: "{bad json", raw: true, expect: 400 });
  const login = await call("auth.login", "POST", "/api/auth/login", { body: { email, password } });
  await call("auth.login.wrongPassword", "POST", "/api/auth/login", { body: { email, password: "wrong-pass" }, expect: 401 });
  const token = login?.data?.token;
  const userId = login?.data?.userId;
  const adminLogin = await call("auth.login.admin", "POST", "/api/auth/login", {
    body: { email: "admin@example.com", password: "admin123" },
  });
  const admin = adminLogin?.data?.token;
  const p = decodeJwt(token ?? "");
  check("jwt.claims", String(p.sub) === String(userId) && p.email === email && p.role === "User" && !!p.jti && !!p.iat && !!p.exp,
    `sub=${p.sub} email=${p.email} role=${p.role} jti=${p.jti} iat=${p.iat}`);
  const pa = decodeJwt(admin ?? "");
  check("jwt.adminRole", pa.role === "Admin", `role=${JSON.stringify(pa.role)}`);
  check("auth.role", login?.data?.role === "User" && adminLogin?.data?.role === "Admin", `role=${login?.data?.role}/${adminLogin?.data?.role}`);

  // ---- Yetki hataları
  await call("auth.missingToken", "GET", "/api/cart", { expect: 401 });
  await call("auth.badToken", "GET", "/api/cart", { token: "abc.def.ghi", expect: 401 });
  await call("auth.forbidden", "GET", "/api/order/admin?pageSize=5", { token, expect: 403 });
  await call("auth.forbidden.productUpdate", "PUT", "/api/product/1", { token, body: { productName: "x" }, expect: 403 });
  await call("route.notFound", "GET", "/api/doesnotexist", { token, expect: 404 });

  // ---- Katalog (anonim)
  const cats = await call("category.list", "GET", "/api/category");
  const catId = cats?.data?.[0]?.id;
  if (catId) await call("category.get", "GET", `/api/category/${catId}`);
  await call("category.notFound", "GET", "/api/category/987654321", { expect: 404 });
  const subs = await call("subcategory.list", "GET", "/api/subcategory");
  const sub0 = subs?.data?.[0];
  if (sub0) {
    await call("subcategory.get", "GET", `/api/subcategory/${sub0.id}`);
    await call("subcategory.byCategory", "GET", `/api/subcategory/category/${sub0.categoryId}`);
  }
  const page = await call("product.page", "GET", "/api/product?pageNumber=1&pageSize=5&sortBy=Id&sortOrder=asc");
  const byPrice = await call("product.page.sortPrice", "GET", "/api/product?pageNumber=1&pageSize=10&sortBy=UnitPrice&sortOrder=desc");
  const prices = (byPrice?.data?.items ?? []).map((x) => x.unitPrice);
  check("product.sort.priceDesc", prices.length > 1 && prices.every((v, i) => i === 0 || prices[i - 1] >= v), `prices=${prices.join(",")}`);
  await call("product.page.search", "GET", "/api/product?searchTerm=a&pageNumber=1&pageSize=3&sortBy=ProductName&sortOrder=asc");
  if (catId) await call("product.page.category", "GET", `/api/product?categoryId=${catId}&pageNumber=1&pageSize=3`);
  if (catId) await call("product.byCategory", "GET", `/api/product/category/${catId}`);
  await call("product.search", "GET", "/api/product/search?q=a");
  await call("product.featured", "GET", "/api/product/featured");
  await call("product.discounted", "GET", "/api/product/discounted");
  const items = page?.data?.items ?? [];
  check("product.page.meta", page?.data?.pageNumber === 1 && page?.data?.pageSize === 5 && typeof page?.data?.totalCount === "number" && Array.isArray(page?.data?.items),
    JSON.stringify(Object.keys(page?.data ?? {})));
  const product = items.find((x) => x.unitInStock > 10 && x.isActive) ?? items[0];
  const pid = product?.id;
  if (pid) {
    await call("product.get", "GET", `/api/product/${pid}`);
    await call("review.byProduct", "GET", `/api/review/product/${pid}`);
    await call("review.summary", "GET", `/api/review/product/${pid}/summary`);
  }
  await call("product.notFound", "GET", "/api/product/987654321", { expect: 404 });
  const camps = await call("campaign.list", "GET", "/api/campaign");
  if (camps?.data?.[0]) await call("campaign.get", "GET", `/api/campaign/${camps.data[0].id}`);
  await call("campaign.active", "GET", "/api/campaign/active");

  // ---- Adres & ödeme
  const addrBody = {
    title: "Ev",
    fullAddress: "Atatürk Cad. No:1",
    city: "İstanbul",
    district: "Kadıköy",
    postalCode: "34710",
    country: "Turkey",
    phoneNumber: "05550000000",
    isDefault: false,
  };
  const addr = await call("address.create", "POST", "/api/address", { token, body: addrBody, expect: 201 });
  const addrId = addr?.data?.id;
  check("address.firstIsDefault", addr?.data?.isDefault === true, `isDefault=${addr?.data?.isDefault}`);
  await call("address.create.invalid", "POST", "/api/address", { token, body: { title: "" }, expect: 400 });
  await call("address.byUser", "GET", `/api/address/user/${userId}`, { token });
  await call("address.byUser.forbidden", "GET", `/api/address/user/${Number(userId) + 1000000}`, { token, expect: 403 });
  if (addrId) {
    await call("address.get", "GET", `/api/address/${addrId}`, { token });
    await call("address.update", "PUT", `/api/address/${addrId}`, {
      token,
      body: { ...addrBody, title: "Ev (güncel)", isDefault: true },
    });
    await call("address.setDefault", "PUT", `/api/address/${addrId}/default`, { token });
  }
  await call("address.notFound", "GET", "/api/address/987654321", { token, expect: 404 });
  const pmBody = {
    type: "Card",
    cardHolderName: "Sozlesme Test",
    cardNumber: "4111 1111 1111 1111",
    expiryMonth: 12,
    expiryYear: new Date().getFullYear() + 3,
    cvv: "123",
    isDefault: true,
  };
  const pm = await call("payment.create", "POST", "/api/paymentmethod", { token, body: pmBody, expect: 201 });
  const pmId = pm?.data?.id;
  check("payment.masked", pm?.data?.cardNumber === "**** **** **** 1111", `cardNumber=${pm?.data?.cardNumber}`);
  await call("payment.create.badNumber", "POST", "/api/paymentmethod", { token, body: { ...pmBody, cardNumber: "12ab" }, expect: 400 });
  await call("payment.create.expired", "POST", "/api/paymentmethod", {
    token,
    body: { ...pmBody, expiryMonth: 1, expiryYear: new Date().getFullYear() - 1 },
    expect: 400,
  });
  await call("payment.byUser", "GET", `/api/paymentmethod/user/${userId}`, { token });
  await call("payment.byUser.forbidden", "GET", `/api/paymentmethod/user/${Number(userId) + 1000000}`, { token, expect: 403 });
  if (pmId) {
    await call("payment.get", "GET", `/api/paymentmethod/${pmId}`, { token });
    const upd = await call("payment.update", "PUT", `/api/paymentmethod/${pmId}`, {
      token,
      body: { ...pmBody, cardNumber: "**** **** **** 1111", cardHolderName: "Sozlesme Test 2" },
    });
    check("payment.update.keepsNumber", upd?.data?.cardNumber === "**** **** **** 1111", `cardNumber=${upd?.data?.cardNumber}`);
    await call("payment.setDefault", "PUT", `/api/paymentmethod/${pmId}/default`, { token });
  }

  // ---- Sepet
  await call("cart.empty", "GET", "/api/cart", { token });
  if (pid) {
    await call("cart.add", "POST", "/api/cart/add", { token, body: { productId: pid, quantity: 2 } });
    const cart = await call("cart.get", "GET", "/api/cart", { token });
    const line = cart?.data?.items?.[0];
    const expectedUnit = Math.round(product.unitPrice * (1 - product.discount / 100) * 100) / 100;
    check("cart.discountedPrice", line && Math.abs(line.unitPrice - expectedUnit) < 0.011, `unitPrice=${line?.unitPrice} beklenen=${expectedUnit}`);
    const sub = cart?.data?.totalAmount ?? 0;
    const expShip = sub >= 150 ? 0 : 34.99;
    check("cart.shipping", cart?.data && Math.abs(cart.data.shippingFee - expShip) < 0.001 && Math.abs(cart.data.grandTotal - (sub + expShip)) < 0.011,
      `sub=${sub} ship=${cart?.data?.shippingFee} grand=${cart?.data?.grandTotal}`);
    const count = await call("cart.count", "GET", "/api/cart/count", { token });
    check("cart.countValue", count?.data === 2, `count=${count?.data}`);
    await call("cart.total", "GET", "/api/cart/total", { token });
    await call("cart.update", "PUT", "/api/cart/update", { token, body: { productId: pid, quantity: 1 } });
    await call("cart.update.tooMany", "PUT", "/api/cart/update", { token, body: { productId: pid, quantity: 1000000 }, expect: 400 });
    await call("cart.remove", "DELETE", `/api/cart/remove/${pid}`, { token });
    await call("cart.update.notInCart", "PUT", "/api/cart/update", { token, body: { productId: pid, quantity: 1 }, expect: 400 });
    await call("cart.add.invalidQty", "POST", "/api/cart/add", { token, body: { productId: pid, quantity: 0 }, expect: 400 });
    await call("cart.add.unknownProduct", "POST", "/api/cart/add", { token, body: { productId: 987654321, quantity: 1 }, expect: 400 });
  }

  // ---- Sipariş
  let orderId;
  if (pid && addrId && pmId) {
    await call("cart.add.forOrder", "POST", "/api/cart/add", { token, body: { productId: pid, quantity: 1 } });
    await call("order.mismatch", "POST", "/api/order", {
      token,
      body: { shippingAddressId: addrId, paymentMethodId: pmId, items: [{ productId: pid, quantity: 3 }] },
      expect: 400,
    });
    await call("order.invalidAddress", "POST", "/api/order", {
      token,
      body: { shippingAddressId: 987654321, paymentMethodId: pmId, items: [{ productId: pid, quantity: 1 }] },
      expect: 400,
    });
    await call("order.badIdempotencyKey", "POST", "/api/order", {
      token,
      headers: { "Idempotency-Key": "short" },
      body: { shippingAddressId: addrId, paymentMethodId: pmId, items: [{ productId: pid, quantity: 1 }] },
      expect: 400,
    });
    const key = `contract-${stamp}`;
    const orderBody = { shippingAddressId: addrId, paymentMethodId: pmId, items: [{ productId: pid, quantity: 1 }], notes: "test" };
    const order = await call("order.create", "POST", "/api/order", { token, body: orderBody, headers: { "Idempotency-Key": key }, expect: 201 });
    orderId = order?.data?.id;
    check("order.totals", order?.data && Math.abs(order.data.totalAmount - (order.data.subtotalAmount + order.data.shippingFee)) < 0.011,
      `total=${order?.data?.totalAmount} sub=${order?.data?.subtotalAmount} ship=${order?.data?.shippingFee}`);
    check("order.paymentMasked", order?.data?.paymentMethod?.cardNumber === "**** **** **** 1111", `cardNumber=${order?.data?.paymentMethod?.cardNumber}`);
    check("order.addressFull", order?.data?.shippingAddress?.country === "Turkey" && order?.data?.shippingAddress?.userId === userId,
      JSON.stringify(order?.data?.shippingAddress));
    const replay = await call("order.idempotentReplay", "POST", "/api/order", { token, body: orderBody, headers: { "Idempotency-Key": key }, expect: 200 });
    check("order.idempotentSameId", replay?.data?.id === orderId, `${replay?.data?.id} vs ${orderId}`);
    const afterCart = await call("cart.afterOrder", "GET", "/api/cart", { token });
    check("cart.clearedAfterOrder", afterCart?.data?.items?.length === 0, `items=${afterCart?.data?.items?.length}`);
    await call("order.list", "GET", "/api/order", { token });
    if (orderId) {
      await call("order.get", "GET", `/api/order/${orderId}`, { token });
      await call("order.get.otherUser", "GET", `/api/order/${orderId}`, { token: admin, expect: 404 });
      await call("order.return.notDelivered", "POST", `/api/order/${orderId}/return-request`, { token, body: { reason: "Beğenmedim" }, expect: 400 });
      for (const step of ["processing", "shipped", "delivered"]) {
        await call(`order.demoAdvance.${step}`, "POST", `/api/order/${orderId}/demo/advance-fulfillment`, { token });
      }
      await call("order.demoAdvance.done", "POST", `/api/order/${orderId}/demo/advance-fulfillment`, { token, expect: 400 });
      await call("order.cancel.delivered", "PUT", `/api/order/${orderId}/cancel`, { token, expect: 400 });
      const ret = await call("order.return", "POST", `/api/order/${orderId}/return-request`, { token, body: { reason: "Beden olmadı" } });
      check("order.returnState", ret?.data?.status === "ReturnRequested" && !!ret?.data?.returnRequestedAt && !!ret?.data?.trackingNumber,
        `status=${ret?.data?.status}`);
    }
    // ikinci sipariş: iptal
    await call("cart.add.forOrder2", "POST", "/api/cart/add", { token, body: { productId: pid, quantity: 1 } });
    const order2 = await call("order.create2", "POST", "/api/order", {
      token,
      body: { shippingAddressId: addrId, paymentMethodId: pmId, items: [{ productId: pid, quantity: 1 }] },
      expect: 201,
    });
    const o2 = order2?.data?.id;
    if (o2) {
      const before = (await call("product.beforeCancel", "GET", `/api/product/${pid}`))?.data?.unitInStock;
      await call("order.cancel", "PUT", `/api/order/${o2}/cancel`, { token, body: { reason: "Vazgeçtim" } });
      const after = (await call("product.afterCancel", "GET", `/api/product/${pid}`))?.data?.unitInStock;
      check("order.cancelRestoresStock", after === before + 1, `${before} → ${after}`);
      await call("order.cancel.again", "PUT", `/api/order/${o2}/cancel`, { token, expect: 400 });
    }
    await call("order.emptyItems", "POST", "/api/order", {
      token,
      body: { shippingAddressId: addrId, paymentMethodId: pmId, items: [] },
      expect: 400,
    });
  }
  await call("cart.clear", "DELETE", "/api/cart/clear", { token });

  // ---- Favoriler & yorum
  let reviewId;
  if (pid) {
    const fav = await call("favorite.add", "POST", "/api/favorite/add", { token, body: { productId: pid } });
    check("favorite.productInfo", !!fav?.data?.productName && typeof fav?.data?.productPrice === "number", JSON.stringify(fav?.data));
    await call("favorite.add.duplicate", "POST", "/api/favorite/add", { token, body: { productId: pid }, expect: 400 });
    const chk = await call("favorite.check", "GET", `/api/favorite/check/${pid}`, { token });
    check("favorite.checkTrue", chk?.data === true, `data=${chk?.data}`);
    await call("favorite.list", "GET", "/api/favorite", { token });
    await call("favorite.remove", "DELETE", `/api/favorite/remove/${pid}`, { token });
    await call("favorite.remove.missing", "DELETE", `/api/favorite/remove/${pid}`, { token, expect: 400 });
    await call("favorite.clear", "DELETE", "/api/favorite/clear", { token });
    const rev = await call("review.create", "POST", "/api/review", {
      token,
      body: { productId: pid, rating: 5, title: "Harika", comment: "Sözleşme testi yorumu" },
      expect: 201,
    });
    reviewId = rev?.data?.id;
    check("review.verifiedPurchase", rev?.data?.isVerified === true && !!rev?.data?.userName && !!rev?.data?.productName, JSON.stringify(rev?.data));
    await call("review.create.duplicate", "POST", "/api/review", { token, body: { productId: pid, rating: 4 }, expect: 400 });
    await call("review.create.invalid", "POST", "/api/review", { token, body: { productId: pid, rating: 9 }, expect: 400 });
    if (reviewId) {
      await call("review.get", "GET", `/api/review/${reviewId}`);
      await call("review.update", "PUT", `/api/review/${reviewId}`, { token, body: { rating: 4 } });
    }
    const prod = await call("product.withRating", "GET", `/api/product/${pid}`);
    check("product.ratingFields", typeof prod?.data?.averageRating === "number" && prod?.data?.totalReviews >= 1, `avg=${prod?.data?.averageRating} total=${prod?.data?.totalReviews}`);
  }

  // ---- Bildirimler
  const sum = await call("notification.summary", "GET", "/api/notification/summary", { token });
  check("notification.orderNotifications", (sum?.data?.totalNotifications ?? 0) >= 1, `total=${sum?.data?.totalNotifications}`);
  await call("notification.byUser", "GET", `/api/notification/user/${userId}?pageNumber=1&pageSize=50`, { token });
  await call("notification.byUser.forbidden", "GET", `/api/notification/user/${Number(userId) + 1000000}`, { token, expect: 403 });
  const nid = sum?.data?.recentNotifications?.[0]?.id;
  if (nid) {
    await call("notification.get", "GET", `/api/notification/${nid}`, { token });
    const tr = await call("notification.toggleRead", "PUT", `/api/notification/${nid}`, { token, body: { isRead: true } });
    check("notification.readAt", tr?.data?.isRead === true && !!tr?.data?.readAt, JSON.stringify(tr?.data));
  }
  await call("notification.markAllRead", "PUT", "/api/notification/mark-all-read", { token });

  // ---- Güvenlik & ayarlar
  await call("settings.user.updateFirst", "PUT", "/api/settings/user", {
    token,
    body: { language: "en", theme: "dark" },
  });
  const us = await call("settings.user", "GET", "/api/settings/user", { token });
  check("settings.user.persisted", us?.data?.language === "en" && us?.data?.theme === "dark" && us?.data?.currency === "TRY", JSON.stringify(us?.data));
  await call("settings.privacy", "GET", "/api/settings/privacy", { token });
  await call("settings.privacy.update", "PUT", "/api/settings/privacy", {
    token,
    body: { profileVisibility: false, allowAnalytics: false },
  });
  const exp = await call("settings.export", "GET", "/api/settings/export", { token });
  let exported = null;
  try {
    exported = JSON.parse(exp?.data ?? "null");
  } catch {}
  check("settings.export.camelCase", exported?.userSettings?.language === "en" && exported?.privacySettings?.profileVisibility === false, String(exp?.data).slice(0, 120));
  await call("settings.reset", "POST", "/api/settings/reset", { token });
  if (typeof exp?.data === "string") {
    await call("settings.import", "POST", "/api/settings/import", { token, body: exp.data });
    const again = await call("settings.user.afterImport", "GET", "/api/settings/user", { token });
    check("settings.import.applied", again?.data?.language === "en", `language=${again?.data?.language}`);
  }
  await call("settings.import.invalid", "POST", "/api/settings/import", { token, body: "not json", expect: 400 });

  await call("security.info", "GET", "/api/security/info", { token });
  const hist = await call("security.loginHistory", "GET", "/api/security/login-history", { token });
  check("security.loginRecorded", (hist?.data?.length ?? 0) >= 2, `entries=${hist?.data?.length}`);
  const sec = await call("security.settings", "GET", "/api/security/settings", { token });
  if (sec?.data) {
    await call("security.settings.update", "PUT", "/api/security/settings", { token, body: { ...sec.data, sessionTimeout: 45 } });
    const sec2 = await call("security.settings.persisted", "GET", "/api/security/settings", { token });
    check("security.settings.saved", sec2?.data?.sessionTimeout === 45, `sessionTimeout=${sec2?.data?.sessionTimeout}`);
  }
  await call("security.changePassword.wrong", "POST", "/api/security/change-password", {
    token,
    body: { currentPassword: "nope", newPassword: "NewPass123!", confirmPassword: "NewPass123!" },
    expect: 400,
  });
  await call("security.changePassword.mismatch", "POST", "/api/security/change-password", {
    token,
    body: { currentPassword: password, newPassword: "NewPass123!", confirmPassword: "Other123!" },
    expect: 400,
  });
  await call("security.changePassword", "POST", "/api/security/change-password", {
    token,
    body: { currentPassword: password, newPassword: "NewPass123!", confirmPassword: "NewPass123!" },
  });
  const newEmail = `contract.${stamp}.new@example.com`;
  await call("security.updateEmail.taken", "POST", "/api/security/update-email", {
    token,
    body: { newEmail: "admin@example.com", currentPassword: "NewPass123!" },
    expect: 400,
  });
  await call("security.updateEmail", "POST", "/api/security/update-email", {
    token,
    body: { newEmail, currentPassword: "NewPass123!" },
  });
  const relog = await call("auth.login.newEmail", "POST", "/api/auth/login", { body: { email: newEmail, password: "NewPass123!" } });
  const token2 = relog?.data?.token;
  await call("security.logoutAll", "POST", "/api/security/logout-all-devices", { token });
  if (token2) await call("security.logoutAll.otherRevoked", "GET", "/api/cart", { token: token2, expect: 401 });
  await call("security.logoutAll.callerStillValid", "GET", "/api/cart", { token });

  // ---- Yardım & destek
  await call("help.faqs", "GET", "/api/helpsupport/faqs?pageSize=30");
  const arts = await call("help.articles", "GET", "/api/helpsupport/articles?pageSize=20");
  const artId = arts?.data?.[0]?.id;
  if (artId) await call("help.article.get", "GET", `/api/helpsupport/articles/${artId}`);
  await call("help.contact", "POST", "/api/helpsupport/contact", {
    body: { name: "Test", email: "t@example.com", phone: "", subject: "Konu", message: "Mesaj içeriği", category: "General" },
  });
  await call("help.contact.invalid", "POST", "/api/helpsupport/contact", { body: { name: "" }, expect: 400 });
  await call("help.ticket.create", "POST", "/api/helpsupport/tickets", {
    token,
    body: { subject: "Kargo", description: "Siparişim nerede?", category: "General", priority: "Medium" },
    expect: 201,
  });
  await call("help.ticket.list", "GET", "/api/helpsupport/tickets", { token });

  // ---- Yönetim
  if (admin) {
    await call("admin.orders", "GET", "/api/order/admin?pageNumber=1&pageSize=100", { token: admin });
    if (orderId) {
      await call("admin.order.status", "PUT", `/api/order/${orderId}/status`, { token: admin, body: { status: "returned", notes: "" } });
      await call("admin.order.status.invalid", "PUT", `/api/order/${orderId}/status`, { token: admin, body: { status: "Teleported" }, expect: 400 });
    }
    await call("admin.order.status.notFound", "PUT", "/api/order/987654321/status", { token: admin, body: { status: "Shipped" }, expect: 404 });
    await call("admin.reviews", "GET", "/api/review/admin?pageNumber=1&pageSize=30", { token: admin });
    await call("admin.tickets", "GET", "/api/helpsupport/tickets/admin?pageNumber=1&pageSize=15", { token: admin });
    await call("admin.notification.create", "POST", "/api/notification", {
      token: admin,
      body: { userId, title: "Duyuru", message: "Sözleşme testi", type: "Info" },
      expect: 201,
    });
    await call("admin.notification.create.noUser", "POST", "/api/notification", {
      token: admin,
      body: { userId: 987654321, title: "Duyuru", message: "x", type: "Info" },
      expect: 400,
    });
    await call("admin.notification.byUser", "GET", `/api/notification/admin/user/${userId}?pageSize=50`, { token: admin });
    await call("admin.article.create", "POST", "/api/helpsupport/articles", {
      token: admin,
      body: { title: "Test makale", content: "İçerik", category: "Genel", tags: ["test", "kargo"], isPublished: true },
      expect: 201,
    });

    const now = Date.now();
    const campBody = {
      title: "Sözleşme kampanya",
      subtitle: "",
      description: "",
      discount: 10,
      imageUrl: "",
      backgroundColor: "#fde68a",
      timeLeft: "",
      buttonText: "Ürünlere git",
      buttonHref: "/products",
      startDate: new Date(now).toISOString(),
      endDate: new Date(now + 7 * 864e5).toISOString(),
      isActive: true,
    };
    await call("admin.campaign.create.badDates", "POST", "/api/campaign", {
      token: admin,
      body: { ...campBody, endDate: new Date(now - 864e5).toISOString() },
      expect: 400,
    });
    const camp = await call("admin.campaign.create", "POST", "/api/campaign", { token: admin, body: campBody, expect: 201 });
    const campId = camp?.data?.id;
    if (campId) {
      await call("admin.campaign.update", "PUT", `/api/campaign/${campId}`, { token: admin, body: { ...campBody, title: "Sözleşme kampanya 2" } });
      await call("admin.campaign.delete", "DELETE", `/api/campaign/${campId}`, { token: admin });
    }

    const catName = `Test Kategori ${stamp}`;
    const cat = await call("admin.category.create", "POST", "/api/category", {
      token: admin,
      body: { categoryName: catName, description: "x", isActive: true },
      expect: 201,
    });
    await call("admin.category.create.duplicate", "POST", "/api/category", {
      token: admin,
      body: { categoryName: catName.toLowerCase() },
      expect: 400,
    });
    const newCatId = cat?.data?.id;
    if (newCatId) {
      await call("admin.category.update", "PUT", `/api/category/${newCatId}`, {
        token: admin,
        body: { categoryName: catName, description: "y", imageUrl: "" },
      });
      const sub = await call("admin.subcategory.create", "POST", "/api/subcategory", {
        token: admin,
        body: { subCategoryName: "Test Alt", description: "x", categoryId: newCatId },
        expect: 201,
      });
      const subId = sub?.data?.id;
      if (subId) {
        await call("admin.subcategory.update", "PUT", `/api/subcategory/${subId}`, {
          token: admin,
          body: { id: subId, subCategoryName: "Test Alt 2", description: "", imageUrl: "", categoryId: newCatId, isActive: false },
        });
        await call("admin.subcategory.update.idMismatch", "PUT", `/api/subcategory/${subId}`, {
          token: admin,
          body: { id: subId + 1, subCategoryName: "x", categoryId: newCatId },
          expect: 400,
        });
        await call("admin.subcategory.delete", "DELETE", `/api/subcategory/${subId}`, { token: admin });
      }
      const prodBody = {
        productName: "Test Ürün",
        unitPrice: 99.9,
        unitInStock: 10,
        quantityPerUnit: "1 adet",
        categoryId: newCatId,
        description: "x",
        discount: 5,
        isActive: true,
      };
      await call("admin.product.create.invalid", "POST", "/api/product", { token: admin, body: { ...prodBody, unitPrice: 0 }, expect: 400 });
      await call("admin.product.create.noCategory", "POST", "/api/product", { token: admin, body: { ...prodBody, categoryId: 987654321 }, expect: 400 });
      const prod = await call("admin.product.create", "POST", "/api/product", { token: admin, body: prodBody, expect: 201 });
      const newPid = prod?.data?.id;
      if (newPid) {
        await call("admin.product.update", "PUT", `/api/product/${newPid}`, {
          token: admin,
          body: { ...prod.data, productName: "Test Ürün 2" },
        });
        await call("admin.product.delete", "DELETE", `/api/product/${newPid}`, { token: admin });
        await call("admin.product.deleted", "GET", `/api/product/${newPid}`, { expect: 404 });
      }
      await call("admin.category.delete", "DELETE", `/api/category/${newCatId}`, { token: admin });
    }
    if (reviewId) await call("admin.review.delete", "DELETE", `/api/review/${reviewId}`, { token: admin });
  }

  await call("auth.logout", "POST", "/api/auth/logout", { token });

  writeFileSync(outFile, JSON.stringify({ base, results }, null, 2));
  const failed = results.filter((r) => r.problems?.length);
  console.log(`\n${results.length} kontrol, ${failed.length} sözleşme ihlali. Çıktı: ${outFile}`);
  process.exitCode = failed.length ? 1 : 0;
}

function diffShapes(a, b, path, out) {
  if (a === "null" || b === "null") return;
  if (Array.isArray(a) || Array.isArray(b)) {
    if (!Array.isArray(a) || !Array.isArray(b)) return void out.push(`${path}: ${JSON.stringify(a)} ≠ ${JSON.stringify(b)}`);
    if (a[0] === "empty" || b[0] === "empty") return;
    return diffShapes(a[0], b[0], `${path}[]`, out);
  }
  if (typeof a === "object" && typeof b === "object") {
    for (const k of new Set([...Object.keys(a), ...Object.keys(b)])) {
      if (!(k in a)) out.push(`${path}.${k}: yalnızca B'de`);
      else if (!(k in b)) out.push(`${path}.${k}: yalnızca A'da`);
      else diffShapes(a[k], b[k], `${path}.${k}`, out);
    }
    return;
  }
  if (a !== b) out.push(`${path}: ${JSON.stringify(a)} ≠ ${JSON.stringify(b)}`);
}

// Mesajlardaki değişken kısımları (sayılar, iki noktadan sonrası) karşılaştırmadan çıkarır.
const normMsg = (m) => (m ?? "").replace(/:.*$/, ":…").replace(/\d+/g, "#").trim();

function compare(fileA, fileB) {
  const A = JSON.parse(readFileSync(fileA, "utf8"));
  const B = JSON.parse(readFileSync(fileB, "utf8"));
  const byName = (rs) => new Map(rs.map((r) => [r.name, r]));
  const a = byName(A.results);
  const b = byName(B.results);
  let problems = 0;
  for (const name of new Set([...a.keys(), ...b.keys()])) {
    const x = a.get(name);
    const y = b.get(name);
    const lines = [];
    if (!x || !y) lines.push(`yalnızca ${x ? "A" : "B"} tarafında çalıştı`);
    else {
      if (x.status !== y.status) lines.push(`HTTP ${x.status} ≠ ${y.status}`);
      if (x.success !== y.success) lines.push(`success ${x.success} ≠ ${y.success}`);
      if ((x.errorCode ?? null) !== (y.errorCode ?? null)) lines.push(`errorCode ${x.errorCode} ≠ ${y.errorCode}`);
      if (x.message !== undefined && y.message !== undefined && normMsg(x.message) !== normMsg(y.message))
        lines.push(`message "${x.message}" ≠ "${y.message}"`);
      if (x.shape !== "check") diffShapes(x.shape, y.shape, "$", lines);
    }
    if (lines.length) {
      problems++;
      console.log(`✗ ${name} (${(x ?? y).method ?? ""} ${(x ?? y).path ?? ""})`);
      for (const l of lines) console.log(`    ${l}`);
    }
  }
  console.log(problems ? `\n${problems} uçta fark var.` : "\nİki backend sözleşme açısından aynı davranıyor.");
  process.exitCode = problems ? 1 : 0;
}

if (mode === "run" && args[0]) await run(args[0], args[1] ?? "contract-result.json");
else if (mode === "compare" && args.length === 2) compare(args[0], args[1]);
else {
  console.log("Kullanım:\n  node contract-smoke.mjs run <baseUrl> [out.json]\n  node contract-smoke.mjs compare <a.json> <b.json>");
  process.exitCode = 2;
}
