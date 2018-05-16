using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using STARS.Applications.VETS.Plugins.SystemMonitor.Properties;
using STARS.Applications.VETS.Interfaces.Devices;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    static class Main
    {
        #region Imports

        public static OnlineResources _onlineResources;
        public static IDeviceManager _deviceManager;

        #endregion

        #region Monitored Variables

        public static SystemState _systemState;
        public static TestState _testState;

        public static TestType _testType;
        public static VehicleType _vehicleType;
        public static VehicleManufacturer _vehicleManufacturer;

        public static IdType _operatorID;
        public static IdType _driverID;

        public static int _idleTimer = Config.MaxTimer;
        public static int _testTimer = 0;

        public static double _speed = 0;
        public static double _targetSpeed = 0;
        public static int _isDynoActive = 0;
        public static bool _shouldDynoActive = false;

        public static double _cellTemperature = 0;
        public static double _relativeHumidity = 0;
        public static double _barometer = 0;
        public static int _numberOfActiveAlarms = 0;

        #endregion

        #region Globals

        public static bool _isInit;

        #endregion

        #region Init

        public static void Init(OnlineResources onlineResources, IDeviceManager deviceManager)
        {
            _systemState = new SystemState();
            _testState = new TestState();

            _testType = new TestType();
            _vehicleType = new VehicleType();
           _vehicleManufacturer = new VehicleManufacturer();

           _operatorID = new IdType();
           _driverID = new IdType();

            _isInit = true;

            SetImports(onlineResources, deviceManager);
            SetSN();
        }

        #endregion

        #region State Changed Event

        public delegate void StateChangedEventHandler(object sender, EventArgs e);
        public static event StateChangedEventHandler StateChanged;

        public static void RaiseStateChangedEvent()
        {
            if (StateChanged != null)
            {
                StateChanged(null, EventArgs.Empty);
            }
        }

        #endregion

        #region Monitor Timer

        private static EventWaitHandle _timerReset;
        private static EventWaitHandle _timerTick;

        private static EventWaitHandle SetEventWaitHandle(string handleName)
        {
            EventWaitHandle ewh = null;
            try { ewh = EventWaitHandle.OpenExisting(handleName); }
            catch { ewh = new EventWaitHandle(false, EventResetMode.AutoReset, handleName); }
            return ewh;
        }

        public static void MonitorTimerReset()
        {
            _timerReset = SetEventWaitHandle(Properties.Resources.TimerReset);
            while (true)
            {
                _timerReset.WaitOne();
                _idleTimer = Config.MaxTimer;
                if (!_systemState.IsCurrentStateRunningTest()) _systemState.SetCurrentStateSystemActive();
                UpdateCommon();
            }
        }

        public static void MonitorTimerTick()
        {
            _timerTick = SetEventWaitHandle(Properties.Resources.TimerTick);
            while (true)
            {
                _timerTick.WaitOne();
                if (_idleTimer > 0) _idleTimer--;
                if (_idleTimer <= 0 && !_systemState.IsCurrentStateRunningTest()) _systemState.SetCurrentStateSystemIdle();
                UpdateCommon();
            }
        }

        #endregion

        #region Update Common

        public static void UpdateCommon()
        {
            UpdateAtmosphereData();
            UpdateDynoData();
            UpdateAlarmData();
            PushToSN();
            RaiseStateChangedEvent();
        }

        public static void UpdateAtmosphereData()
        {
            _cellTemperature = Math.Round((_onlineResources.GetValueAsDouble("CellTemperature") - 273.15), 1);
            _relativeHumidity = Math.Round((_onlineResources.GetValueAsDouble("RelativeHumidity") * 100), 1);
            _barometer = Math.Round((_onlineResources.GetValueAsDouble("Barometer") / 1000), 1);
        }

        public static void UpdateDynoData()
        {
            if (!_shouldDynoActive && _testState.StateName == TestState.DriveUnitRunning) _shouldDynoActive = true;
            if
            (
                _shouldDynoActive &&
                _testState.StateName != TestState.DriveUnitRunning &&
                _testState.StateName != TestState.CoastDownRunning &&
                _testState.StateName != TestState.CoastDownCompleting
            )
            {
                _shouldDynoActive = false;
            }

            _speed = _onlineResources.GetValueAsDouble("Speed");

            if (_shouldDynoActive) _targetSpeed = _onlineResources.GetValueAsDouble("TargetSpeed");
            else _targetSpeed = 0;

            _speed = Math.Round(_speed, 2)*2.23694;
            _targetSpeed = Math.Round(_targetSpeed, 2)*2.23694;

            _isDynoActive = (_speed > 0 ? 1 : 0);
        }

        public static void UpdateAlarmData()
        {
            int num = 0;
            if (_deviceManager != null && !_isInit)
            {
                var hi = _deviceManager.GetDevices().Where(device => (device is IProvideDeviceAlarms)).Select(device => device as IProvideDeviceAlarms).Distinct();
                foreach (var ok in hi)
                {
                    num += ok.GetAlarms().Length;
                }
            }
            _numberOfActiveAlarms = num;
        }

        #endregion

        #region Set Imports

        public static void SetImports(OnlineResources onlineResources, IDeviceManager deviceManager)
        {
            _onlineResources = onlineResources;
            _deviceManager = deviceManager;
        }

        #endregion

        #region Standard Names

        public static void SetSN()
        {
            // Get monitored data from these SN
            _onlineResources.AddEntry("Speed", "SN_Speed");
            _onlineResources.AddEntry("TargetSpeed", "SN_TargetSpeed");
            _onlineResources.AddEntry("CellTemperature", "SN_CellTemperature");
            _onlineResources.AddEntry("RelativeHumidity", "SN_RelativeHumidity");
            _onlineResources.AddEntry("Barometer", "SN_Barometer");

            //Push monitored data to these SN
            _onlineResources.AddEntry("MonitorSystemState", "SN_MonitorSystemState");
            _onlineResources.AddEntry("MonitorTestType", "SN_MonitorTestType");
            _onlineResources.AddEntry("MonitorVehicleManufacturer", "SN_MonitorVehicleManufacturer");
            _onlineResources.AddEntry("MonitorVehicleType", "SN_MonitorVehicleType");
            _onlineResources.AddEntry("MonitorDriverID", "SN_MonitorDriverID");
            _onlineResources.AddEntry("MonitorDynoActive", "SN_MonitorDynoActive");
            _onlineResources.AddEntry("MonitorDynoSpeed", "SN_MonitorDynoSpeed");
            _onlineResources.AddEntry("MonitorTestCellTemperature", "SN_MonitorTestCellTemperature");
            _onlineResources.AddEntry("MonitorTestCellHumidity", "SN_MonitorTestCellHumidity");
            _onlineResources.AddEntry("MonitorTestCellPressure", "SN_MonitorTestCellPressure");
            _onlineResources.AddEntry("MonitorNumberOfActiveErrors", "SN_MonitorNumberOfActiveErrors");
            _onlineResources.AddEntry("MonitorTestState", "SN_MonitorTestState");
            _onlineResources.AddEntry("MonitorOperatorID", "SN_MonitorOperatorID");
            _onlineResources.AddEntry("MonitorTargetSpeed", "SN_MonitorTargetSpeed");
            _onlineResources.AddEntry("MonitorIdleTimer", "SN_MonitorIdleTimer");
            _onlineResources.AddEntry("MonitorTestTimer", "SN_MonitorTestTimer");
        }

        public static void PushToSN()
        {
            _onlineResources.SetValue("MonitorSystemState", _systemState.StateEnum);
            _onlineResources.SetValue("MonitorTestType", _testType.TypeEnum);
            _onlineResources.SetValue("MonitorVehicleManufacturer", _vehicleManufacturer.TypeEnum);
            _onlineResources.SetValue("MonitorVehicleType", _vehicleType.TypeEnum);
            _onlineResources.SetValue("MonitorDriverID", _driverID.TypeEnum);
            _onlineResources.SetValue("MonitorDynoActive", _isDynoActive);
            _onlineResources.SetValue("MonitorDynoSpeed", _speed);
            _onlineResources.SetValue("MonitorTestCellTemperature", _cellTemperature);
            _onlineResources.SetValue("MonitorTestCellHumidity", _relativeHumidity);
            _onlineResources.SetValue("MonitorTestCellPressure", _barometer);
            _onlineResources.SetValue("MonitorNumberOfActiveErrors", _numberOfActiveAlarms);
            _onlineResources.SetValue("MonitorTestState", _testState.StateKey);
            _onlineResources.SetValue("MonitorOperatorID", _operatorID.TypeEnum);
            _onlineResources.SetValue("MonitorTargetSpeed", _targetSpeed);
            _onlineResources.SetValue("MonitorIdleTimer", _idleTimer);
            _onlineResources.SetValue("MonitorTestTimer", _testTimer);
        }

        #endregion

    }
}
