using Microsoft.Bot.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
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
        string cacheConnectionString = string.Empty;
        ITurnContext _turnContext;
        CancellationToken _cancellationToken;

        public UserLoginDetectService(ILogger<UserLoginDetectService> logger, IMemoryCache cache, IConfiguration _iconfiguration)
        {
            Logger = logger;
            Cache = cache;

            string cacheConnectionString = _iconfiguration["RedisCacheConnection"];
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);


            //IDatabase db = connection.GetDatabase();
            //string userId = "48124";
            //var val = db.StringGet(userId);
            //var SessionData = JsonConvert.DeserializeObject(db.StringGet(userId));
            //SessionDataObj = (SessionModel)SessionData;
        }

        public UserLoginDetectService(CancellationToken cancellationToken, IMemoryCache cache, ITurnContext turnContext)
        {
            Cache = cache;
            _turnContext = turnContext;
            _cancellationToken = cancellationToken;
            StartAsync(cancellationToken);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Logger.LogInformation("UserLoginDetectService is starting.");
            Timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            cacheConnectionString = "HexaChatBotRedis.redis.cache.windows.net:6380,password=gItUtui8ogouVxo48BUEozsSnMg4JeHkgg2RX7TmPH8=,ssl=True,abortConnect=false,allowAdmin=true";
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);

            IDatabase db = connection.GetDatabase();
            //string userId = "RamukDB5";
            var val = db.StringGet(_turnContext.Activity.From.Id);
            if (!val.IsNullOrEmpty)
            {

                var SessionData = JsonConvert.DeserializeObject(db.StringGet(_turnContext.Activity.From.Id));

                dynamic blogObject = JsonConvert.DeserializeObject<dynamic>(db.StringGet(_turnContext.Activity.From.Id));
                string name = blogObject["DisplayName"];
                string UserName = blogObject["EmailId"];
                string SessionKey = blogObject["SessionKey"];
                bool IsSkipIntro = blogObject["IsSkipIntro"];

                SessionModel SessionModel = new SessionModel();
                SessionModel.DisplayName = name;
                SessionModel.EmailId = UserName;
                SessionModel.SessionKey = SessionKey;

                //user1.UserName = name;
                if (!string.IsNullOrEmpty(name) && !IsSkipIntro)
                {
                    DateTime dt = DateTime.Now;
                    SessionModel.IsSkipIntro = true;
                    db.StringSet(_turnContext.Activity.From.Id, JsonConvert.SerializeObject(SessionModel));
                    db.KeyExpire(_turnContext.Activity.From.Id, dt.AddMinutes(60));
                    var text = $"Hi {name}, you have successfully logged in!";

                    _turnContext.SendActivityAsync(MessageFactory.Text(text, text), _cancellationToken);
                    //user1.UserName = name;
                    //user1.LoginDetected = false;
                    //Cache.Set("users", user1, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(7)));
                }
            }
            //List<CacheUser> users;
            //if (!Cache.TryGetValue("users", out users))
            //{
            //    users = new List<CacheUser>();
            //}
            //user.UserName = userName;
            //user.LoginDetected = false;
            //_cache.Set("users", users, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(7)));

            ////SessionModel SS = (SessionModel)JsonConvert.DeserializeObject(db.StringGet("RamukDB"));
            ////List<CacheUser> users = new List<CacheUser>();

            ////string user = SessionDataObj.DisplayName;

            //if (Cache.TryGetValue("users", out users))
            //{
            //    for (int i = 0; i < users.Count; i++)
            //    {
            //        var user = users[i];
            //        if (user.UserName != "" && !user.LoginDetected)
            //        {
            //            var text = $"Hi {users[i].UserName}, you have successfully logged in!";
            //            users[i].LoginDetected = true;
            //            users[i].TurnContext.SendActivityAsync(MessageFactory.Text(text, text), users[i].CancellationToken);
            //            Cache.Set("users", users);
            //        }
            //    }
            //}
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

