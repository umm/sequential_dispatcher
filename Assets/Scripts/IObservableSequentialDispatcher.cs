using System;
using JetBrains.Annotations;
using UniRx;

namespace UnityModule
{
    public enum DispatchState
    {
        None,
        Dispatching,
        Dispatched,
    }

    public interface IDispatchStateObservable
    {
        IReactiveProperty<DispatchState> DispatchStateProperty { get; }
    }

    public interface IDispatchStateObservable<T>
    {
        IReactiveProperty<(DispatchState state, T value)> DispatchStateProperty { get; }
    }

    public interface IObservableWillDispatcher : IDispatchStateObservable
    {
    }

    public interface IObservableDidDispatcher : IDispatchStateObservable
    {
    }

    public interface IObservableWillDispatcher<T> : IDispatchStateObservable<T>
    {
    }

    public interface IObservableDidDispatcher<T> : IDispatchStateObservable<T>
    {
    }

    public interface IObservablePreDispatcher
    {
        IObservable<Unit> PreDispatchAsObservable();
    }

    public interface IObservableDispatcher
    {
        IObservable<Unit> DispatchAsObservable();
    }

    public interface IObservablePostDispatcher
    {
        IObservable<Unit> PostDispatchAsObservable();
    }

    public interface IObservablePreDispatcher<T>
    {
        IObservable<T> PreDispatchAsObservable(T value);
    }

    public interface IObservableDispatcher<T>
    {
        IObservable<T> DispatchAsObservable(T value);
    }

    public interface IObservablePostDispatcher<T>
    {
        IObservable<T> PostDispatchAsObservable(T value);
    }

    public interface IObservableSequentialDispatcher :
        IObservableWillDispatcher,
        IObservableDidDispatcher,
        IObservablePreDispatcher,
        IObservableDispatcher,
        IObservablePostDispatcher
    {
    }

    public interface IObservableSequentialDispatcher<T> :
        IObservableWillDispatcher<T>,
        IObservableDidDispatcher<T>,
        IObservablePreDispatcher<T>,
        IObservableDispatcher<T>,
        IObservablePostDispatcher<T>
    {
    }

    [PublicAPI]
    public static class ObservableSequentialDispatcher
    {
        public static IObservable<Unit> RunAsObservable(this IObservableSequentialDispatcher self, bool resetOnFinish = true)
        {
            self.WillDispatchAsObservable()
                .ContinueWith(_ => self.PreDispatchAsObservable())
                .ContinueWith(_ => self.DispatchAsObservable())
                .ContinueWith(_ => self.PostDispatchAsObservable())
                .Subscribe(_ => self.SetDispatchState(DispatchState.Dispatched));
            if (resetOnFinish)
            {
                self.DidDispatchAsObservable().Subscribe(_ => self.ResetDispatchState());
            }
            return self.DidDispatchAsObservable();
        }

        public static IObservable<T> RunAsObservable<T>(this IObservableSequentialDispatcher<T> self, T defaultValue = default, bool resetOnFinish = true)
        {
            self.WillDispatchAsObservable()
                .Select(_ => defaultValue)
                .ContinueWith(self.PreDispatchAsObservable)
                .ContinueWith(self.DispatchAsObservable)
                .ContinueWith(self.PostDispatchAsObservable)
                .Subscribe(_ => self.SetDispatchState(DispatchState.Dispatched));
            if (resetOnFinish)
            {
                self.DidDispatchAsObservable().Subscribe(_ => self.ResetDispatchState());
            }
            return self.DidDispatchAsObservable();
        }

        public static IObservable<Unit> WillDispatchAsObservable(this IObservableWillDispatcher self)
        {
            return self.DispatchStateProperty.Where(x => x == DispatchState.Dispatching).AsUnitObservable();
        }

        public static IObservable<T> WillDispatchAsObservable<T>(this IObservableWillDispatcher<T> self)
        {
            return self.DispatchStateProperty.Where(x => x.state == DispatchState.Dispatching).Select(x => x.value);
        }

        public static IObservable<Unit> DidDispatchAsObservable(this IObservableDidDispatcher self)
        {
            return self.DispatchStateProperty.Where(x => x == DispatchState.Dispatched).AsUnitObservable();
        }

        public static IObservable<T> DidDispatchAsObservable<T>(this IObservableDidDispatcher<T> self)
        {
            return self.DispatchStateProperty.Where(x => x.state == DispatchState.Dispatched).Select(x => x.value);
        }

        public static void ResetDispatchState(this IDispatchStateObservable self)
        {
            self.SetDispatchState(DispatchState.None);
        }

        public static void ResetDispatchState<T>(this IDispatchStateObservable<T> self)
        {
            self.SetDispatchState(DispatchState.None);
        }

        public static void SetDispatchState(this IDispatchStateObservable self, DispatchState dispatchState)
        {
            self.DispatchStateProperty.Value = dispatchState;
        }

        public static void SetDispatchState<T>(this IDispatchStateObservable<T> self, DispatchState dispatchState)
        {
            self.DispatchStateProperty.Value = (dispatchState, self.DispatchStateProperty.Value.value);
        }
    }
}