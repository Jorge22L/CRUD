using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiddlewareCustom
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Ocurrió un error inesperado: {e.Message}");
                await HandleExceptionAsync(context, e);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Argumentos no correctos en la petición";
                    response.Details = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "No autorizado";
                    response.Details = exception.Message;
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "Recurso no encontrado";
                    response.Details = exception.Message;
                    break;

                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = "Operación inválida";
                    response.Details = exception.Message;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Error interno del servidor";
                    response.Details = exception.Message;
                    break;
            }

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime TimeStamps { get; set; } = DateTime.UtcNow;
        public string TraceId { get; set; } = Guid.NewGuid().ToString();
    }
}
