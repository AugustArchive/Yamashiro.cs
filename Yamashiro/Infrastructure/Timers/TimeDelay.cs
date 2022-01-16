using System.Timers;
using System;

namespace Yamashiro.Infrastructure.Timers
{
    /// <summary>
    /// Creates a timed delay, inspired of JavaScript's setTimeout function
    ///  
    /// Credit: https://stackoverflow.com/a/57430133
    /// </summary>
    public class TimeDelay
    {
        /// <summary>The timer itself</summary>
        private readonly Timer timer;

        /// <summary>The action invoker</summary>
        private readonly Action func;

        public TimeDelay(Action _func, int after)
        {
            if (func == null) throw new Exception("Function can't be null");
            if (after <= 0) throw new Exception("Time cannot be less then zero");

            func = _func;
            timer = new Timer
            {
                Interval = after,
                Enabled = false
            };

            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs args)
        {
            timer.Stop();
            func.Invoke();
            timer.Dispose();
            GC.SuppressFinalize(timer);
        }
    }
}
