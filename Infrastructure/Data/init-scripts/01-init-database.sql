-- Veritabanı başlatma script'i
-- Bu script PostgreSQL container başlatıldığında otomatik olarak çalışır

-- Veritabanı oluştur (eğer yoksa)
-- PostgreSQL'de IF NOT EXISTS CREATE DATABASE syntax'ı farklı
SELECT 'CREATE DATABASE ecommerce_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ecommerce_db')\gexec

-- Kullanıcı oluştur (eğer yoksa)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'ecommerce_user') THEN
        CREATE ROLE ecommerce_user WITH LOGIN PASSWORD 'ecommerce_password';
    END IF;
END
$$;

-- Veritabanına erişim izni ver
GRANT ALL PRIVILEGES ON DATABASE ecommerce_db TO ecommerce_user;

-- Bağlantıyı yeni veritabanına değiştir
\c ecommerce_db;

-- Schema oluştur
CREATE SCHEMA IF NOT EXISTS public;

-- Kullanıcıya schema üzerinde izin ver
GRANT ALL ON SCHEMA public TO ecommerce_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ecommerce_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO ecommerce_user;

-- Varsayılan izinleri ayarla
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO ecommerce_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO ecommerce_user;
