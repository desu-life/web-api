using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using System.Text.Json;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Clear();
            var configPath = "config.toml";
            if (File.Exists(configPath))
            {
                Config.Inner = Config.Load(configPath);
            }
            else
            {
                Config.Inner = Config.Base.Default();
                Config.Inner.Save(configPath);
            }
            var config = Config.Inner!;


            // security check
            Checker.Check();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                }
            );
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // builder.Services.AddLogging();
            builder.Services.AddSingleton<ResponseService>();
            builder.Services.AddSingleton<Cookies>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (config.Dev)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            string baseUrl = "http://";

            if (config.Api!.UseSSL)
            {
                app.UseHttpsRedirection();
                baseUrl = "https://";
            }

            app.Urls.Add($"{baseUrl}{config.Api.ApiHost}:{config.Api.ApiPort}");
            app.MapControllers();
            app.Run();
        }
    }
}
