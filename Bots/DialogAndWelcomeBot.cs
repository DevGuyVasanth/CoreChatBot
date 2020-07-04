// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.9.2

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

namespace CoreBot.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        IMemoryCache _cache;
        UserLoginDetectService _userLoginDetectService = null;
        IConfiguration _iconfiguration;
        ILogger<UserLoginDetectService> _logger;

        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IConfiguration iconfiguration)
            : base(conversationState, userState, dialog, logger, iconfiguration)
        {
            _iconfiguration = iconfiguration;
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
            //foreach (var member in membersAdded)
            //{
            //    // Greet anyone that was not the target (recipient) of this message.
            //    // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
            //    if (member.Id != turnContext.Activity.Recipient.Id)
            //    {
            //        var welcomeCard = CreateAdaptiveCardAttachment();
            //        var response = MessageFactory.Attachment(welcomeCard);
            //        await turnContext.SendActivityAsync(response, cancellationToken);
            //        await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            //    }
            //}
            if (turnContext.Activity.ChannelId != "directline" && turnContext.Activity.ChannelId != "webchat")
            {
                foreach (var member in membersAdded)
                {
                    if (member.Id != turnContext.Activity.Recipient.Id)
                    {
                        bool stat = CheckSignin(turnContext.Activity.From.Id);
                        if (!stat)
                            await ShowSigninCard(turnContext, cancellationToken);
                    }
                }
            }
        }

        public bool CheckSignin(string botid)
        {
            string cacheConnectionString = "HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=false,allowAdmin=true";
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

            string loginUrl = _iconfiguration["RedirectURL"] + "?botId=" + botClientUserId + "&conversationid=" + botConversationId + "";

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            var signinCard = new SigninCard
            {
                Text = "Login , BotId: " + botClientUserId,
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
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);

            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost,allowAdmin=true");
            //var server = connection.GetServer(cacheConnectionString);
            //server.FlushDatabase();

            //var endpoints = connection.GetEndPoints(true);
            //foreach (var endpoint in endpoints)
            //{
            //    var server = connection.GetServer(endpoint);
            //    server.FlushAllDatabases();
            //}

            IDatabase db = connection.GetDatabase();

            SessionModel SessionModel = new SessionModel();
            SessionModel.DisplayName = "";
            SessionModel.EmailId = "";
            SessionModel.SessionKey = botClientUserId;
            SessionModel.ConversationID = botConversationId;
            SessionModel.IsSkipIntro = false;

            db.StringSet(botClientUserId, JsonConvert.SerializeObject(SessionModel));

            UserLoginDetectService userLoginDetect = new UserLoginDetectService(cancellationToken, _cache, turnContext);

        }

        // Load attachment from embedded resource.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith("welcomeCard.json"));

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
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

