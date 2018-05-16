using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using STARS.Applications.Interfaces.Dialogs;
using STARS.Applications.Interfaces.EntityManager;
using STARS.Applications.VETS.Interfaces;
using STARS.Applications.VETS.Interfaces.Constants;
using STARS.Applications.VETS.Interfaces.Entities;
using STARS.Applications.VETS.Interfaces.Logging;
using STARS.Applications.VETS.Interfaces.TestExecution;
using STARS.Applications.VETS.Interfaces.TestExecution.Activities;
using STARS.Applications.VETS.Interfaces.TestExecution.Activities.Attributes;
using Stars.ApplicationManager;
using Stars.Resources;
using log4net;
using System.Threading;
using STARS.Applications.VETS.Interfaces.Devices;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    /// <summary>
    /// Activity to update VETS resources from VTS file
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [AsyncTestActivity(typeof(IEmissionTestRunContext),
        TriggerState = EmissionTestStates.PreTestConfiguration,
        BlockStateExit = EmissionTestStates.PreTestConfiguration)]
    internal class UpdateSystemStateTestStart : IAsyncTestActivity<IEmissionTestRunContext>
    {

        private static Thread _monitorRunningTestThread;

        internal IStarsApplication _starsApplication;
        internal ILocalResourceSupport _localResourceSupport;
        internal ITestStatus _testStatus;
        internal IEntityQuery _entityQuery;
        internal IVETSEntityManagerView _entityManagerView;
        internal IDialogService _dialogService;
        internal ISystemLogManager _systemLogManager;

        #region Imports
        #pragma warning disable 649

        [ImportingConstructor]
        public UpdateSystemStateTestStart
        (
            IStarsApplication starsApplication,
            ILocalResourceSupport localResourceSupport,
            ITestStatus testStatus,
            IEntityQuery entityQuery,
            IVETSEntityManagerView entityManagerView,
            IDialogService dialogService,
            ISystemLogManager systemLogManager
            
        )
        {
            _starsApplication = starsApplication;
            _localResourceSupport = localResourceSupport;
            _testStatus = testStatus;
            _entityQuery = entityQuery;
            _entityManagerView = entityManagerView;
            _dialogService = dialogService;
            _systemLogManager = systemLogManager;
        }

        #pragma warning restore 649
        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {

        }

        #endregion

        #region Implementation of IAsyncTestActivity

        /// <summary>
        /// Do all test stand updating and re-activate if there have been any changes
        /// </summary>
        /// <param name="runContext">The context to use to run the activity</param>
        /// <param name="activityStatus">Call methods on this object to indicate state and progress of activity</param>
        public void Run(IActivityRunContext<IEmissionTestRunContext> runContext, IActivityStatus activityStatus)
        {
            _runContext = runContext;
            _activityStatus = activityStatus;
            UpdateSystemState();

            try
            {
                _activityStatus.Completed();
            }
            catch (Exception e)
            {
                if (!_aborted)
                    _activityStatus.Failed(e);
            }
        }

        private void UpdateSystemState()
        {        
            Test runningTest = _entityQuery.FirstOrDefault<Test>(x => x.ID == _testStatus.RunningTests.Last().RunningTestID);
            Vehicle runningVehicle = _entityQuery.FirstOrDefault<Vehicle>(x => x.Name == runningTest.VehicleName);
           
            try { Main._testType.SetByString(runningTest.Properties.FirstOrDefault(x => x.Key == "TestProcedureName").Value.ToString()); }
            catch { Main._testType.SetByString(String.Empty); }

            try { Main._operatorID.SetByString(runningTest.CustomFieldValues.FirstOrDefault(x => x.CustomFieldID == "OperatorID").Value); }
            catch { Main._operatorID.SetByString(String.Empty); }
            try { Main._driverID.SetByString(runningTest.CustomFieldValues.FirstOrDefault(x => x.CustomFieldID == "DriverID").Value); }
            catch { Main._driverID.SetByString(String.Empty); }

            try { Main._vehicleType.SetByString(runningVehicle.CustomFieldValues.FirstOrDefault(x => x.CustomFieldID == "ModelType").Value); }
            catch { Main._vehicleType.SetByString(String.Empty); }
            try { Main._vehicleManufacturer.SetByString(runningVehicle.CustomFieldValues.FirstOrDefault(x => x.CustomFieldID == "ManufacturerName").Value.ToString()); }
            catch { Main._vehicleManufacturer.SetByString(String.Empty); }

            Main._systemState.SetCurrentStateRunningTest();
            Main._isInit = false;

            _monitorRunningTestThread = new Thread(_ => MonitorRunningTest());
            _monitorRunningTestThread.IsBackground = true;
            _monitorRunningTestThread.Start();
        }

        private void MonitorRunningTest()
        {
            int runningTests = _testStatus.RunningTests.Count();
            Main._testTimer = 0;
            while (_testStatus.RunningTests.Count() == runningTests)
            {
                Main._testState.SetStateByNumber(_testStatus.RunningTests.Last().TestState.CurrentState);
                Thread.Sleep(1000);
                Main._testTimer++;
            }
            if (_testStatus.RunningTests.Count() < runningTests)
            {
                Main._testState.UpdateForTestEnded();
                Main._testType.SetAsNoRunningTest();
                Main._operatorID.SetAsNoRunningTest();
                Main._driverID.SetAsNoRunningTest();
                Main._vehicleType.SetAsNoRunningTest();
                Main._vehicleManufacturer.SetAsNoRunningTest();
                Main._idleTimer = Config.MaxTimer;
                Main._systemState.SetCurrentStateSystemActive();
            }
        }

        /// <summary>
        /// Abort the execution of this activity. Will only be called if the activity is running.
        /// </summary>
        /// <param name="runContext">The run context</param>
        public void Abort(IActivityRunContext<IEmissionTestRunContext> runContext)
        {
            _aborted = true;
        }

        private void ShowMessage(string message)
        {
            var result = _dialogService.PromptUser(
               "Title",
                string.Format(message),
                DialogIcon.Warning,
                DialogButton.Yes,
                DialogButton.Yes, DialogButton.No);
        }

        /// <summary>
        /// An action that will rollback any actions that the activity has taken. This will only be called
        /// if the activity has run and completed.
        /// </summary>
        public Action<IActivityRunContext<IEmissionTestRunContext>, Action<Exception>> Rollback
        {
            get { return null; }
        }

        #endregion 

        #region Fields

        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IActivityRunContext<IEmissionTestRunContext> _runContext;

        private IActivityStatus _activityStatus;

        private bool _aborted;

        private readonly ActionExecutor _actionExecutor = new ActionExecutor();

        #endregion
    }
}
