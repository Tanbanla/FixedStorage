using BIVN.FixedStorage.Services.Common.API;

namespace Identity.Background.Services
{
    public class DeviceTokenService : BackgroundService
    {
        private readonly ILogger<DeviceTokenService> _logger;
        private readonly IRestClient _restClient;
        private readonly IConfiguration _configuration;

        public DeviceTokenService(ILogger<DeviceTokenService> logger, IRestClient restClient, IConfiguration configuration)
        {
            _logger = logger;
            _restClient = restClient;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            // When the timer should have no due-time, then do the work once now.
            await RemoveExpiredToken();

            using PeriodicTimer timer = new(TimeSpan.FromDays(1));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    //remove expired token
                    await RemoveExpiredToken();
                    
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        // Could also be a async method, that can be awaited in DoWork above
        private async Task RemoveExpiredToken()
        {
            try
            {
                {
                    var restRequest = new RestRequest(Constants.Endpoint.API_Identity_Remove_Expired_Token);
                    restRequest.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                    restRequest.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
                    var response = await _restClient.DeleteAsync(restRequest);

                  
                    if (response.IsSuccessful == false)
                        _logger.LogInformation($"Record removed");
                }



            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Fatal error establishing database connection");
            }
        }

    }
}
