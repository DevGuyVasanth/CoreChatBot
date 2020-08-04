using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Model
{
    public class ArtConstants
    {

        string local_validateuser_api_url = "http://192.168.225.144:16200/api/user/ValidateUserID";

        string validateuser_api_url = "https://art.ricoh-usa.com/botapi/api/user/ValidateUserID";
        string register_api_enroll = "https://art.ricoh-usa.com/botapi/api/user/register";
        string authorizeUserandSendOtp_api_enroll = "https://art.ricoh-usa.com/botapi/api/user/AuthorizeUserAndSendOTP";
        string validateOTPForRegistration_api_enroll = "https://art.ricoh-usa.com/botapi/api/user/ValidateOTPForRegistration";
        string enrollDobAuthorization_api_enroll = "https://art.ricoh-usa.com/botapi/api/user/EnrollDobAuthorization";
        string sendOTPForAuthorization_api = "https://art.ricoh-usa.com/botapi/api/user/SendOTPForAuthorization";
        string UnlockAccountOTPAuthorization_api = "https://art.ricoh-usa.com/botapi/api/user/UnlockAccountOTPAuthorization";
        string resetPassword_api_url = "https://art.ricoh-usa.com/botapi/api/user/ResetPassword";
        string validateOTPForAuthorization_api_url = "https://art.ricoh-usa.com/botapi/api/user/validateOTPForAuthorization";
        string getSecurityQuestions_api_url = "https://art.ricoh-usa.com/botapi/api/user/GetSecurityQuestions";
        string authorizeUserSQA_api_url = "https://art.ricoh-usa.com/botapi/api/user/AuthorizeUserSQA";
        string unlockAccountSQAAuthorization_api_url = "https://art.ricoh-usa.com/botapi/api/user/UnlockAccountSQAAuthorization";



        string us_validateUserID_AU = "https://art.ricoh-usa.com/botapi/api/user/ValidateUserID";
        string us_enroll_final_register = "https://art.ricoh-usa.com/botapi/api/user/register";
        string us_enroll_yes_authorizeUserAndSendOTP = "https://art.ricoh-usa.com/botapi/api/user/AuthorizeUserAndSendOTP";
        string us_enroll_yes_validateOTPForRegistration = "https://art.ricoh-usa.com/botapi/api/user/ValidateOTPForRegistration";
        string us_enroll_valid_enrollDobAuthorization = "https://art.ricoh-usa.com/botapi/api/user/EnrollDobAuthorization";
        string us_sendOTPForAuthorization_AU = "https://art.ricoh-usa.com/botapi/api/user/SendOTPForAuthorization";
        string us_unlockAccountOTPAuthorization_AU = "https://art.ricoh-usa.com/botapi/api/user/UnlockAccountOTPAuthorization";
        string us_resetPassword_RP = "https://art.ricoh-usa.com/botapi/api/user/ResetPassword";
        string us_validateOTPForAuthorization_RP = "https://art.ricoh-usa.com/botapi/api/user/validateOTPForAuthorization";
        string us_getSecurityQuestions_AU = "https://art.ricoh-usa.com/botapi/api/user/GetSecurityQuestions";
        string us_authorizeUserSQA_RP = "https://art.ricoh-usa.com/botapi/api/user/AuthorizeUserSQA";
        string us_unlockAccountSQAAuthorization_AU = "https://art.ricoh-usa.com/botapi/api/user/UnlockAccountSQAAuthorization";


        string la_validateUserID_AU = "https://art.ricoh-la.com/botapi/api/user/ValidateUserID";
        string la_sendOTPForAuthorization_AU = "https://art.ricoh-la.com/botapi/api/user/SendOTPForAuthorization";
        string la_unlockAccountOTPAuthorization_AU = "https://art.ricoh-la.com/botapi/api/user/UnlockAccountOTPAuthorization";
        string la_getSecurityQuestions_AU = "https://art.ricoh-la.com/botapi/api/user/GetSecurityQuestions";
        string la_unlockAccountSQAAuthorization_AU = "https://art.ricoh-la.com/botapi/api/user/UnlockAccountSQAAuthorization";
        string la_resetPassword_RP = "https://art.ricoh-la.com/botapi/api/user/ResetPassword";
        string la_validateOTPForAuthorization_RP = "https://art.ricoh-la.com/botapi/api/user/validateOTPForAuthorization";
        string la_authorizeUserSQA_RP = "https://art.ricoh-la.com/botapi/api/user/AuthorizeUserSQA";
        string la_changePassword_CP = "https://art.ricoh-la.com/botapi/api/user/ChangePassword";
        string la_enroll_final_register = "https://art.ricoh-la.com/botapi/api/user/register";
        string la_enroll_yes_authorizeUserAndSendOTP = "https://art.ricoh-la.com/botapi/api/user/AuthorizeUserAndSendOTP";
        string la_enroll_yes_validateOTPForRegistration = "https://art.ricoh-la.com/botapi/api/user/ValidateOTPForRegistration";
        string la_enroll_valid_authorizeUser = "https://art.ricoh-la.com/botapi/api/user/AuthorizeUser";
        string la_enroll_valid_enrollDobAuthorization = "https://art.ricoh-la.com/botapi/api/user/EnrollDobAuthorization";


        string ca_validateUserID_AU = "https://art.ricoh.ca/botapi/api/user/ValidateUserID";
        string ca_sendOTPForAuthorization_AU = "https://art.ricoh.ca/botapi/api/user/SendOTPForAuthorization";
        string ca_unlockAccountOTPAuthorization_AU = "https://art.ricoh.ca/botapi/api/user/UnlockAccountOTPAuthorization";
        string ca_getSecurityQuestions_AU = "https://art.ricoh.ca/botapi/api/user/GetSecurityQuestions";
        string ca_unlockAccountSQAAuthorization_AU = "https://art.ricoh.ca/botapi/api/user/UnlockAccountSQAAuthorization";
        string ca_resetPassword_RP = "https://art.ricoh.ca/botapi/api/user/ResetPassword";
        string ca_validateOTPForAuthorization_RP = "https://art.ricoh.ca/botapi/api/user/validateOTPForAuthorization";
        string ca_authorizeUserSQA_RP = "https://art.ricoh.ca/botapi/api/user/AuthorizeUserSQA";
        string ca_changePassword_CP = "https://art.ricoh.ca/botapi/api/user/ChangePassword";
        string ca_enroll_final_register = "https://art.ricoh.ca/botapi/api/user/register";
        string ca_enroll_yes_authorizeUserAndSendOTP = "https://art.ricoh.ca/botapi/api/user/AuthorizeUserAndSendOTP";
        string ca_enroll_yes_validateOTPForRegistration = "https://art.ricoh.ca/botapi/api/user/ValidateOTPForRegistration";
        string ca_enroll_valid_authorizeUser = "https://art.ricoh.ca/botapi/api/user/AuthorizeUser";
        string ca_enroll_valid_enrollDobAuthorization = "https://art.ricoh.ca/botapi/api/user/EnrollDobAuthorization";
    }
}
