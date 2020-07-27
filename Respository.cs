using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace CoreBot
{
    public class Respository
    {
        public static Attachment GenerateAdaptiveCardTextBlock(string message)
        {
            string[] arr = message.Split("-");

            var str_arr = string.Empty;

            foreach (string each in arr)
            {
                str_arr += @"{
                      'type': 'TextBlock',
                      'spacing': 'medium',
                      'size': 'default',
                      'weight': 'bolder',
                      'text': '" + each + @"',
                      'wrap': true,
                      'maxLines': 0
                    },";
            }

            var json = @"{'$schema': 'http://adaptivecards.io/schemas/adaptive-card.json',
                  'type': 'AdaptiveCard',
                  'version': '1.0',
                  'body': [" + str_arr + @"]}";

            json = json.Replace("\n", "");

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(json.ToString()),
            };
            return adaptiveCardAttachment;
        }

        public static Attachment GenerateAdaptiveCardActionItems(string message)
        {
            string[] arr = message.Split("-").Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();

            var str_arr = string.Empty;

            foreach (string each in arr.Skip(1))
            {
                str_arr += @"{
                            'type': 'Action.ShowCard',
                            'title': '" + each + @"'
                            },";
            }

            var json = @"{'$schema': 'http://adaptivecards.io/schemas/adaptive-card.json',
                  'type': 'AdaptiveCard',
                  'version': '1.0',
                   'body': [{
                      'type': 'TextBlock',
                      'spacing': 'medium',
                      'size': 'default',
                      'weight': 'bolder',
                      'text': '" + arr[0] + @"',
                      'wrap': true,
                      'maxLines': 0
                    }],
                    'actions': [" + str_arr + @"]
                    }";

            json = json.Replace("\n", "");

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(json.ToString()),
            };
            return adaptiveCardAttachment;
        }
    }
}
