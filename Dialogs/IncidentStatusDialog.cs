using CoreBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace CoreBot.Dialogs
{
    public class IncidentStatusDialog : CancelAndHelpDialog
    {

        private const string EmailIDStepMsgText = "Please enter your valid Email Id";
        private const string INCStatusChkStepMsgText = "Please enter the Incident number.";
        string restricOption = string.Empty;

        public IncidentStatusDialog() : base(nameof(IncidentStatusDialog))
        {

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                EmailIDStepAsync,
                INCStatusCheckStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> EmailIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;
            restricOption = string.Empty;
            restricOption = (string)stepContext.Result;
            if (restricOption == "Create Incident" || restricOption == "Check Incident Status" || restricOption == "Top 5 Incidents List")
                return await stepContext.EndDialogAsync(null, cancellationToken);

            incidentDetails.ChoiceID = "2";

            if (incidentDetails.EmailID == null)
            {
                //var promptMessage = MessageFactory.Text(EmailIDStepMsgText, EmailIDStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(EmailIDStepMsgText)) }, cancellationToken);
            }

            return await stepContext.NextAsync(incidentDetails.IncidentDesc, cancellationToken);
        }

        private async Task<DialogTurnResult> INCStatusCheckStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;

            restricOption = string.Empty;
            restricOption = (string)stepContext.Result;
            if (restricOption == "Create Incident" || restricOption == "Check Incident Status" || restricOption == "Top 5 Incidents List")
                return await stepContext.EndDialogAsync(null, cancellationToken);

            incidentDetails.EmailID = (string)stepContext.Result;

            if (incidentDetails.IncidentNo == null)
            {
                //var promptMessage = MessageFactory.Text(INCStatusChkStepMsgText, INCStatusChkStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(INCStatusChkStepMsgText)) }, cancellationToken);
            }
            return await stepContext.NextAsync(incidentDetails.IncidentNo, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText = string.Empty;

            var incidentDetails = (Incident)stepContext.Options;

            restricOption = string.Empty;
            restricOption = (string)stepContext.Result;
            if (restricOption == "Create Incident" || restricOption == "Check Incident Status" || restricOption == "Top 5 Incidents List")
                return await stepContext.EndDialogAsync(null, cancellationToken);

            incidentDetails.IncidentNo = (string)stepContext.Result;
            messageText = $"Please confirm, you have requested for checking the status of the Incident #" + incidentDetails.IncidentNo + ". Is this correct?";

            //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
           
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(messageText)), Style= ListStyle.HeroCard }, cancellationToken);
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
