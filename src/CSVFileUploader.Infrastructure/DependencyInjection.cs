using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Infrastructure.CSV;
using CSVFileUploader.Infrastructure.Persistence;
using CSVFileUploader.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CSVFileUploader.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found.");

            services.AddDbContext<ApplicationDbContext>(
                options =>
                    options.UseSqlServer(
                        connectionString,
                        sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure();
                        }));

            services.AddScoped<
                IImportedRecordRepository,
                ImportedRecordRepository>();

            services.AddScoped<
                IUploadRepository,
                UploadRepository>();

            services.AddScoped<
                IUploadHistoryRepository,
                UploadHistoryRepository>();

            services.AddScoped<
                IUnitOfWork,
                UnitOfWork>();

            services.AddScoped<
                CsvUploadOptions>();

            services.AddScoped<
                ICsvReader,
                CsvReader>();

            services.AddScoped<
                ICsvStructureValidator,
                CsvStructureValidator>();

            return services;
        }
    }
}