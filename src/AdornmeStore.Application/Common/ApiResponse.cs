using System;

namespace AdornmeStore.Application.Common
{
    /// <summary>
    /// Generic API response wrapper used across the application and API layers.
    /// Contains a message, status code and optional data payload.
    /// </summary>
    public sealed class ApiResponse<T>
    {
        public bool Success { get; init; }

        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// HTTP status code (200, 400, 404, ...). Keep as int for flexibility.
        /// </summary>
        public int Status { get; init; }

        /// <summary>
        /// The payload. Can be null for responses without data.
        /// </summary>
        public T? Data { get; init; }

        private ApiResponse() { }

        public static ApiResponse<T> SuccessResponse(T? data, string message = "Success", int status = 200)
            => new ApiResponse<T>
            {
                Success = true,
                Message = message ?? string.Empty,
                Status = status,
                Data = data
            };

        public static ApiResponse<T> FailureResponse(string message, int status = 400)
            => new ApiResponse<T>
            {
                Success = false,
                Message = message ?? string.Empty,
                Status = status,
                Data = default
            };
    }
}
