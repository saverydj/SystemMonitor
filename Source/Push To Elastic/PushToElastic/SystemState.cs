using System;
using PushToElastic.StaticTools;

namespace PushToElastic
{
    class SystemState
    {
        public const int SystemInactive = 0;
        public const int SystemActive = 1;
        public const int TestRunning = 2;

        // Time = the number of seconds that the state was x
        // Date = the date time at which the state changed from x to y
        public double Time;
        public string Date;
        private int _previousState;
        private int _currentState;
        private DateTime _dateTime;

        public SystemState()
        {
            Init();          
        }

        public void Init()
        {
            //The first time we record a state change 
            //we don't push any info to Elastic
            Time = 0;
            Date = String.Empty;
            _previousState = -1;
            _currentState = -1;
            _dateTime = DateTime.Now;
        }

        #region Set State

        public void SetCurrentState(DateTime now, int state)
        {
            if(now == new DateTime()) Init();
            else
            {
                Time = (now - _dateTime).TotalSeconds;
                Date = JsonTime.Convert(now);
                _previousState = _currentState;
                _currentState = state;
                _dateTime = now;
            }
        }

        public void SetCurrentStateSystemInactive(DateTime now)
        {
            SetCurrentState(now, SystemInactive);
        }

        public void SetCurrentStateSystemActive(DateTime now)
        {
            SetCurrentState(now, SystemActive);
        }

        public void SetCurrentStateTestRunning(DateTime now)
        {
            SetCurrentState(now, TestRunning);
        }

        #endregion

        #region Get State

        public bool IsPreviousStateSystemInactive()
        {
            return _previousState == SystemInactive;
        }

        public bool IsPreviousStateSystemActive()
        {
            return _previousState == SystemActive;
        }

        public bool IsPreviousStateTestRunning()
        {
            return _previousState == TestRunning;
        }

        public bool IsPreviousState(int currentState)
        {
            return currentState == _previousState;
        }

        public int GetPreviousState()
        {
            return _previousState;
        }

        public bool IsCurrentState(int currentState)
        {
            return currentState == _currentState;
        }

        public int GetCurrentState()
        {
            return _currentState;
        }

        #endregion

    }
}
