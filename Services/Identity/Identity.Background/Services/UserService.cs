using BIVN.FixedStorage.Services.Common.API;

namespace Identity.Background.Services
{
    public interface IUserService
    {
        Task LockUsersExpiredPasswordOrUnactiveAsync(CancellationToken stoppingToken, IRestClient _restClient);
    }
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;

        public UserService(ILogger<UserService> logger, IConfiguration configuration
            )
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task LockUsersExpiredPasswordOrUnactiveAsync(CancellationToken stoppingToken, IRestClient _restClient)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            var restRequest = new RestRequest(Constants.Endpoint.API_Identity_Lock_Users);
            restRequest.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            restRequest.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
            var response = await _restClient.PutAsync(restRequest);
            if (response.IsSuccessful == false)
            {
                _logger.LogInformation($"Error at lock users service");
            }
            var result = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<LockUserListDto>>(response?.Content ?? string.Empty);
            if (result != null)
            {
                var lockedUserList = result.Data != null ? result?.Data?.LockUserListResult : new List<LockUserDto>();
                _logger.LogInformation($"LOCK COUNT {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")}: {result?.Data?.LockCount}");
                if (lockedUserList?.Any() == true)
                {
                    foreach (var user in lockedUserList)
                    {
                        _logger.LogInformation($"({user.Type}) '{user.UserId}' has changed from '{user.OldStatusName}' to '{user.NewStatusName}' at '{user.UpdatedDate}'");
                    }
                }
            }

        }
    }
}
