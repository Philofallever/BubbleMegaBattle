using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class AsyncTools
{
    public static int                    MainThreadId      { get; private set; }
    public static SynchronizationContext MainThreadContext { get; private set; }

    private static Awaiter _mainThreadAwaiter;
    private static Awaiter _threadPoolAwaiter;
    private static Awaiter _nextFrameAwaiter;

    public static void Initialize()
    {
        MainThreadId      = Thread.CurrentThread.ManagedThreadId;
        MainThreadContext = SynchronizationContext.Current;

        _mainThreadAwaiter = new SynchronizationContextAwaiter(MainThreadContext);
        _threadPoolAwaiter = new ThreadPoolContextAwaiter();
        //_nextFrameAwaiter  = new NextFrameAwaiter();
    }

    public static void WhereAmI(string text)
    {
        if (IsMainThread())
        {
            Debug.Log($"{text}: main thread, frame: {Time.frameCount}");
        }
        else
        {
            Debug.Log($"{text}: background thread, id: {Thread.CurrentThread.ManagedThreadId}");
        }
    }

    /// <summary>
    /// Returns true if called from the Unity's main thread, and false otherwise.
    /// </summary>
    public static bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == MainThreadId;

    /// <summary>
    /// Switches execution to a background thread.
    /// </summary>
    public static Awaiter ToThreadPool() => _threadPoolAwaiter;

    /// <summary>
    /// Switches execution to the Update context of the main thread.
    /// </summary>
    public static Awaiter ToMainThread() => _mainThreadAwaiter;

    /// <summary>
    /// Downloads a file as an array of bytes.
    /// </summary>
    /// <param name="address">File URL</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    public static Task<byte[]> DownloadAsBytesAsync(string address
                                                  , CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.Run(() =>
                        {
                            using (var webClient = new WebClient())
                            {
                                return webClient.DownloadData(address);
                            }
                        }
                      , cancellationToken);
    }

    /// <summary>
    /// Downloads a file as a string.
    /// </summary>
    /// <param name="address">File URL</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    public static Task<string> DownloadAsStringAsync(string address
                                                   , CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.Run(() =>
                        {
                            using (var webClient = new WebClient())
                            {
                                return webClient.DownloadString(address);
                            }
                        }
                      , cancellationToken);
    }

    //public static AsyncTools.Awaiter GetAwaiter(this (GameObject GameObject, float Second) info)
    //{
    //    return info.Second <= 0f ? _nextFrameAwaiter : new DelayAwaiter(info.GameObject, info.Second);
    //}

    /// <summary>
    /// Waits until all the tasks are completed.
    /// </summary>
    public static TaskAwaiter GetAwaiter(this IEnumerable<Task> tasks) => Task.WhenAll(tasks).GetAwaiter();

    public static Awaiter GetAwaiter(this SynchronizationContext context) => new SynchronizationContextAwaiter(context);

    /// <summary>
    /// Waits until the process exits.
    /// </summary>
    public static TaskAwaiter<int> GetAwaiter(this Process process)
    {
        var tcs = new TaskCompletionSource<int>();
        process.EnableRaisingEvents =  true;
        process.Exited              += (sender, eventArgs) => tcs.TrySetResult(process.ExitCode);
        if (process.HasExited)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        return tcs.Task.GetAwaiter();
    }

    /// <summary>
    /// Waits for AsyncOperation completion
    /// </summary>
    public static Awaiter GetAwaiter(this AsyncOperation asyncOp) => new AsyncOperationAwaiter(asyncOp);

#region Various awaiters

    public abstract class Awaiter : INotifyCompletion
    {
        public abstract bool IsCompleted { get; }
        public abstract void OnCompleted(Action action);
        public Awaiter GetAwaiter() => this;

        public void GetResult()
        {
        }
    }

   

    private class SynchronizationContextAwaiter : Awaiter
    {
        private readonly SynchronizationContext _context;

        public SynchronizationContextAwaiter(SynchronizationContext context)
        {
            _context = context;
        }

        public override bool IsCompleted => _context == null || _context == SynchronizationContext.Current;
        public override void OnCompleted(Action action) => _context.Post(state => action(), null);
    }

    private class ThreadPoolContextAwaiter : Awaiter
    {
        public override bool IsCompleted => IsMainThread() == false;
        public override void OnCompleted(Action action) => ThreadPool.QueueUserWorkItem(state => action(), null);
    }

    //public class NextFrameAwaiter : Awaiter
    //{
    //    public override bool IsCompleted => false;
    //    public override void OnCompleted(Action action)
    //    {
    //        manager.Instance.NextFrameHelper.Enqueue(action);
    //    }
    //}

    private class AsyncOperationAwaiter : Awaiter
    {
        private readonly AsyncOperation _asyncOp;
        public AsyncOperationAwaiter(AsyncOperation asyncOp) => _asyncOp = asyncOp;
        public override bool IsCompleted => _asyncOp.isDone;
        public override void OnCompleted(Action action) => _asyncOp.completed += _ => action();
    }

#endregion
}