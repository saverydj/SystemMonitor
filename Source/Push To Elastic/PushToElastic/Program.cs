using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http.Headers;
using PushToElastic.StaticTools;
using System.Security.Permissions;
using System.Collections.Generic;

namespace PushToElastic
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    class Program
    {
        private static HttpClient _client = new HttpClient();
        private static FileSystemWatcher _watcher = new FileSystemWatcher();

        private static LogInfo _logInfo = new LogInfo();
        private static ManualResetEvent _readLiveEvent = new ManualResetEvent(true);

        private static List<string[]> systemStateQueue = new List<string[]>();
        private static List<string[]> testResultsQueue = new List<string[]>();

        private static Semaphore systemStateQueueSem = new Semaphore(1, 1);
        private static Semaphore testResultsQueueSem = new Semaphore(1, 1);

        private static TestTypeList _testTypeList = new TestTypeList();
        private static VehicleTypeList _vehicleTypeList = new VehicleTypeList();
        private static IdTypeList _idTypeList = new IdTypeList();

        public static void Main(string[] args)
        {
            SetWebAddress();
            SetCredentials();
            CreateIndices();

            Thread readLogThread = new Thread(_ => ReadLiveLog());
            readLogThread.IsBackground = true;
            readLogThread.Start();

            Thread monitorQueueThread = new Thread(_ => MonitorQueue());
            monitorQueueThread.IsBackground = true;
            monitorQueueThread.Start();

            _watcher.Path = Config.LogFilePath;
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.Changed += new FileSystemEventHandler(OnLogFileChanged);
            _watcher.EnableRaisingEvents = true;

            Thread handleConsoleInputThread = new Thread(_ => HandleConsoleInput());
            handleConsoleInputThread.IsBackground = true;
            handleConsoleInputThread.Start();

            ManualResetEvent readExistingEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eArgs) => { readExistingEvent.Set(); eArgs.Cancel = true; };
            readExistingEvent.WaitOne();
        }

        #region Console Input

        private static void HandleConsoleInput()
        {
            ConsoleKeyInfo keyinfo;
            while (true)
            {
                keyinfo = Console.ReadKey(true);
                if ((keyinfo.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    if (keyinfo.Key == ConsoleKey.Q) ReadExistingLogs();
                    if (keyinfo.Key == ConsoleKey.J)
                    {
                        DeleteIndex("*system*");
                        DeleteIndex("*tests*");
                    }
                    if (keyinfo.Key == ConsoleKey.K) DeleteIndex("*system*");
                    if (keyinfo.Key == ConsoleKey.L) DeleteIndex("*tests*");
                    if (keyinfo.Key == ConsoleKey.T) DeleteIndex(Config.SystemInactive);
                    if (keyinfo.Key == ConsoleKey.Y) DeleteIndex(Config.SystemActive);
                    if (keyinfo.Key == ConsoleKey.U) DeleteIndex(Config.TestRunning);
                    if (keyinfo.Key == ConsoleKey.I) DeleteIndex(Config.TestCompleted);
                    if (keyinfo.Key == ConsoleKey.O) DeleteIndex(Config.TestAborted);
                    if (keyinfo.Key == ConsoleKey.P) DeleteIndex(Config.TestTotal);
                }
            }
        }

        #endregion

        #region Server Client Init

        private static void SetWebAddress()
        {
            _client.BaseAddress = new Uri(Config.WebAddr);
            _client.DefaultRequestHeaders.Clear();
        }

        private static void SetCredentials()
        {
            if (!String.IsNullOrEmpty(Config.UserName) && !String.IsNullOrEmpty(Config.Password))
            {
                var byteArray = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", Config.UserName, Config.Password));
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        private static void CreateIndices()
        {
            CreateIndex(Config.SystemActive);
            CreateIndex(Config.SystemInactive);
            CreateIndex(Config.TestRunning);
            CreateIndex(Config.TestCompleted);
            CreateIndex(Config.TestAborted);
            CreateIndex(Config.TestTotal);
            Console.WriteLine("\r\n");
        }

        private static void CreateIndex(string indexName)
        {
            while (true)
            {
                try
                {
                    var httpContent = new StringContent("", Encoding.UTF8, "application/json");
                    HttpResponseMessage response = _client.PutAsync(indexName, httpContent).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Search index {0} already exists.\r\nResponse: {1}.", indexName, response.StatusCode);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("New index \"{0}\" was created.", indexName);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to create new search index {0}.\r\nException: {1}.", indexName, e.Message);
                }
                Thread.Sleep(Config.ConnectionRefreshTime);
            }
        }

        #endregion

        #region Watch Log

        private static void OnLogFileChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                _watcher.EnableRaisingEvents = false;
                string newFilePath = (new DirectoryInfo(Config.LogFilePath)).GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;
                if (newFilePath == _logInfo.FilePath || !IsValidLog(newFilePath, ref _logInfo)) return;
                Console.WriteLine("Watching File: {0}\r\n", newFilePath);
            }
            catch
            {
                //Ignore double use case
            }
            finally
            {
                _watcher.EnableRaisingEvents = true;
            }
        }

        #endregion

        #region Read Log

        private static void ReadExistingLogs()
        {      
            List<LogInfo> orderedLogFiles = new List<LogInfo>();
            foreach (string filepath in Directory.GetFiles(Config.LogFilePath))
            {
                LogInfo logInfo = new LogInfo();
                if (IsValidLog(filepath, ref logInfo)) orderedLogFiles.Add(logInfo);
            }
            BubbleSort(orderedLogFiles);

            SystemState systemState = new SystemState();
            TestResults testResults = new TestResults();
            List<string> logLines = new List<string>();
            foreach (LogInfo logInfo in orderedLogFiles)
            {
                Console.WriteLine("Parsing File: {0}\r\n", logInfo.FilePath);
                using (FileStream fileStream = new FileStream(logInfo.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream, Encoding.Default))
                    {
                        ReadAllLinesOnStream(streamReader, logLines);
                    }
                }
                ParseLogLines(logLines, logInfo, systemState, testResults);
                OnSwapFile(logLines, logInfo, systemState, testResults);
            }
        }

        private static void ReadLiveLog()
        {
            SystemState systemState = new SystemState();
            TestResults testResults = new TestResults();
            List<string> logLines = new List<string>();
            while (true)
            {
                while (String.IsNullOrEmpty(_logInfo.FilePath)) Thread.Sleep(100);
                using (FileStream fileStream = new FileStream(_logInfo.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream, Encoding.Default))
                    {
                        string filePath = _logInfo.FilePath;
                        while (filePath == _logInfo.FilePath)
                        {
                            ReadAllLinesOnStream(streamReader, logLines);
                            ParseLogLines(logLines, _logInfo, systemState, testResults);
                            while (streamReader.EndOfStream && filePath == _logInfo.FilePath) Thread.Sleep(100);
                        }
                    }
                }
                OnSwapFile(logLines, _logInfo, systemState, testResults);
            }
        }

        private static bool IsValidLog(string filePath, ref LogInfo LogInfo)
        {
            List<string> logLines = new List<string>();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader streamReader = new StreamReader(fileStream, Encoding.Default))
                {
                    while (!streamReader.EndOfStream && logLines.Count < 5)
                    {
                        string line = streamReader.ReadLine();
                        if (line != null) logLines.Add(line);
                    }
                }
            }
            return GetIndexData(filePath, logLines, ref LogInfo);
        }

        private static bool GetIndexData(string filePath, List<string> logLines, ref LogInfo logInfo)
        {
            if (logLines.Count < 5) return false;
            if (logLines[1] == null || logLines[4] == null) return false;

            List<string> lineSplit = logLines[1].Split(',').ToList();
            if (lineSplit.Count < typeof(LogInfo).GetProperties().Count()) return false;

            if (!lineSplit.Contains(Config.Date) ||
                !lineSplit.Contains(Config.Time) ||
                !lineSplit.Contains(Config.SystemState) ||
                !lineSplit.Contains(Config.TestState) ||
                !lineSplit.Contains(Config.TestType) ||
                !lineSplit.Contains(Config.VehicleType) ||
                !lineSplit.Contains(Config.DriverID)) return false;

            int date = lineSplit.FindIndex(x => x == Config.Date);
            int time = lineSplit.FindIndex(x => x == Config.Time);
            int systemState = lineSplit.FindIndex(x => x == Config.SystemState);
            int testState = lineSplit.FindIndex(x => x == Config.TestState);
            int testType = lineSplit.FindIndex(x => x == Config.TestType);
            int vehicleType = lineSplit.FindIndex(x => x == Config.VehicleType);
            int driverID = lineSplit.FindIndex(x => x == Config.DriverID);

            int max = Math.Max(date, time);
            max = Math.Max(max, systemState);
            max = Math.Max(max, testState);
            max = Math.Max(max, testType);
            max = Math.Max(max, vehicleType);
            max = Math.Max(max, driverID);

            lineSplit = logLines[4].Split(',').ToList();
            if (lineSplit.Count < max) return false;

            DateTime dateTime = ParseDateTimeInfo(lineSplit.ToArray(), date, time);
            if (dateTime == new DateTime()) return false;

            logInfo = new LogInfo
            (
                filePath,
                date,
                time,
                systemState,
                testState,
                testType,
                vehicleType,
                driverID,
                max,
                dateTime
            );

            return true;
        }

        private static void ReadAllLinesOnStream(StreamReader streamReader, List<string> logLines)
        {
            logLines.Clear();
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                if (line != null) logLines.Add(line);
            }
        }

        #endregion

        #region Parse Log

        private static void ParseLogLines(List<string> logLines, LogInfo logInfo, SystemState systemState, TestResults testResults)
        {
            foreach (string line in logLines)
            {
                if (IsLogBreak(line, systemState, testResults)) continue;
                string[] lineSplit = line.Split(',');
                if (lineSplit.Length <= logInfo.Max) continue;
                ParseSystemStateInfo(lineSplit, logInfo, systemState);
                ParseTestResultsInfo(lineSplit, logInfo, systemState, testResults);
            }
        }

        private static bool IsLogBreak(string line, SystemState systemState, TestResults testResults)
        {
            if (!line.Contains(",,,,,,,,,,,,,,,,,,,,,,,")) return false;
            systemState.Init();
            testResults.Init();
            return true;
        }

        private static void ParseSystemStateInfo(string[] lineSplit, LogInfo logInfo, SystemState systemState)
        {
            string systemStateString = lineSplit[logInfo.SystemState];
            if (!TypeCast.IsInt(systemStateString)) return;

            int systemStateInt = TypeCast.ToInt(systemStateString);
            if (systemState.IsCurrentState(systemStateInt) || systemStateInt < 0 || systemStateInt > 2) return;

            systemState.SetCurrentState(ParseDateTimeInfo(lineSplit, logInfo), systemStateInt);
            if (systemState.Time > 0) QueueSystemState(systemState);
        }

        private static void ParseTestResultsInfo(string[] lineSplit, LogInfo logInfo, SystemState systemState, TestResults testResults)
        {
            string testStateString = lineSplit[logInfo.TestState];
            string testTypeString = lineSplit[logInfo.TestType];
            string vehicleTypeString = lineSplit[logInfo.VehicleType];
            string driverIDString = lineSplit[logInfo.DriverID];
            if (!TypeCast.IsInt(testStateString) || !TypeCast.IsInt(testTypeString) || !TypeCast.IsInt(vehicleTypeString) || !TypeCast.IsInt(driverIDString)) return;

            int testState = TypeCast.ToInt(testStateString);
            testTypeString = _testTypeList.GetStringValue(TypeCast.ToInt(testTypeString));
            vehicleTypeString = _vehicleTypeList.GetStringValue(TypeCast.ToInt(vehicleTypeString));
            driverIDString = _idTypeList.GetStringValue(TypeCast.ToInt(driverIDString));

            if (!testResults.IsTestRunning() && systemState.IsCurrentState(SystemState.TestRunning))
            {
                testResults.OnTestStart(ParseDateTimeInfo(lineSplit, logInfo), driverIDString, testTypeString, vehicleTypeString);
            }
            else if (testResults.IsTestRunning())
            {
                if (testState == 16000)
                {
                    testResults.SetTestCompleted(ParseDateTimeInfo(lineSplit, logInfo));
                    QueueTestResults(testResults);
                }
                else if (testState == 17000)
                {
                    testResults.SetTestAborted(ParseDateTimeInfo(lineSplit, logInfo));
                    QueueTestResults(testResults);
                }
                else
                {
                    testResults.CheckIfSameTest(driverIDString, testTypeString, vehicleTypeString);
                }
            }
        }

        public static DateTime ParseDateTimeInfo(string[] lineSplit, LogInfo logInfo)
        {
            try { return Convert.ToDateTime(ParseDateTimeInfoAsString(lineSplit, logInfo)); }
            catch { return new DateTime(); }
        }

        public static DateTime ParseDateTimeInfo(string[] lineSplit, int date, int time)
        {
            try { return Convert.ToDateTime(ParseDateTimeInfoAsString(lineSplit, date, time)); }
            catch { return new DateTime(); }
        }

        private static string ParseDateTimeInfoAsString(string[] lineSplit, LogInfo logInfo)
        {
            return lineSplit[logInfo.Date] + " " + lineSplit[logInfo.Time];
        }

        private static string ParseDateTimeInfoAsString(string[] lineSplit, int date, int time)
        {
            return lineSplit[date] + " " + lineSplit[time];
        }

        private static void OnSwapFile(List<string> logLines, LogInfo logInfo, SystemState systemState, TestResults testResults)
        {
            testResults.Init();
            if (logLines.Count < 1)
            {
                systemState.Init();
                return;
            }

            systemState.SetCurrentState(ParseDateTimeInfo(logLines.Last().Split(','), logInfo), -1);
            if (systemState.Time > 0) QueueSystemState(systemState);
        }

        #endregion

        #region Server Client Queue

        private static void QueueSystemState(SystemState systemState)
        {
            systemStateQueueSem.WaitOne();
            if (systemState.IsPreviousStateSystemActive()) systemStateQueue.Add(new string[] { Config.SystemActive, JsonConvert.SerializeObject(systemState), systemState.Date });
            if (systemState.IsPreviousStateSystemInactive()) systemStateQueue.Add(new string[] { Config.SystemInactive, JsonConvert.SerializeObject(systemState), systemState.Date });
            if (systemState.IsPreviousStateTestRunning()) systemStateQueue.Add(new string[] { Config.TestRunning, JsonConvert.SerializeObject(systemState), systemState.Date });
            systemStateQueueSem.Release();
        }

        private static void QueueTestResults(TestResults testResults)
        {
            testResultsQueueSem.WaitOne();
            if (testResults.IsTestCompleted()) testResultsQueue.Add(new string[] { Config.TestCompleted, JsonConvert.SerializeObject(testResults), testResults.Date });
            if (testResults.IsTestAborted()) testResultsQueue.Add(new string[] { Config.TestAborted, JsonConvert.SerializeObject(testResults), testResults.Date });
            testResultsQueue.Add(new string[] { Config.TestTotal, JsonConvert.SerializeObject(testResults), testResults.Date });
            testResultsQueueSem.Release();
        }

        #endregion

        #region Push To Server

        private static void MonitorQueue()
        {
            string[] systemStateEntry;
            string[] testResultsEntry;

            while (true)
            {
                while (systemStateQueue.Count > 0)
                {
                    systemStateEntry = systemStateQueue.First();
                    PostInstance(systemStateEntry[0], systemStateEntry[1], systemStateEntry[2]);
                    systemStateQueueSem.WaitOne();
                    systemStateQueue.Remove(systemStateEntry);
                    systemStateQueueSem.Release();
                }

                while (testResultsQueue.Count > 0)
                {
                    testResultsEntry = testResultsQueue.First();
                    PostInstance(testResultsEntry[0], testResultsEntry[1], testResultsEntry[2]);
                    testResultsQueueSem.WaitOne();
                    testResultsQueue.Remove(testResultsEntry);
                    testResultsQueueSem.Release();
                }

                Thread.Sleep(100);
            }
        }

        private static void PostInstance(string indexName, string jsonObject, string dateTime)
        {
            while (true)
            {
                try
                {
                    string guid = indexName + "-" + dateTime;
                    var httpContent = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = _client.PutAsync(indexName + "/_doc/" + guid, httpContent).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed to add instance to index {0}. Response: {1}", indexName, response.StatusCode);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("New instance was added to index {0}.", indexName);
                        Console.WriteLine(jsonObject.Replace("\"","").Replace("{", "").Replace("}", "").Replace(",", "\r\n") + "\r\n");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(String.Format("Failed to add new instance to index {0}.\r\nException: {1}.", indexName, e.Message));
                }
                Thread.Sleep(Config.ConnectionRefreshTime);
            }
        }

        private static void DeleteIndex(string index)
        {
            while (true)
            {
                try
                {
                    //var httpContent = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = _client.DeleteAsync(index).GetAwaiter().GetResult(); ; //.PutAsync(indexName + "/_doc/" + guid, httpContent).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed to delete index {0}. Response: {1}", index, response.StatusCode);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Index {0} was deleted.", index);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(String.Format("Failed to delete index {0}.\r\nException: {0}.", index, e.Message));
                }
                Thread.Sleep(Config.ConnectionRefreshTime);
            }
        }

        #endregion

        #region Misc

        private static void BubbleSort(List<LogInfo> orderedLogFiles)
        {
            bool doneSorting = false;
            LogInfo placeholder;
            if (orderedLogFiles.Count < 2) doneSorting = true;
            while (!doneSorting)
            {
                doneSorting = true;
                for (int i = 1; i < orderedLogFiles.Count; i++)
                {
                    if (orderedLogFiles[i - 1].DateTime > orderedLogFiles[i].DateTime)
                    {
                        placeholder = orderedLogFiles[i];
                        orderedLogFiles[i] = orderedLogFiles[i - 1];
                        orderedLogFiles[i - 1] = placeholder;
                        doneSorting = false;
                    }
                }
            }
        }

        #endregion

        #region Error Handle

        private static void OnError(string msg)
        {
            Console.WriteLine(msg);
            OnExit();
            Environment.Exit(0);
        }

        private static void OnExit()
        {
            if (Config.IsDebug)
            {
                while (Console.KeyAvailable) Console.ReadKey(true);
                Console.ReadKey();
            }
                
        }
        #endregion
    }
}
