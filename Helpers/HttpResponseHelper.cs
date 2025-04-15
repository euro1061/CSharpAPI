using Microsoft.AspNetCore.Mvc;

namespace QBackend.Helpers
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public static class HttpResponseHelper
    {
        public static IActionResult Error(string message, int statusCode = 500)
        {
            return new ObjectResult(new ApiResponse
            {
                Success = false,
                Message = message
            })
            {
                StatusCode = statusCode
            };
        }

        public static IActionResult Success<T>(T data, string message = "Success")
        {
            return new OkObjectResult(new
            {
                Success = true,
                Message = message,
                Data = data
            });
        }
    }
}