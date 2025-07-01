using FluentValidation.Results;
using System.Net;

namespace AppleShop.Share.Exceptions
{
    public class AppleException : Exception
    {
        public int StatusCode { get; set; }
        public List<string> Details { get; set; } = new();

        public static void ThrowException(int statusCode, params string[] messages)
        {
            throw new AppleException
            {
                StatusCode = statusCode,
                Details = messages.ToList()
            };
        }

        public static void ThrowValidation(ValidationResult validationResult)
        {
            var details = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
            if (details.Length > 0)
            {
                ThrowException((int)HttpStatusCode.BadRequest, details);
            }
        }

        public static void ThrowNotFound(Type? entityType = null, string? message = null)
        {
            message ??= $"{entityType?.Name ?? "entity"} is not found.";
            ThrowException((int)HttpStatusCode.NotFound, message);
        }

        public static void ThrowConflict(params string[] messages)
        {
            ThrowException((int)HttpStatusCode.Conflict, messages);
        }

        public static void ThrowInternalServer(params string[] messages)
        {
            ThrowException((int)HttpStatusCode.InternalServerError, messages);
        }

        public static void ThrowUnAuthorization(params string[] messages)
        {
            ThrowException((int)HttpStatusCode.Unauthorized, messages);
        }

        public static void ThrowForbiden(params string[] messages)
        {
            ThrowException((int)HttpStatusCode.Forbidden, messages);
        }
    }
}