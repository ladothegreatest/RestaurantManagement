namespace RestaurantManagement.Common.ApiResponses
{
    public class ApiResponseExtensions
    {
        public static ApiResponse SuccessResponse(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Errors = null
            };
        }

        public static ApiResponse FailureResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
