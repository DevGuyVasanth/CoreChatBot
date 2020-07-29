using CoreBot.Cards;
using CoreBot.Model;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class ARTEnrollFinalDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<ArtOTP> _userProfileAccessor;
        static string AdaptivePromptId = "adaptive";
        private static readonly string DlgOTPId = "OTPDlg";
        private static readonly string DlgCountryCode = "CountryCodeDlg";
        static int MobNoAttemptCount = 0;
        static int OTPAttemptCount = 0;
        static bool IsValidMobNumber = true;
        static bool IsValidOTPNumber = true;
        static string MobileNumber = string.Empty;
        static string OTPNumber = string.Empty;
        static string CountryCode = string.Empty;
        static string Password = string.Empty;
        static string UserId = string.Empty;
        private static readonly ILogger Logger;
        static string defaultCountryCode = "+1";

        public ARTEnrollFinalDialog(UserState userState)
            : base(nameof(ARTEnrollFinalDialog))
        {
            _userProfileAccessor = userState.CreateProperty<ArtOTP>("ArtOTP");


            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                ConfirmStepAsync,
                SelectCountryCodeStepAsync,
                GetCountryCodeStepAsync,
                VerifyMobNumberAndSendOtpStepAsync,
                ValidateOTPStepAsync,
                OTPSucessStepAsync,
                SelectSecurityQn1StepAsync,
                SelectSecurityAn1StepAsync,
                SuccessStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), MobileNumberValidation));
            //AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), OTPValidation));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            //AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), MobileNumberValidation));
            AddDialog(new NumberPrompt<int>(DlgOTPId, OTPValidation));
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText = string.Empty;

            messageText = $"Are you in the US or Canada?";

            //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(messageText)), Style = ListStyle.HeroCard }, cancellationToken);

            //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SelectCountryCodeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otpDetails = (ArtOTP)stepContext.Options;
            if ((bool)stepContext.Result)
            {
                otpDetails.CountryCode = defaultCountryCode;
                return await stepContext.NextAsync(otpDetails.CountryCode, cancellationToken);
            }


            string card = "\\Cards\\artCountryDrpDown.json";
            //ArtOTP otpDetails = new ArtOTP();

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

        private static async Task<DialogTurnResult> GetCountryCodeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var empDet = stepContext.Result.ToString();
            var otpDetails = (ArtOTP)stepContext.Options;
            if (empDet.ToString() == "+1")
            {
                otpDetails.CountryCode = defaultCountryCode;
                Password = otpDetails.Password;
                UserId = otpDetails.EmpID;
                return await stepContext.NextAsync(otpDetails.CountryCode, cancellationToken);
            }
            var details = JObject.Parse(empDet);
            //ArtOTP otpDetails = new ArtOTP();


            string text = "Selected " + details["myCountry"].ToString() + " Country code";
            otpDetails.CountryCode = details["myCountry"].ToString().Replace("+", "");


            CountryCode = details["myCountry"].ToString().Replace("+", "");
            Password = otpDetails.Password;
            UserId = otpDetails.EmpID;

            //string msg = "Great, now enter your mobile number.";
            //if (!string.IsNullOrWhiteSpace(msg))
            //{
            //    //var promptOptions = new PromptOptions
            //    //{
            //    //    Prompt = MessageFactory.Text(msg),
            //    //    RetryPrompt = MessageFactory.Text("Enter Valid mobile No")
            //    //};
            //    //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);

            //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock(msg)), RetryPrompt = (Activity)MessageFactory.Attachment(Respository.GenerateAdaptiveCardTextBlock("Enter Valid mobile No")) }, cancellationToken);
            //}

            return await stepContext.NextAsync(otpDetails.mobileNo, cancellationToken);
        }

        private async Task<DialogTurnResult> VerifyMobNumberAndSendOtpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otpDetails = (ArtOTP)stepContext.Options;
            if (!string.IsNullOrWhiteSpace(otpDetails.CountryCode))
            {

                // User said "yes" so we will be prompting for the age.
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Great, now enter your mobile number."),
                    RetryPrompt = MessageFactory.Text("That isn't a valid number, make sure you only enter numbers."),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ValidateOTPStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (MobNoAttemptCount == 3 && !IsValidMobNumber)
            {
                MobNoAttemptCount = 0;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please try again later."), cancellationToken);
                return await stepContext.EndDialogAsync();
            }

            var otpDetails = (ArtOTP)stepContext.Options;

            if (stepContext.Result.ToString() == "0" && MobileNumber != "0")
                otpDetails.mobileNo = MobileNumber;
            else
                otpDetails.mobileNo = stepContext.Result.ToString();

            MobileNumber = otpDetails.mobileNo;

            if (!string.IsNullOrWhiteSpace(otpDetails.mobileNo))
            {
                // User said "yes" so we will be prompting for the age.
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Your mobile device shoud get a text message in a minute with a pin number. Please enter it below."),
                    RetryPrompt = MessageFactory.Text("That's not the code, try again"),
                };

                return await stepContext.PromptAsync(DlgOTPId, promptOptions, cancellationToken);
            }
            else
            {
                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> OTPSucessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (OTPAttemptCount == 3 && !IsValidOTPNumber)
            {
                OTPAttemptCount = 0;
                string card = "\\Cards\\artEnrollLoginFailure.json";
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
            //otpDetails.OTPNo = stepContext.Result.ToString();
            if (stepContext.Result.ToString() == "0" && OTPNumber != "0")
                otpDetails.OTPNo = OTPNumber;
            else
                otpDetails.OTPNo = stepContext.Result.ToString();

            if (!string.IsNullOrWhiteSpace(otpDetails.CountryCode))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Great news, your mobile device is now registered for one time pin!"), cancellationToken);
            }

            return await stepContext.NextAsync(otpDetails.mobileNo, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectSecurityQn1StepAsync_old(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string card = "\\Cards\\artSecQn1.json";
            var otpDetails = (ArtOTP)stepContext.Options;
            //otpDetails.IsValidMobile = false;

            //otpDetails.IsValidOTP = false;
            var turnState = stepContext.Context.TurnState.Values.ElementAt(6).ToString();
            var turnStateDetails = JObject.Parse(turnState);
            string botUseriD = turnStateDetails.SelectToken("activity.from.id").ToString();

            string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"NewPassword\":\"null\",\"ConfirmNewPassword\":\"null\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"OTP\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            json = json.Replace("{UserID}", otpDetails.EmpID).Replace("{sessionId}", botUseriD);

            Root secQns = await APIRequest.GetAllSecurityQuestions(json);

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

        private async Task<DialogTurnResult> SelectSecurityQn1StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otpDetails = (ArtOTP)stepContext.Options;

            string Option = stepContext.Result.ToString();

            if (Option == "Main Menu")
            {
                return await stepContext.EndDialogAsync("MainMenu", cancellationToken);
            }
            else if (Option == "Chat with a live agent")
            {
                return await stepContext.EndDialogAsync("Chat with a live agent", cancellationToken);
            }

            string card = "\\Cards\\artSecQn1.json";
            //otpDetails.IsValidMobile = false;

            //otpDetails.IsValidOTP = false;
            var turnState = stepContext.Context.TurnState.Values.ElementAt(6).ToString();
            var turnStateDetails = JObject.Parse(turnState);
            string botUseriD = turnStateDetails.SelectToken("activity.from.id").ToString();

            string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"NewPassword\":\"null\",\"ConfirmNewPassword\":\"null\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"OTP\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            json = json.Replace("{UserID}", otpDetails.EmpID).Replace("{sessionId}", botUseriD);

            Root secQns = await APIRequest.GetAllSecurityQuestions(json);

            string AdaptiveCard = "{\"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\"type\": \"AdaptiveCard\",\"version\": \"1.0\",\"body\": [{QnsSet}],\"actions\": [{\"type\": \"Action.Submit\",\"title\": \"OK\"}]}";
            string finaltemplate = string.Empty;

            //string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"NewPassword\":\"null\",\"ConfirmNewPassword\":\"null\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"OTP\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            //json = json.Replace("{UserID}", "51296").Replace("{sessionId}", "123456789012sdsd");

            //Root secQns = await GetAllSecurityQuestions(json);

            for (int i = 0; i < secQns.questions.Count; i++)
            {
                string template = "{\"type\": \"TextBlock\",\"text\": \"Choose question in [Category] Category\"},{\"type\": \"Input.ChoiceSet\",\"id\": \"Qn[i]\",\"style\": \"compact\",\"isMultiSelect\": false,\"value\": \"1\",\"choices\": [{Qns}]},{\"type\": \"Input.Text\",\"id\": \"An[i]\",\"placeholder\": \"Answer\"}";

                template = template.Replace("[i]", (i + 1).ToString()).Replace("[Category]", secQns.questions[i].categoryname);
                string qnsSet = "{\"title\": \"{Qn}\",\"value\": \"{QnId}\"}";
                for (int j = 0; j < secQns.questions[i].lstQuestions.Count; j++)
                {
                    if (j == 0)
                    {
                        qnsSet = qnsSet.Replace("{Qn}", secQns.questions[i].lstQuestions[j].question_text).Replace("{QnId}", secQns.questions[i].lstQuestions[j].question_id.ToString());
                    }
                    else
                    {
                        qnsSet = qnsSet + "," + qnsSet.Replace("{Qn}", secQns.questions[i].lstQuestions[j].question_text).Replace("{QnId}", secQns.questions[i].lstQuestions[j].question_id.ToString());
                    }
                    template = template.Replace("{Qns}", qnsSet);
                }

                if (i == 0)
                {
                    finaltemplate = template;
                }
                else
                {
                    finaltemplate = finaltemplate + "," + template;
                }

            }
            AdaptiveCard = AdaptiveCard.Replace("{QnsSet}", finaltemplate);

            var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(AdaptiveCard.ToString()),
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

            var otpDetails = (ArtOTP)stepContext.Options;
            //string card = "\\Cards\\artEnrollSuccess.json";

            //var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);
            //var adaptiveCardAttachment = new Attachment()
            //{
            //    ContentType = "application/vnd.microsoft.card.adaptive",
            //    Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
            //};

            //var opts = new PromptOptions
            //{
            //    Prompt = new Activity
            //    {
            //        Attachments = new List<Attachment>() { adaptiveCardAttachment },
            //        Type = ActivityTypes.Message,
            //    }
            //};

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Congratulation! You are now enrolled in ART and can use the tool to unlock your account, reset your password or change your password."), cancellationToken);

            string card = "\\Cards\\artSecuritySuccess.json";
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
            else
                return await stepContext.EndDialogAsync();
        }


        private async Task<DialogTurnResult> SuccessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otpDetails = (ArtOTP)stepContext.Options;

            string choice = (string)stepContext.Result;

            if (choice == "Home")
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if (choice == "ART Account Management")
            {
                return await stepContext.BeginDialogAsync(nameof(ARTEnrollFinalDialog), otpDetails, cancellationToken);
            }
            else
                return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static async Task<bool> MobileNumberValidation(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {

            string mobno = promptContext.Context.Activity.Text.ToString();
            string mobno1 = promptContext.Recognized.Value.ToString();
            //if (promptContext.Recognized.Value.ToString() == "0" && mobno!= mobno1)
            //{
            //    promptContext.Recognized.Value = Convert.ToInt32(mobno);
            //}
            var turnState = promptContext.Context.TurnState.Values.ElementAt(6).ToString();
            var turnStateDetails = JObject.Parse(turnState);
            string botUseriD = turnStateDetails.SelectToken("activity.from.id").ToString();

            bool IsValidMob = await VaidateMobileNumber(mobno, botUseriD);
            // This condition is our validation rule. You can also change the value at this point.


            if (!IsValidMob && promptContext.AttemptCount == 3)
            {
                MobNoAttemptCount = promptContext.AttemptCount;
                IsValidMobNumber = IsValidMob;

                return await Task.FromResult(true);
            }

            MobileNumber = mobno;
            //return Task.FromResult(promptContext.Recognized.Succeeded && IsValidMob);
            return await Task.FromResult(IsValidMob);
        }

        private static async Task<bool> OTPValidation(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            string otpno = promptContext.Context.Activity.Text.ToString();
            string otpno1 = promptContext.Recognized.Value.ToString();
            string retryMsg = promptContext.Options.RetryPrompt.Text;

            var turnState = promptContext.Context.TurnState.Values.ElementAt(6).ToString();
            var turnStateDetails = JObject.Parse(turnState);
            string botUseriD = turnStateDetails.SelectToken("activity.from.id").ToString();

            bool IsValidOTP = await VaidateOTPNumber(otpno, botUseriD);
            if (!IsValidOTP && promptContext.AttemptCount == 1)
            {
                //promptContext.Options.RetryPrompt.Text = "That's still not right, try one more time to enter the pin code that was sent to your mobile device.";
            }
            else if (!IsValidOTP && promptContext.AttemptCount == 2)
            {
                promptContext.Options.RetryPrompt.Text = "That's still not right, try one more time to enter the pin code that was sent to your mobile device.";
            }
            else if (!IsValidOTP && promptContext.AttemptCount == 3)
            {
                OTPAttemptCount = promptContext.AttemptCount;
                IsValidOTPNumber = IsValidOTP;

                return await Task.FromResult(true);
            }
            OTPNumber = otpno;
            // This condition is our validation rule. You can also change the value at this point.
            return await Task.FromResult(promptContext.Recognized.Succeeded && IsValidOTP);
        }

        private static async Task<bool> VaidateMobileNumber(string mobileNumber, string botUserId)
        {
            string mobNo = mobileNumber.Trim();
            bool IsMobValid = false;

            string MatchPhoneNumberPattern = "^[0-9]{10}$";/*"^\\(?([0-9]{3})\\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";*/
            if (mobNo != null)
                IsMobValid = Regex.IsMatch(mobNo, MatchPhoneNumberPattern);

            string json = "{\"UserID\":\"{UserID}\",\"Password\":\"{Password}\",\"Activity\":\"UserEnrollment\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"{MobileNumber}\",\"IsPrivate\":\"false\",\"IsRegisteredForOTP\":\"true\",\"CountryCode\":\"{CountryCode}\",\"QuestionAnswerModelList\":\"null\"}";
            json = json.Replace("{UserID}", UserId).Replace("{Password}", Password).Replace("{sessionId}", botUserId).Replace("{MobileNumber}", mobNo).Replace("{CountryCode}", CountryCode);
            //Logger.LogError("before Validation -VaidateMobileNumber : " + json + " IsOTPValid : " + IsMobValid.ToString());

            if (IsMobValid)
                IsMobValid = await APIRequest.SendOTP(json);
            // Logger.LogError("after Validation - VaidateMobileNumber : " + json + " IsOTPValid : " + IsMobValid.ToString());

            return IsMobValid;
        }

        private static async Task<bool> VaidateOTPNumber(string otpNo, string botUserID)
        {
            string otp = otpNo.Trim();
            bool IsOTPValid = false;

            string MatchPhoneNumberPattern = "^[0-9]{6}$";
            if (otp != null)
                IsOTPValid = Regex.IsMatch(otp, MatchPhoneNumberPattern);

            string json = "{\"UserID\":\"{UserID}\",\"Password\":\"{Password}\",\"Activity\":\"UserEnrollment\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"{MobileNumber}\",\"IsPrivate\":\"false\",\"IsRegisteredForOTP\":\"true\",\"CountryCode\":\"{CountryCode}\",\"OTP\":\"{OTP}\",\"QuestionAnswerModelList\":\"null\"}";
            json = json.Replace("{UserID}", UserId).Replace("{Password}", Password).Replace("{sessionId}", botUserID).Replace("{MobileNumber}", MobileNumber).Replace("{CountryCode}", CountryCode).Replace("{OTP}", otpNo);
            //Logger.LogError("before Validation -VaidateOTPNumber : " + json + " IsOTPValid : " + IsOTPValid.ToString());

            if (IsOTPValid)
                IsOTPValid = await APIRequest.ValidateOTP(json);

            //Logger.LogError("after Validation - VaidateOTPNumber : " + json + " IsOTPValid : " + IsOTPValid.ToString());

            return IsOTPValid;
        }
    }
}
