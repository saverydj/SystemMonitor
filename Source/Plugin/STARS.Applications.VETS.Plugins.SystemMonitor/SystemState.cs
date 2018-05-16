using System;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    public class SystemState
    {

        public const string SYSTEM_IDLE = "System Idle";
        public const string SYSTEM_ACTIVE = "System Active";
        public const string RUNNING_TEST = "Running Test";

        public int StateEnum = 1;

        #region Set State

        public void SetCurrentStateSystemIdle()
        {
            StateEnum = 0;
        }

        public void SetCurrentStateSystemActive()
        {
            StateEnum = 1;
        }

        public void SetCurrentStateRunningTest()
        {
            StateEnum = 2;
        }

        #endregion

        #region Is State

        public bool IsCurrentStateSystemIdle()
        {
            return StateEnum == 0;
        }

        public bool IsCurrentStateSystemActive()
        {
            return StateEnum == 1;
        }

        public bool IsCurrentStateRunningTest()
        {
            return StateEnum == 2;
        }

        #endregion

        #region GetState

        public string GetStateAsString()
        {
            if (StateEnum == 0) return SYSTEM_IDLE;
            else if (StateEnum == 1) return SYSTEM_ACTIVE;
            else return RUNNING_TEST;
        }

        #endregion

    }
}
