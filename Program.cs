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
        public override string ToString()
        {
            return this.Content;
        }
    }

    public class TranslatePair
    {
        public string SourceString { get; set; }
        public string TargetString { get; set; }
    }

    public class CoreTranslator
    {
        private readonly BingTranslator _bingtranslator;
        public CoreTranslator(BingTranslator bingTranslator)
        {
            _bingtranslator = bingTranslator;
        }
        public void DoWork()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            string[] cshtmls = Directory.GetFileSystemEntries(currentDirectory, "*.cshtml", SearchOption.AllDirectories);
            foreach (var cshtml in cshtmls.Where(t => !t.EndsWith("td.cshtml")))
            {
                Console.WriteLine($"Analysing: {cshtml}");
                var fileName = Path.GetFileName(cshtml);
                if (fileName.Contains("_ViewStart") || fileName.Contains("_ViewImports"))
                {
                    continue;
                }
                var file = File.ReadAllText(cshtml);
                var document = RenderFile(file);
                var xmlResources = new List<TranslatePair>();
                Console.WriteLine($"Translating: {cshtml}");
                for (int i = 0; i < document.Count; i++)
                {
                    var textPart = document[i];
                    if (textPart.StringType == StringType.Text && textPart.Content.Trim() != string.Empty && !textPart.Content.Contains('@'))
                    {
                        xmlResources.Add(new TranslatePair
                        {
                            SourceString = textPart.Content,
                            TargetString = _bingtranslator.CallTranslate(textPart.Content, "zh")
                        });
                        textPart.Content = Translate(textPart.Content);
                    }
                    else if (textPart.StringType == StringType.Tag && textPart.Content.ToLower().StartsWith("<script"))
                    {
                        document[i + 1].StringType = StringType.Tag;
                    }
                    else if (textPart.StringType == StringType.Tag && textPart.Content.ToLower().StartsWith("<link"))
                    {
                        document[i + 1].StringType = StringType.Tag;
                    }
                }
                Console.WriteLine($"Rendering: {cshtml}");
                var translated = RenderCSHtml(document);
                var translatedResources = GenerateXML(xmlResources);


                var xmlPosition = cshtml.Replace("\\Views\\", "\\Resources\\Views\\").Replace(".cshtml", ".zh.resx");
                var toWrite = Directory.CreateDirectory(new FileInfo(xmlPosition).Directory.FullName);
                File.WriteAllText(xmlPosition, translatedResources);
                File.WriteAllText(cshtml.Replace(".cshtml", ".cshtml"), translated);
                Console.WriteLine($"Writting: {xmlPosition}");
            }
        }
        public List<StringPart> RenderFile(string html)
        {
            var document = new List<StringPart>();
            while (html.Trim().Length > 0)
            {
                var (newpart, remainingHtml) = GetNextPart(html);
                html = remainingHtml;
                document.Add(newpart);
            }
            return document;
        }

        public string RenderCSHtml(List<StringPart> parts)
        {
            string cshtml = "";
            foreach (var part in parts)
            {
                if (part != null)
                {
                    cshtml += part.Content;
                }
            }
            return cshtml;
        }

        public string Translate(string input)
        {
            return $"@Localizer[\"{input.Trim()}\"]";
        }

        public (StringPart, string) GetNextPart(string html)
        {
            var part = new StringPart();
            if (html.Trim().Length < 1)
            {
                throw new Exception();
            }
            if (html[0] == '<')
            {
                part.StringType = StringType.Tag;
                part.Content = html.Substring(0, html.IndexOf('>') + 1);
                return (part, html.Substring(html.IndexOf('>') + 1));
            }
            else if (html.Trim()[0] == '@' || html.Trim()[0] == '}')
            {
                part.StringType = StringType.Razor;
                var endPoint = html.IndexOf('<');
                if (endPoint > 0)
                {
                    part.Content = html.Substring(0, endPoint);
                    return (part, html.Substring(endPoint));
                }
                else
                {
                    part.Content = html;
                    return (part, "");
                }
            }
            else
            {
                part.StringType = StringType.Text;
                var endPoint = html.IndexOf('<');
                if (endPoint > 0)
                {
                    part.Content = html.Substring(0, endPoint);
                    return (part, html.Substring(endPoint));
                }
                else
                {
                    part.Content = html;
                    return (part, "");
                }
            }
        }

        //public string BingTranslate(string input)
        //{
        //    return BingTranslator.CallTranslate(input, "zh");
        //}

        public string GenerateXML(List<TranslatePair> sourceDocument)
        {
            var programPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var generatedItems = string.Empty;
            foreach (var item in sourceDocument)
            {
                generatedItems += $"<data name=\"{item.SourceString.Trim()}\" xml:space=\"preserve\"><value>{item.TargetString.Trim()}</value></data>\r\n";
            }
            var template = File.ReadAllText(programPath + "\\Template.xml");
            var translated = template.Replace("{{Content}}", generatedItems);
            return translated;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<BingTranslator>()
                .AddScoped<CoreTranslator>()
                .BuildServiceProvider();

            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            //do the actual work here
            var translator = serviceProvider.GetService<CoreTranslator>();
            translator.DoWork();

            logger.LogDebug("All done!");
        }
    }
}
