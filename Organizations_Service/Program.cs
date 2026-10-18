using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.WebEncoders;
using OrganizationsService.Applications;
using OrganizationsService.Infrastructure.Persistence;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using OrganizationsService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

if (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } port)
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Don't encode kiri characters in HTML-entities.
builder.Services.Configure<WebEncoderOptions>(o =>
    o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// For backward compatibility with reverse proxies like Nginx or Traefik, configure the forwarded headers middleware to process the X-Forwarded-For and X-Forwarded-Proto headers.
// This is important for scenarios where the application is behind a reverse proxy that terminates SSL and forwards requests to the application over HTTP.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});
builder.Services.AddHealthChecks();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

app.UseForwardedHeaders();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


await app.RunAsync();

// Makes the auto-generated Program class public so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
