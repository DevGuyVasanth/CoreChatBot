using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class LstQuestion
    {
        public int question_id { get; set; }
        public string question_text { get; set; }
        public bool isEnabled { get; set; }
        public int categoryid { get; set; }
        public bool isSelected { get; set; }

    }

    public class Question
    {
        public string categoryname { get; set; }
        public List<LstQuestion> lstQuestions { get; set; }

    }

    public class Root
    {
        public int TotalNumberOfQuestions { get; set; }
        public int TotalQuestionsToAnswer { get; set; }
        public List<Question> questions { get; set; }
        public bool IsLogin { get; set; }

    }


}
