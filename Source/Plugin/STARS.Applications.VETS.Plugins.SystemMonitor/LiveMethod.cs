using System;
using System.ComponentModel.Composition;
using System.Linq;
using log4net;
using Stars.DataDistribution;
using STARS.Applications.VETS.Interfaces;
using STARS.Applications.VETS.Interfaces.ResourceData;
using STARS.Applications.VETS.Interfaces.TestExecution;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    /// <summary>
    /// A connection to a method on a loaded (live) resource
    /// </summary>
    [Export(typeof(ILiveMethod))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class LiveMethod : ILiveMethod
    {
        [Import]
        public IDynamicMethod DynamicMethod { get; set; }
        [Import]
        public IDeployPath DeployPath { get; set; }
        [Import]
        public IOperationRetryIgnore OperationRetryIgnore;
        [Import]
        internal IMethodOverrides MethodOverrides { get; set; }

        protected static readonly ILog Logger = LogManager.GetLogger("STARS.Applications.VETS.Execution.Methods");

        #region Implementation of ILiveMethod

        /// <summary>
        /// Get an IMethodCaller to allow the specified method to be called
        /// </summary>
        /// <param name="method">The method using any path supported by IDeployPath.GetDeployPath</param>
        /// <returns>An object that can be used to call the method</returns>
        public IMethodCaller GetMethod(ResourceReference method)
        {
            return new MethodCaller(DynamicMethod, DeployPath, OperationRetryIgnore, MethodOverrides, method);
        }

        internal class MethodCaller : IMethodCaller
        {
            private readonly IDynamicMethod _dynamicMethod;
            private readonly string _deployPath;
            private readonly ResourceReference _method;
            private readonly IOperationRetryIgnore _operationRetryIgnore;

            public MethodCaller(IDynamicMethod dynamicMethod, IDeployPath deployPath, IOperationRetryIgnore operationRetryIgnore, IMethodOverrides methodOverrides, ResourceReference method)
            {
                _method = method;
                _dynamicMethod = dynamicMethod;
                _operationRetryIgnore = operationRetryIgnore;
                _deployPath = (method.RootType == ReferenceRoot.SR) ?
                    deployPath.GetDeployPath(methodOverrides.GetOverride(method)) : deployPath.GetDeployPath(method);
            }

            public string MethodName
            {
                get { return _method.ToString(); }
            }

            #region Implementation of IMethodCaller

            /// <summary>
            /// Call the methods
            /// </summary>
            /// <param name="args">Method arguments</param>
            /// <returns>Method return</returns>
            public object[] Invoke(params object[] args)
            {
                Func<object[], string> getArgumentsString = values => string.Join(", ", values.Select(p => p.ToString()).ToArray());

                Logger.InfoFormat("Calling method '{0}' with arguments: {1}", _deployPath, getArgumentsString(args));
                var retval = _dynamicMethod.Invoke(_deployPath, args);
                Logger.InfoFormat("{0} returned from method '{1}' with arguments: {2}", getArgumentsString(retval), _deployPath, getArgumentsString(args));
                return retval;
            }

            /// <summary>
            /// Call the methods
            /// </summary>
            /// <param name="args">Method arguments</param>
            /// <returns>Method return</returns>
            /// <remarks>Will throw if method does not have a single return value of the specified type</remarks>
            public T Invoke<T>(params object[] args)
            {
                var methodReturn = Invoke(args);

                try
                {
                    return (T)methodReturn[0];
                }
                catch (Exception)
                {
                    throw new ApplicationException(
                        string.Format("Method '{1} 'type incorrect, must be '{1}', is '{2}'."  , _deployPath, typeof(T).Name, methodReturn));
                }
            }

            public void InvokeWithReturnCheckRetryIgnore(bool allowRetry, bool allowIgnore, params object[] args)
            {
                InvokeWithReturnCheckRetryIgnore(null, null, null, allowRetry, allowIgnore, args);
            }

            /// <summary>
            /// Calls the method and, if it returns an integer, checks that return
            /// value for a non-zero error code. If the return value is not zero then
            /// an exception is thrown.
            /// </summary>
            /// <param name="operationName"></param>
            /// <param name="exceptionMessage"></param>
            /// <param name="abortMessage"></param>
            /// <param name="allowRetry">If the method fail, user can retry it</param>
            /// <param name="allowIgnore">If the method fail, user can ignore it and continue test</param>
            /// <param name="args">Method arguments</param>
            public void InvokeWithReturnCheckRetryIgnore(string operationName = null,
                string exceptionMessage = null, string abortMessage = null, bool allowRetry = true, bool allowIgnore = false,
                params object[] args)
            {
                _operationRetryIgnore.Execute(
                    operationName ??
                    string.Join(" ", _method.NodeNames.Reverse().Take(2).Reverse().ToArray()),
                    () =>
                    {
                        try
                        {
                            InvokeWithReturnCheck(args);
                        }
                        catch (Exception e)
                        {
                            return exceptionMessage ?? e.Message;
                        }
                        return null;
                    }, abortMessage, allowRetry, allowIgnore);
            }

            public void InvokeWithReturnCheck(params object[] args)
            {
                var retval = Invoke(args);
                if (retval != null && retval.Length > 0 && retval[0] is int)
                {
                    var returnCode = (int)retval[0];
                    if (returnCode != 0)
                        throw GetExceptionFromReturnCode(returnCode);
                }
            }

            /// <summary>
            /// Get an excption for a non-zero ULI method return code. 
            /// If the method is a call on an SR then the return code
            /// will be used to attempt to look up a string resource and that will be 
            /// included in the exception method.
            /// </summary>
            /// <param name="returnCode"></param>
            /// <returns></returns>
            public Exception GetExceptionFromReturnCode(int returnCode)
            {
                var methodParts = _method.NodeNames.ToArray();
                string errorMessage = null;
                if (methodParts.Length > 1)
                {
                    // Look for a resource string of the form ReturnCodes_[Device]_[ReturnCode]
                    errorMessage =
                        "hi";
                    if (errorMessage != null)
                        errorMessage = "bye";
                }

                errorMessage = errorMessage ??
                               "hello";
                var exception = new ApplicationException(errorMessage);
                return exception;
            }

            #endregion
        }

        #endregion
    }
}