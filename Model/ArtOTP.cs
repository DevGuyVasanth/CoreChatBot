using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Model
{
    public class ArtOTP
    {
        public string mobileNo { get; set; }
        public bool  IsValidMobile { get; set; }
        public string OTPNo { get; set; }
        public bool IsValidOTP { get; set; }
        public string EmpID { get; set; }
        public string Password { get; set; }
        public string CountryCode { get; set; }


    }
}
