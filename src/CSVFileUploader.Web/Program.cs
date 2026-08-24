using CSVFileUploader.Web.Components;
using CSVFileUploader.Application;
using CSVFileUploader.Infrastructure;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var csvUploadOptions =
    builder.Configuration
        .GetSection(CsvUploadOptions.SectionName)
        .Get<CsvUploadOptions>()
    ?? new CsvUploadOptions();

CsvUploadOptionsValidator.Validate(
    csvUploadOptions);

builder.Services.AddSingleton(
    csvUploadOptions);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<UiErrorHandler>();

var app = builder.Build();

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

app.Run();
