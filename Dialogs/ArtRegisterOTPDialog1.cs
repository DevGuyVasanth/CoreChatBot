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
    public class ArtRegisterOTPDialog1 : CancelAndHelpDialog
    {
        static string AdaptivePromptId = "adaptive";
        static string DlgMobileId = "MobileDlg";
        static string DlgLanguageId = "LanguageListDlg";

        public ArtRegisterOTPDialog1() : base(nameof(ArtRegisterOTPDialog1))
        {
            //AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new NumberPrompt<int>(DlgMobileId, MobileNumberValidation));
            AddDialog(new ChoicePrompt(DlgLanguageId, ChoiceValidataion));

            //AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                UserFormAsync,
                ResultUserFormAsync,
                MobileNumberAsync,
                //SendOtpStepAsync,
                //ReenterMobileNoStepAsync,
                //VaidateOtpStepAsync,
                //ReVaidateOtpStepAsync,
                //SelectSecurityQn1StepAsync,
                //SelectSecurityAn1StepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> UserFormAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string card = "\\Cards\\artCountryDrpDown.json";
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;
            otpDetails.IsValidMobile = false;
            otpDetails.IsValidOTP = false;

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

            string text = "Selected " + details["myCountry"].ToString() + " Country code";
            otpDetails.CountryCode = details["myCountry"].ToString().Replace("+", "");
            string msg = "Great, now enter your mobile number.";
            if (!string.IsNullOrWhiteSpace(msg))
            {
                //var promptOptions = new PromptOptions
                //{
                //    Prompt = MessageFactory.Text(msg),
                //    RetryPrompt = MessageFactory.Text("Enter Valid mobile No")
                //};
                //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
               
               return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)),RetryPrompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock("Enter Valid mobile No")) }, cancellationToken);
            }

            return await stepContext.NextAsync(otpDetails.mobileNo, cancellationToken);
        }

        private Task<bool> ChoiceValidataion(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }


        private static async Task<bool> MobileNumberValidation(PromptValidatorContext<int> promptcontext, CancellationToken cancellationtoken)
        {
            if (!promptcontext.Recognized.Succeeded)
            {
                await promptcontext.Context.SendActivityAsync("Hello, Please enter the valid mobile no",
                    cancellationToken: cancellationtoken);

                return false;
            }

            int count = Convert.ToString(promptcontext.Recognized.Value).Length;
            if (count != 10)
            {
                await promptcontext.Context.SendActivityAsync("Hello , you are missing some number !!!",
                    cancellationToken: cancellationtoken);
                return false;
            }

            return true;
        }


        private static async Task<DialogTurnResult> MobileNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        {
            var mobileNo = stepContext.Result;

            var newMovieList = new List<string> { " Tamil ", " English ", " kaanda " };

            return await stepContext.PromptAsync(DlgLanguageId, new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select the Language"),
                Choices = ChoiceFactory.ToChoices(newMovieList),
                RetryPrompt = MessageFactory.Text("Select from the List")
            }, cancellationtoken);
        }


        private async Task<DialogTurnResult> SendOtpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;

            string mobileNumber = (string)stepContext.Result;
            //ArtOTP otpDetails = new ArtOTP();
            otpDetails.IsValidMobile = false;
            //mobile number validation
            bool IsValidMob = VaidateMobileNumber(mobileNumber);

            if (IsValidMob)
            {
                otpDetails.IsValidMobile = true;
                otpDetails.mobileNo = mobileNumber;
                string msg = "Pls enter the otp send to your mobile " + mobileNumber;
                //var promptOptions = new PromptOptions
                //{
                //    Prompt = MessageFactory.Text(msg)
                //};
                //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
            }
            else if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                otpDetails.IsValidMobile = false;
                string msg = "Pls enter valid mobile number.";
                //var promptOptions = new PromptOptions
                //{
                //    Prompt = MessageFactory.Text(msg)
                //};
                //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                //await stepContext.RepromptDialogAsync(cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
            }

            return await stepContext.NextAsync(otpDetails.mobileNo, cancellationToken);
        }

        private async Task<DialogTurnResult> ReenterMobileNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var incidentDetails = (Incident)stepContext.Options;
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;
            otpDetails.OTPNo = (string)stepContext.Result;
            if (!otpDetails.IsValidMobile)
            {
                string mobileNumber = (string)stepContext.Result;
                bool IsValidMob = VaidateMobileNumber(mobileNumber);

                //mobile number validation
                if (IsValidMob)
                {
                    otpDetails.IsValidMobile = true;
                    otpDetails.mobileNo = mobileNumber;
                    string msg = "Pls enter the otp send to your mobile " + mobileNumber;
                    var promptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text(msg)
                    };
                    //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
                }
                else if (!IsValidMob)
                {
                    otpDetails.IsValidMobile = false;
                    string msg = "Try again.";
                    var promptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text(msg)
                    };
                    //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
                }

                if (otpDetails.IsValidMobile == false)
                    return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            return await stepContext.NextAsync(otpDetails.OTPNo, cancellationToken);
        }

        private async Task<DialogTurnResult> VaidateOtpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;

            string otpNumber = (string)stepContext.Result;
            //ArtOTP otpDetails = new ArtOTP();
            bool IsValidOTP = VaidateOTPNumber(otpDetails.OTPNo);
            string botid = (string)stepContext.State["turn.Activity.From.Id"];
            bool IsValidUser = await APIRequest.ValidateEnrollUser(botid);
            //OTP number validation
            if (IsValidOTP)
            {
                string msg = "OTP Verification done " + otpNumber;
                otpDetails.IsValidOTP = true;
                otpDetails.OTPNo = otpNumber;
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(msg)
                };
                //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
            }
            else
            {
                string msg = "Invalid OTP.";
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(msg)
                };
                //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
            }

            //return await stepContext.NextAsync(otpDetails.OTPNo, cancellationToken);
        }

        private async Task<DialogTurnResult> ReVaidateOtpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;

            if (!otpDetails.IsValidOTP)
            {
                string otpNumber = (string)stepContext.Result;
                bool IsValidOTP = VaidateOTPNumber(otpNumber);

                //OTP number validation
                if (IsValidOTP)
                {
                    otpDetails.IsValidOTP = true;
                    otpDetails.OTPNo = otpNumber;
                    string msg = "OTP Verification done " + otpNumber;
                    var promptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text(msg)
                    };
                    //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
                }
                else
                {
                    string msg = "Pls try again.";
                    var promptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text(msg)
                    };
                    //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
                }
            }
            else
                return await stepContext.NextAsync(otpDetails.OTPNo, cancellationToken);

            //if (otpDetails.IsValidMobile == false)
            //    return await stepContext.EndDialogAsync(null, cancellationToken);
            //return await stepContext.NextAsync(otpDetails.OTPNo, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectSecurityQn1StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string card = "\\Cards\\artSecQn1.json";
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;
            otpDetails.IsValidMobile = false;
            otpDetails.IsValidOTP = false;

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
                }
            };

            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> SelectSecurityAn1StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var empDet = stepContext.Result.ToString();
            var details = JObject.Parse(empDet);
            //ArtOTP otpDetails = new ArtOTP();
            var otpDetails = (ArtOTP)stepContext.Options;
            string card = "\\Cards\\artEnrollSuccess.json";
            //string msg = "Enrolled successfully.";
            //if (!string.IsNullOrWhiteSpace(msg))
            //{
            //    var promptOptions = new PromptOptions
            //    {
            //        Prompt = MessageFactory.Text(msg)
            //    };
            //    //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)) }, cancellationToken);
            //}
            //return await stepContext.NextAsync(otpDetails.mobileNo, cancellationToken);
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
                }
            };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otpDetails = (ArtOTP)stepContext.Options;
            var empDet = stepContext.Result.ToString();

            if (otpDetails.IsValidOTP)
            {
                return await stepContext.EndDialogAsync(otpDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }

        private bool VaidateMobileNumber(string mobileNumber)
        {
            string mobNo = mobileNumber.Trim();

            string MatchPhoneNumberPattern = "^\\(?([0-9]{3})\\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
            if (mobNo != null)
                return Regex.IsMatch(mobNo, MatchPhoneNumberPattern);
            else
                return false;
        }
        private bool VaidateOTPNumber(string otpNo)
        {
            string otp = otpNo.Trim();

            string MatchPhoneNumberPattern = "^[0-9]{6}$";
            if (otp != null)
                return Regex.IsMatch(otp, MatchPhoneNumberPattern);
            else
                return false;
        }

    }
}
