namespace WebApp.Application.Middlewares
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
            _request = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context) => InvokeAsync(context);

        private async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _request(context);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {context.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
            }
        }
    }
}
