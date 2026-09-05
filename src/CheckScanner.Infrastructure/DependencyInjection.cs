using CheckScanner.Application.Interfaces;
using CheckScanner.Infrastructure.Options;
using CheckScanner.Infrastructure.Persistence;
using CheckScanner.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CheckScanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCheckScannerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = OrDefault(
            configuration.GetConnectionString("CheckScannerDb"),
            "Host=localhost;Database=checkscanner;Timeout=2");
        services.AddSingleton(NpgsqlDataSource.Create(connectionString));

        services.AddScoped<IReceiptRepository, ReceiptRepository>();

        services.Configure<PhotoStorageOptions>(configuration.GetSection(PhotoStorageOptions.SectionName));
        services.AddSingleton<IPhotoStore, FileSystemPhotoStore>();

        return services;
    }

    /// <summary>
    /// Applies the embedded schema best-effort: an unreachable Postgres at
    /// startup is logged, not fatal, so a transient outage doesn't take the
    /// process down -- the Receipts/Upload pages surface the resulting
    /// failure on the first real query instead.
    /// </summary>
    public static async Task EnsureCheckScannerSchemaBestEffortAsync(
        this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        try
        {
            var dataSource = services.GetRequiredService<NpgsqlDataSource>();
            await SchemaInitializer.EnsureSchemaAsync(dataSource, cancellationToken);
        }
        catch (Exception ex)
        {
            services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("CheckScanner.Infrastructure.Persistence")
                .LogWarning(ex, "Failed to apply CheckScanner database schema at startup.");
        }
    }

    private static string OrDefault(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
