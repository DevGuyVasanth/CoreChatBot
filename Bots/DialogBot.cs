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
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);

            CacheUser user = GetLogOnUser(turnContext);
            if (user == null)
            {
                (bool, string, bool) stat = CheckSignin(turnContext.Activity.From.Id);
                if (!stat.Item1)
                {
                    var text = "Please login first";
                    await turnContext.SendActivityAsync(MessageFactory.Text(text, text), cancellationToken);
                    await ShowSigninCard(turnContext, cancellationToken);
                }
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
            }
            else
            {
                var text = $"Hi {user.UserName}, welcome to use the Bot!";
                await turnContext.SendActivityAsync(MessageFactory.Text(text, text), cancellationToken);
            }

        }

        public (bool, string, bool) CheckSignin(string botid)
        {
            string cacheConnectionString = "HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=false";
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);

            IDatabase db = connection.GetDatabase();
            //string userId = "RamukDB5";
            var val = db.StringGet(botid);
            bool IsSkipIntro = false;
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

            //string cacheConnectionString = "HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=False";
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

            StackExchange.Redis.IDatabase db = connection.GetDatabase();

            SessionModel SessionModel = new SessionModel();
            SessionModel.DisplayName = "";
            SessionModel.EmailId = "";
            SessionModel.SessionKey = botClientUserId;
            SessionModel.ConversationID = botConversationId;
            SessionModel.IsSkipIntro = false;

            db.StringSet(botClientUserId, JsonConvert.SerializeObject(SessionModel));

            UserLoginDetectService userLoginDetect = new UserLoginDetectService(cancellationToken, _cache, turnContext);
        }
    }
}
