using CoreBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json.Linq;
using System.IO;


namespace CoreBot.Dialogs
{
    public class IncidentDialog : CancelAndHelpDialog
    {
       private const string ChoiceStepMsgText = "**Please Select anyone of the below option to proceed.** \n\n 1) Create Incident \n2) Incident Status Check \n3) Top 5 Incident List \n \n Eg : 1 ";
        private const string IncidentDescStepMsgText = "Please enter the description for creating Incident";
        private const string EmailIDStepMsgText = "Enter your valid Email Id";
        private const string INCStatusChkStepMsgText = "Please enter the Incident number.";
        private const string DestinationStepMsgText = "Where would you like to travel to?";
        private const string OriginStepMsgText = "Where are you traveling from?";
        string choice =string.Empty;
        bool skiptoLast = false;

      //  public string CHOICEPROMPT { get; private set; }

        public IncidentDialog() : base(nameof(IncidentDialog))
        {

        
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
           AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
           
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
           //    PromptWithAdaptiveCardAsync,
                ChoiceDescStepAsync,
                 SelectedOptionDescStepAsync,
                IncidentDescStepAsync,
                EmailIDStepAsync,
                INCStatusCheckStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
     
        private async Task<DialogTurnResult> ChoiceDescStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;
            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + "\\Cards\\incidentCard.json");
            JObject json = JObject.Parse(adaptiveCardJson);
              var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(json.ToString()),
            };
           
        if (incidentDetails.IncidentDesc == null)
            {
             //   var response = MessageFactory.Attachment(welcomeCard);
             //   await turnContext.SendActivityAsync(response, cancellationToken);
              //  await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
           //var promptMessage = MessageFactory.Text(ChoiceStepMsgText, ChoiceStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment) }, cancellationToken);
            }
            return await stepContext.NextAsync(incidentDetails.IncidentDesc, cancellationToken);
        }


        private async Task<DialogTurnResult> SelectedOptionDescStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;
            incidentDetails.ChoiceID = (string)stepContext.Result;


            string errormsg = string.Empty;
            string Nextmsg = string.Empty;

            if (incidentDetails.IncidentDesc == null)
            {
                bool isInterger = false;
                skiptoLast = false;
                try
                {
                    choice = stepContext.Result.ToString();
                    isInterger = true;
                }
                catch
                {

                }

                //if (isInterger)
                //{
                //    if (choice < 0 || choice > 3)
                //    {
                //        skiptoLast = true;
                //        errormsg = "**Wrong Choice**.Please enter value between 1 and 3";
                //        var promptMessage = MessageFactory.Text(errormsg, errormsg, InputHints.ExpectingInput);
                //        await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                //        return await stepContext.EndDialogAsync(null, cancellationToken);

                //    }
                //}
                //else
                //{
                //    skiptoLast = true;
                //    errormsg = "Please enter interger values between 1 and 3.";
                //    var promptMessage = MessageFactory.Text(errormsg, errormsg, InputHints.ExpectingInput);
                //    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                //    return await stepContext.EndDialogAsync(null, cancellationToken);
                //}
                //if (!isInterger || choice < 0 || choice > 3)
                //    return await stepContext.EndDialogAsync(null, cancellationToken);

                if (choice == "Create Incident")
                {
                    Nextmsg = "Please enter the description for creating Incident.";
                }
                else if (choice == "Check Incident Status")
                {
                    Nextmsg = "Please enter the incident number";
                }
                else if (choice == "Top 5 Incidents List")
                {
                    Nextmsg = "Please find the Top 5 Incident List";
                }

                if (choice != "Top 5 Incidents List")
                {
                    //if(stepContext.Result)
                   // var promptMessage = MessageFactory.Text(Nextmsg, Nextmsg, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(Nextmsg)) }, cancellationToken);
                }
            }
            return await stepContext.NextAsync(incidentDetails.ChoiceID, cancellationToken);
        }
   
        private async Task<DialogTurnResult> IncidentDescStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;
            if (!skiptoLast)
            {
                if (choice.ToString() == "Create Incident")
                {
                    incidentDetails.IncidentDesc = (string)stepContext.Result;
                    //if (incidentDetails.IncidentDesc == null)
                    //{
                    //    var promptMessage = MessageFactory.Text(IncidentDescStepMsgText, IncidentDescStepMsgText, InputHints.ExpectingInput);
                    //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                    //}
                }
                else if (choice.ToString() == "Check Incident Status")
                {
                    incidentDetails.IncidentNo = (string)stepContext.Result;
                }
            }
            return await stepContext.NextAsync(incidentDetails.IncidentDesc, cancellationToken);
        }
        private async Task<DialogTurnResult> EmailIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;
         
            if (!skiptoLast)
            {
                incidentDetails.IncidentDesc = (string)stepContext.Result;

                if (incidentDetails.EmailID == null)
                {
                    var promptMessage = MessageFactory.Text(EmailIDStepMsgText, EmailIDStepMsgText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
            }
            return await stepContext.NextAsync(incidentDetails.IncidentDesc, cancellationToken);
        }
        //private async Task<DialogTurnResult> TravelDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var bookingDetails = (BookingDetails)stepContext.Options;

        //    bookingDetails.Origin = (string)stepContext.Result;

        //    if (bookingDetails.TravelDate == null || IsAmbiguous(bookingDetails.TravelDate))
        //    {
        //        return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), bookingDetails.TravelDate, cancellationToken);
        //    }

        //    return await stepContext.NextAsync(bookingDetails.TravelDate, cancellationToken);
        //}

        private async Task<DialogTurnResult> INCStatusCheckStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var incidentDetails = (Incident)stepContext.Options;
            if (!skiptoLast)
            {
                incidentDetails.EmailID = (string)stepContext.Result;
                if (choice.ToString() == "Check Incident Status")
                {
                    if (incidentDetails.IncidentNo == null)
                    {
                        var promptMessage = MessageFactory.Text(INCStatusChkStepMsgText, INCStatusChkStepMsgText, InputHints.ExpectingInput);
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                    }
                }
            }
            return await stepContext.NextAsync(incidentDetails.IncidentNo, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText = string.Empty;
            if (!skiptoLast)
            {
                var incidentDetails = (Incident)stepContext.Options;
                if (choice == "Create Incident")
                {
                    //incidentDetails.EmailID = (string)stepContext.Result;
                    incidentDetails.IncidentDate = DateTime.Now.ToString();

                    messageText = $"Please confirm, you have requested for creating Incident with below details - Description : {incidentDetails.IncidentDesc} - EmailID: {incidentDetails.EmailID} - Dated: {incidentDetails.IncidentDate}. Is this correct?";
                }
                else if (choice == "Check Incident Status")
                {
                    incidentDetails.IncidentNo = (string)stepContext.Result;
                    messageText = $"Please confirm, you have requested for checking the status of the Incident #" + incidentDetails.IncidentNo + ". Is this correct?";
                }
                else if (choice == "Top 5 Incidents List")
                {
                    messageText = "Are you requesting to display top 5 Incident?";
                }
            }
            else
            {
                messageText = "Are you want to reset the coversation?";
            }
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(messageText)) }, cancellationToken);
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


        private async Task<DialogTurnResult> PromptWithAdaptiveCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            // Define choices
            var choices = new[] { "Create Incident", "Incident Status Check", "Top 5 Incident List" };
            // Create card
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = choices.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            // Prompt
            return await stepContext.PromptAsync(nameof(ChoicePrompt),

                new PromptOptions
                {
                    Prompt = (Activity)MessageFactory.Attachment(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        // Convert the AdaptiveCard to a JObject
                        Content = JObject.FromObject(card),
                    }),
                    Choices = ChoiceFactory.ToChoices(choices),
                    // Don't render the choices outside the card
                    Style = ListStyle.None,
                },
                cancellationToken);



        }
    }
}