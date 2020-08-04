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
using System.Text.RegularExpressions;

namespace CoreBot.Dialogs
{
    public class ArtOTPDialog : CancelAndHelpDialog
    {
        static string AdaptivePromptId = "adaptive";

        public ArtOTPDialog() : base(nameof(ArtOTPDialog))
        {
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                sendOtp,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> sendOtp(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            string response = string.Empty;

            string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"AccountUnlock\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            json = json.Replace("{UserID}", "51296").Replace("{sessionId}", stepContext.Context.Activity.From.Id);

            response = await APIRequest.ValidateUserID(json);

            if (response == "true")
            {
                return await otpStep(stepContext);
            }
            else
            {
                var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Sorry!! something went wrong…") };
                await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                return await stepContext.EndDialogAsync(null,cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> otpStep(WaterfallStepContext stepContext)
        {
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter the 6 digit One Time Pin Code sent to your smartphone") };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions);
        }

        private static async Task<DialogTurnResult> otpCheck(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var otpInput = stepContext.Context.Activity.Text;


            string result = (string)stepContext.Result;

            result = result.Trim();
            bool otpPatterncheck = false;

            string OTPNumberPattern = "^[0-9]{6}$";
            if (result != null)
                otpPatterncheck = Regex.IsMatch(result, OTPNumberPattern);

            if (otpPatterncheck)
            {
                return await stepContext.NextAsync();
            }
            else
            {
                var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter six digit numeric pin code") };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> otpCheck1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var otpInput = stepContext.Context.Activity.Text;

            string result = (string)stepContext.Result;

            result = result.Trim();
            bool otpPatterncheck = false;

            string OTPNumberPattern = "^[0-9]{6}$";
            if (result != null)
                otpPatterncheck = Regex.IsMatch(otpInput, OTPNumberPattern);

            if (otpPatterncheck)
            {
                return await stepContext.NextAsync();
            }
            else
            {
                var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter six digit numeric pin code") };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> otpCheck2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var otpInput = stepContext.Context.Activity.Text;

            string result = (string)stepContext.Result;

            result = result.Trim();
            bool otpPatterncheck = false;

            string OTPNumberPattern = "^[0-9]{6}$";
            if (result != null)
                otpPatterncheck = Regex.IsMatch(otpInput, OTPNumberPattern);

            if (otpPatterncheck)
            {
                return await stepContext.NextAsync();
            }
            else
            {
                var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter six digit numeric pin code") };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> checkOTPStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _activitySessionId = stepContext.Context.Activity.From.Id;
            var otpInput = stepContext.Context.Activity.Text;

            string result = (string)stepContext.Result;

            result = result.Trim();
            bool otpPatterncheck = false;

            string OTPNumberPattern = "^[0-9]{6}$";
            if (result != null)
                otpPatterncheck = Regex.IsMatch(otpInput, OTPNumberPattern);

            string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"AccountUnlock\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            json = json.Replace("{UserID}", "48123").Replace("{sessionId}", stepContext.Context.Activity.From.Id);

            string response = await APIRequest.ValidateUserID(json);

            if (response == "true")
            {
                return await stepContext.NextAsync();
            }
            else
            {
                var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter six digit numeric pin code") };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
        }
    }
}