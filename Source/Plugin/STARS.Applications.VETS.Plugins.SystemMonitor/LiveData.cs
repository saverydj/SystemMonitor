using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using STARS.Applications.VETS.Interfaces.ResourceData;
using STARS.Applications.VETS.Interfaces.TestExecution;
using Stars.DataDistribution;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    [Export(typeof(ILiveData))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class LiveData : ILiveData
    {
        [Import] public IDeployPath DeployPath { get; set; }
        [Import] public IProvideValues ProvideValues { get; set; }

        #region Implementation of ILiveData

        /// <summary>
        /// Get live data values
        /// </summary>
        /// <param name="variables">Names of the variables to read</param>
        /// <returns>The current values of those variables, keyed by name.</returns>
        public IDictionary<string, object> GetValues(IEnumerable<ResourceReference> variables)
        {
            var namesArray = variables.Select(v => DeployPath.GetDeployPath(v)).ToArray();
            var values = ProvideValues.GetValues(namesArray);
            var dictionary = new Dictionary<string, object>();
            for (int index = 0; index < values.Length; index++)
            {
                dictionary[namesArray[index]] = values[index];
            }
            return dictionary;
        }

        /// <summary>
        /// Get a single live value
        /// </summary>
        /// <param name="variable">The variable to read</param>
        /// <returns>The value of that variable</returns>
        /// <remarks>Will return null if the variable does not exist</remarks>
        public object GetValue(ResourceReference variable)
        {
            return ProvideValues.GetValue(DeployPath.GetDeployPath(variable));
        }

        /// <summary>
        /// Get a single live value
        /// </summary>
        /// <param name="variable">The variable to read</param>
        ///<typeparam name="T">The expected type of the variable</typeparam>
        /// <returns>The value of that variable</returns>
        /// <remarks>Will throw if the variable does not exist</remarks>
        public T GetValue<T>(ResourceReference variable)
        {
            string deployPath = DeployPath.GetDeployPath(variable);

            var value = GetValue(variable);
            if (value == null)
                throw new ApplicationException(
                    string.Format("Cannot read variable '{0}', on deploy path '{1}'", variable, deployPath));

            T retVal;

            try
            {
                retVal = (T)value;
            }
            catch (Exception)
            {
                throw new ApplicationException(
                    string.Format("Deploy path '{0}' type incorrect, must be {1}, is {2}.", deployPath, typeof(T).Name, value.GetType().Name));
            }

            return retVal;
        }

        ///<summary>
        /// Set a live varible value
        ///</summary>
        ///<param name="variable">The variable to write</param>
        ///<param name="value">The value to set the variable to</param>
        public void SetValue(ResourceReference variable, object value)
        {
            ProvideValues.SetValue(DeployPath.GetDeployPath(variable), value);
        }

        #endregion
    }
}
