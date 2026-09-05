using CheckScanner.Application;
using CheckScanner.Application.Interfaces;
using CheckScanner.Infrastructure;
using CheckScanner.Web.Components;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCheckScannerApplication();
builder.Services.AddCheckScannerInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.EnsureCheckScannerSchemaBestEffortAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var contentTypeProvider = new FileExtensionContentTypeProvider();
app.MapGet("/photos/{fileName}", async (string fileName, IPhotoStore photoStore, CancellationToken cancellationToken) =>
{
    // storagePath is a server-generated GUID filename; strip any path segments defensively before touching disk.
    var safeName = Path.GetFileName(fileName);
    if (!contentTypeProvider.TryGetContentType(safeName, out var contentType))
    {
        contentType = "application/octet-stream";
    }

    try
    {
        var stream = await photoStore.OpenReadAsync(safeName, cancellationToken);
        return Results.Stream(stream, contentType);
    }
    catch (FileNotFoundException)
    {
        return Results.NotFound();
    }
});

app.Run();
