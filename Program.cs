using Flurl.Http;
using LanguageExt.UnsafeValueAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

namespace desu_life_web_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Clear();
            var configPath = "config.toml";
            if (File.Exists(configPath))
            {
                Config.inner = Config.load(configPath);
            }
            else
            {
                Config.inner = Config.Base.Default();
                Config.inner.save(configPath);
            }
            var config = Config.inner!;


            // security check
            Security.KeyChecker();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // builder.Services.AddLogging();
            builder.Services.AddSingleton<ResponseService>();
            builder.Services.AddSingleton<Cookies>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (config.dev)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            string baseUrl = "http://";

            if (config.api!.useSSL)
            {
                app.UseHttpsRedirection();
                baseUrl = "https://";
            }

            app.Urls.Add($"{baseUrl}{config.api.apiHost}:{config.api.apiPort}");
            app.MapControllers();
            app.Run();
        }
    }
}
