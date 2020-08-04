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
    public class ArtAccountUnlockWithoutLogin : CancelAndHelpDialog
    {
        private static readonly string DlgEmpID = "EmpIDDlg";
        string restricOption = string.Empty;
        static int EmpIDAttemptCount = 0;
        static bool IsValidEmpID = true;
        static string EmpID = string.Empty;
        protected readonly BotState UserState1;
        private static readonly ILogger Logger;
        static string LoginUserID = string.Empty;
        public ArtAccountUnlockWithoutLogin(UserState userstate) : base(nameof(ArtAccountUnlockWithoutLogin))
        {
            UserState1 = userstate;
            AddDialog(new AdaptiveCardPrompt(DlgEmpID, EmpIdValidation));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ValidateEmpIDStepAsync,
                ChoiceStepAsync,
                DecideOptionsStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ValidateEmpIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var cardJson = PrepareCard.ReadCard("artEmpId.json");

            //var cardAttachment = new Attachment()
            //{
            //    ContentType = "application/vnd.microsoft.card.adaptive",
            //    Content = JsonConvert.DeserializeObject(cardJson),
            //};

            var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
            var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());

            LoginUserID = sessionModels.UserID;
            var otpDetails = (ArtOTP)stepContext.Options;
            bool IsValidUser = false;
            if (LoginUserID == otpDetails.EmpID)
            {
                string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
                json = json.Replace("{UserID}", LoginUserID).Replace("{sessionId}", stepContext.Context.Activity.From.Id);

                IsValidUser = await APIRequest.ValidateEnrollUser(json);
            }

            if (!IsValidUser)
            {

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("I'm not able to find that employee ID number.  Try again."), cancellationToken);

                string card = "\\Cards\\artEmpId.json";
                string retryCard = "\\Cards\\artEmpId.json";
                //ArtOTP otpDetails = new ArtOTP();
                //var otpDetails = (ArtOTP)stepContext.Options;
                var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
                };

                var retryAdaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + retryCard);
                var retryAdaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(retryAdaptiveCardJson.ToString()),
                };

                var opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Attachments = new List<Attachment>() { adaptiveCardAttachment },
                        Type = ActivityTypes.Message,
                        //Text = "Please fill this form",
                    },
                    RetryPrompt = new Activity
                    {
                        Attachments = new List<Attachment>() { retryAdaptiveCardAttachment },
                        Type = ActivityTypes.Message,
                    }
                };

                return await stepContext.PromptAsync(DlgEmpID, opts, cancellationToken);
            }
            else
            {
                var dt = "Great! Now let's finish up your enrollment.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(dt), cancellationToken);
                return await stepContext.NextAsync(otpDetails.EmpID, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> ChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (EmpIDAttemptCount == 2 && !IsValidEmpID)
            {
                EmpIDAttemptCount = 0;
                string card = "\\Cards\\artEnrollValidError.json";
                var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
                };
                if (!string.IsNullOrWhiteSpace(card))
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
                }
            }

            var otpDetails = (ArtOTP)stepContext.Options;

            if (stepContext.Result.ToString() == "0" && EmpID != "0")
                otpDetails.EmpID = EmpID;
            else
            {
                var empDet = stepContext.Result.ToString();
                //var details = JObject.Parse(empDet);
                otpDetails.EmpID = empDet;
            }

            //var empDet = stepContext.Result.ToString();
            //var details = JObject.Parse(empDet);
            ////ArtOTP otpDetails = new ArtOTP();
            //var otpDetails = (ArtOTP)stepContext.Options;
            //otpDetails.EmpID = details["Employeeid"].ToString();
            string botid = (string)stepContext.State["turn.Activity.From.Id"];
            
            //ValidateEnrollUser
            //bool IsValidUser = true;//await APIRequest.ValidateEnrollUser(details["Employeeid"].ToString(), botid);
            //ValidateEnrollUser

            if (IsValidEmpID)
            {
                string card = "\\Cards\\artChoice.json";
                var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
                };
                if (!string.IsNullOrWhiteSpace(card))
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
                }
            }

            return await stepContext.EndDialogAsync(otpDetails.EmpID, cancellationToken);
            //return await stepContext.NextAsync(otpDetails.EmpID, cancellationToken);
        }

        private async Task<DialogTurnResult> PromptEnrollmentChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string card = "\\Cards\\artChoice.json";
            Incident incidentDetails = new Incident();
            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
            //JObject json = JObject.Parse(adaptiveCardJson.Replace("{botID}", botid));
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
            };
            if (!string.IsNullOrWhiteSpace(card))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
            }

            return await stepContext.NextAsync(incidentDetails.IncidentDesc, cancellationToken);
        }

        private async Task<DialogTurnResult> DecideOptionsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otpDetails = (ArtOTP)stepContext.Options;

            string choice = (string)stepContext.Result;

            if (choice == "Register for One-Time Pin")
            {

                var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
                var sessionModels = await sessionModelsAccessors.GetAsync(stepContext.Parent.Context, () => new SessionModel());

                otpDetails.Password = sessionModels.Password;

                return await stepContext.BeginDialogAsync(nameof(ARTEnrollFinalDialog), otpDetails, cancellationToken);
            }
            else if (choice == "I'll come back later")
            {
                var dt = "Sounds like a plan. When you've located your employee ID, you can finish the process with me or at your areas ART webpage \n\n https://art.ricoh-usa.com \n\n https://art.ricoh.ca \n\n https://art.ricoh-la.com";
                //string msg = "Sounds like a plan. When you've located your employee ID, you can finish the process with me or at your areas ART webpage \n\n https://art.ricoh-usa.com \n\n https://art.ricoh.ca \n\n https://art.ricoh-la.com";
                //await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock1(msg)) }, cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(dt), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if (choice == "No Thanks")
            {
                var dt = "No Problem, you can still enroll by answering security questions.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(dt), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            return await stepContext.NextAsync(otpDetails.EmpID, cancellationToken);
        }

        private static async Task<bool> EmpIdValidation(PromptValidatorContext<JObject> promptContext, CancellationToken cancellationToken)
        {

            //string empid = promptContext.Context.Activity.Text.ToString();
            //string empid1 = promptContext.Recognized.Value.ToString();

            // var empDet = promptContext.Context.Activity.Text.ToString();
            var empDet1 = promptContext.Recognized.Value.ToString();
            //string botuserid = promptContext.Context.TurnState["turn.Activity.From.Id"].ToString();

            //var details = JObject.Parse(empDet);
            var details1 = JObject.Parse(empDet1);

            // string EmpID = details["Employeeid"].ToString();
            string EmpID1 = details1["Employeeid"].ToString();

            var turnState = promptContext.Context.TurnState.Values.ElementAt(6).ToString();
            var turnStateDetails = JObject.Parse(turnState);
            string botUseriD = turnStateDetails.SelectToken("activity.from.id").ToString();
            bool IsValidUser = false;
            if (LoginUserID == EmpID1)
            {
                string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
                json = json.Replace("{UserID}", EmpID1).Replace("{sessionId}", botUseriD);
                //Logger.LogError("before Validation -EmpIdValidation : " + json);

                IsValidUser = await APIRequest.ValidateEnrollUser(json);
                // This condition is our validation rule. You can also change the value at this point.
                //EmpNo = empid;
                //Logger.LogError("after Validation -EmpIdValidation : " + json + " IsOTPValid : " + IsValidUser.ToString());

                if (!IsValidUser && promptContext.AttemptCount == 1)
                {
                    promptContext.Context.SendActivityAsync(MessageFactory.Text("That still isn't a valid employee ID number. Your employee ID number is the numeric number used to identify you here at Ricoh. If you need help locating this number, you can check in eBiz, on your pay stub or ask your manager for help."), cancellationToken);
                }
                if (!IsValidUser && promptContext.AttemptCount == 2)
                {
                    //promptContext.Context.SendActivityAsync(MessageFactory.Text("I'm not able to find that employee ID number.  Try again."), cancellationToken);

                    EmpIDAttemptCount = promptContext.AttemptCount;
                    IsValidEmpID = IsValidUser;

                    return await Task.FromResult(true);
                }
            }
            else
            {
                if (!IsValidUser && promptContext.AttemptCount == 1)
                {
                    promptContext.Context.SendActivityAsync(MessageFactory.Text("That still isn't a valid employee ID number. Your employee ID number is the numeric number used to identify you here at Ricoh. If you need help locating this number, you can check in eBiz, on your pay stub or ask your manager for help."), cancellationToken);
                }

                if (!IsValidUser && promptContext.AttemptCount == 2)
                {
                    //promptContext.Context.SendActivityAsync(MessageFactory.Text("It seems you are having trouble locating your employee ID number.  Do you want to speak with a live agent or come back and register once you have located your employee ID number?"), cancellationToken);

                    EmpIDAttemptCount = promptContext.AttemptCount;
                    IsValidEmpID = IsValidUser;

                    return await Task.FromResult(true);
                }
                //await promptContext.Context.SendActivityAsync(MessageFactory.Text("Emp id is not matched."), cancellationToken);
                return await Task.FromResult(IsValidUser);
            }

            EmpID = EmpID1;

            //return Task.FromResult(promptContext.Recognized.Succeeded && IsValidMob);
            return await Task.FromResult(IsValidUser);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText = string.Empty;

            var incidentDetails = (Incident)stepContext.Options;

            restricOption = string.Empty;
            restricOption = (string)stepContext.Result;
            if (restricOption == "Create Incident" || restricOption == "Check Incident Status" || restricOption == "Top 5 Incidents List")
                return await stepContext.EndDialogAsync(null, cancellationToken);

            incidentDetails.EmailID = (string)stepContext.Result;
            incidentDetails.IncidentDate = DateTime.Now.ToString();

            messageText = $"Please confirm, you have requested for creating Incident with below details - Description : {incidentDetails.IncidentDesc} - EmailID: {incidentDetails.EmailID} - Dated: {incidentDetails.IncidentDate}. Is this correct?";

            //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(messageText)), Style = ListStyle.HeroCard }, cancellationToken);

        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var incidentDetails = (Incident)stepContext.Options;

                return await stepContext.EndDialogAsync(incidentDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}


