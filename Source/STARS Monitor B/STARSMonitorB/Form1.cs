using System;
using System.Drawing;
using System.Windows.Forms;

namespace STARSMonitorB
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Size = new Size(0, 0);
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Program.LookForPartner();
        }

    }
}
