using CheckScanner.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CheckScanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCheckScannerApplication(this IServiceCollection services)
    {
        services.AddScoped<ReceiptUploadService>();
        services.AddScoped<ReceiptReviewService>();
        services.AddScoped<PriceHistoryService>();
        services.AddScoped<SpendingTrendsService>();
        return services;
    }
}
