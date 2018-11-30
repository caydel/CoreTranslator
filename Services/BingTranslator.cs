using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using CoreTranslator.Services.BingModels;
using Microsoft.Extensions.Logging;

namespace CoreTranslator.Services
{
    public class BingTranslator
    {
        private readonly ILogger<BingTranslator> _logger;
        private static string _apiKey;

        public BingTranslator(
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BingTranslator>();
        }

        public void Init(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string CallTranslate(string input, string targetLanguage)
        {
            var apiAddress = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={targetLanguage}";
            var client = new RestClient(apiAddress);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");
            var inputSource = new List<Translation>
            {
                new Translation
                {
                    Text = input
                }
            };
            var inputJson = JsonConvert.SerializeObject(inputSource);
            request.AddParameter("undefined", inputJson, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var json = response.Content;
            var result = JsonConvert.DeserializeObject<List<BingResponse>>(json);
            _logger.LogInformation($"\t\tCalled Bing: {input} - {result[0].Translations[0].Text}");
            return result[0].Translations[0].Text;
        }
    }
}
