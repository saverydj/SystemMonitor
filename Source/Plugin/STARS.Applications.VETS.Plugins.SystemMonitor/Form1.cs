using System;
using System.Windows.Forms;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Main.StateChanged += ShowNewState;
        }

        private void ShowNewState(object sender, EventArgs e)
        {
            if (Main._systemState.IsCurrentStateSystemActive())
            {
                textBox1.Text = Main._systemState.GetStateAsString() + " " + Main._idleTimer;
            }
            else if (Main._systemState.IsCurrentStateRunningTest())
            {
                textBox1.Text = Main._systemState.GetStateAsString() + " " + Main._testTimer;
            }
            else
            {
                textBox1.Text = Main._systemState.GetStateAsString();
            }
            textBox2.Text = Main._operatorID.GetStringValue();
            textBox3.Text = Main._vehicleManufacturer.GetStringValue();
            textBox4.Text = Main._testState.StateName;
            textBox5.Text = Main._speed.ToString();
            textBox6.Text = Main._targetSpeed.ToString();
            textBox7.Text = Main._cellTemperature.ToString();
            textBox8.Text = Main._relativeHumidity.ToString();
            textBox9.Text = Main._barometer.ToString();
            textBox12.Text = Main._driverID.GetStringValue();
            textBox13.Text = Main._vehicleType.GetStringValue();
            textBox15.Text = Main._testType.GetStringValue();
            textBox16.Text = Main._numberOfActiveAlarms.ToString();
        }
    }
}
