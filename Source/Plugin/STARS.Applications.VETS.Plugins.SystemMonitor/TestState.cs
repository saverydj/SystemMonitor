using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    public class TestState
    {
        public const string Idle = "Idle";
        public const string PreTestConfig = "Pre-test configuration";
        public const string PrepResources = "Preparing resources";
        public const string LoadResources = "Loading resources";
        public const string Connecting = "Connecting";
        public const string PreInit = "PreInitialize";
        public const string PreTest = "Pre-test";
        public const string PrepareForTest = "Prepare for test";
        public const string PrepareForCycle = "Prepare for cycle";
        public const string PrepareForDrive = "Prepare for drive";
        public const string ReadyToRun = "Ready to run";
        public const string TestStart = "Test start";
        public const string PendantWait = "Pendant Wait";
        public const string DriveUnitRunning = "Drive unit running";
        public const string HotSoak = "Hot Soak";
        public const string CoastDownRunning = "Coast down running";
        public const string CoastDownCompleting = "Coast down completing";
        public const string PrepareForIdleCheck = "Prepare for idle check";
        public const string IdleCheckRunning = "Idle check running";
        public const string IdleCheckCompeting = "Idle check completing";
        public const string Logging = "Logging";
        public const string WaitingForBagReads = "Waiting for bag reads";
        public const string Analysis = "Analysis";
        public const string Report = "Report";
        public const string FinalTestOperations = "Final test operations";
        public const string TestEnding = "Test ending";
        public const string Aborted = "Test aborted";
        public const string Completed = "Test completed";
        public const string NoRunningTest = "No Running Test";

        public string StateName;
        public int StateKey;

        public TestState()
        {
            SetStateByNumber(18000);
        }

        public void SetStateByNumber(int number)
        {
            if (number == 0) StateName = Idle;
            if (number == 1000) StateName = PreTestConfig;
            if (number == 2000) StateName = PrepResources;
            if (number == 3000) StateName = LoadResources;
            if (number == 3200) StateName = Connecting;
            if (number == 3400) StateName = PreInit;
            if (number == 4000) StateName = PreTest;
            if (number == 4100) StateName = PreTest;
            if (number == 4200) StateName = PreTest;
            if (number == 4300) StateName = PrepareForTest;
            if (number == 4400) StateName = PrepareForTest;
            if (number == 5000) StateName = PrepareForCycle;
            if (number == 6000) StateName = PrepareForDrive;
            if (number == 7000) StateName = ReadyToRun;
            if (number == 8000) StateName = TestStart;
            if (number == 8500) StateName = PendantWait;
            if (number == 9000) StateName = DriveUnitRunning;
            if (number == 10000) StateName = HotSoak;
            if (number == 10600) StateName = CoastDownRunning;
            if (number == 10700) StateName = CoastDownCompleting;
            if (number == 10750) StateName = PrepareForIdleCheck;
            if (number == 10800) StateName = PrepareForIdleCheck;
            if (number == 10810) StateName = IdleCheckRunning;
            if (number == 10820) StateName = IdleCheckCompeting;
            if (number == 11000) StateName = Logging;
            if (number == 11500) StateName = WaitingForBagReads;
            if (number == 12000) StateName = Analysis;
            if (number == 13000) StateName = Report;
            if (number == 14000) StateName = FinalTestOperations;
            if (number == 15000) StateName = TestEnding;
            if (number == 16000) StateName = Completed;
            if (number == 17000) StateName = Aborted;
            if (number == 18000) StateName = NoRunningTest;
            StateKey = number;
        }

        public void UpdateForTestEnded()
        {
            if (StateKey == 15000) SetStateByNumber(16000);
            else if (StateKey == 16000) return;
            else SetStateByNumber(17000);
        }

    }
}
