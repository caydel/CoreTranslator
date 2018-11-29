#pragma warning disable IDE1006 // Naming Styles
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CoreTranslator
{
    public class DetectedLanguage
    {
        /// <summary>
        /// 
        /// </summary>
        public string language { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double score { get; set; }
    }

    public class TranslationsItem
    {
        /// <summary>
        /// 我可以。
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string to { get; set; }
    }

    public class W
    {
        /// <summary>
        /// 
        /// </summary>
        public DetectedLanguage detectedLanguage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<TranslationsItem> translations { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles

    public class BingTranslator
    {
        public static string CallTranslate(string input)
        {
            Console.WriteLine($"\t\tCalling Bing: {input}");
            var client = new RestClient("https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=zh");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Ocp-Apim-Subscription-Key", "key");
            request.AddHeader("Content-Type", "application/json");

            var inputSource = new List<Translation>();
            inputSource.Add(new Translation
            {
                Text = input
            });
            var inputJson = JsonConvert.SerializeObject(inputSource);
            request.AddParameter("undefined", inputJson, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var json = response.Content;
            var result = JsonConvert.DeserializeObject<List<W>>(json);
            return result[0].translations[0].text;
        }
    }
}
