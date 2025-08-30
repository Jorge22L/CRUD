using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiddlewareCustom
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
