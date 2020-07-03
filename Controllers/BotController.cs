// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.9.2

using System.Threading.Tasks;
using CoreBot.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CoreBot.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot, IConfiguration _iconfiguration)
        {
            Adapter = adapter;
            Bot = bot;
            //string cacheConnectionString = _iconfiguration["RedisCacheConnection"];
            //ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnectionString);

            //IDatabase db = connection.GetDatabase();
            //string userId = "48124"; //"RAMUKLAP";//"48123";
            //var val = db.StringGet(userId);
            //var SessionData = JsonConvert.DeserializeObject(db.StringGet(userId));
        }

        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await Adapter.ProcessAsync(Request, Response, Bot);
        }
    }
}
