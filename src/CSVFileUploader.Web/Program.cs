using CSVFileUploader.Application;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Infrastructure;
using CSVFileUploader.Infrastructure.Persistence;
using CSVFileUploader.Infrastructure.Persistence.Identity;
using CSVFileUploader.Web.Components;
using CSVFileUploader.Web.Services;
using Microsoft.AspNetCore.Identity;

var builder =
    WebApplication.CreateBuilder(args);

var csvUploadOptions =
    builder.Configuration
        .GetSection(
            CsvUploadOptions.SectionName)
        .Get<CsvUploadOptions>()
    ?? new CsvUploadOptions();

CsvUploadOptionsValidator.Validate(
    csvUploadOptions);

builder.Services.AddSingleton(
    csvUploadOptions);

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
        })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath =
            "/Identity/Account/Login";

        options.AccessDeniedPath =
            "/Identity/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddRazorPages();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<
    UiErrorHandler>();

var app =
    builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorPages();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program
{
    
}