using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using STARSMonitorA.Properties;

namespace STARSMonitorA
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form2());
            }
            catch (Exception e)
            {
                Environment.Exit(0);
            }
        }

    }
}