using CheckScanner.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CheckScanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCheckScannerApplication(this IServiceCollection services)
    {
        services.AddScoped<ReceiptUploadService>();
        return services;
    }
}
