// README ekran görüntüleri: Swagger UI + örnek API cevapları + sözleşme testi çıktısı.
//   node capture.mjs <backend:dotnet|spring> <baseUrl> <outDir> [compareOutputFile]
import { createRequire } from "node:module";
import { mkdirSync, readFileSync, existsSync } from "node:fs";
const require = createRequire("/opt/node22/lib/node_modules/");
const { chromium } = require("playwright");

const [, , backend, base, outDir, compareFile] = process.argv;
mkdirSync(outDir, { recursive: true });

const theme = backend === "dotnet"
  ? { name: ".NET 9", accent: "#512bd4", soft: "#ede9fe" }
  : { name: "Spring Boot 3.4", accent: "#3f8f29", soft: "#e8f5e2" };

const esc = (s) => s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

function highlight(json) {
  return esc(json).replace(
    /("(?:\\.|[^"\\])*")(\s*:)?|\b(true|false|null)\b|(-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)/g,
    (m, str, colon, lit, num) => {
      if (str) return colon ? `<span class="k">${str}</span>${colon}` : `<span class="s">${str}</span>`;
      if (lit) return `<span class="l">${lit}</span>`;
      return `<span class="n">${num}</span>`;
    },
  );
}

function trimJson(value, depth = 0) {
  if (Array.isArray(value)) {
    const head = value.slice(0, depth === 0 ? 3 : 2).map((v) => trimJson(v, depth + 1));
    if (value.length > head.length) head.push(`… ${value.length - head.length} öğe daha`);
    return head;
  }
  if (value && typeof value === "object") {
    const o = {};
    for (const [k, v] of Object.entries(value)) {
      o[k] = k === "token" && typeof v === "string" ? v.slice(0, 36) + "…" : trimJson(v, depth + 1);
    }
    return o;
  }
  return value;
}

function card({ method, path, status, body, note }) {
  const ok = status >= 200 && status < 300;
  return `<!doctype html><html><head><meta charset="utf-8"><style>
  :root{--accent:${theme.accent};--soft:${theme.soft}}
  *{box-sizing:border-box} body{margin:0;padding:28px;background:#f4f5f7;font:15px/1.5 -apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#1f2328}
  .card{background:#fff;border:1px solid #d0d7de;border-radius:12px;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,.06);width:940px}
  .bar{display:flex;align-items:center;gap:12px;padding:14px 18px;border-bottom:1px solid #d0d7de;background:#fafbfc}
  .m{font:700 13px ui-monospace,SFMono-Regular,Menlo,monospace;padding:3px 9px;border-radius:6px;background:var(--accent);color:#fff}
  .p{font:600 14px ui-monospace,SFMono-Regular,Menlo,monospace;flex:1;word-break:break-all}
  .st{font:700 13px ui-monospace,monospace;padding:3px 9px;border-radius:6px;background:${ok ? "#dafbe1" : "#ffebe9"};color:${ok ? "#1a7f37" : "#cf222e"}}
  .be{font-size:12px;font-weight:600;padding:3px 9px;border-radius:999px;background:var(--soft);color:var(--accent)}
  pre{margin:0;padding:18px 20px;font:13px/1.55 ui-monospace,SFMono-Regular,Menlo,monospace;white-space:pre-wrap;word-break:break-word}
  .k{color:#0550ae}.s{color:#0a3069}.n{color:#953800}.l{color:#8250df}
  .note{padding:10px 18px;border-top:1px solid #d0d7de;background:#fafbfc;color:#57606a;font-size:13px}
  </style></head><body><div class="card"><div class="bar"><span class="m">${method}</span><span class="p">${esc(path)}</span>
  <span class="st">${status}</span><span class="be">${theme.name}</span></div>
  <pre>${highlight(JSON.stringify(trimJson(body), null, 2))}</pre>${note ? `<div class="note">${esc(note)}</div>` : ""}</div></body></html>`;
}

function terminal(title, text) {
  const colored = esc(text)
    .replace(/^✓.*$/gm, (l) => `<span style="color:#3fb950">${l}</span>`)
    .replace(/^✗.*$/gm, (l) => `<span style="color:#f85149">${l}</span>`);
  return `<!doctype html><html><head><meta charset="utf-8"><style>
  body{margin:0;padding:28px;background:#f4f5f7;font-family:-apple-system,Segoe UI,Roboto,sans-serif}
  .t{width:940px;background:#0d1117;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.2)}
  .h{display:flex;gap:8px;align-items:center;padding:12px 16px;background:#161b22;color:#8b949e;font-size:13px}
  .d{width:12px;height:12px;border-radius:50%}
  pre{margin:0;padding:18px 20px;color:#c9d1d9;font:13px/1.6 ui-monospace,SFMono-Regular,Menlo,monospace;white-space:pre-wrap}
  </style></head><body><div class="t"><div class="h"><span class="d" style="background:#ff5f56"></span><span class="d" style="background:#ffbd2e"></span><span class="d" style="background:#27c93f"></span><span style="margin-left:8px">${esc(title)}</span></div><pre>${colored}</pre></div></body></html>`;
}

async function api(method, path, { token, body } = {}) {
  const headers = { Accept: "application/json" };
  if (body) headers["Content-Type"] = "application/json";
  if (token) headers.Authorization = `Bearer ${token}`;
  const res = await fetch(base + path, { method, headers, body: body ? JSON.stringify(body) : undefined });
  const text = await res.text();
  let json;
  try { json = JSON.parse(text); } catch { json = text; }
  return { status: res.status, body: json };
}

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1000, height: 800 }, deviceScaleFactor: 2 });

async function shotHtml(file, html) {
  await page.setContent(html, { waitUntil: "load" });
  const el = await page.$(".card, .t");
  await el.screenshot({ path: `${outDir}/${file}` });
  console.log("✓", file);
}

// 1) Swagger UI
const swaggerUrl = backend === "dotnet" ? `${base}/swagger` : `${base}/swagger-ui/index.html`;
const sw = await browser.newPage({ viewport: { width: 1280, height: 900 }, deviceScaleFactor: 1.5 });
await sw.goto(swaggerUrl, { waitUntil: "networkidle" });
await sw.waitForTimeout(1500);
await sw.screenshot({ path: `${outDir}/swagger.png` });
console.log("✓ swagger.png");

// 2) Örnek cevaplar
const login = await api("POST", "/api/auth/login", { body: { email: "user1@example.com", password: "user123" } });
await shotHtml("login.png", card({ method: "POST", path: "/api/auth/login", ...login, note: "JWT claim'leri: sub (kullanıcı id), email, role, jti, iat, exp, iss, aud — iki backend'de aynı" }));
const token = login.body?.data?.token;

const products = await api("GET", "/api/product?pageNumber=1&pageSize=2&sortBy=UnitPrice&sortOrder=desc");
await shotHtml("products.png", card({ method: "GET", path: "/api/product?pageNumber=1&pageSize=2&sortBy=UnitPrice&sortOrder=desc", ...products }));

const pid = products.body?.data?.items?.find((p) => p.unitInStock > 3)?.id ?? products.body?.data?.items?.[0]?.id;
await api("DELETE", "/api/cart/clear", { token });
if (pid) await api("POST", "/api/cart/add", { token, body: { productId: pid, quantity: 1 } });
const cart = await api("GET", "/api/cart", { token });
await shotHtml("cart.png", card({ method: "GET", path: "/api/cart", ...cart, note: "İndirimli birim fiyat, 150 TL altı 34,99 TL kargo — sözleşme §5.1" }));
await api("DELETE", "/api/cart/clear", { token });

const unauth = await api("GET", "/api/order");
await shotHtml("error-401.png", card({ method: "GET", path: "/api/order (token yok)", ...unauth, note: "Tüm hatalar aynı zarf: success=false, message, errorCode, traceId (= X-Correlation-Id)" }));

const validation = await api("POST", "/api/auth/register", { body: { email: "gecersiz", password: "1" } });
await shotHtml("error-validation.png", card({ method: "POST", path: "/api/auth/register", ...validation }));

// 3) Sözleşme karşılaştırması
if (compareFile && existsSync(compareFile)) {
  await shotHtml("contract-compare.png", terminal("node scripts/contract-smoke.mjs compare out-dotnet.json out-spring.json", readFileSync(compareFile, "utf8")));
}

await browser.close();
