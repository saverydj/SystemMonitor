using System;
using System.ComponentModel.Composition;
using System.Timers;
using log4net;

namespace STARS.Applications.VETS.Execution.LiveData
{
    /// <summary>
    /// Singleton instance to provide clock ticks to time based components.
    /// </summary>
    [Export(typeof(ISampler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class Sampler : ISampler, IDisposable
    {
        protected static ILog Logger = LogManager.GetLogger("STARS.Applications.VETS.Execution.LiveData.Sampler");
        private readonly Timer _timer = new Timer();
        private const int UpdateInterval = 250; //Update interval in milliseconds
    
        public event Action OnTick;

        public Sampler()
        {
            _timer.Interval = UpdateInterval;
            _timer.Elapsed += (sender, args) =>
            {
                if (OnTick == null)
                    return;

                try
                {
                    OnTick();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            };
            _timer.Start();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
