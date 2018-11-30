using CoreTranslator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreTranslator
{
    public enum StringType
    {
        Tag,
        Razor,
        Text
    }

    public class Program
    {
        static void Main(string[] args)
        {
            BuildApplication()
                .GetService<TranslatorCore>()
                .DoWork();

            Console.ReadLine();
        }

        static ServiceProvider BuildApplication()
        {
            var startUp = new StartUp();
            var services = new ServiceCollection();

            startUp.ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var bingTranslator = serviceProvider.GetService<BingTranslator>();

            startUp.Configure(loggerFactory, bingTranslator);
            return serviceProvider;
        }
    }
}
