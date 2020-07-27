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
        public string EmpID { get; set; }
        public string LastName { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public bool IsLoginEnrolled { get; set; }
        public int UserLoginDetectServiceChk { get; set; }
        public string ARTEnrollLoginStatus { get; set; }

    }
}
