using AdornmeStore.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Extensions
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Wrap data into a standardized ApiResponse and return an ActionResult with the appropriate status code.
        /// Use this from controllers to ensure consistent responses.
        /// </summary>
        public static ActionResult<ApiResponse<T>> ApiResult<T>(this ControllerBase controller, T? data, string? message = null, int statusCode = 200)
        {
            var resp = ApiResponse<T>.SuccessResponse(data, message ?? (statusCode == 200 ? "Success" : string.Empty), statusCode);

            if (statusCode == 200)
                return controller.Ok(resp);

            return new ObjectResult(resp) { StatusCode = statusCode };
        }

        /// <summary>
        /// Return a standardized error response.
        /// </summary>
        public static ActionResult<ApiResponse<object?>> ApiError(this ControllerBase controller, string message, int statusCode = 400)
        {
            var resp = ApiResponse<object?>.FailureResponse(message, statusCode);
            return new ObjectResult(resp) { StatusCode = statusCode };
        }
    }
}
