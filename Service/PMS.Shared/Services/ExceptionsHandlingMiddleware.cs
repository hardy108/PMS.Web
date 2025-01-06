using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using PMS.Shared.Models;

namespace PMS.Shared.Services
{
    public class ExceptionsHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionsHandlingMiddleware> _logger;

        public ExceptionsHandlingMiddleware(RequestDelegate next, ILogger<ExceptionsHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
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
            var result = new ExceptionMessage(exception);

            _logger.LogError(exception, result.Message);

            if (!context.Response.HasStarted)
            {
                int statusCode = (int)HttpStatusCode.InternalServerError; // 500


                //#if DEBUG

                //#else
                //                result = new ExceptionMessage("An unhandled exception has occurred");
                //#endif
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(result.ToString());
            }
        }

        private async Task HandleInvalidTokenAsync(HttpContext context)
        {
            var result = new ExceptionMessage("Sesi login tidak valid, silakan login ulang");
            _logger.LogError(result.Message);

            if (!context.Response.HasStarted)
            {
                int statusCode = (int)HttpStatusCode.Unauthorized; // 401

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(result.ToString());
            }
        }

        
    }
}
