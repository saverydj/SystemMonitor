using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.MemoryMappedFiles;
using STARSMonitorA.Properties;

namespace STARSMonitorA
{
    public partial class Form2 : Form
    {
        private string PartnerProcessPath = Resources.PartnerPath + Resources.PartnerName + ".exe";

        private const int HOOK_REFRESH = 300;
        private int _hookRefresh = HOOK_REFRESH;
        public int HookRefresh
        {
            get { return _hookRefresh; }
            set { _hookRefresh = value; if (_hookRefresh <= 0) RefreshHook(); }
        }

        private bool _isHooked = false;
        public static bool _isActivity = false;
        private Point _mousePosition = Cursor.Position;

        private MemoryMappedFile _mappedFile;
        private EventWaitHandle _timerReset;
        private  EventWaitHandle _timerTick;

        public Form2()
        {
            InitializeComponent();

            _mappedFile = MemoryMappedFile.CreateNew(Resources.MyName, 128);
            _timerReset = SetEventWaitHandle(Resources.TimerReset);
            _timerTick = SetEventWaitHandle(Resources.TimerTick);

            Thread t = new Thread(_ => OnNewThread());
            t.IsBackground = true;
            t.Start();

            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LookForPartner();
            CheckForMouseMove();
            if (_isActivity)
            {
                _timerReset.Set();
                _isActivity = false;
            }
            else _timerTick.Set();
        }

        private void CheckForMouseMove()
        {
            Point currentMousePosition = Cursor.Position;
            if (currentMousePosition != _mousePosition)
            {
                _mousePosition = currentMousePosition;
                _isActivity = true;
            }
        }

        private EventWaitHandle SetEventWaitHandle(string handleName)
        {
            EventWaitHandle ewh = null;
            try { ewh = EventWaitHandle.OpenExisting(handleName); }
            catch { ewh = new EventWaitHandle(false, EventResetMode.AutoReset, handleName); }
            return ewh;
        }

        public void LookForPartner()
        {
            try
            {
                MemoryMappedFile _lookForPartner = MemoryMappedFile.OpenExisting(Resources.PartnerName);
                _lookForPartner.Dispose();
            }
            catch
            {
                Revive();
            }
        }

        private void Revive()
        {
            try
            {
                Process partnerProcess = new Process();
                partnerProcess.StartInfo.FileName = PartnerProcessPath;

                if (File.Exists(PartnerProcessPath))
                {
                    partnerProcess.Start();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        private void SetHook()
        {
            if (_isHooked) return;
            LLMouseHook.Hook();
            LLKeyboardHook.Hook();
            WriteHooksToFile();
            _isHooked = true;
        }

        private void ReleaseHook()
        {
            if (!_isHooked) return;
            LLMouseHook.UnHook();
            LLKeyboardHook.UnHook();
            ClearHooksFromFile();
            _isHooked = false;
        }

        private void RefreshHook()
        {
            ReleaseHook();
            SetHook();
            _hookRefresh = HOOK_REFRESH;
        }

        private void ClearExistingHook()
        {
            long val;
            if (Int64.TryParse(Config.HookPtr1, out val)) LLMouseHook.UnHook(val);
            if (Int64.TryParse(Config.HookPtr2, out val)) LLKeyboardHook.UnHook(val);
        }

        private void WriteHooksToFile()
        {
            Config.HookPtr1 = LLMouseHook._hookID.ToString();
            Config.HookPtr2 = LLKeyboardHook._hookID.ToString();
        }

        private void ClearHooksFromFile()
        {
            Config.HookPtr1 = " ";
            Config.HookPtr2 = " ";
        }

        private void OnNewThread()
        {
            try
            {
                ClearExistingHook();
                SetHook();

                Form1 form1 = new Form1();
                form1.ShowDialog();

                if (_isHooked) ReleaseHook();
            }
            catch (Exception e)
            {
                Environment.Exit(0);
            }
        }
    }
}
