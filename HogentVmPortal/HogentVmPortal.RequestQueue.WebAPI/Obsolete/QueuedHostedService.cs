namespace HogentVmPortal.RequestQueue.WebAPI.Obsolete
{
    //taskqueue injected as singleton
    [Obsolete("wtf KISS")]
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;

            // Subscribe to the TaskAvailable event
            _taskQueue.TaskAvailable += OnTaskAvailable;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                //non concurrent
                //Dequeue a background work item
                //    var workItem = await _taskQueue.Dequeue(stoppingToken);
                //try
                //{
                //    await workItem(stoppingToken);
                //}

                //Each task in the queue will be dequeued and will be executed in a background thread
                var workItem = await _taskQueue.Dequeue(stoppingToken);
                try
                {
                    // Dequeue a task using the updated Dequeue method

                    // Execute the dequeued task concurrently
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await workItem(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred executing the dequeued task.");
                        }
                    }, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    if (workItem == null)
                    {
                        _logger.LogError(ex, "Error occurred executing the work item");
                    }
                    else
                    {
                        _logger.LogError(ex, "Error occurrred executing {workItem}", nameof(workItem));
                    }
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
