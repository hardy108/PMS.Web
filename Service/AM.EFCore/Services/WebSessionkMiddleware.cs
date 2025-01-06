using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;
using PMS.Shared.Models;
using PMS.Shared.Services;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AM.EFCore.Services
{
    public class WebSessionkMiddleware
    {
        private readonly RequestDelegate _next;
        
        private readonly ILogger<WebSessionkMiddleware> _logger;

        private readonly IWebSessionService _service;
        public WebSessionkMiddleware(RequestDelegate next,ILogger<WebSessionkMiddleware> logger,IWebSessionService service)
        {
            _next = next;
            _logger = logger;
            _service = service;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var headers = httpContext.Request.Headers;                
                string token = string.Empty;
                try
                {
                    token = headers["Authorization"].ToArray()[0];
                    if (token.ToLower().StartsWith("bearer "))
                        token = token.Substring(7);
                    else
                        token = string.Empty;
                }
                catch { }
                if (!string.IsNullOrWhiteSpace(token)) {
                    string path = httpContext.Request.Path.Value.ToLower();
                    if (path != "logout")
                    {   
                        var invalidTokenException = new ExceptionInvalidToken();
                        try
                        {
                            var authenticatedSession = _service.ValidateSession(token);
                            httpContext.Request.Headers.Add("X-Session", authenticatedSession.Serialize());
                            if (authenticatedSession == null)
                            {
                                await HandleUnhandledExceptionAsync(httpContext, invalidTokenException);
                                return;
                            }
                        }
                        catch(ExceptionInvalidToken ex)
                        {
                            await HandleUnhandledExceptionAsync(httpContext, ex);
                            return;

                        }
                        catch (Exception ex)
                        {
                            await HandleUnhandledExceptionAsync(httpContext, ex);
                            return;

                        }
                    }
                    
                }
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleUnhandledExceptionAsync(httpContext, ex);
            }
        }

        

        private async Task HandleUnhandledExceptionAsync(HttpContext context,
                               Exception exception)
        {
            HttpStatusCode httpStatus = HttpStatusCode.InternalServerError;
            if (exception.GetType() == typeof(ExceptionInvalidToken))
                httpStatus = HttpStatusCode.Unauthorized;
            var result = new ExceptionMessage(exception);

            _logger.LogError(exception, exception.Message);            
//#if DEBUG
//#else
//            if (httpStatus != HttpStatusCode.Unauthorized)            
//                result.Message = "An unhandled exception has occurred";//hide real info on production

//#endif
            if (!context.Response.HasStarted)
            {
                int statusCode = (int)httpStatus;

                
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)httpStatus; 
                await context.Response.WriteAsync(result.ToString());
            }
        }


        private async Task Logout(HttpContext context)
        {
            HttpStatusCode httpStatus = HttpStatusCode.OK;            
            if (!context.Response.HasStarted)
            {
                int statusCode = (int)httpStatus;

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)httpStatus;
                await context.Response.WriteAsync("OK");
            }
        }

        private async Task TokenStatus(HttpContext context,string newToken)
        {
            HttpStatusCode httpStatus = HttpStatusCode.OK;
            if (!context.Response.HasStarted)
            {
                int statusCode = (int)httpStatus;

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)httpStatus;
                await context.Response.WriteAsync(newToken);
            }
        }
    }
}
