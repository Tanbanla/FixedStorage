namespace Identity.Background.Services
{
    public class ConsumeScopedServiceHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<ConsumeScopedServiceHostedService> _logger;
        private readonly IConfiguration _configuration;
        private Timer _timer = null;

        public ConsumeScopedServiceHostedService(IServiceProvider services,
            ILogger<ConsumeScopedServiceHostedService> logger,
            IConfiguration configuration)
        {
            _services = services;
            _logger = logger;
            _configuration = configuration;
        }

        public IServiceProvider _services { get; }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //DateTime.Today: 00:00:00
            TimeSpan delayTime = DateTime.Today.AddHours(23).AddMinutes(59) - DateTime.Now;
            TimeSpan intervalTime = TimeSpan.FromDays(1); //86400000s == 1 day
            _timer = new Timer(DoWork, null, delayTime, intervalTime);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Timed Hosted Service running.");
            using (var scope = _services.CreateScope())
            {
                try
                {
                    var _restClient = scope.ServiceProvider.GetRequiredService<IRestClient>();
                    var _userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    var task = _userService.LockUsersExpiredPasswordOrUnactiveAsync(new CancellationToken(), _restClient);
                    task.Wait();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Timed Hosted Service is stopping.");
                }
            }
        }

    }
}
