using System.Timers;
using System;

namespace Yamashiro.Infrastructure.Timers
{
    public class SetInterval
    {
        public static Timer Create(Action action, TimeSpan timeout)
        {
            var timer = new Timer(timeout.TotalMilliseconds) 
            {
                AutoReset = true,
                Enabled = true
            };

            timer.Elapsed += (sender, args) =>
            {
                action.Invoke();
            };

            return timer;
        }

        public static void Destroy(Timer timer)
        {
            timer.Stop();
            timer.Dispose();
            GC.SuppressFinalize(timer);
        }
    }
}