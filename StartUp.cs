using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using CoreTranslator.Services;

namespace CoreTranslator
{
    public class StartUp
    {
        public StartUp()
        {

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<BingTranslator>();
            services.AddScoped<TranslatorCore>();
        }

        public void Configure(ILoggerFactory loggerFactory, BingTranslator translator)
        {
            loggerFactory
                .AddConsole(LogLevel.Debug)
                .AddDebug();


            Console.WriteLine("Enter your bing API key:");
            var key = Console.ReadLine().Trim();
            translator
                .Init(key);
        }
    }
}
