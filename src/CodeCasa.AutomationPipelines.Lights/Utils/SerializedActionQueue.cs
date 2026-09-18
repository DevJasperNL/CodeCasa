using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace CodeCasa.AutomationPipelines.Lights.Utils;

/*
 * Runs actions one at a time. Unlike a lock, a caller on another thread never waits: it enqueues and returns, and
 * the thread that is already draining runs the action. Holding a lock while calling into child nodes deadlocked
 * nested reactive nodes (outer input vs. inner trigger, see 0b4571e for the same problem in Pipeline). Actions
 * enqueued from within a running action execute inline, which keeps synchronous emissions ordered as they would be
 * without the queue.
 */
internal sealed class SerializedActionQueue
{
    private readonly ConcurrentQueue<Action> _queue = new();
    private int _drainingThreadId;

    public void Run(Action action)
    {
        var currentThreadId = Environment.CurrentManagedThreadId;
        if (Volatile.Read(ref _drainingThreadId) == currentThreadId)
        {
            action();
            return;
        }

        ExceptionDispatchInfo? failure = null;
        _queue.Enqueue(action);
        while (!_queue.IsEmpty && Interlocked.CompareExchange(ref _drainingThreadId, currentThreadId, 0) == 0)
        {
            try
            {
                while (_queue.TryDequeue(out var next))
                {
                    try
                    {
                        next();
                    }
                    catch (Exception e)
                    {
                        // A throwing action must not strand the actions other threads queued behind it.
                        failure ??= ExceptionDispatchInfo.Capture(e);
                    }
                }
            }
            finally
            {
                Volatile.Write(ref _drainingThreadId, 0);
            }
        }

        failure?.Throw();
    }

    public void Clear() => _queue.Clear();
}
