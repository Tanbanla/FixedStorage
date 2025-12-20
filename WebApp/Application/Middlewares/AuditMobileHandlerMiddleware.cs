namespace WebApp.Application.Middlewares
{
    /// <summary>
    /// Central error/exception handler Middleware
    /// </summary>
    public class AuditMobileHandlerMiddleware
    {
        private readonly RequestDelegate _request;
        private readonly ILogger<AuditMobileHandlerMiddleware> _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditMobileHandlerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        public AuditMobileHandlerMiddleware(RequestDelegate next, ILogger<AuditMobileHandlerMiddleware> logger)
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
            var isAuditMobileAccount = context.UserFromContext()?.AccountType == nameof(AccountType.TaiKhoanGiamSat);
            if (isAuditMobileAccount)
            {
                var path = context.Request.Path.Value.ToLower();
                bool acceptRoute = path == (commonAPIConstant.Endpoint.Index) || path.StartsWith($"/{commonAPIConstant.Endpoint.WebApp_AuditMobile}");
                if (!acceptRoute)
                {
                    context.Response.Redirect(commonAPIConstant.Endpoint.Index);
                }
            }

            await _request(context);
        }
    }
}
