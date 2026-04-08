using System.Runtime.CompilerServices;

namespace EcommerceBackend.IntegrationTests;

/// <summary>
/// Ryuk (resource reaper) bazı ortamlarda (Docker içinde test, dar zaman aşımı) başlatılamayabiliyor.
/// Devre dışı bırakmak konteyner temizliğini azaltır; testler yerel/Docker masaüstünde sorunsuz çalışır.
/// </summary>
internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Init()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED")))
            Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
    }
}
