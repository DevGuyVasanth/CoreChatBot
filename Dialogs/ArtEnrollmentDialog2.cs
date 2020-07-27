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

namespace CoreBot.Dialogs
{
    public class ArtEnrollmentDialog2 : ComponentDialog
    {

        static string AdaptivePromptId = "adaptive";
        string restricOption = string.Empty;
        bool isNotSkip = true;
        private readonly IStatePropertyAccessor<ArtOTP> _userProfileAccessor;
        static int EmpIDAttemptCount = 0;
        static bool IsValidEmpID = true;
        static string EmpNo = string.Empty;
        public ArtEnrollmentDialog2(UserState userState) : base(nameof(ArtEnrollmentDialog2))
        {
            _userProfileAccessor = userState.CreateProperty<ArtOTP>("ArtOTP");

            var waterfallSteps = new WaterfallStep[]
            {
                ValidateEmpIDStepAsync,
                DecideOptionsStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), EmpIdValidation));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> ValidateEmpIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otpDetails = (ArtOTP)stepContext.Options;

                string card = "\\Cards\\artEmpId.json";
                //ArtOTP otpDetails = new ArtOTP();
                //var otpDetails = (ArtOTP)stepContext.Options;
                var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
                };

                var opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Attachments = new List<Attachment>() { adaptiveCardAttachment },
                        Type = ActivityTypes.Message,
                        //Text = "Please fill this form",
                    }
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), opts, cancellationToken);
  
        }

        private static Task<bool> EmpIdValidation(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {

            string empid = promptContext.Context.Activity.Text.ToString();
            string empid1 = promptContext.Recognized.Value.ToString();

            bool IsValidUser = true;
            // This condition is our validation rule. You can also change the value at this point.
            EmpNo = empid;
            if (!IsValidUser && promptContext.AttemptCount == 3)
            {
                EmpIDAttemptCount = promptContext.AttemptCount;
                IsValidEmpID = IsValidUser;

                return Task.FromResult(true);
            }
            //return Task.FromResult(promptContext.Recognized.Succeeded && IsValidMob);
            return Task.FromResult(IsValidUser);
        }

        private static async Task<DialogTurnResult> UserFormAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var cardJson = PrepareCard.ReadCard("artEmpId.json");

            //var cardAttachment = new Attachment()
            //{
            //    ContentType = "application/vnd.microsoft.card.adaptive",
            //    Content = JsonConvert.DeserializeObject(cardJson),
            //};

            string card = "\\Cards\\artEmpId.json";
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;
            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
            };

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() { adaptiveCardAttachment },
                    Type = ActivityTypes.Message,
                    //Text = "Please fill this form",
                }
            };

            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> ResultUserFormAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var empDet = stepContext.Result.ToString();
            var details = JObject.Parse(empDet);
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;
            otpDetails.EmpID = details["Employeeid"].ToString();
            string botid = (string)stepContext.State["turn.Activity.From.Id"];

            //ValidateEnrollUser
            bool IsValidUser = true;//await APIRequest.ValidateEnrollUser(details["Employeeid"].ToString(), botid);
            //ValidateEnrollUser


            //string text = "Hi " + details["Employeeid"].ToString() + " your are valid user";
            //string msg = "Great Now lets finish up your enrollment";
            //if (!string.IsNullOrWhiteSpace(msg))
            //{
            //    var promptOptions = new PromptOptions
            //    {
            //        Prompt = MessageFactory.Text(msg)
            //    };
            //    //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
            //}

            //return await stepContext.NextAsync(incidentDetails.IncidentDesc, cancellationToken);
            if (IsValidUser)
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

            return await stepContext.BeginDialogAsync(otpDetails.EmpID, cancellationToken);
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

        private async Task<DialogTurnResult> DecideOptionsStepAsync
            (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otpDetails = (ArtOTP)stepContext.Options;

            string choice = (string)stepContext.Result;

            if (choice == "One Time Pin Code")
            {
                return await stepContext.BeginDialogAsync(nameof(ARTEnrollFinalDialog), otpDetails, cancellationToken);
            }

            return await stepContext.NextAsync(otpDetails.EmpID, cancellationToken);
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
