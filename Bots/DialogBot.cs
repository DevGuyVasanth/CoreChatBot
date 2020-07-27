// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.9.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Model;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using Microsoft.AspNetCore.Session;
using System.IO;
using System.Net;

namespace CoreBot.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        protected readonly IMemoryCache _cache;
        IConfiguration _iconfiguration;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IConfiguration iconfiguration)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
            _iconfiguration = iconfiguration;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            //await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);

            var val = turnContext.Activity.Value;
            var activity = turnContext.Activity;
            if (val != null)
            {
                if (val.ToString().Contains("Employeeid"))
                {
                    activity.Text = "ART Enrollment";
                    activity.TextFormat = "message";
                }
            }
            //var activity = turnContext.Activity;

            IMessageActivity reply = null;

            if (activity.Attachments != null && activity.Attachments.Any())
            {
                // We know the user is sending an attachment as there is at least one item
                // in the Attachments list.
                reply = HandleIncomingAttachment(activity);
            }
            else
            {
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                // Send at attachment to the user.
                //reply = await HandleOutgoingAttachment(turnContext, activity, cancellationToken);
            }


            //await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

            CacheUser user = GetLogOnUser(turnContext);
            if (user == null)
            {
                //(bool, string, bool) stat = CheckSignin(turnContext.Activity.From.Id);
                //if (!stat.Item1)
                //{
                //    var text = "Please login first";
                //    await turnContext.SendActivityAsync(MessageFactory.Text(text, text), cancellationToken);
                //    await ShowSigninCard(turnContext, cancellationToken);
                //}
                //else
                //{
                //    if (!stat.Item3)
                //    {
                //        string cacheConnectionString = "HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=False";
                //        ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);

                //        StackExchange.Redis.IDatabase db = connection.GetDatabase();

                //        SessionModel SessionModel = new SessionModel();
                //        SessionModel.IsSkipIntro = true;

                //        db.StringSet("RamukDB", JsonConvert.SerializeObject(SessionModel));

                //        var text = $"Hi {stat.Item2}, welcome to use the Bot!";
                //        await turnContext.SendActivityAsync(MessageFactory.Text(text, text), cancellationToken);
                //        IsSkipIntro = true;
                //    }
                //}

                //var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(UserProfile));
                //var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
                //if (string.IsNullOrEmpty(userProfile.Name))
                //{
                //    // Set the name to what the user provided.  
                //    userProfile.Name = turnContext.Activity.Text?.Trim();
                //    userProfile.botID = turnContext.Activity.From.Id;
                //    // Acknowledge that we got their name.  
                //    await turnContext.SendActivityAsync($"Thanks {userProfile.Name}. To see conversation data, type anything.");
                //}

                //var sessionModelsAccessors = UserState.CreateProperty<SessionModel>(nameof(SessionModel));
                //var sessionModels = await sessionModelsAccessors.GetAsync(turnContext, () => new SessionModel());
                //sessionModels.Password = "1234567";
                //sessionModels.DisplayName = "1234567";
                //sessionModels.EmailId = "1234567@qwe.com";
                //sessionModels.SessionKey = turnContext.Activity.From.Id.ToString();

                string botClientUserId = turnContext.Activity.From.Id;
                string botConversationId = turnContext.Activity.Conversation.Id;
                string botchannelID = turnContext.Activity.ChannelId;

                string cacheConnectionString = _iconfiguration["RedisCacheConnection"];

                try
                {
                    ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);

                    StackExchange.Redis.IDatabase db = connection.GetDatabase();

                    SessionModel SessionModel = new SessionModel();
                    SessionModel.DisplayName = "";
                    SessionModel.EmailId = "";
                    SessionModel.SessionKey = botClientUserId;
                    SessionModel.ConversationID = botConversationId;
                    SessionModel.IsSkipIntro = false;
                    SessionModel.UserLoginDetectServiceChk = 0;

                    db.StringSet(botClientUserId, JsonConvert.SerializeObject(SessionModel));
                    db.StringSet(botClientUserId + "artEnroll", JsonConvert.SerializeObject(SessionModel));

                    var sessionModelsAccessors = UserState.CreateProperty<SessionModel>(nameof(SessionModel));
                    var sessionModels = await sessionModelsAccessors.GetAsync(turnContext, () => new SessionModel());
                    if (string.IsNullOrWhiteSpace(sessionModels.DisplayName) && sessionModels.UserLoginDetectServiceChk == 0)
                    {
                        sessionModels.Password = "";
                        sessionModels.DisplayName = "";
                        sessionModels.EmailId = "";
                        sessionModels.SessionKey = botClientUserId;
                        sessionModels.ConversationID = botConversationId;
                        sessionModels.IsSkipIntro = false;
                        UserLoginDetectService userLoginDetect = new UserLoginDetectService(cancellationToken, _cache, turnContext, cacheConnectionString, UserState);
                    }
                }
                catch (Exception ex)
                {

                }



            }
            else
            {
                var text = $"Hi {user.UserName}, welcome to use the Bot!";
                await turnContext.SendActivityAsync(MessageFactory.Text(text, text), cancellationToken);
            }

        }

        private static async Task<IMessageActivity> HandleOutgoingAttachment(ITurnContext turnContext, IMessageActivity activity, CancellationToken cancellationToken)
        {
            // Look at the user input, and figure out what kind of attachment to send.
            IMessageActivity reply = null;
            //if (activity.Text.StartsWith("1"))
            //{
            //    reply = MessageFactory.Text("This is an inline attachment.");
            //    reply.Attachments = new List<Attachment>() { GetInlineAttachment() };
            //}
            //else if (activity.Text.StartsWith("2"))
            //{
            //    reply = MessageFactory.Text("This is an attachment from a HTTP URL.");
            //    reply.Attachments = new List<Attachment>() { GetInternetAttachment() };
            //}
            //else if (activity.Text.StartsWith("3"))
            //{
            //    reply = MessageFactory.Text("This is an uploaded attachment.");

            //    // Get the uploaded attachment.
            //    var uploadedAttachment = await GetUploadedAttachmentAsync(turnContext, activity.ServiceUrl, activity.Conversation.Id, cancellationToken);
            //    reply.Attachments = new List<Attachment>() { uploadedAttachment };
            //}
            //else
            //{
            //    // The user did not enter input that this bot was built to handle.
            //    reply = MessageFactory.Text("Your input was not recognized please try again.");
            //}

            return reply;
        }


        // Handle attachments uploaded by users. The bot receives an <see cref="Attachment"/> in an <see cref="Activity"/>.
        // The activity has a "IList{T}" of attachments.    
        // Not all channels allow users to upload files. Some channels have restrictions
        // on file type, size, and other attributes. Consult the documentation for the channel for
        // more information. For example Skype's limits are here
        // <see ref="https://support.skype.com/en/faq/FA34644/skype-file-sharing-file-types-size-and-time-limits"/>.
        private static IMessageActivity HandleIncomingAttachment(IMessageActivity activity)
        {
            var replyText = string.Empty;
            foreach (var file in activity.Attachments)
            {
                // Determine where the file is hosted.
                var remoteFileUrl = file.ContentUrl;

                // Save the attachment to the system temp directory.
                var localFileName = Path.Combine(Path.GetTempPath(), file.Name);

                // Download the actual attachment
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(remoteFileUrl, localFileName);
                }

                replyText += $"Attachment \"{file.Name}\"" +
                             $" has been received and saved to \"{localFileName}\"\r\n";
            }

            return MessageFactory.Text(replyText);
        }

        public (bool, string, bool) CheckSignin(string botid)
        {
            bool IsSkipIntro = false;
            string cacheConnectionString = _iconfiguration["RedisCacheConnection"]; //"HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=false";
            try
            {
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);
                IDatabase db = connection.GetDatabase();
                var val = db.StringGet(botid);
                if (!val.IsNullOrEmpty)
                {
                    var SessionData = JsonConvert.DeserializeObject(db.StringGet(botid));

                    dynamic blogObject = JsonConvert.DeserializeObject<dynamic>(db.StringGet(botid));
                    string name = blogObject["DisplayName"];
                    string description = blogObject["EmailId"];
                    string description1 = blogObject["SessionKey"];
                    IsSkipIntro = blogObject["IsSkipIntro"];

                    if (!string.IsNullOrEmpty(name))
                    {
                        return (true, name, IsSkipIntro);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "", IsSkipIntro);
            }
            return (false, "", IsSkipIntro);
        }

        private CacheUser GetLogOnUser(ITurnContext turnContext)
        {
            CacheUser user = null;
            //string userid = turnContext.Activity.From.Id;
            //string conversationId = turnContext.Activity.Conversation.Id;
            //List<CacheUser> users;

            //if (_cache.TryGetValue("users", out users))
            //{
            //    user = users.Find(u => u.BotClientUserId == userid && u.BotConversationId == conversationId && u.UserName != "");
            //}
            return user;
        }

        private async Task ShowSigninCard(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            //StateClient stateClient = new StateClient(new MicrosoftAppCredentials("xxxxxxxxxxx-4d43-4131-be5b-xxxxxxxxxxxx", "jkmbt39>:xxxxxxxxxxxx!~"));
            ////BotData userData = stateClient.BotState.GetUserData(context.Activity.ChannelId, context.Activity.From.Id);
            //BotData userData = stateClient.BotState.GetUserData("Azure", turnContext.Activity.From.Id);
            //string accesstoken = userData.GetProperty<string>("AccessToken");

            string botClientUserId = turnContext.Activity.From.Id;
            string botConversationId = turnContext.Activity.Conversation.Id;
            string botchannelID = turnContext.Activity.ChannelId;
            //var loginUrl = "http://localhost:3978/index.html?userid={botClientUserId}&conversationid={botConversationId}";
            //var loginUrl = "https://localhost:44327?userid={" + botClientUserId+"}&conversationid={"+botConversationId+"}";

            // string loginUrl = "http://localhost:29190/home/Index?botId={" + botClientUserId + "}&conversationid={" + botConversationId + "}";
            // string loginUrl = "http://localhost:29190/Home/Index?botId=" + botClientUserId + "&conversationid=" + botConversationId + "";
            string loginUrl = _iconfiguration["RedirectURL"] + "?botId=" + botClientUserId + "&conversationid=" + botConversationId + "&request_Type=artMainLogin";




            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            var signinCard = new SigninCard
            {
                Text = "Please Sign In",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign-in", value: loginUrl) },
            };
            reply.Attachments.Add(signinCard.ToAttachment());

            //List<CacheUser> users;
            //if (!_cache.TryGetValue("users", out users))
            //{
            //    users = new List<CacheUser>();
            //}
            //if (!users.Any(u => u.BotClientUserId == botClientUserId && u.BotConversationId == botConversationId))
            //{
            //    users.Add(new CacheUser(botClientUserId, botConversationId, turnContext, cancellationToken));
            //    _cache.Set("users", users, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(7)));
            //}
            await turnContext.SendActivityAsync(reply, cancellationToken);

            //string cacheConnectionString = "HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=False";
            string cacheConnectionString = _iconfiguration["RedisCacheConnection"];

            try
            {
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);

                StackExchange.Redis.IDatabase db = connection.GetDatabase();

                SessionModel SessionModel = new SessionModel();
                SessionModel.DisplayName = "";
                SessionModel.EmailId = "";
                SessionModel.SessionKey = botClientUserId;
                SessionModel.ConversationID = botConversationId;
                SessionModel.IsSkipIntro = false;

                db.StringSet(botClientUserId, JsonConvert.SerializeObject(SessionModel));
                db.StringSet(botClientUserId + "artEnroll", JsonConvert.SerializeObject(SessionModel));

                //var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(SessionModel));
                //var userProfile = await userStateAccessors.GetAsync(turnContext, () => new SessionModel());
                //if (string.IsNullOrEmpty(userProfile.DisplayName))
                //{

                //    //// Set the name to what the user provided.  
                //    //userProfile.Name = turnContext.Activity.Text?.Trim();
                //    //userProfile.botID = turnContext.Activity.From.Id;
                //    //// Acknowledge that we got their name.  
                //    //await turnContext.SendActivityAsync($"Thanks {userProfile.Name}. To see conversation data, type anything.");
                //}

            }
            catch (Exception ex)
            {

            }

            var sessionModelsAccessors = UserState.CreateProperty<SessionModel>(nameof(SessionModel));
            var sessionModels = await sessionModelsAccessors.GetAsync(turnContext, () => new SessionModel());
            if (string.IsNullOrWhiteSpace(sessionModels.DisplayName))
            {
                sessionModels.Password = "";
                sessionModels.DisplayName = "";
                sessionModels.EmailId = "";
                sessionModels.SessionKey = botClientUserId;
                sessionModels.ConversationID = botConversationId;
                sessionModels.IsSkipIntro = false;
            }
            UserLoginDetectService userLoginDetect = new UserLoginDetectService(cancellationToken, _cache, turnContext, cacheConnectionString, UserState);
        }
    }
}
