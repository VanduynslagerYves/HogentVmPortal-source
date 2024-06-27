using System.Collections.Concurrent;

namespace HogentVmPortalWebAPI
{
    public interface IBackgroundTaskQueue
    {
        void Enqueue(Func<CancellationToken, Task> workItem);
        Task<Func<CancellationToken, Task>> Dequeue(CancellationToken cancellationToken);

        //event that signals when a task is available in the queue
        event EventHandler TaskAvailable;
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        public event EventHandler? TaskAvailable;

        public void Enqueue(Func<CancellationToken, Task> workItem)
        {
            if(workItem == null) throw new ArgumentNullException(nameof(workItem));

            _workItems.Enqueue(workItem);

            // Raise the TaskAvailable event when a task is queued
            OnTaskAvailable();
        }

        /*
        Non-blocking Dequeue: The goal is to provide a non-blocking way to dequeue tasks. If a task is available in the queue, it is returned immediately.
        If no tasks are available, the method waits asynchronously until a task becomes available.
        
        Event-Driven Waiting: Instead of busy-waiting or constantly polling the queue, which can be resource-intensive, the implementation uses events to wait for a task to become available.
        This approach is more efficient and better suited for an asynchronous environment.
        
        Cancellation Support: The method supports cancellation through a CancellationToken, allowing the waiting operation to be canceled.
        */
        public Task<Func<CancellationToken, Task>> Dequeue(CancellationToken cancellationToken)
        {
            // Check if there's a task available in the queue, and if so return it
            if (_workItems.TryDequeue(out var workItem)) return Task.FromResult(workItem);


            // If no task is available, create a TaskCompletionSource to wait for a task when none are available
            // TaskCompletionSource represents the waiting operation (promise like)
            var tcs = new TaskCompletionSource<Func<CancellationToken, Task>>();
            // Define a handler that will be called when the TaskAvailable event is raised
            EventHandler? handler = null;
            // This handler will complete the TaskCompletionSource when a task becomes available.
            handler = (s, e) =>
            {
                if (_workItems.TryDequeue(out var workItem))
                {
                    // If a work item is dequeued successfully, complete the task.
                    tcs.SetResult(workItem);
                    // Unsubscribe the handler to prevent memory leaks and unnecessary calls.
                    TaskAvailable -= handler;
                }
            };
            // Subscribe the handler to the TaskAvailable event, so the handler will be executed when a task becomes available (through TaskAvailable?.Invoke(...))
            TaskAvailable += handler;


            // Register a callback to handle cancellation.
            using (cancellationToken.Register(() =>
            {
                // If canceled, try to set the TaskCompletionSource to a canceled state and remove the handler
                tcs.TrySetCanceled();
                // Unsubscribe the handler to prevent memory leaks and unnecessary calls.
                TaskAvailable -= handler;
            }))
            {
                // Return the Task from the TaskCompletionSource, allowing the caller to await the task until it is either completed with a dequeued work item or canceled.
                return tcs.Task;
            }
        }

        protected virtual void OnTaskAvailable()
        {
            TaskAvailable?.Invoke(this, EventArgs.Empty);
        }
    }
}
