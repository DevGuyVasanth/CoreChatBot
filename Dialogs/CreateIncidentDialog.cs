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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace CoreBot.Dialogs
{
    public class CreateIncidentDialog : CancelAndHelpDialog
    {

        private const string ChoiceStepMsgText = "**Please Select anyone of the below option to proceed.** \n\n 1) Create Incident \n2) Incident Status Check \n3) Top 5 Incident List \n \n Eg : 1 ";
        private const string IncidentDescStepMsgText = "Please enter the description for creating Incident";
        private const string EmailIDStepMsgText = "Please enter your valid Email Id";
        private const string INCStatusChkStepMsgText = "Please enter the Incident number.";
        private const string DestinationStepMsgText = "Where would you like to travel to?";
        private const string OriginStepMsgText = "Where are you traveling from?";
        private const string PromptMsg = "Are you sure to reset the coversation?";

        string restricOption = string.Empty;
        public CreateIncidentDialog() : base(nameof(CreateIncidentDialog))
        {

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IncidentDescStepAsync,
                EmailIDStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IncidentDescStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;
            restricOption = string.Empty;
            restricOption = (string)stepContext.Result;
            if (restricOption == "Create Incident" || restricOption == "Check Incident Status" || restricOption == "Top 5 Incidents List")
                return await stepContext.EndDialogAsync(null, cancellationToken);
            incidentDetails.ChoiceID = "1";

            if (incidentDetails.IncidentDesc == null)
            {
                //var promptMessage = MessageFactory.Text(IncidentDescStepMsgText, IncidentDescStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(IncidentDescStepMsgText)) }, cancellationToken);

                //var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                //await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            return await stepContext.NextAsync(incidentDetails.IncidentDesc, cancellationToken);
        }


        private async Task<DialogTurnResult> EmailIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;
            //bool Isprompt = false;
            //restricOption = string.Empty;
            //restricOption = (string)stepContext.Result;
            //if (restricOption == "Create Incident" || restricOption == "Check Incident Status" || restricOption == "Top 5 Incidents List")
            //{
            //    Isprompt = true;
            //    var promptMessage = MessageFactory.Text(PromptMsg, PromptMsg, InputHints.ExpectingInput);
            //    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            //}

            //if (Isprompt)
            //    return await stepContext.EndDialogAsync(null, cancellationToken);

            restricOption = string.Empty;
            restricOption = (string)stepContext.Result;
            if (restricOption == "Create Incident" || restricOption == "Check Incident Status" || restricOption == "Top 5 Incidents List")
                return await stepContext.EndDialogAsync(null, cancellationToken);
            incidentDetails.IncidentDesc = (string)stepContext.Result;

            if (incidentDetails.EmailID == null)
            {
                //var promptMessage = MessageFactory.Text(EmailIDStepMsgText, EmailIDStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(EmailIDStepMsgText)) }, cancellationToken);
            }

            return await stepContext.NextAsync(incidentDetails.IncidentDesc, cancellationToken);
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
