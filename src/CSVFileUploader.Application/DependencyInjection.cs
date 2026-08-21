using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.CSV.UploadHistory;
using CSVFileUploader.Application.CSV.Validators;
using CSVFileUploader.Application.DTOs;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CSVFileUploader.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<CSV.UploadCsv.UploadCsvCommandHandler>();

            services.AddScoped<IValidator<CsvRowDto>, CsvRowValidator>();

            services.AddScoped<IValidator<UploadCsvCommand>, UploadCsvCommandValidator>();

            services.AddScoped<GetUploadHistoryQueryHandler>();

            services.AddScoped<GetUploadHistoryDetailsQueryHandler>();

            return services;
        }
    }
}
