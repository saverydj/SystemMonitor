using System;

namespace STARS.Applications.VETS.Execution.LiveData
{
    /// <summary>
    /// Shared service to provide clock ticks to time based components.
    /// </summary>
    internal interface ISampler
    {
        event Action OnTick;
    }
}