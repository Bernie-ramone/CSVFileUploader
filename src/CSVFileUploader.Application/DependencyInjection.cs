using Microsoft.Extensions.DependencyInjection;

namespace CSVFileUploader.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<CSV.UploadCsv.UploadCsvCommandHandler>();

            return services;
        }
    }
}
