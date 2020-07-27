using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Model
{
    public class UserLoginDetectService : IHostedService, IDisposable
    {
        private readonly ILogger Logger;
        private Timer Timer;
        private IMemoryCache Cache;
        SessionModel SessionDataObj = new SessionModel();
        string _cacheConnectionString = string.Empty;
        ITurnContext _turnContext;
        CancellationToken _cancellationToken;
        protected readonly BotState UserState1;
        Dialog _dialog;
        public UserLoginDetectService(ILogger<UserLoginDetectService> logger, IMemoryCache cache)
        {
            Logger = logger;
            Cache = cache;
        }

        public UserLoginDetectService(CancellationToken cancellationToken, IMemoryCache cache, ITurnContext turnContext, string conString, BotState UserState)
        {
            Cache = cache;
            _turnContext = turnContext;
            _cancellationToken = cancellationToken;
            StartAsync(cancellationToken);
            _cacheConnectionString = conString;
            UserState1 = UserState;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Logger.LogInformation("UserLoginDetectService is starting.");
            Timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            //cacheConnectionString = "HexaBotRedis.redis.cache.windows.net,abortConnect=false,ssl=true,allowAdmin=true,password=YyiOPF+o8h8ygN42CwZuWVoMETSv0F+ZyZnP8AxCRHI=";// "HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=false";
            try
            {
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(_cacheConnectionString);

                IDatabase db = connection.GetDatabase();
                var val = db.StringGet(_turnContext.Activity.From.Id);

                if (!val.IsNullOrEmpty)
                {
                    var SessionData = JsonConvert.DeserializeObject(db.StringGet(_turnContext.Activity.From.Id));

                    dynamic blogObject = JsonConvert.DeserializeObject<dynamic>(db.StringGet(_turnContext.Activity.From.Id));
                    string name = blogObject["DisplayName"];
                    string UserName = blogObject["EmailId"];
                    string SessionKey = blogObject["SessionKey"];
                    bool IsSkipIntro = blogObject["IsSkipIntro"];
                    string password = blogObject["Password"];
                    string empID = blogObject["EmpID"];
                    string userId = blogObject["UserID"];
                    string ConversationID = blogObject["ConversationID"];
                    string LastName = blogObject["LastName"];
                    int UserLoginDetectServiceChk = blogObject["UserLoginDetectServiceChk"];

                    SessionModel sessionModel = new SessionModel();
                    sessionModel.DisplayName = name;
                    sessionModel.EmailId = UserName;
                    sessionModel.SessionKey = SessionKey;


                    if (sessionModel.UserLoginDetectServiceChk == 0)
                    {
                        DateTime dt = DateTime.Now;
                        sessionModel.UserLoginDetectServiceChk = 1;
                        db.StringSet(_turnContext.Activity.From.Id, JsonConvert.SerializeObject(sessionModel));
                        db.KeyExpire(_turnContext.Activity.From.Id, dt.AddMinutes(60));
                    }
                    if (!string.IsNullOrEmpty(name) && !IsSkipIntro)
                    {
                        DateTime dt = DateTime.Now;
                        sessionModel.IsSkipIntro = true;
                        db.StringSet(_turnContext.Activity.From.Id, JsonConvert.SerializeObject(sessionModel));
                        db.KeyExpire(_turnContext.Activity.From.Id, dt.AddMinutes(60));
                        var text = $"Hi {name}, what can I get you started with today?";

                        var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
                        var sessionModels = await sessionModelsAccessors.GetAsync(_turnContext, () => new SessionModel());
                        if (string.IsNullOrWhiteSpace(sessionModels.DisplayName))
                        {
                            sessionModels.Password = password;
                            sessionModels.DisplayName = name;
                            sessionModels.EmailId = UserName;
                            sessionModels.SessionKey = SessionKey;
                            sessionModels.ConversationID = ConversationID;
                            sessionModels.EmpID = empID;
                            sessionModels.UserID = userId;
                            sessionModels.LastName = LastName;
                            sessionModels.IsSkipIntro = sessionModel.IsSkipIntro;
                        }
                        //Logger.LogError("Session model storage : " + sessionModels.ToString());

                        await UserState1.SaveChangesAsync(_turnContext, false, _cancellationToken);

                        await _turnContext.SendActivityAsync(MessageFactory.Text(text, text), _cancellationToken);
                    }
                }

                string cacheKey = _turnContext.Activity.From.Id + "artEnrollLogin";
                var ArtEnroll = db.StringGet(cacheKey);

                if (!ArtEnroll.IsNullOrEmpty)
                {

                    var artEnrollSessionData = JsonConvert.DeserializeObject(db.StringGet(cacheKey));

                    dynamic blogObject = JsonConvert.DeserializeObject<dynamic>(db.StringGet(cacheKey));
                    string name = blogObject["DisplayName"];
                    string UserName = blogObject["EmailId"];
                    string SessionKey = blogObject["SessionKey"];
                    bool IsLoginEnrolled = blogObject["IsLoginEnrolled"];
                    bool IsSkipIntro = blogObject["IsSkipIntro"];
                    string password = blogObject["Password"];
                    string empID = blogObject["EmpID"];
                    string userId = blogObject["UserID"];
                    string ConversationID = blogObject["ConversationID"];
                    string LastName = blogObject["LastName"];
                    int UserLoginDetectServiceChk = blogObject["UserLoginDetectServiceChk"];
                    string ARTEnrollLoginStatus = blogObject["ARTEnrollLoginStatus"];

                    SessionModel artEnrollSessionModel = new SessionModel();
                    artEnrollSessionModel.DisplayName = name;
                    artEnrollSessionModel.EmailId = UserName;
                    artEnrollSessionModel.SessionKey = SessionKey;
                    artEnrollSessionModel.ConversationID = ConversationID;
                    artEnrollSessionModel.EmpID = empID;
                    artEnrollSessionModel.UserID = userId;
                    artEnrollSessionModel.LastName = LastName;
                    artEnrollSessionModel.IsSkipIntro = IsSkipIntro;
                    artEnrollSessionModel.ARTEnrollLoginStatus = ARTEnrollLoginStatus;

                    if (string.IsNullOrEmpty(name) && ARTEnrollLoginStatus=="Fail")
                    {
                        SessionModel sessionModel = new SessionModel();
                        DateTime dt = DateTime.Now;
                        sessionModel.ARTEnrollLoginStatus = "SUCCESS";
                        db.StringSet(cacheKey, JsonConvert.SerializeObject(sessionModel));
                        db.KeyExpire(cacheKey, dt.AddMinutes(60));

                        string card = "\\Cards\\artEnrollLoginFailure.json";
                        var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);

                        Attachment adaptiveCardAttachment = new Attachment()
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
                        };

                        var activity = _turnContext.Activity;
                        //activity.Text = "ART Enrollment";
                        //activity.TextFormat = "message";

                        var reply = activity.CreateReply();
                        reply.Attachments = new List<Attachment>() { adaptiveCardAttachment };

                        await _turnContext.SendActivityAsync(reply, _cancellationToken);
                    }

                    if (!string.IsNullOrEmpty(name) && IsLoginEnrolled)
                    {
                        DateTime dt = DateTime.Now;
                        artEnrollSessionModel.IsSkipIntro = true;
                        db.StringSet(cacheKey, JsonConvert.SerializeObject(artEnrollSessionModel));
                        db.KeyExpire(cacheKey, dt.AddMinutes(60));
                        var text = $"Your enrollment login is successful.";

                        var sessionModelsAccessors = UserState1.CreateProperty<SessionModel>(nameof(SessionModel));
                        var sessionModels = await sessionModelsAccessors.GetAsync(_turnContext, () => new SessionModel());
                        if (string.IsNullOrWhiteSpace(sessionModels.DisplayName))
                        {
                            sessionModels.DisplayName = name;
                            sessionModels.EmailId = UserName;
                            sessionModels.SessionKey = SessionKey;
                            sessionModels.ConversationID = ConversationID;
                            sessionModels.EmpID = empID;
                            sessionModels.UserID = userId;
                            sessionModels.LastName = LastName;
                            sessionModels.IsSkipIntro = IsSkipIntro;
                            sessionModels.IsLoginEnrolled = false;
                        }

                        await UserState1.SaveChangesAsync(_turnContext, false, _cancellationToken);
                        await _turnContext.SendActivityAsync(MessageFactory.Text(text, text), _cancellationToken);

                        string card = "\\Cards\\artEmpId.json";
                        var adaptiveCardJson = File.ReadAllText(Environment.CurrentDirectory + card);

                        Attachment adaptiveCardAttachment = new Attachment()
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = JsonConvert.DeserializeObject(adaptiveCardJson.ToString()),
                        };

                        var activity = _turnContext.Activity;
                        activity.Text = "ART Enrollment";
                        activity.TextFormat = "message";

                        var reply = activity.CreateReply();
                        //activity.Attachments = new List<Attachment>() { adaptiveCardAttachment };
                        reply.Attachments = new List<Attachment>() { adaptiveCardAttachment };

                        //await _turnContext.SendActivityAsync(reply, _cancellationToken);

                        var reply1 = MessageFactory.Text("Are you sure you want to continue to Enroll?");

                        reply1.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                                {
                                    new CardAction() { Title = "Yes", Type = ActionTypes.ImBack, Value = "ART Enrollment" },
                                    new CardAction() { Title = "No", Type = ActionTypes.ImBack, Value = "None" },
                                },
                        };

                        //reply1.Attachments = new List<Attachment>() { adaptiveCardAttachment };
                        //await _turnContext.SendActivityAsync(reply1, _cancellationToken);

                        //var reply = MessageFactory.Attachment(adaptiveCardAttachment);
                        await _turnContext.SendActivityAsync(reply, _cancellationToken);

                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            //var dialogContext = await _dialog.CreateContextAsync(turnContext, cancellationToken);
            //await dialogContext.BeginDialogAsync("details", null, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Timer?.Dispose();
        }
    }
}

