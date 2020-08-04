using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BotDetect.Web;
using BotDetect.Web.Mvc;
using CoreBot.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreBot.Controllers
{
    [Route("api/get-captcha-image")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        [HttpPost, HttpGet]
        public CaptchaResult GetCaptchaImage()
        {
            //int width = 100;
            //int height = 36;
            int width = 140;
            int height = 36;
            var captchaCode = CustomCaptcha.GenerateCaptchaCode();
            CaptchaResult result = CustomCaptcha.GenerateCaptchaImage(width, height, captchaCode);
            //HttpContext.Session.SetString("CaptchaCode", result.CaptchaCode);
            //Stream s = new MemoryStream(result.CaptchaByteData);
            //return new FileStreamResult(s, "image/png");
            return result;
        }
    }
}
