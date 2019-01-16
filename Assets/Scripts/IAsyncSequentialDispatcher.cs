using System.Threading;
using JetBrains.Annotations;
using UniRx.Async;

namespace UnityModule
{
    public interface IAsyncPreDispatcher
    {
        UniTask PreDispatchAsync(CancellationToken cancellationToken = default);
    }

    public interface IAsyncDispatcher
    {
        UniTask DispatchAsync(CancellationToken cancellationToken = default);
    }

    public interface IAsyncPostDispatcher
    {
        UniTask PostDispatchAsync(CancellationToken cancellationToken = default);
    }

    public interface IAsyncPreDispatcher<T>
    {
        UniTask<T> PreDispatchAsync(CancellationToken cancellationToken = default);
    }

    public interface IAsyncDispatcher<T>
    {
        UniTask<T> DispatchAsync(CancellationToken cancellationToken = default);
    }

    public interface IAsyncPostDispatcher<T>
    {
        UniTask<T> PostDispatchAsync(CancellationToken cancellationToken = default);
    }

    public interface IAsyncSequentialDispatcher :
        IAsyncPreDispatcher,
        IAsyncDispatcher,
        IAsyncPostDispatcher
    {
    }

    public interface IAsyncSequentialDispatcher<T> :
        IAsyncPreDispatcher<T>,
        IAsyncDispatcher<T>,
        IAsyncPostDispatcher<T>
    {
    }

    [PublicAPI]
    public static class AsyncSequentialDispatcherExtension
    {
        public static async UniTask Run(this IAsyncSequentialDispatcher self)
        {
            await new UniTask()
                .ContinueWith(() => self.PreDispatchAsync())
                .ContinueWith(() => self.DispatchAsync())
                .ContinueWith(() => self.PostDispatchAsync());
        }

        public static async UniTask<T> Run<T>(this IAsyncSequentialDispatcher<T> self)
        {
            return await new UniTask<T>()
                .ContinueWith(_ => self.PreDispatchAsync())
                .ContinueWith(_ => self.DispatchAsync())
                .ContinueWith(_ => self.PostDispatchAsync());
        }
    }
}