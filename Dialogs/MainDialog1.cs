// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.9.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

using CoreBot.CognitiveModels;
using CoreBot.Model;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.Security.Authentication;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CoreBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly FlightBookingRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        IConfiguration _iconfiguration;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(FlightBookingRecognizer luisRecognizer, IncidentDialog bookingDialog, ILogger<MainDialog> logger, IConfiguration iconfiguration)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            _iconfiguration = iconfiguration;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(bookingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                //await stepContext.Context.SendActivityAsync(
                //    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?\nSay something like \"Book a flight from Paris to Berlin on March 22, 2020\"";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(IncidentDialog), new Incident(), cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case FlightBooking.Intent.BookFlight:
                    await ShowWarningForUnsupportedCities(stepContext.Context, luisResult, cancellationToken);

                    // Initialize BookingDetails with any entities we may have found in the response.
                    var bookingDetails = new BookingDetails()
                    {
                        // Get destination and origin from the composite entities arrays.
                        Destination = luisResult.ToEntities.Airport,
                        Origin = luisResult.FromEntities.Airport,
                        TravelDate = luisResult.TravelDate,
                    };

                    // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(IncidentDialog), bookingDetails, cancellationToken);

                case FlightBooking.Intent.GetWeather:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getWeatherMessageText = "TODO: get weather flow here";
                    var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
                    break;

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        private static async Task ShowWarningForUnsupportedCities(ITurnContext context, FlightBooking luisResult, CancellationToken cancellationToken)
        {
            var unsupportedCities = new List<string>();

            var fromEntities = luisResult.FromEntities;
            if (!string.IsNullOrEmpty(fromEntities.From) && string.IsNullOrEmpty(fromEntities.Airport))
            {
                unsupportedCities.Add(fromEntities.From);
            }

            var toEntities = luisResult.ToEntities;
            if (!string.IsNullOrEmpty(toEntities.To) && string.IsNullOrEmpty(toEntities.Airport))
            {
                unsupportedCities.Add(toEntities.To);
            }

            if (unsupportedCities.Any())
            {
                var messageText = $"Sorry but the following airports are not supported: {string.Join(',', unsupportedCities)}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await context.SendActivityAsync(message, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            if (stepContext.Result is Incident result)
            {
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                //var timeProperty = new TimexProperty(result.IncidentDate);
                //var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);

                string messageText = string.Empty;
                //string jsonss = _iconfiguration.GetValue<string>("IncidentCreateJson");
                //string url = _iconfiguration.GetValue<string>("IncidentCreateURL");
                //string url1 = _iconfiguration.GetValue<string>("IncidentStatusCheckByID");
                //string url2 = _iconfiguration.GetValue<string>("LastFiveIncidentNo");

                if (result.ChoiceID == "Create Incident")
                {
                    string json = _iconfiguration.GetValue<string>("IncidentCreateJson");
                    json = json.Replace("{shortdesc}", result.IncidentDesc).Replace("{desc}", result.IncidentDesc);
                    string incident_number = CreateIncident(result.IncidentDesc, result.EmailID, json);
                    // string incident_number = "INC123456789";

                    messageText = $"I have created a INC **#{incident_number}** with this details Description : {result.IncidentDesc} from {result.EmailID} on {DateTime.Now.ToString()}";
                }
                else if (result.ChoiceID == "Check Incident Status")
                {
                    string incident_number = result.IncidentNo;
                    string status = CheckIncidentStatusByID(result.EmailID, result.IncidentNo);// "CANCELLED";
                    if (string.IsNullOrWhiteSpace(status))
                        messageText = $"**Unable to find the status for Incident #{incident_number}.**";
                    else
                        messageText = $"Current status of this **#{incident_number} : {status}**";
                }
                else if (result.ChoiceID == "Top 5 Incidents List")
                {
                    string incLst = "Please find the list of Last 5 Incidents-" + LastFiveIncidentLst(result.EmailID);
                    if (string.IsNullOrWhiteSpace(incLst))
                        messageText = "No INC list available for you.";
                    else
                        messageText = incLst;
                    //messageText = "1) INC123456789 \n 2) INC123456789 \n 3) INC123456789 \n 4) INC123456789 \n 5) INC123456789";
                }

                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardActionItems(messageText)) }, cancellationToken);

                //var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                //await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private string CreateIncident(string desc, string emailid, string json)
        {

            string incidentNumber = string.Empty;
            string url = _iconfiguration.GetValue<string>("IncidentCreateURL"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
            (bool, string) response = PostData(json, url);

            if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
            {
                JObject jObj = JObject.Parse(response.Item2);

                incidentNumber = Convert.ToString(jObj.SelectToken("result.number"));
            }

            return incidentNumber;
        }

        private string LastFiveIncidentLst(string emailid)
        {

            string incidentNumberLst = string.Empty;
            string url = _iconfiguration.GetValue<string>("LastFiveIncidentNo"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
            url = url.Replace("{email}", emailid);
            (bool, string) response = GetData("", url);

            if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
            {
                JObject o = JObject.Parse(response.Item2);
                IList<Object> results = o.SelectToken("result").Select(s => (Object)s).ToList();

                if (results.Count > 0)
                {
                    foreach (Object str in results)
                    {
                        JObject arResult = JObject.Parse(Convert.ToString(str));
                        incidentNumberLst = Convert.ToString(arResult.SelectToken("number")) + " - " + incidentNumberLst;
                    }
                }

                //JObject jObj = JObject.Parse(response.Item2);


                //incidentNumberLst = Convert.ToString(jObj.SelectToken("result.number"));
            }

            return incidentNumberLst;
        }

        private string CheckIncidentStatusByID(string emailid, string incno)
        {
            string status = string.Empty;
            string url = _iconfiguration.GetValue<string>("IncidentStatusCheckByID"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
            url = url.Replace("{email}", emailid).Replace("{incidentId}", incno);
            (bool, string) response = GetData("", url);

            if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
            {
                JObject jObj = JObject.Parse(response.Item2);
                IList<Object> results = jObj.SelectToken("result").Select(s => (Object)s).ToList();

                if (results.Count > 0)
                    status = Convert.ToString(jObj.SelectToken("result[0].state"));


                //Need check for all other status
                if (status == "12")
                    status = "Work In Progress";
                else if (status == "6")
                    status = "Resolved";
                else if (status == "7")
                    status = "Closed";
                else if (status == "-7")
                    status = "Pending Change";
                else if (status == "-5")
                    status = "Pending User Info";
                else if (status == "-8")
                    status = "Pending Vendor";
            }

            return status;

        }

        public (bool, string) PostData(string json, string customUrl)
        {
            (bool, string) output;
            try
            {
                output = CallApi(json, "POST", string.Empty, customUrl).Result;
            }
            catch (Exception ex)
            {

                output = (false, ex.Message);
            }
            return output;
        }

        public (bool, string) GetData(string json, string customUrl)
        {
            (bool, string) output;
            try
            {
                output = CallApi(json, "GET", string.Empty, customUrl).Result;
            }
            catch (Exception ex)
            {

                output = (false, ex.Message);
            }
            return output;
        }

        internal async Task<(bool, string)> CallApi(string json, string method, string tableName, string customUrl = null)
        {
            // json = _iconfiguration.GetValue<string>("IncidentCreateJson"); //"{\"short_description\":\"Testing\",\"category\":\"General\",\"subcategory\":\"General\"}";
            string response = string.Empty;
            try
            {
                string url = string.Empty;
                if (customUrl != null)
                {
                    url = customUrl;
                }
                else
                {
                    url = url + tableName;
                }
                HttpClientHandler restHandler;
                string proxyAuthNeeded = "No";
                if (proxyAuthNeeded.ToUpper() == "Yes")
                {
                    string proxyUrl = "";
                    string ProxyUserName = "";
                    string ProxyPassword = "";

                    WebProxy proxy = new WebProxy(proxyUrl);
                    restHandler = new HttpClientHandler
                    {

                        Credentials = new NetworkCredential(ProxyUserName, ProxyPassword),
                        Proxy = proxy,
                        UseProxy = true,
                        UseDefaultCredentials = false
                    };
                }
                else
                {
                    restHandler = new HttpClientHandler
                    {
                        UseDefaultCredentials = true
                    };
                }
                restHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                restHandler.SslProtocols = SslProtocols.Tls12;
                restHandler.AllowAutoRedirect = false;

                using (HttpClient rest = new HttpClient(restHandler))
                {
                    rest.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    rest.DefaultRequestHeaders.Add("authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("int_user" + ":" + "Admin@123")));

                    if (method == "GET")
                    {
                        Uri sNowURI = null;
                        if (!string.IsNullOrEmpty(json))
                        {
                            sNowURI = new Uri(url + "?" + json);
                        }
                        else
                        {
                            sNowURI = new Uri(url);
                        }


                        using (HttpResponseMessage res = rest.GetAsync(sNowURI).GetAwaiter().GetResult())
                        {

                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                response = res.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }


                    else if (method == "POST")
                    {
                        Uri sNowURI = new Uri(url);
                        using (HttpResponseMessage res = await rest.PostAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/json")))
                        {
                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                response = res.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }
                    else if (method == "XmlPOST")
                    {
                        Uri sNowURI = new Uri(url);
                        using (HttpResponseMessage res = await rest.PostAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/xml")))
                        {
                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                response = res.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }
                    else
                    {
                        Uri sNowURI = new Uri(url);
                        using (HttpResponseMessage res = await rest.PutAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/json")))
                        {
                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                response = res.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                string snowDetails = "Snow api url: " + customUrl + ", request: " + json + ", snow response: " + response;
                snowDetails = snowDetails.Replace("\"", "\\\"");
                snowDetails = snowDetails.Replace(",", " ^ nl ^ ");
                return (false, ex.Message + "~" + snowDetails);
            }
            return (true, response);
        }

    }

}