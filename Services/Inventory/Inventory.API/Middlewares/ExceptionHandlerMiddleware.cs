namespace BIVN.FixedStorage.Services.Inventory.API.Middlewares
{
    /// <summary>
    /// Central error/exception handler Middleware
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _request;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            this._request = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context) => this.InvokeAsync(context);

        private async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this._request(context);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {context.Request.Path} : {exMess}; {innerExMess}");
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var response = new ErrorDetailsDto()
            {
                Code = context.Response.StatusCode,
                Message = "Internal Server Error from the custom middleware."
            };
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, JsonDefaults.CamelCasing));
        }
    }
}
