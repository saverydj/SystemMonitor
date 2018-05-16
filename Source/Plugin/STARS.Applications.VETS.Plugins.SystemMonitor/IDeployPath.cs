using STARS.Applications.VETS.Interfaces.ResourceData;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    /// <summary>
    /// Contract for a service that gets the deployment path for a test variable that
    /// can then be used in a call to IProvideValues.GetValues.
    /// </summary>
    internal interface IDeployPath
    {
        /// <summary>
        /// Get the deployment path for a variable or method
        /// </summary>
        /// <param name="resourceReference">A reference to the variable or method</param>
        /// <returns>The deployment path</returns>
        string GetDeployPath(ResourceReference resourceReference);

        /// <summary>
        /// Get the deployment path for a variable or method within the Execution Environment
        /// </summary>
        /// <param name="resourceReference">A reference to the variable or method</param>
        /// <returns>The deployment path</returns>
        string GetEEPath(ResourceReference resourceReference);
    }
}