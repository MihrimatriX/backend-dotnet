// Vitrin (temp-shop-net) ekran görüntüleri.
//   node shop-shot.mjs <apiBase> <outDir> <prefix>
// Sandbox ağı dış resim sunucularını engellediği için dış görseller yer tutucu SVG ile değiştirilir.
import { createRequire } from "node:module";
import { mkdirSync } from "node:fs";
const require = createRequire("/opt/node22/lib/node_modules/");
const { chromium } = require("playwright");

const [, , apiBase, outDir, prefix] = process.argv;
mkdirSync(outDir, { recursive: true });
const SHOP = "http://localhost:3000";

const palette = ["#fde2e4", "#e2ece9", "#dfe7fd", "#fff1e6", "#e9edc9", "#f0efeb", "#e8e8f8", "#fae1dd"];
function placeholder(url) {
  let h = 0;
  for (const c of url) h = (h * 31 + c.charCodeAt(0)) >>> 0;
  const bg = palette[h % palette.length];
  const fg = palette[(h >> 3) % palette.length];
  return `<svg xmlns="http://www.w3.org/2000/svg" width="400" height="400" viewBox="0 0 400 400">
  <rect width="400" height="400" fill="${bg}"/>
  <circle cx="${120 + (h % 160)}" cy="${140 + ((h >> 5) % 120)}" r="${70 + (h % 40)}" fill="${fg}" opacity=".9"/>
  <rect x="${90 + ((h >> 2) % 120)}" y="${200 + ((h >> 7) % 60)}" width="150" height="110" rx="18" fill="#ffffff" opacity=".55"/>
  </svg>`;
}

async function login() {
  const res = await fetch(`${apiBase}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: "user1@example.com", password: "user123" }),
  });
  return (await res.json()).data.token;
}

const browser = await chromium.launch();
async function newPage(token) {
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1.25 });
  await ctx.route(/^https?:\/\/(?!localhost|127\.0\.0\.1)/, (route) => {
    if (route.request().resourceType() === "image") {
      return route.fulfill({ status: 200, contentType: "image/svg+xml", body: placeholder(route.request().url()) });
    }
    return route.abort();
  });
  if (token) await ctx.addInitScript((t) => localStorage.setItem("temp-shop-token", t), token);
  return ctx.newPage();
}

async function shot(page, path, file) {
  await page.goto(SHOP + path, { waitUntil: "networkidle" });
  await page.waitForTimeout(2000);
  await page.screenshot({ path: `${outDir}/${prefix}-${file}` });
  console.log("✓", `${prefix}-${file}`);
}

const anon = await newPage();
await shot(anon, "/", "home.png");
await shot(anon, "/products?sortBy=UnitPrice&sortOrder=desc", "products.png");

const token = await login();
const products = await (await fetch(`${apiBase}/api/product?pageSize=10&sortBy=UnitPrice&sortOrder=desc`)).json();
const p = products.data.items.find((x) => x.unitInStock > 3) ?? products.data.items[0];
const auth = { "Content-Type": "application/json", Authorization: `Bearer ${token}` };
await fetch(`${apiBase}/api/cart/clear`, { method: "DELETE", headers: auth });
await fetch(`${apiBase}/api/cart/add`, { method: "POST", headers: auth, body: JSON.stringify({ productId: p.id, quantity: 1 }) });
const user = await newPage(token);
await shot(user, `/products/${p.id}`, "product.png");
await shot(user, "/cart", "cart.png");
await fetch(`${apiBase}/api/cart/clear`, { method: "DELETE", headers: auth });
await browser.close();
