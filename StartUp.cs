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

        public void Configure(ILoggerFactory loggerFactory)
        {
            loggerFactory
                .AddConsole(LogLevel.Debug)
                .AddDebug();
        }
    }
}
