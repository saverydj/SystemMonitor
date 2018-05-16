using System;
using System.Threading;

namespace STARS.Applications.VETS.Execution.LiveData
{
    internal static class LiveDataExtensions
    {
        /// <summary>
        /// Periodically poll the function until it returns true or a timeout is 
        /// reached
        /// </summary>
        /// <param name="variableCheckFunc">Function to poll</param>
        /// <param name="timeout">Time to poll for</param>
        /// <returns>True if function returned true within the timeout, otherwise false.</returns>
        internal static bool WaitFor(this Func<bool> variableCheckFunc, TimeSpan timeout)
        {
            TimeSpan pollPeriod = TimeSpan.FromSeconds(0.5); 
            var varStartTime = DateTime.Now;
            while (varStartTime + timeout > DateTime.Now)
            {
                if (variableCheckFunc())
                    return true;

                Thread.Sleep(pollPeriod.Milliseconds);
            }

            return false;
        }        
    }
}