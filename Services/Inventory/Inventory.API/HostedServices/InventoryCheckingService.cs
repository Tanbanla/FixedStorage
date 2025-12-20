namespace Inventory.API.HostedServices
{

    public class InventoryCheckingService : IHostedService, IDisposable
    {
        private readonly ILogger<InventoryCheckingService> _logger;
        private readonly IConfiguration _configuration;
        private Timer _timer = null;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public IServiceProvider _serviceProvider { get; }

        public InventoryCheckingService(IServiceScopeFactory serviceScopeFactory,
            ILogger<InventoryCheckingService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;

        }




        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //DateTime.Today: 00:00:00
            TimeSpan delayTime = DateTime.Today.AddDays(1).AddHours(00).AddMinutes(00).AddSeconds(1) - DateTime.Now;
            //TimeSpan delayTime = DateTime.Today.AddHours(17).AddMinutes(10).AddSeconds(30) - DateTime.Now;
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
            if (state != null && state is true)
            {
                Task.Run(async () => await StopAsync(new CancellationTokenSource(TimeSpan.Zero).Token));
                state = false;
                return;
            }
            _logger.LogInformation("Timed Hosted Service running.");
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                    {
                        var inventory = inventoryContext.Inventories.OrderByDescending(x => x.InventoryDate).FirstOrDefault();
                        if (inventory != null)
                        {
                            if (DateTime.Now > inventory.InventoryDate.AddDays(30) && inventory.InventoryStatus != InventoryStatus.Finish)
                            {
                                inventory.InventoryStatus = InventoryStatus.Finish;
                                inventoryContext.Inventories.Update(inventory);
                                inventoryContext.SaveChanges();

                                //invoke reporting background task
                                if (inventory.IsReportRunning)
                                {

                                    inventory.IsReportRunning = false;
                                    inventoryContext.Inventories.Update(inventory);
                                    inventoryContext.SaveChanges();


                                }

                            }
                            else if (inventory.InventoryDate.AddDays(-10) <= DateTime.Now && DateTime.Now <= inventory.InventoryDate.AddDays(30))
                            {
                                if (inventory.InventoryStatus == InventoryStatus.NotYet)
                                {
                                    inventory.InventoryStatus = InventoryStatus.Doing;
                                    inventoryContext.Inventories.Update(inventory);
                                    inventoryContext.SaveChanges();
                                }



                            }

                        }
                    }

                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Timed Hosted Service is stopping.");
                }
            }
        }
        //private void StartReporting(object state)
        //{
        //    using (var scope = _serviceScopeFactory.CreateScope())
        //    {
        //        var reportService = scope.ServiceProvider.GetRequiredService<InventoryReportingService>();
        //        reportService.Start();
        //    }
        //}

    }


}
