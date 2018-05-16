using System;
using PushToElastic.StaticTools;

namespace PushToElastic
{
    class TestResults
    {
        public const int TestCompleted = 0;
        public const int TestAborted = 1;

        public double Time;
        public string Date;
        public string Driver;
        public string TestType;
        public string VehicleType;

        private int _results;
        private DateTime _dateTime;
        private bool _isTestRunning;

        public TestResults()
        {
            Init();
        }

        public void Init()
        {
            Time = 0;
            Date = String.Empty;
            Driver = String.Empty;
            TestType = String.Empty;
            VehicleType = String.Empty;
            _results = 0;
            _dateTime = DateTime.Now;
            _isTestRunning = false;
        }

        #region Set Results

        public void OnTestStart(DateTime now, string driver, string testType, string vehicleType)
        {
            if (now == new DateTime()) Init();
            else
            {
                _dateTime = now;
                Driver = driver;
                TestType = testType;
                VehicleType = vehicleType;
                _isTestRunning = true;
            }
        }

        public void CheckIfSameTest(string driver, string testType, string vehicleType)
        {
            if (_isTestRunning && (driver != Driver || testType != TestType || vehicleType != VehicleType))
            {
                Init();
            }
        }

        public void SetResults(DateTime now, int results)
        {
            Time = (now - _dateTime).TotalSeconds;
            Date = JsonTime.Convert(now);
            _results = results;
            _dateTime = now;
            _isTestRunning = false;
        }

        public void SetTestCompleted(DateTime now)
        {
            SetResults(now, TestCompleted);
        }

        public void SetTestAborted(DateTime now)
        {
            SetResults(now, TestAborted);
        }

        #endregion

        #region Get Results

        public bool IsTestCompleted()
        {
            return _results == TestCompleted && !_isTestRunning;
        }

        public bool IsTestAborted()
        {
            return _results == TestAborted && !_isTestRunning;
        }

        public int GetResults()
        {
            return _results;
        }

        public bool IsTestRunning()
        {
            return _isTestRunning;
        }

        #endregion

    }
}
