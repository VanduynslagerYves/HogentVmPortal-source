using System.Collections.Concurrent;

namespace HogentVmPortalWebAPI
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
        event EventHandler TaskAvailable;
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        public event EventHandler? TaskAvailable;

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if(workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            OnTaskAvailable();
        }

        public Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            if(_workItems.TryDequeue(out var workItem))
            {
                return Task.FromResult(workItem);
            }

            var tcs = new TaskCompletionSource<Func<CancellationToken, Task>>();

            EventHandler? handler = null;
            handler = (s, e) =>
            {
                if (_workItems.TryDequeue(out var workItem))
                {
                    tcs.SetResult(workItem);
                    TaskAvailable -= handler;
                }
            };

            TaskAvailable += handler;

            using(cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
                TaskAvailable -= handler;
            }))
            {
                return tcs.Task;
            }
        }

        protected virtual void OnTaskAvailable()
        {
            TaskAvailable?.Invoke(this, EventArgs.Empty);
        }
    }
}
