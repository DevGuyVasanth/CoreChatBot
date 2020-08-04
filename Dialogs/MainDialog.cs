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
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.Security.Authentication;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using CoreBot;
using AdaptiveCards;
using CoreBot.Cards;
using CoreBot.Model;
using CoreBot.Dialogs;
using CoreBot.CognitiveModels;

namespace CoreBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly FlightBookingRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        IConfiguration _iconfiguration;
        string intentName = string.Empty;
        bool IsinputCheck = false;
        protected readonly BotState UserState1;
        //static string AdaptivePromptId = "adaptive";
        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(FlightBookingRecognizer luisRecognizer, CreateIncidentDialog createIncidentDialog, IncidentStatusDialog incidentStatusDialog, LastFiveINCDialog lastFiveINCDialog, ArtEnrollmentDialog artEnrollment, ArtRegisterOTPDialog artRegisterOTP, UserProfileDialog userProfileDialog, ARTEnrollFinalDialog aRTEnrollFinalDialog, ArtAccountUnlockWithLogin artAccountUnlockWithLogin, ArtAccountUnlockWithoutLogin artAccountUnlockWithoutLogin, ArtOTPDialog artOTPDialog, ILogger<MainDialog> logger, IConfiguration iconfiguration, UserState userState)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            _iconfiguration = iconfiguration;
            Logger = logger;
            UserState1 = userState;
            //AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(createIncidentDialog);
            AddDialog(incidentStatusDialog);
            AddDialog(lastFiveINCDialog);
            AddDialog(artEnrollment);
            AddDialog(artRegisterOTP);
            AddDialog(userProfileDialog);
            AddDialog(aRTEnrollFinalDialog);
            AddDialog(artAccountUnlockWithLogin);
            AddDialog(artAccountUnlockWithoutLogin);
            AddDialog(artOTPDialog);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                //InitiateStepAsync,
                MenuStepAsync,
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private Attachment GenericAdaptiveCard(string message)
        {
            var json = @"{'$schema': 'http://adaptivecards.io/schemas/adaptive-card.json',
                  'type': 'AdaptiveCard',
                  'version': '1.0',
                  'body': [{
                      'type': 'TextBlock',
                      'spacing': 'medium',
                      'size': 'default',
                      'weight': 'bolder',
                      'text': '" + message + @"',
                      'wrap': true,
                      'maxLines': 0
                    }]
                    }";

            json = json.Replace("\n", "");

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(json.ToString()),
            };
            return adaptiveCardAttachment;
        }

        private async Task<DialogTurnResult> InitiateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Incident incidentDetails = new Incident();

            if (incidentDetails.IncidentDesc == null)
            {
                var messageText = stepContext.Options?.ToString() ?? "Hello this is Rida.your virtual assistant.what can i help you with today?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> MenuStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string card = string.Empty;
            intentName = (string)stepContext.Result;
            if (intentName == null)
                intentName = "NONE";

            IsinputCheck = false;
            if (!string.IsNullOrEmpty(intentName) || intentName.ToUpper() != "NONE")
            {
                //intentName = GetLuisJSON(intentName.ToLower());
                //intentName = (string)stepContext.Result;
                if (intentName.ToUpper() == "CREATE INCIDENT") // || intentName.ToUpper() != "CHECK INCIDENT STATUS" || intentName.ToUpper() != "RETRIEVE INCIDENTS" || intentName.ToUpper() != "TOP 5 INCIDENTS LIST" || intentName.ToUpper() != "RETRIEVE LAST 5 INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "CHECK INCIDENT STATUS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "RETRIEVE INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "TOP 5 INCIDENTS LIST")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "RETRIEVE LAST 5 INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ART ENROLLMENT")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ACCOUNT UNLOCK")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ONE TIME PIN CODE")
                    IsinputCheck = true;

                if (!IsinputCheck)
                    intentName = GetLuisJSON(intentName.ToLower());
            }

            var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
            var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());
            if (string.IsNullOrWhiteSpace(sessionModels.DisplayName))
                card = "\\Cards\\menuCardWithoutLogin.json";
            else
                card = "\\Cards\\menuCard.json";

            if (!IsinputCheck)
            {
                Incident inc = new Incident();
                var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                JObject json = JObject.Parse(adaptiveCardJson);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(json.ToString()),
                };
                if (inc.IncidentDesc == null)
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(intentName) || intentName.ToUpper() != "NONE")
            {
                //intentName = GetLuisJSON(intentName.ToLower());
                //intentName = (string)stepContext.Result;
                intentName = (string)stepContext.Result;
                if (intentName.ToUpper() == "CREATE INCIDENT") // || intentName.ToUpper() != "CHECK INCIDENT STATUS" || intentName.ToUpper() != "RETRIEVE INCIDENTS" || intentName.ToUpper() != "TOP 5 INCIDENTS LIST" || intentName.ToUpper() != "RETRIEVE LAST 5 INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "CHECK INCIDENT STATUS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "RETRIEVE INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "TOP 5 INCIDENTS LIST")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "RETRIEVE LAST 5 INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ART ENROLLMENT")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ACCOUNT UNLOCK")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ONE TIME PIN CODE")
                    IsinputCheck = true;

                if (!IsinputCheck)
                    intentName = GetLuisJSON(intentName.ToLower());
            }

            if (!IsinputCheck)
            {
                string card = string.Empty;
                string input = (string)stepContext.Result;
                intentName = GetLuisJSON(input.ToLower());

                if (!_luisRecognizer.IsConfigured)
                {
                    if (string.IsNullOrEmpty(intentName) || intentName == "None")
                    {
                        if (input == "ServiceNow")
                        {
                            card = "\\Cards\\incidentCard.json";
                        }
                        else if (input == "ART Account Management")
                        {
                            card = "\\Cards\\artCard.json";
                            //string enrollUrl = "http://192.168.225.178:16011/" + "?botId=" + stepContext.Parent.Context.Activity.From.Id.ToString() + "&conversationid=" + stepContext.Parent.Context.Activity.Conversation.Id.ToString() + "&request_Type=artEnrollLogin";

                        }
                        else if (input == "LiveAgent")
                        {

                        }
                        else
                            intentName = GetLuisJSON(input.ToLower());

                        //AdaptiveCardRenderer renderer = new AdaptiveCardRenderer();
                        //// For fun, check the schema version this renderer supports
                        //AdaptiveSchemaVersion schemaVersion = renderer.SupportedSchemaVersion;
                        //AdaptiveCard card1 = new AdaptiveCard(renderer.SupportedSchemaVersion)
                        //{
                        //    Body = { new AdaptiveTextBlock() { Text = "Hello World" } }
                        //};

                        //try
                        //{
                        //    // Render the card
                        //    RenderedAdaptiveCard renderedCard = renderer.RenderCard(card1);
                        //    // Get the output HTML 
                        //    HtmlTag html = renderedCard.Html;
                        //    // (Optional) Check for any renderer warnings
                        //    // This includes things like an unknown element type found in the card
                        //    // Or the card exceeded the maximum number of supported actions, etc
                        //    IList<AdaptiveWarning> warnings = renderedCard.Warnings;
                        //}
                        //catch (AdaptiveException ex)
                        //{
                        //    // Failed rendering
                        //}

                        if (!string.IsNullOrWhiteSpace(card))
                        {
                            string botid = (string)stepContext.State["turn.Activity.From.Id"];
                            string conversationid = (string)stepContext.State["turn.Activity.Conversation.Id"];

                            Incident inc = new Incident();
                            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                            JObject json = JObject.Parse(adaptiveCardJson.Replace("{botID}", botid).Replace("{conversationid}", conversationid));
                            adaptiveCardJson = adaptiveCardJson.Replace("{botId}", botid).Replace("{conversationid}", conversationid);
                            var adaptiveCardAttachment = new Attachment()
                            {
                                ContentType = "application/vnd.microsoft.card.adaptive",
                                Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
                            };

                            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
                        }
                    }
                    return await stepContext.NextAsync(null, cancellationToken);
                }
            }
            else
                return await stepContext.NextAsync(null, cancellationToken);

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "Hello this is Rida.your virtual assistant.what can i help you with today?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var val = stepContext.Context.Activity.Value;
            if (val != null)
            {
                if (val.ToString().Contains("Employeeid"))
                {
                    var details = JObject.Parse(val.ToString());

                    string EmpID = details["Employeeid"].ToString();

                    ArtOTP otpDetails = new ArtOTP();
                    otpDetails.EmpID = EmpID;
                    return await stepContext.BeginDialogAsync(nameof(ArtEnrollmentDialog), otpDetails, cancellationToken);
                }
            }
            IsinputCheck = false;
            if (string.IsNullOrEmpty(intentName) || intentName.ToUpper() == "NONE")
            {
                intentName = (string)stepContext.Result;
                if (intentName.ToUpper() == "CREATE INCIDENT") // || intentName.ToUpper() != "CHECK INCIDENT STATUS" || intentName.ToUpper() != "RETRIEVE INCIDENTS" || intentName.ToUpper() != "TOP 5 INCIDENTS LIST" || intentName.ToUpper() != "RETRIEVE LAST 5 INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "CHECK INCIDENT STATUS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "RETRIEVE INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "TOP 5 INCIDENTS LIST")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "RETRIEVE LAST 5 INCIDENTS")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ART ENROLLMENT")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ACCOUNT UNLOCK")
                    IsinputCheck = true;
                if (intentName.ToUpper() == "ONE TIME PIN CODE")
                    IsinputCheck = true;

                if (!IsinputCheck)
                    intentName = GetLuisJSON(intentName.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(intentName))
            {
                if (!_luisRecognizer.IsConfigured)
                {

                    // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                    if (intentName == "Create Incident")
                        return await stepContext.BeginDialogAsync(nameof(CreateIncidentDialog), new Incident(), cancellationToken);
                    else if (intentName == "Check Incident Status" || intentName == "Retrieve Incidents")
                        return await stepContext.BeginDialogAsync(nameof(IncidentStatusDialog), new Incident(), cancellationToken);
                    else if (intentName == "Top 5 Incidents List" || intentName == "Retrieve last 5 incidents")
                        return await stepContext.BeginDialogAsync(nameof(LastFiveINCDialog), new Incident(), cancellationToken);
                    else if (intentName == "ART Enrollment")
                    {
                        //string loginUrl = "http://192.168.225.178:16011/" + "?botId=" + stepContext.Parent.Context.Activity.From.Id.ToString() + "&conversationid=" + stepContext.Parent.Context.Activity.Conversation.Id.ToString() + "&request_Type=artEnrollLogin";

                        //var attachments = new List<Attachment>();
                        //var reply = MessageFactory.Attachment(attachments);
                        //var signinCard = new SigninCard
                        //{
                        //    Text = "",
                        //    Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "ART Enrollment", value: loginUrl) },
                        //};
                        //reply.Attachments.Add(signinCard.ToAttachment());

                        //await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                        ////bool IsLoginEnrolled = await GetEnrollStatus(stepContext, 0);
                        //var result = Task.Run(async () => await GetEnrollStatus(stepContext, 0)).Result;

                        //if (!result)
                        //    return await stepContext.EndDialogAsync();
                        //else
                        return await stepContext.BeginDialogAsync(nameof(ArtEnrollmentDialog), new ArtOTP(), cancellationToken);
                    }
                    else if (intentName == "Account Unlock")
                    {
                        var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
                        var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());
                        if (string.IsNullOrEmpty(sessionModels.DisplayName))
                        {
                            return await stepContext.BeginDialogAsync(nameof(ArtAccountUnlockWithLogin), new ArtOTP(), cancellationToken);
                        }
                        else
                        {
                            return await stepContext.BeginDialogAsync(nameof(ArtAccountUnlockWithLogin), new ArtOTP(), cancellationToken);
                        }
                    }
                    else if (intentName == "One Time Pin Code")
                    {
                        return await stepContext.BeginDialogAsync(nameof(ArtOTPDialog), new ArtOTP(), cancellationToken);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(intentName) || intentName == "None")
                        {
                            Incident inc = new Incident();
                            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + "\\Cards\\menuCard.json");
                            JObject json = JObject.Parse(adaptiveCardJson);
                            var adaptiveCardAttachment = new Attachment()
                            {
                                ContentType = "application/vnd.microsoft.card.adaptive",
                                Content = JsonConvert.DeserializeObject(json.ToString()),
                            };
                            if (inc.IncidentDesc == null)
                            {
                                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
                            }
                        }
                        return await stepContext.BeginDialogAsync(nameof(CreateIncidentDialog), new Incident(), cancellationToken);
                    }

                }
            }
            else
            {

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
                        return await stepContext.BeginDialogAsync(nameof(CreateIncidentDialog), new Incident(), cancellationToken);

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
                APIRequest apicall = new APIRequest();

                string messageText = string.Empty;
                //string jsonss = _iconfiguration.GetValue<string>("IncidentCreateJson");
                //string url = _iconfiguration.GetValue<string>("IncidentCreateURL");
                //string url1 = _iconfiguration.GetValue<string>("IncidentStatusCheckByID");
                //string url2 = _iconfiguration.GetValue<string>("LastFiveIncidentNo");
                if (result.ChoiceID == "1")
                {
                    string json = _iconfiguration.GetValue<string>("IncidentCreateJson");
                    json = json.Replace("{shortdesc}", result.IncidentDesc).Replace("{desc}", result.IncidentDesc);
                    if (!string.IsNullOrEmpty(result.EmailID))
                    {
                        if (result.EmailID.ToLower() == "siva@naseramuk.onmicrosoft.com")
                            json = json.Replace("{caller_id}", "Sivasubramanian");
                        else if (result.EmailID.ToLower() == "veera@naseramuk.onmicrosoft.com")
                            json = json.Replace("{caller_id}", "veera");
                        else
                            json = json.Replace("{caller_id}", "int_user");
                    }
                    else
                        json = json.Replace("{caller_id}", "int_user");

                    string url = _iconfiguration.GetValue<string>("IncidentCreateURL");
                    string incident_number = apicall.CreateIncident(result.IncidentDesc, result.EmailID, json, url);
                    // string incident_number = "INC123456789";

                    messageText = $"I have created a INC **#{incident_number}** with this details - Description : {result.IncidentDesc} from {result.EmailID} on {DateTime.Now.ToString()}";
                }
                else if (result.ChoiceID == "2")
                {
                    string url = _iconfiguration.GetValue<string>("IncidentStatusCheckByID");
                    string incident_number = result.IncidentNo;
                    string status = apicall.CheckIncidentStatusByID(result.EmailID, result.IncidentNo, url);// "CANCELLED";
                    if (string.IsNullOrWhiteSpace(status) || status == "Issue")
                        messageText = $"**Unable to find the status for Incident #{incident_number}.**";
                    else
                        messageText = $"Current status of this **#{incident_number} : {status}**";
                }
                else if (result.ChoiceID == "3")
                {
                    string url = _iconfiguration.GetValue<string>("LastFiveIncidentNo");
                    string incLst = apicall.LastFiveIncidentLst(result.EmailID, url);
                    if (string.IsNullOrWhiteSpace(incLst))
                        messageText = "No INC list available for you.";
                    else
                        messageText = "Please find the list of last 5 incidents -" + incLst;
                    //messageText = "1) INC123456789 \n 2) INC123456789 \n 3) INC123456789 \n 4) INC123456789 \n 5) INC123456789";
                }

                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(messageText)) }, cancellationToken);

                //var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                //await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            //return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);

            //var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + "\\Cards\\menuCard.json");
            //JObject json1 = JObject.Parse(adaptiveCardJson);
            //var adaptiveCardAttachment = new Attachment()
            //{
            //    ContentType = "application/vnd.microsoft.card.adaptive",
            //    Content = JsonConvert.DeserializeObject(json1.ToString()),
            //};
            //return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
            //var promptMessage = adaptiveCardAttachment;
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }


        //private async Task<bool> GetEnrollStatus(WaterfallStepContext stepContext, int retry)
        //{
        //    var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
        //    var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());

        //    bool IsLoginEnrolled = sessionModels.IsLoginEnrolled;

        //    if (!IsLoginEnrolled && retry <= 100)
        //    {
        //        retry++;
        //        await GetEnrollStatus(stepContext, retry);
        //    }

        //    return IsLoginEnrolled;
        //}

        //public async Task<bool> GetEnrollStatus(WaterfallStepContext stepContext, int retry)
        //{
        //    while (true)
        //    {
        //        var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
        //        var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());
        //        bool IsLoginEnrolled = sessionModels.IsLoginEnrolled;
        //        //var stream = GetStream(streamPosition);

        //        if (IsLoginEnrolled)
        //            return IsLoginEnrolled;
        //        else
        //            await GetEnrollStatus(stepContext, retry);

        //        await Task.Yield();
        //    }
        //}

        private string GetLuisJSON1(string input)
        {
            string intentName = string.Empty;
            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + "\\LUIS\\Ricoh-prod.json");

            try
            {
                JObject json = JObject.Parse(adaptiveCardJson);
                JObject match = json["utterances"].Values<JObject>().Where(m => m["text"].Value<string>() == input).FirstOrDefault();

                if (match != null)
                    intentName = match["intent"].ToString();
                else
                    intentName = "None";

                if (intentName == "None")
                {
                    JObject match1 = json["phraselists"].Values<JObject>().Where(m => m["words"].Value<string>().Contains(input)).FirstOrDefault();
                    if (match1 != null)
                        intentName = match1["name"].ToString();
                    else
                        intentName = "None";
                }
            }
            catch
            {
                intentName = "None";
            }
            return intentName;
        }

        private string GetLuisJSON(string input)
        {
            string intentName = string.Empty;
            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + "\\LUIS\\Rbot.json");

            try
            {
                JObject json = JObject.Parse(adaptiveCardJson);
                JObject match = json["utterances"].Values<JObject>().Where(m => m["text"].Value<string>() == input).FirstOrDefault();

                if (match != null)
                    intentName = match["intent"].ToString();
                else
                    intentName = "None";

                if (intentName == "None")
                {
                    JObject match1 = json["phraselists"].Values<JObject>().Where(m => m["words"].Value<string>().Contains(input)).FirstOrDefault();
                    if (match1 != null)
                        intentName = match1["name"].ToString();
                    else
                        intentName = "None";
                }
            }
            catch
            {
                intentName = "None";
            }
            return intentName;
        }
        //private string CreateIncident(string desc, string emailid, string json, string url)
        //{

        //    string incidentNumber = string.Empty;
        //    //string url = _iconfiguration.GetValue<string>("IncidentCreateURL"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
        //    (bool, string) response = PostData(json, url);

        //    if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
        //    {
        //        JObject jObj = JObject.Parse(response.Item2);

        //        incidentNumber = Convert.ToString(jObj.SelectToken("result.number"));
        //    }

        //    return incidentNumber;
        //}
        //private string LastFiveIncidentLst(string emailid, string url)
        //{

        //    string incidentNumberLst = string.Empty;
        //    //string url = _iconfiguration.GetValue<string>("LastFiveIncidentNo"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
        //    url = url.Replace("{email}", emailid);
        //    (bool, string) response = GetData("", url);

        //    if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
        //    {
        //        JObject o = JObject.Parse(response.Item2);
        //        IList<Object> results = o.SelectToken("result").Select(s => (Object)s).ToList();

        //        if (results.Count > 0)
        //        {

        //            foreach (Object str in results)
        //            {
        //                JObject arResult = JObject.Parse(Convert.ToString(str));
        //                incidentNumberLst = Convert.ToString(arResult.SelectToken("number")) + " \n " + incidentNumberLst;
        //            }
        //        }

        //        //JObject jObj = JObject.Parse(response.Item2);


        //        //incidentNumberLst = Convert.ToString(jObj.SelectToken("result.number"));
        //    }

        //    return incidentNumberLst;
        //}
        //private string CheckIncidentStatusByID(string emailid, string incno, string url)
        //{
        //    string status = string.Empty;
        //    //string url = _iconfiguration.GetValue<string>("IncidentStatusCheckByID"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
        //    url = url.Replace("{email}", emailid).Replace("{incidentId}", incno);
        //    (bool, string) response = GetData("", url);

        //    if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
        //    {
        //        JObject jObj = JObject.Parse(response.Item2);
        //        IList<Object> results = jObj.SelectToken("result").Select(s => (Object)s).ToList();

        //        if (results.Count > 0)
        //            status = Convert.ToString(jObj.SelectToken("result[0].state"));


        //        //Need check for all other status
        //        if (status == "12")
        //            status = "Work In Progress";
        //        else if (status == "11")
        //            status = "Assigned";
        //        else if (status == "6")
        //            status = "Resolved";
        //        else if (status == "7")
        //            status = "Closed";
        //        else if (status == "-7")
        //            status = "Pending Change";
        //        else if (status == "-5")
        //            status = "Pending User Info";
        //        else if (status == "-8")
        //            status = "Pending Vendor";
        //    }

        //    return status;

        //}
        //public (bool, string) PostData(string json, string customUrl)
        //{
        //    (bool, string) output;
        //    try
        //    {
        //        output = CallApi(json, "POST", string.Empty, customUrl).Result;
        //    }
        //    catch (Exception ex)
        //    {

        //        output = (false, ex.Message);
        //    }
        //    return output;
        //}
        //public (bool, string) GetData(string json, string customUrl)
        //{
        //    (bool, string) output;
        //    try
        //    {
        //        output = CallApi(json, "GET", string.Empty, customUrl).Result;
        //    }
        //    catch (Exception ex)
        //    {

        //        output = (false, ex.Message);
        //    }
        //    return output;
        //}
        //internal async Task<(bool, string)> CallApi(string json, string method, string tableName, string customUrl = null)
        //{
        //    // json = _iconfiguration.GetValue<string>("IncidentCreateJson"); //"{\"short_description\":\"Testing\",\"category\":\"General\",\"subcategory\":\"General\"}";
        //    string response = string.Empty;
        //    try
        //    {
        //        string url = string.Empty;
        //        if (customUrl != null)
        //        {
        //            url = customUrl;
        //        }
        //        else
        //        {
        //            url = url + tableName;
        //        }
        //        HttpClientHandler restHandler;
        //        string proxyAuthNeeded = "No";
        //        if (proxyAuthNeeded.ToUpper() == "Yes")
        //        {
        //            string proxyUrl = "";
        //            string ProxyUserName = "";
        //            string ProxyPassword = "";

        //            WebProxy proxy = new WebProxy(proxyUrl);
        //            restHandler = new HttpClientHandler
        //            {

        //                Credentials = new NetworkCredential(ProxyUserName, ProxyPassword),
        //                Proxy = proxy,
        //                UseProxy = true,
        //                UseDefaultCredentials = false
        //            };
        //        }
        //        else
        //        {
        //            restHandler = new HttpClientHandler
        //            {
        //                UseDefaultCredentials = true
        //            };
        //        }
        //        restHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
        //        restHandler.SslProtocols = SslProtocols.Tls12;
        //        restHandler.AllowAutoRedirect = false;

        //        using (HttpClient rest = new HttpClient(restHandler))
        //        {
        //            rest.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            rest.DefaultRequestHeaders.Add("authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("int_user" + ":" + "Admin@123")));

        //            if (method == "GET")
        //            {
        //                Uri sNowURI = null;
        //                if (!string.IsNullOrEmpty(json))
        //                {
        //                    sNowURI = new Uri(url + "?" + json);
        //                }
        //                else
        //                {
        //                    sNowURI = new Uri(url);
        //                }


        //                using (HttpResponseMessage res = rest.GetAsync(sNowURI).GetAwaiter().GetResult())
        //                {

        //                    res.EnsureSuccessStatusCode();
        //                    if (res.IsSuccessStatusCode)
        //                    {
        //                        response = res.Content.ReadAsStringAsync().Result;

        //                    }
        //                }
        //            }


        //            else if (method == "POST")
        //            {
        //                Uri sNowURI = new Uri(url);
        //                using (HttpResponseMessage res = await rest.PostAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/json")))
        //                {
        //                    res.EnsureSuccessStatusCode();
        //                    if (res.IsSuccessStatusCode)
        //                    {
        //                        response = res.Content.ReadAsStringAsync().Result;

        //                    }
        //                }
        //            }
        //            else if (method == "XmlPOST")
        //            {
        //                Uri sNowURI = new Uri(url);
        //                using (HttpResponseMessage res = await rest.PostAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/xml")))
        //                {
        //                    res.EnsureSuccessStatusCode();
        //                    if (res.IsSuccessStatusCode)
        //                    {
        //                        response = res.Content.ReadAsStringAsync().Result;

        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Uri sNowURI = new Uri(url);
        //                using (HttpResponseMessage res = await rest.PutAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/json")))
        //                {
        //                    res.EnsureSuccessStatusCode();
        //                    if (res.IsSuccessStatusCode)
        //                    {
        //                        response = res.Content.ReadAsStringAsync().Result;

        //                    }
        //                }
        //            }


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string snowDetails = "Snow api url: " + customUrl + ", request: " + json + ", snow response: " + response;
        //        snowDetails = snowDetails.Replace("\"", "\\\"");
        //        snowDetails = snowDetails.Replace(",", " ^ nl ^ ");
        //        return (false, ex.Message + "~" + snowDetails);
        //    }
        //    return (true, response);
        //}

    }
}