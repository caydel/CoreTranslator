using CoreTranslator.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoreTranslator.Services
{
    public class TranslatorCore
    {
        private readonly BingTranslator _bingtranslator;
        private readonly DocumentAnalyser _documentAnalyser;
        private readonly ILogger<TranslatorCore> _logger;

        public TranslatorCore(
            BingTranslator bingTranslator,
            DocumentAnalyser documentAnalyser,
            ILoggerFactory loggerFactory)
        {
            _bingtranslator = bingTranslator;
            _documentAnalyser = documentAnalyser;
            _logger = loggerFactory.CreateLogger<TranslatorCore>();
        }

        public void DoWork()
        {
            _logger.LogInformation("Starting application...");
            var currentDirectory = Directory.GetCurrentDirectory();
            string[] cshtmls = Directory.GetFileSystemEntries(currentDirectory, "*.cshtml", SearchOption.AllDirectories);
            foreach (var cshtml in cshtmls)
            {
                _logger.LogInformation($"Analysing: {cshtml}");
                var fileName = Path.GetFileName(cshtml);
                if (fileName.Contains("_ViewStart") || fileName.Contains("_ViewImports"))
                {
                    continue;
                }

                var file = File.ReadAllText(cshtml);
                var document = _documentAnalyser.AnalyseFile(file);
                var xmlResources = new List<TranslatePair>();
                _logger.LogInformation($"Translating: {cshtml}");
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
                _logger.LogInformation($"Rendering: {cshtml}");
                var translated = RenderCSHtml(document);
                var translatedResources = GenerateXML(xmlResources);


                var xmlPosition = cshtml.Replace("\\Views\\", "\\Resources\\Views\\").Replace(".cshtml", ".zh.resx");
                var toWrite = Directory.CreateDirectory(new FileInfo(xmlPosition).Directory.FullName);

                _logger.LogInformation($"Writting: {xmlPosition}");
                File.WriteAllText(xmlPosition, translatedResources);
                File.WriteAllText(cshtml.Replace(".cshtml", ".cshtml"), translated);
            }
        }

        public string RenderCSHtml(List<HTMLPart> parts)
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
            var toTranslate = input.Trim();
            if (toTranslate.Length == 0)
            {
                return "";
            }
            var translated = $"@Localizer[\"{toTranslate}\"]";
            return input.Replace(toTranslate, translated);
        }

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
}
