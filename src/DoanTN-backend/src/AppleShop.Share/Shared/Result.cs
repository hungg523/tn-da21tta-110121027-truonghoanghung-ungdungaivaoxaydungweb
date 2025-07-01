using AppleShop.Share.Errors;
using System.Net;
using System.Text.Json.Serialization;

namespace AppleShop.Share.Shared
{
    public class Result<T> where T : class
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Error? Error { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        public static implicit operator Result<T>(Result<object> result)
        {
            return new Result<T>
            {
                IsSuccess = result.IsSuccess,
                StatusCode = result.StatusCode,
                Error = result.Error
            };
        }

        public static implicit operator Result<T>(T model)
        {
            return new Result<T>
            {
                IsSuccess = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = model
            };
        }

        public static Result<object> Ok()
        {
            return new Result<object>
            {
                IsSuccess = true,
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        public static Result<object> Failure()
        {
            return new Result<object>
            {
                IsSuccess = false,
                StatusCode = (int)HttpStatusCode.Conflict,
            };
        }

        public static Result<T> Errors(string value)
        {
            return new Result<T>
            {
                IsSuccess = false,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = value
            };
        }

        public static Result<TEntity> Ok<TEntity>(TEntity value) where TEntity : class
        {
            return new Result<TEntity>
            {
                IsSuccess = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = value
            };
        }
    }
}