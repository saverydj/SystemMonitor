using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.MemoryMappedFiles;
using STARSMonitorB.Properties;

namespace STARSMonitorB
{
    static class Program
    {
        private static string PartnerProcessPath = Resources.PartnerPath + Resources.PartnerName + ".exe";

        private static Process _partnerProcess = new Process();
        private static MemoryMappedFile _mappedFile;

        [STAThread]
        static void Main()
        {
            try
            {
                _partnerProcess.StartInfo.FileName = PartnerProcessPath;
                _mappedFile = MemoryMappedFile.CreateNew(Resources.MyName, 128);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        public static void LookForPartner()
        {
            try
            {
                MemoryMappedFile lookForPartner = MemoryMappedFile.OpenExisting(Resources.PartnerName);
                lookForPartner.Dispose();
            }
            catch
            {
                Revive();
            }
        }

        private static void Revive()
        {
            try
            {
                if (File.Exists(PartnerProcessPath))
                {
                    _partnerProcess.Start();
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
    }
}
