using AppleShop.Share.Errors;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using System.Net;

namespace AppleShop.API.DependencyInjection.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionHandler> logger;
        private readonly IWebHostEnvironment env;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
        {
            this.next = next;
            this.logger = logger;
            this.env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            logger.LogError("Error Message: {exceptionMessage}, Time of occurrence {time}", exception.Message, DateTime.UtcNow);

            var isProduction = env.IsProduction();

            var errorDetails = isProduction ? null : new Error(
                stackTrace: exception.StackTrace ?? "No stack trace available",
                details: exception.Message
            );

            var result = new Result<object>
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                IsSuccess = false,
                Error = errorDetails
            };

            if (exception is AppleException appleException)
            {
                errorDetails = isProduction ? null : new Error(
                    stackTrace: appleException.StackTrace ?? "No stack trace available",
                    details: appleException.Details.ToArray()
                );

                result = new Result<object>
                {
                    StatusCode = appleException.StatusCode,
                    IsSuccess = false,
                    Error = errorDetails
                };
            }

            context.Response.StatusCode = result.StatusCode;

            await context.Response.WriteAsJsonAsync(result);
        }
    }

}