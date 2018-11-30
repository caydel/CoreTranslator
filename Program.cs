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
    public class StringPart
    {
        public StringType StringType { get; set; }
        public string Content { get; set; }
        public override string ToString() => this.Content;
    }

    public class TranslatePair
    {
        public string SourceString { get; set; }
        public string TargetString { get; set; }
    }


    public class Program
    {
        static void Main(string[] args)
        {
            BuildApplication()
                .GetService<TranslatorCore>()
                .DoWork();
        }

        static ServiceProvider BuildApplication()
        {
            var startUp = new StartUp();
            var services = new ServiceCollection();

            startUp.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            startUp.Configure(serviceProvider.GetService<ILoggerFactory>(), serviceProvider.GetService<BingTranslator>());

            return serviceProvider;
        }
    }
}
