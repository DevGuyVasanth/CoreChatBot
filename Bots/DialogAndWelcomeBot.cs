// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
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

namespace CoreBot.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        IMemoryCache _cache;
        UserLoginDetectService _userLoginDetectService = null;
        IConfiguration _iconfiguration;
        ILogger<UserLoginDetectService> _logger;
        protected readonly BotState UserState1;

        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IConfiguration iconfiguration)
            : base(conversationState, userState, dialog, logger, iconfiguration)
        {
            _iconfiguration = iconfiguration;
            UserState1 = userState;
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "webchat/join")
            {
                bool stat = CheckSignin(turnContext.Activity.From.Id);
                if (!stat)
                    await ShowSigninCard(turnContext, cancellationToken);
                //await ShowSigninCard(turnContext, cancellationToken);

            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    string botClientUserId = turnContext.Activity.From.Id;
                    string botConversationId = turnContext.Activity.Conversation.Id;
                    string botchannelID = turnContext.Activity.ChannelId;

                    var welcomeCard = CreateAdaptiveCardAttachment(botClientUserId, botConversationId);
                    
                    var response = MessageFactory.Attachment(welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);

                    

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
            }
            //if (turnContext.Activity.ChannelId != "directline" && turnContext.Activity.ChannelId != "webchat")
            //{
            //    foreach (var member in membersAdded)
            //    {
            //        if (member.Id != turnContext.Activity.Recipient.Id)
            //        {
            //            bool stat = CheckSignin(turnContext.Activity.From.Id);
            //            if (!stat)
            //                await ShowSigninCard(turnContext, cancellationToken);
            //        }
            //    }
            //}
        }

        public bool CheckSignin(string botid)
        {
            string cacheConnectionString = _iconfiguration["RedisCacheConnection"]; // "HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=false,allowAdmin=true";
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

                    if (!string.IsNullOrEmpty(name))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
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

            //string loginUrl = "https://localhost:44332/home/LoginWithAzure?channelId={" + botchannelID + "}&userId={" + botClientUserId + "}";

            string loginUrl = _iconfiguration["RedirectURL"] + "?botId=" + botClientUserId + "&conversationid=" + botConversationId + "&request_Type=artMainLogin";

            //var userStateAccessors = UserState1.CreateProperty<UserProfile>(nameof(UserProfile));
            //var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
            //if (string.IsNullOrEmpty(userProfile.Name))
            //{
            //    // Set the name to what the user provided.  
            //    userProfile.Name = turnContext.Activity.Text?.Trim();
            //    userProfile.botID = turnContext.Activity.From.Id;
            //    // Acknowledge that we got their name.  
            //    //await turnContext.SendActivityAsync($"Thanks {userProfile.Name}. To see conversation data, type anything.");
            //}

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

            string cacheConnectionString = _iconfiguration["RedisCacheConnection"];
            try
            {
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);

                IDatabase db = connection.GetDatabase();

                SessionModel SessionModel = new SessionModel();
                SessionModel.DisplayName = "";
                SessionModel.EmailId = "";
                SessionModel.SessionKey = botClientUserId;
                SessionModel.ConversationID = botConversationId;
                SessionModel.IsSkipIntro = false;

                db.StringSet(botClientUserId, JsonConvert.SerializeObject(SessionModel));
                db.StringSet(botClientUserId + "artEnroll", JsonConvert.SerializeObject(SessionModel));
            }
            catch (Exception ex)
            {

            }

            var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
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

            UserLoginDetectService userLoginDetect = new UserLoginDetectService(cancellationToken, _cache, turnContext, cacheConnectionString, UserState1);
        }

        // Load attachment from embedded resource.
        private Attachment CreateAdaptiveCardAttachment(string botId,string conversationId)
        {
            var cardResourcePath = "CoreBot.Cards.welcomeCard.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    adaptiveCard = adaptiveCard.Replace("{botId}", botId).Replace("{conversationid}", conversationId);
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }
        }
    }
}

