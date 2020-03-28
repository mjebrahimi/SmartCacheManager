using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCacheManager.Utilities
{
    /// <summary>
    /// Abstraction for lock with async/await support
    /// </summary>
    public interface IAsyncLock
    {
        /// <summary>
        /// Asynchronously waits to enter the lock
        /// </summary>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>IDisposable</returns>
        Task<IDisposable> LockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronously waits to enter the lock
        /// </summary>
        /// <returns>IDisposable</returns>
        IDisposable Lock();
    }

    /// <summary>
    /// A lock with async/await support
    /// Source from EF Core Repository: 
    /// https://github.com/dotnet/efcore/blob/0d76bbf45a42148924b413ef8f37bf49c1ce10d3/src/EFCore/Internal/AsyncLock.cs
    /// Alternatives :
    /// https://github.com/bmbsqd/AsyncLock
    /// https://github.com/StephenCleary/AsyncEx
    /// Benchmark in Release/NoDebugging
    /// 1- EFCoreAsyncLock : 550
    /// 2- Bmbsqd.AsyncLock : 600
    /// 3- Nito.AsyncEx : 3300
    /// </summary>
    public sealed class SemaphoreSlimAsyncLock : IAsyncLock
    {
        //Instantiate a Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        //Releaser instance for sync method to Release() inner AsyncLock._semaphoreSlim
        private readonly IDisposable _releaser;

        //Releaser instance for async method to Release() inner AsyncLock._semaphoreSlim
        private readonly Task<IDisposable> _releaserTask;

        public SemaphoreSlimAsyncLock()
        {
            _releaser = new Releaser(this);
            _releaserTask = Task.FromResult(_releaser);
        }

        /// <summary>
        /// Asynchronously waits to enter the lock
        /// </summary>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>IDisposable</returns>
        public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
        {
            var wait = _semaphoreSlim.WaitAsync(cancellationToken);

            return wait.IsCompleted
                ? _releaserTask
                : wait.ContinueWith(
                    (_, state) => ((SemaphoreSlimAsyncLock)state)._releaser,
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Synchronously waits to enter the lock
        /// </summary>
        /// <returns>IDisposable</returns>
        public IDisposable Lock()
        {
            _semaphoreSlim.Wait();

            return _releaser;
        }

        private readonly struct Releaser : IDisposable
        {
            private readonly SemaphoreSlimAsyncLock _asyncLock;

            internal Releaser(SemaphoreSlimAsyncLock asyncLock)
            {
                _asyncLock = asyncLock.NotNull(nameof(asyncLock));
            }

            public void Dispose()
            {
                _asyncLock._semaphoreSlim.Release();
            }
        }
    }

    /// <summary>
    /// Null async lock implementation of IAsyncLock
    /// </summary>
    public sealed class NullAsyncLock : IAsyncLock
    {
        private static readonly NullDisposable nullDisposable = new NullDisposable();

        /// <inheritdoc/>
        public IDisposable Lock()
        {
            return nullDisposable;
        }

        /// <inheritdoc/>
        public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IDisposable>(nullDisposable);
        }
    }

    internal readonly struct NullDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
