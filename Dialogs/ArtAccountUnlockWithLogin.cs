using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.AspNetCore.Mvc;
using CoreBot;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.IO;
using Newtonsoft.Json;
using CoreBot.Cards;
using Newtonsoft.Json.Linq;
using CoreBot.Model;
using CoreBot.Dialogs;
using Microsoft.Extensions.Logging;

namespace CoreBot.Dialogs
{
    public class ArtAccountUnlockWithLogin : CancelAndHelpDialog
    {
        private static readonly string UserEmpID = "UserIDDlg";
        string restricOption = string.Empty;
        static int EmpIDAttemptCount = 0;
        static bool IsValidEmpID = true;
        static string EmpID = string.Empty;
        protected readonly BotState UserState1;
        private static readonly ILogger Logger;
        static string LoginUserID = string.Empty;
        static string troubleText = "I'm having trouble finding your account. Do you want to talk with a live agent or see the main menu again?";
        string captcha = string.Empty;

        public ArtAccountUnlockWithLogin(UserState userstate) : base(nameof(ArtAccountUnlockWithLogin))
        {
            UserState1 = userstate;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                apiCallStep,
                apiCallStep1,
                apiCallStep2,
                confirmSte,
                confirmStep,
                confirmStep1,
                confirmStep3,
                confirmStep4,
                confirmStep5,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> apiCallStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
            var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());

            string EmailId = sessionModels.EmailId;
            LoginUserID = sessionModels.UserID;
            var otpDetails = (ArtOTP)stepContext.Options;

            string response = string.Empty;

            string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"AccountUnlock\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            json = json.Replace("{UserID}", "51296").Replace("{sessionId}", stepContext.Context.Activity.From.Id);

            response = await APIRequest.ValidateUserID(json);

            if (response == "true")
            {
                return await stepContext.NextAsync(response, cancellationToken);
            }
            else if (response.Replace("\"", "") == "Invalid Userid. Please try with correct userid.")
            {
                var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Hmm… I didn't find an account with the email address: ${EmailId}.  Try again or ask to speak to a live agent") };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
            else if (response.Replace("\"", "") == "User not enrolled in ART. Please do enroll before proceeding to Unlock/Reset.")
            {
                var artUserNotEnrolled = CreateAdaptiveCardAttachment("CoreBot.Cards.artUserNotEnrolled.json");
                var cardResponse = MessageFactory.Attachment(artUserNotEnrolled);
                await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                return await stepContext.EndDialogAsync(response, cancellationToken);
            }
            else if (response.Replace("\"","") == "User account is not locked.")
            {
                //artAccountNotLock
                var artAccountNotLock = CreateAdaptiveCardAttachment("CoreBot.Cards.artAccountNotLock.json");
                var cardResponse = MessageFactory.Attachment(artAccountNotLock);
                await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                return await stepContext.EndDialogAsync(response, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(troubleText);
                var artAccountNotLock = CreateAdaptiveCardAttachment("CoreBot.Cards.artTroubleAccount.json");
                var cardResponse = MessageFactory.Attachment(artAccountNotLock);
                await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                return await stepContext.EndDialogAsync("false", cancellationToken);
            }
        }
        private async Task<DialogTurnResult> apiCallStep1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((string)stepContext.Result == "true")
            {
                return await stepContext.NextAsync((string)stepContext.Result, cancellationToken);
            }
            else
            {
                var _activitySessionId = stepContext.Context.Activity.From.Id;
                var textReq = stepContext.Context.Activity.Text;

                var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
                var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());

                string EmailId = sessionModels.EmailId;
                LoginUserID = sessionModels.UserID;
                var otpDetails = (ArtOTP)stepContext.Options;

                string response = string.Empty;

                string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"AccountUnlock\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
                json = json.Replace("{UserID}", "51296").Replace("{sessionId}", stepContext.Context.Activity.From.Id);

                response = await APIRequest.ValidateUserID(json);


                if (response == "true")
                {
                    return await stepContext.NextAsync(response, cancellationToken);
                }
                else if (response.Replace("\"", "") == "Invalid Userid. Please try with correct userid.")
                {
                    var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Hmm… I didn't find an account with the email address: ${EmailId}.  Try again or ask to speak to a live agent") };
                    return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                }
                else if (response.Replace("\"", "") == "User not enrolled in ART. Please do enroll before proceeding to Unlock/Reset.")
                {
                    var artUserNotEnrolled = CreateAdaptiveCardAttachment("CoreBot.Cards.artUserNotEnrolled.json");
                    var cardResponse = MessageFactory.Attachment(artUserNotEnrolled);
                    await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                    return await stepContext.EndDialogAsync(response, cancellationToken);
                }
                else if (response.Replace("\"", "") == "User account is not locked.")
                {
                    //artAccountNotLock
                    var artAccountNotLock = CreateAdaptiveCardAttachment("CoreBot.Cards.artAccountNotLock.json");
                    var cardResponse = MessageFactory.Attachment(artAccountNotLock);
                    await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                    return await stepContext.EndDialogAsync(response, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(troubleText);
                    var artAccountNotLock = CreateAdaptiveCardAttachment("CoreBot.Cards.artTroubleAccount.json");
                    var cardResponse = MessageFactory.Attachment(artAccountNotLock);
                    await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                    return await stepContext.EndDialogAsync("false", cancellationToken);
                }
            }
        }
        private async Task<DialogTurnResult> apiCallStep2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
            var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());

            //CaptchaResult result = new CaptchaResult();

            //result = await APIRequest.GetCaptcha();

            //string card = "\\Cards\\artCaptcha.json";
            //var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
            //adaptiveCardJson = adaptiveCardJson.Replace("{str}", result.CaptchBase64Data);
            //JObject json = JObject.Parse(adaptiveCardJson);
            //var adaptiveCardAttachment = new Attachment()
            //{
            //    ContentType = "application/vnd.microsoft.card.adaptive",
            //    Content = JsonConvert.DeserializeObject(json.ToString()),
            //};
            //var cardResponse = MessageFactory.Attachment(adaptiveCardAttachment);
            //await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
            //return await stepContext.EndDialogAsync();

            if ((string)stepContext.Result == "true")
            {
                var _activitySessionId = stepContext.Context.Activity.From.Id;
                var textReq = stepContext.Context.Activity.Text;
                object stata = stepContext.ActiveDialog.State["stepIndex"];
                CaptchaResult result = new CaptchaResult();

                result = await APIRequest.GetCaptcha();
                captcha = result.CaptchaCode;
                string card = "\\Cards\\artCaptcha.json";
                var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                adaptiveCardJson = adaptiveCardJson.Replace("{str}", result.CaptchBase64Data);
                JObject json = JObject.Parse(adaptiveCardJson);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(json.ToString()),
                };
                var cardResponse = MessageFactory.Attachment(adaptiveCardAttachment);
                await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                object stata1 = stepContext.ActiveDialog.State["stepIndex"];

                return await stepContext.NextAsync(captcha, cancellationToken);
            }
            else
            {
                var _activitySessionId = stepContext.Context.Activity.From.Id;
                var textReq = stepContext.Context.Activity.Text;

                //var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
                //var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());

                string EmailId = sessionModels.EmailId;
                LoginUserID = sessionModels.UserID;
                var otpDetails = (ArtOTP)stepContext.Options;

                string response = string.Empty;

                string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"AccountUnlock\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
                json = json.Replace("{UserID}", LoginUserID).Replace("{sessionId}", stepContext.Context.Activity.From.Id);

                response = await APIRequest.ValidateUserID(json);
                object stata = stepContext.ActiveDialog.State["stepIndex"];

                if (response == "true")
                {
                    CaptchaResult result = new CaptchaResult();

                    result = await APIRequest.GetCaptcha();
                    captcha = result.CaptchaCode;
                    string card = "\\Cards\\artCaptcha.json";
                    var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                    adaptiveCardJson = adaptiveCardJson.Replace("{str}", result.CaptchBase64Data);
                    JObject json1 = JObject.Parse(adaptiveCardJson);
                    var adaptiveCardAttachment = new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(json1.ToString()),
                    };
                    var cardResponse = MessageFactory.Attachment(adaptiveCardAttachment);
                    await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                    return await stepContext.NextAsync(captcha, cancellationToken);
                }
                else if (response.Replace("\"", "") == "Invalid Userid. Please try with correct userid.")
                {
                    var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Hmm… I didn't find an account with the email address: ${EmailId}.  Try again or ask to speak to a live agent") };
                    return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                }
                else if (response.Replace("\"", "") == "User not enrolled in ART. Please do enroll before proceeding to Unlock/Reset.")
                {
                    var artUserNotEnrolled = CreateAdaptiveCardAttachment("CoreBot.Cards.artUserNotEnrolled.json");
                    var cardResponse = MessageFactory.Attachment(artUserNotEnrolled);
                    await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                    return await stepContext.EndDialogAsync(response, cancellationToken);
                }
                else if (response.Replace("\"", "") == "User account is not locked.")
                {
                    //artAccountNotLock
                    var artAccountNotLock = CreateAdaptiveCardAttachment("CoreBot.Cards.artAccountNotLock.json");
                    var cardResponse = MessageFactory.Attachment(artAccountNotLock);
                    await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                    return await stepContext.EndDialogAsync(response, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(troubleText);
                    var artAccountNotLock = CreateAdaptiveCardAttachment("CoreBot.Cards.artTroubleAccount.json");
                    var cardResponse = MessageFactory.Attachment(artAccountNotLock);
                    await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                    return await stepContext.EndDialogAsync("false", cancellationToken);
                }
            }
        }
        private async Task<DialogTurnResult> confirmSte(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Enter the Captcha"),
            };
            object stata = stepContext.ActiveDialog.State["stepIndex"];
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
        private async Task<DialogTurnResult> confirmStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var userCaptcha = stepContext.Context.Activity.Text;
            object stata = stepContext.ActiveDialog.State["stepIndex"];

            if (userCaptcha == captcha)
                return await stepContext.NextAsync(userCaptcha, cancellationToken);
            else
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("That's not right captcha"),
                };

                //await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("That's not right captcha"), cancellationToken);
                CaptchaResult result = new CaptchaResult();

                result = await APIRequest.GetCaptcha();
                captcha = result.CaptchaCode;
                string card = "\\Cards\\artCaptcha.json";
                var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                adaptiveCardJson = adaptiveCardJson.Replace("{str}", result.CaptchBase64Data);
                JObject json1 = JObject.Parse(adaptiveCardJson);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(json1.ToString()),
                };
                var cardResponse = MessageFactory.Attachment(adaptiveCardAttachment);
                await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);

                object stata1 = stepContext.ActiveDialog.State["stepIndex"];

                return await stepContext.NextAsync(null, cancellationToken);

                //string card = "\\Cards\\artCaptcha.json";
                //var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                //adaptiveCardJson = adaptiveCardJson.Replace("{str}", result.CaptchBase64Data);
                //var adaptiveCardAttachment = new Attachment()
                //{
                //    ContentType = "application/vnd.microsoft.card.adaptive",
                //    Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
                //};
                //if (!string.IsNullOrWhiteSpace(card))
                //{
                //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
                //}
                //return await stepContext.NextAsync(captcha, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> confirmStep1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var userCaptcha = stepContext.Context.Activity.Text;

            if (userCaptcha == captcha)
                return await stepContext.NextAsync(userCaptcha, cancellationToken);
            else
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Try entering the Captcha again"),
                };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> confirmStep3(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var userCaptcha = stepContext.Context.Activity.Text;

            if (userCaptcha == captcha)
                return await stepContext.NextAsync(userCaptcha,cancellationToken);
            else
            {
                //var promptOptions = new PromptOptions
                //{
                //    Prompt = MessageFactory.Text("That's still not right captcha"),
                //};
                //await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("That's still not right captcha"), cancellationToken);
                CaptchaResult result = new CaptchaResult();

                result = await APIRequest.GetCaptcha();
                captcha = result.CaptchaCode;
                string card = "\\Cards\\artCaptcha.json";
                var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                adaptiveCardJson = adaptiveCardJson.Replace("{str}", result.CaptchBase64Data);
                JObject json1 = JObject.Parse(adaptiveCardJson);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(json1.ToString()),
                };
                var cardResponse = MessageFactory.Attachment(adaptiveCardAttachment);
                await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);


                return await stepContext.NextAsync(-1, cancellationToken);

            }
        }
        private async Task<DialogTurnResult> confirmStep4(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var userCaptcha = stepContext.Context.Activity.Text;

            if (userCaptcha == captcha)
                return await stepContext.NextAsync(userCaptcha,cancellationToken);
            else
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Try enter one more time"),
                };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> confirmStep5(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var userCaptcha = stepContext.Context.Activity.Text;

            if (userCaptcha == captcha)
            {
                //var artOTP = CreateAdaptiveCardAttachment("CoreBot.Cards.artOTP.json");
                var artOTP = CreateAdaptiveCardAttachment1("\\Cards\\artOTP.json");
                var cardResponse = MessageFactory.Attachment(artOTP);
                await stepContext.Context.SendActivityAsync(cardResponse, cancellationToken);
                return await stepContext.EndDialogAsync();
            }
            else
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Hmm… somethings not right.  Hang on while I direct you to a live agent."),
                };
                await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);

                return await stepContext.EndDialogAsync();
            }
        }
        private Attachment CreateAdaptiveCardAttachment(string cardPath)
        {
            var cardResourcePath = "CoreBot.Cards.artOTP.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }
        }
        private Attachment CreateAdaptiveCardAttachment1(string card)
        {
            //var cardResourcePath = "CoreBot.Cards.artOTP.json";

            //using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            //{
            //    using (var reader = new StreamReader(stream))
            //    {
            //        var adaptiveCard = reader.ReadToEnd();
            //        return new Attachment()
            //        {
            //            ContentType = "application/vnd.microsoft.card.adaptive",
            //            Content = JsonConvert.DeserializeObject(adaptiveCard),
            //        };
            //    }
            //}

            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
            JObject json1 = JObject.Parse(adaptiveCardJson);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(json1.ToString()),
            };

            return adaptiveCardAttachment;
        }
    }
}


