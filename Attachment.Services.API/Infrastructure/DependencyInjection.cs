using Microsoft.Extensions.DependencyInjection;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Services;

namespace Attachment.Services.API.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAttachmentServices(this IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddScoped<IBaseFileService, BaseFileService>();
            services.AddScoped<IFileValidator, FileValidator>();
            services.AddScoped<IImageValidator, ImageValidator>();
            services.AddScoped<IVideoValidator, VideoValidator>();
            services.AddScoped<IAudioValidator, AudioValidator>();

            services.AddSingleton<CustomFileProvider>(serviceProvider =>
            {
                var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                var webRootPath = UploadPaths.GetStorageRoot()
                    ?? environment.WebRootPath
                    ?? Path.Combine(environment.ContentRootPath, "wwwroot");
                return new CustomFileProvider(webRootPath);
            });

            return services;
        }
    }
}
