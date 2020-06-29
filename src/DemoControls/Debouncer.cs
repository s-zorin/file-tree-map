using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoControls
{
    internal class Debouncer
    {
        private readonly AutoResetEvent autoResetEvent;
        private readonly TimeSpan delay;
        private CancellationTokenSource? cts;

        public Debouncer(TimeSpan delay)
        {
            this.delay = delay;
            autoResetEvent = new AutoResetEvent(true);
        }

        public void Enqueue(Action action)
        {
            autoResetEvent.WaitOne();

            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            Task.Delay(delay, cts.Token)
                .ContinueWith(
                    task => action(),
                    cts.Token,
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                    TaskScheduler.Current);

            autoResetEvent.Set();
        }
    }
}
