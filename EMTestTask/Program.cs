using EMTestTask.Configs;
using EMTestTask.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;


namespace EMTestTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Add localization
            var supportedCultures = new[]
            {
                new CultureInfo("ru-RU"),
                new CultureInfo("en-US")
            };
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("ru-RU");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });


            // Add StatisticBot
            builder.Services.Configure<StatisticBotSetting>(builder.Configuration.GetSection("StatisticBot"));
            builder.Services.AddScoped<StatisticBotService>();

            // Main task
            builder.Services.Configure<AdvertisingSetting>(builder.Configuration.GetSection("AdvertisingSetting"));
            builder.Services.AddSingleton<IAdvertisingService, AdvertisingService>();

            // swagger
            var useSwagger = bool.TryParse(builder.Configuration["UseSwagger"], out bool resConfBool) && resConfBool;
            if (useSwagger)
            {
                builder.Services.AddOpenApiDocument();
            }


            builder.Services.AddControllers();

            var app = builder.Build();

            var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>()?.Value;
            if (localizationOptions != null)
            {
                app.UseRequestLocalization(localizationOptions);
            }

            if (useSwagger)
            {
                app.UseOpenApi();
                app.UseSwaggerUi();
            }

            // Configure the HTTP request pipeline.
            app.UseCors();

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
