using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Model
{
    public class SessionModel
    {
        public string SessionKey { get; set; }
        public string DisplayName { get; set; }
        public string EmailId { get; set; }
        public bool IsSkipIntro { get; set; }
        public string ConversationID { get; set; }
    }
}
