using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToElastic
{
    public class LogInfo
    {
        public string FilePath { get; private set; } //Path to csv log file
        public int Date { get; private set; } //Index of Date column in csv
        public int Time { get; private set; } //Index of Time column in csv
        public int SystemState { get; private set; } //Index of System State column in csv
        public int TestState { get; private set; } //Index of Test State column in csv
        public int TestType { get; private set; } //Index of Test Type column in csv
        public int VehicleType { get; private set; } //Index of Vehicle Type column in csv
        public int DriverID { get; private set; } //Index of Driver ID column in csv
        public int Max { get; private set; } //Max index from above
        public DateTime DateTime { get; private set; } //First date time in log file, on row 5 

        public LogInfo()
        {

        }

        public LogInfo
        (
            string filePath, 
            int date, 
            int time,
            int systemState, 
            int testState, 
            int testType, 
            int vehicleType, 
            int driverID, 
            int max, 
            DateTime dateTime
        )
        {
            FilePath = filePath;
            Date = date;
            Time = time;
            SystemState = systemState;
            TestState = testState;
            TestType = testType;
            VehicleType = vehicleType;
            DriverID = driverID;
            Max = max;
            DateTime = dateTime;
        }

    }
}
