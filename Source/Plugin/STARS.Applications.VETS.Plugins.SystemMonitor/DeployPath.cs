using System;
using System.ComponentModel.Composition;
using System.Linq;
using STARS.Applications.VETS.Interfaces.Constants;
using STARS.Applications.VETS.Interfaces.ResourceData;
using Stars.Resources;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    /// <summary>
    /// Implementation of IDeployPath
    /// </summary>
    [Export(typeof(IDeployPath))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class DeployPath : IDeployPath
    {
        private const string WorkstationResourcePath = @"\\Root_UserData_SR_WorkstationStatus";
        private const string RtNodeString = "RT";
        private string _rootDeployPath;

        [Import] public ILiveResource LiveResources { get; set; }

        #region Implementation of IDeployPath

        /// <summary>
        /// Get the deployment path for a variable
        /// </summary>
        /// <param name="resourceReference">A reference to the variable</param>
        /// <returns>The deployment path</returns>
        public string GetDeployPath(ResourceReference resourceReference)
        {
            var starsSeperator = new string(new[]{ResourceReference.PathSeparator});
            switch (resourceReference.RootType)
            {
                case ReferenceRoot.SR:
                    return RootDeployPath + ResourceReference.SrNodeString + starsSeperator +
                           string.Join(starsSeperator, resourceReference.NodeNames.ToArray());

                case ReferenceRoot.SN:
                    return RootDeployPath + ResourceReference.SnNodeString + starsSeperator +
                           string.Join(starsSeperator, resourceReference.NodeNames.ToArray());

                case ReferenceRoot.RT:
                    return RootDeployPath + RtNodeString + starsSeperator +
                           string.Join(starsSeperator, resourceReference.NodeNames.ToArray());

                //case ReferenceRoot.ActiveTestStand:
                //    string testStandDeployPath = GetTestStandDeployPath();
                //    return testStandDeployPath == null
                //               ? null
                //               : testStandDeployPath + starsSeperator + string.Join(starsSeperator, resourceReference.NodeNames.ToArray());

                case ReferenceRoot.VETS:
                    return ValueProviders.BagData + starsSeperator + string.Join(starsSeperator, resourceReference.NodeNames.ToArray());

                case ReferenceRoot.ExecutionRoot:
                    return RootDeployPath + string.Join(starsSeperator, resourceReference.NodeNames.ToArray());

                default:
                    return string.Join(starsSeperator, resourceReference.NodeNames.ToArray());
            }
        }

        /// <summary>
        /// Get the deployment path for a variable or method within the Execution Environment
        /// </summary>
        /// <param name="resourceReference">A reference to the variable or method</param>
        /// <returns>The deployment path</returns>
        public string GetEEPath(ResourceReference resourceReference)
        {
            var deployPath = GetDeployPath(resourceReference);
            if (!String.IsNullOrEmpty(RootDeployPath) && deployPath.StartsWith(RootDeployPath))
                return ResourceReference.PathRoot + deployPath.Substring(RootDeployPath.Length);

            return resourceReference.ToString();
        }

        #endregion

        #region Implemementation

        /// <summary>
        /// Get the deployment path of the currently loaded test stand
        /// </summary>
        /// <returns>The deployment path</returns>
        //private string GetTestStandDeployPath()
        //{
        //    var testStandDeployPaths = LiveResources.GetDeployPath(ResourceHelper.TESTSTAND_TYPE, Environment.MachineName, null);
        //    string testStandDeployPath = null;

        //    if (testStandDeployPaths != null && testStandDeployPaths.Length > 0)
        //        testStandDeployPath = testStandDeployPaths[0];
        //    return testStandDeployPath;
        //}

        /// <summary>
        /// The root deployment path of the local EE
        /// </summary>
        private string RootDeployPath
        {
            get
            {
                if (_rootDeployPath == null)
                {
                    // Bit of a hack this but allows us to form SR deploy paths before items are loaded so was don't have to wait for the test load.
                    string[] deployPaths = LiveResources.GetDeployPath(WorkstationResourcePath, new[] { Environment.MachineName });
                    if (deployPaths == null)
                    {
                        throw new ApplicationException("whoops");
                    }

                    _rootDeployPath = deployPaths[0].Replace("SR_WorkstationStatus", null);

                }
                return _rootDeployPath;
            }
        }
        #endregion
    }
}
