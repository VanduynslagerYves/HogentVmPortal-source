namespace HogentVmPortalWebAPI.Services
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _taskQueue.TaskAvailable += OnTaskAvailable;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurrred executing {workItem}.", nameof(workItem));
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }

        // This method will be invoked when a new task is available
        // Only used to notify when a new task is available
        private void OnTaskAvailable(object? sender, EventArgs e)
        {
            _logger.LogInformation("New task is available.");
        }

        public override void Dispose()
        {
            _taskQueue.TaskAvailable -= OnTaskAvailable;
            base.Dispose();
        }
    }
}
