using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Model
{
    public class CacheUser
    {
        public string UserName { get; set; }
        public string BotClientUserId { get; set; }
        public string BotConversationId { get; set; }
        public ITurnContext TurnContext { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public bool LoginDetected { get; set; }
        public CacheUser(string botClientUserId, string botConversationId, ITurnContext turnContext, System.Threading.CancellationToken cancellationToken)
        {
            BotClientUserId = botClientUserId;
            BotConversationId = botConversationId;
            UserName = "";
            TurnContext = turnContext;
            CancellationToken = cancellationToken;
            LoginDetected = false;
        }
    }
}
