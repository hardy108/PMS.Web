using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using AM.EFCore.Services;
using PMS.Shared.Models;

namespace PMS.Web.Services.Controllers
{
    
    
    [ApiController]
    public class TokenController : ControllerBase
    {
        private AuthenticationServiceEstate _service;

        public TokenController(AuthenticationServiceEstate tokenService)
        {
            _service = tokenService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("token")]
        public IActionResult Authenticate([FromBody]LoginParameter loginParameter)
        {
            return Ok(_service.Login(loginParameter.Username, loginParameter.Password).Token);
        }

        [Authorize]
        [HttpGet]       
        [Route("token/checktokenstatus")]
        public IActionResult CheckTokenStatus()
        {
            return Ok(_service.LoginBySession(Request.Headers).Token);
        }

        [Authorize]
        [HttpGet]        
        [Route("logout")]        
        public IActionResult Logout()
        {
            _service.Logout(Request.Headers);
            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("token/changepassword")]
        public IActionResult ChangePassword([FromBody]ChangePasswordParameter param)
        {
            return Ok(_service.ChangePassword(Request.Headers, param.OldPassword, param.NewPassword).Token);
        }


        
        [HttpGet]
        [Route("token/resetpasswordtoken")]
        public IActionResult ResetPasswordToken([FromQuery]string username)
        {
            return Ok(_service.ResetPasssordRequest(username));
        }

        [HttpPost]
        [Route("token/resetpassword")]
        public IActionResult ResetPassword([FromBody]ResetPasswordParameter param)
        {
            return Ok(_service.ResetPassword(param.Token, param.NewPassword));
        }


    }
}