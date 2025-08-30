using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MiddlewareCustom
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();
            context.Items["RequestId"] = requestId;

            await LogRequestAsync(context, requestId);

            var originalBody = context.Response.Body;
            using var newBody = new MemoryStream();
            context.Response.Body = newBody;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);

                newBody.Seek(0, SeekOrigin.Begin);
                await newBody.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId)
        {
            var req = context.Request;
            _logger.LogInformation("[{RequestId}] {Method} {Path}{QueryString}",
           
                requestId, req.Method, req.Path, req.QueryString);

            if (req.ContentLength > 0 && req.ContentType?.Contains("application/json") == true)
            {
                req.EnableBuffering();
                using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                req.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                    _logger.LogInformation("[{RequestId}] Request Body: {Body}", requestId, body);
            }
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, long elapsedMs)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var status = context.Response.StatusCode;
            if (status >= 400)
                _logger.LogWarning("[{RequestId}] {StatusCode} in {Elapsed}ms. Response: {Body}",
                    requestId, status, elapsedMs, bodyText);
            else
                _logger.LogInformation("[{RequestId}] {StatusCode} in {Elapsed}ms", requestId, status, elapsedMs);
        }


    }
}
