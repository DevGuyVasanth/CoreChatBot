using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Model
{
    public class Incident
    {
        public string IncidentDesc { get; set; }

        public string EmailID { get; set; }

        public string IncidentDate { get; set; }

        public string IncidentNo { get; set; }

        public string ChoiceID { get; set; }
    }

    public class ArtEnroll
    {
        public string IncidentDesc { get; set; }

        public string EmailID { get; set; }

        public string IncidentDate { get; set; }

        public string IncidentNo { get; set; }

        public string ChoiceID { get; set; }
    }

    public class MySettingsConfig
    {
        public string IncidentCreateJson { get; set; }
        public string LuisAPIHostName { get; set; }
        public string LuisAPIKey { get; set; }
    }

}
