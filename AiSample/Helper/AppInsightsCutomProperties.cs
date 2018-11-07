using Microsoft.AspNetCore.Builder;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace AiSample.Helper
{
    public static class UseAppInsightsCutomPropertiesMiddlewareExtensions
    {
        public static IApplicationBuilder UseAppInsightsCutomProperties(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AppInsightsCutomProperties>();
        }
    }

    public class AppInsightsCutomProperties
    {
        private readonly RequestDelegate _next;
        private readonly AppInsightsCustomMiddlewareSettings _AppInsightsConf;
        readonly IConfiguration _configuration;

        public AppInsightsCutomProperties(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
            _AppInsightsConf = new AppInsightsCustomMiddlewareSettings();
            _configuration.Bind("AppInsightsCustomMiddlewareSettings", _AppInsightsConf);
        }
        public async Task InvokeAsync(HttpContext context)
        {
            RequestTelemetry requestTelemetry = context.Features.Get<RequestTelemetry>();
            Stream RequestStream = null, ResponseStream = null;
            MemoryStream RequestBodyMemoryBuffer = null, ResponseBodyMemoryBuffer = null;

            if (_AppInsightsConf.RequestHeaders)
            {
                foreach (var header in context.Request.Headers)
                {
                    if (header.Key.StartsWith("XX-"))
                        requestTelemetry.Properties.Add($"Request-{header.Key}", header.Value);
                }
            }

            //Request Body Pre Actions
            if (_AppInsightsConf.RequestBody)
            {
                RequestStream = context.Request.Body;
                RequestBodyMemoryBuffer = new MemoryStream();
                await RequestStream.CopyToAsync(RequestBodyMemoryBuffer);
                RequestBodyMemoryBuffer.Position = 0L;
                if (RequestBodyMemoryBuffer.ToArray() != null)
                requestTelemetry.Properties.Add("RequestBody", Encoding.ASCII.GetString(RequestBodyMemoryBuffer.ToArray()));
                context.Request.Body = RequestBodyMemoryBuffer;
            }

            //Response Body Pre Actions
            if (_AppInsightsConf.ResponseBody)
            {
                ResponseStream = context.Response.Body;
                ResponseBodyMemoryBuffer = new MemoryStream();
                context.Response.Body = ResponseBodyMemoryBuffer;
            }

            await _next(context);

            //Response Body Post Action
            if (_AppInsightsConf.ResponseBody)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                string BodyStr = await new StreamReader(context.Response.Body).ReadToEndAsync();
                requestTelemetry.Properties.Add("ResponseBody", BodyStr);
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await ResponseBodyMemoryBuffer.CopyToAsync(ResponseStream);
                ResponseBodyMemoryBuffer.Dispose();
            }

            //Request Body Post Actions
            if (_AppInsightsConf.RequestBody)
            {
                context.Request.Body = RequestStream;
                RequestBodyMemoryBuffer.Dispose();
            }

            if (_AppInsightsConf.ResponseHeaders)
            {
                foreach (var header in context.Response.Headers)
                {
                    if (header.Key.StartsWith("XX-") && !requestTelemetry.Properties.ContainsKey(header.Key))
                        requestTelemetry.Properties.Add($"Response-{header.Key}", header.Value);
                }
            }
        }
    }
}