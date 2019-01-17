using System;
using JetBrains.Annotations;
using UniRx;

namespace UnityModule
{
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

    public interface IObservableSequenceTrigger
    {
        IObservable<Unit> TriggerSequenceAsObservable();
    }

    public interface IObservableSequenceTrigger<out T>
    {
        IObservable<T> TriggerSequenceAsObservable();
    }

    public interface IObservableSequentialDispatcher :
        IObservableSequenceTrigger,
        IObservablePreDispatcher,
        IObservableDispatcher,
        IObservablePostDispatcher
    {
    }

    public interface IObservableSequentialDispatcher<T> :
        IObservableSequenceTrigger<T>,
        IObservablePreDispatcher<T>,
        IObservableDispatcher<T>,
        IObservablePostDispatcher<T>
    {
    }

    [PublicAPI]
    public static class ObservableSequentialDispatcher
    {
        public static IObservable<Unit> CreateSequenceAsObservable(this IObservableSequentialDispatcher self)
        {
            return self.TriggerSequenceAsObservable()
                .SelectMany(_ => self.PreDispatchAsObservable())
                .SelectMany(_ => self.DispatchAsObservable())
                .SelectMany(_ => self.PostDispatchAsObservable());
        }

        public static IObservable<T> CreateSequenceAsObservable<T>(this IObservableSequentialDispatcher<T> self)
        {
            return self.TriggerSequenceAsObservable()
                .SelectMany(self.PreDispatchAsObservable)
                .SelectMany(self.DispatchAsObservable)
                .SelectMany(self.PostDispatchAsObservable);
        }
    }
}