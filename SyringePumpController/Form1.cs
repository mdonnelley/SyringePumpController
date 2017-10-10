using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace SyringePumpController
{
    public partial class Form1 : Form
    {
        SerialPort ComPort = new SerialPort();
        delegate void SetTextCallback(string text);
        bool SerialPortOpen = false;

        public static class WPI
        {
            public static int[] capacity = {500,1000,5000,10000,25000,50000,100000,250000,500000,1000000};
            public static int[] maxrate = {20,40,202,406,1022,2035,4088,9999,9999,9999};
            public const int pause = 10;                    // Pause for serial commands (msec)
            public static string logfilename;               // Logfile name
            public static System.IO.StreamWriter logfile;   // Logfile file ID
        }

        static class Constants
        {
            public const string SoftwareVersion = "1.0.0";

        }

        public Form1()
        {
            InitializeComponent();

            // the settings for the rs232 port are baud rate 9600, 8 data bits, 1 start bit, 1 stop bit. Flow control must be set to none.
            ComPort.BaudRate = 9600;
            ComPort.DataBits = 8;
            //ComPort.Parity = Parity.None;
            ComPort.StopBits = StopBits.One;
            //ComPort.Handshake = Handshake.None;
            ComPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived_1);

            timer1 = new Timer();
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            LoadSettings();
            this.Text = "WPI UMP2 Micro4 Syringe Pump Controller v" + Constants.SoftwareVersion;
            WPI.logfile = new System.IO.StreamWriter(WPI.logfilename, append: true);
            WPI.logfile.WriteLine("-------------------------------------------------------------------");
            WPI.logfile.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff ") + "Syringe pump version " + Constants.SoftwareVersion + " opened");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //btnPortState.Text = "Open";
            //SerialPortOpen = false;
            //ComPort.DiscardInBuffer();
            //ComPort.Close();
            //labelRxCommand.Text = "Serial port not open";

            WPI.logfile.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff ") + "Syringe pump controller closed");
            WPI.logfile.Close();
            SaveSettings();
        }

        private void port_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            //string response = ComPort.ReadLine();
            //Console.Write(response);

            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.Write(indata);

            //switch (response)
            //{

            //}

            //if (strcmp(parameter, 'V') | strcmp(parameter, 'C') | strcmp(parameter, 'R')),
            //    response = WPIsendCommand(['?', parameter]);
            //    response = str2num(response(3:length(response)));
            //elseif(strcmp(parameter, 'M') | strcmp(parameter, 'S') | strcmp(parameter, 'D') | strcmp(parameter, 'U') | strcmp(parameter, 'G')),
            //    response = WPIsendCommand(['?', parameter]);
            //    response = response(3);

        }

        //private void SetText(string text)
        //{
        //    this.labelRxCommand.Text = text;
        //}

        private void btnGetSerialPorts_Click(object sender, EventArgs e)
        {
            cboPorts.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports) cboPorts.Items.Add(port);
            cboPorts.Text = ports[0];
        }

        private void btnPortState_Click(object sender, EventArgs e)
        {
            if (SerialPortOpen)
            {
                btnPortState.Text = "Open";
                SerialPortOpen = false;
                ComPort.DiscardInBuffer();
                ComPort.Close();
                labelRxCommand.Text = "Serial port not open";
            }
            else
            {
                btnPortState.Text = "Close";
                SerialPortOpen = true;
                ComPort.PortName = Convert.ToString(cboPorts.Text);
                ComPort.BaudRate = 9600;
                ComPort.DataBits = 8;
                ComPort.StopBits = StopBits.One;
                ComPort.Parity = Parity.None;
                ComPort.Open();
                ComPort.NewLine = "\n";

                Console.WriteLine("Opened port for WPI communication");

                //// Setup the log file
                //saveFileDialog1.Title = "Set log file location";
                //saveFileDialog1.FileName = "WPI Infusion Pump " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
                //saveFileDialog1.ShowDialog();
                //WPI.logfile = new System.IO.StreamWriter(WPI.logfilename, append: true);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            //Properties.Settings.Default.Reset();
            //LoadSettings();
            //SendAllSettings();
        }

        private void buttonRun1_Click(object sender, EventArgs e)
        {
            // Add check for log file location, and values in V, C, R
            if (String.IsNullOrEmpty(textBoxCounter1.Text))
            {
                MessageBox.Show("Enter current syringe volume first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                WPIsendCommand("L1;");
                if (comboBoxDirection1.Text == "I") WPIbolus();
                else if (comboBoxDirection1.Text == "W") WPIwithdraw();
            }
        }

        private void buttonRun2_Click(object sender, EventArgs e)
        {

        }

        private void buttonRun3_Click(object sender, EventArgs e)
        {

        }

        private void buttonRun4_Click(object sender, EventArgs e)
        {

        }

        private void buttonContinuous1_Click(object sender, EventArgs e)
        {
            // Add check for log file location, and values in V, C, R, Time interval
            if (String.IsNullOrEmpty(textBoxCounter1.Text) || String.IsNullOrEmpty(textBoxInterval1.Text))
            {
                MessageBox.Show("Enter current syringe volume and interval first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                WPIsendCommand("L1;");
                WPIbolus();
                timer1.Interval = (int)(1000 * Convert.ToDouble(textBoxInterval1.Text));
                timer1.Start();
            }
        }

        private void buttonContinuous2_Click(object sender, EventArgs e)
        {

        }

        private void buttonContinuous3_Click(object sender, EventArgs e)
        {

        }

        private void buttonContinuous4_Click(object sender, EventArgs e)
        {

        }

        private void buttonReload1_Click(object sender, EventArgs e)
        {
            WPIreload();
        }

        private void buttonReload2_Click(object sender, EventArgs e)
        {

        }

        private void buttonReload3_Click(object sender, EventArgs e)
        {

        }

        private void buttonReload4_Click(object sender, EventArgs e)
        {

        }

        private void buttonStop1_Click(object sender, EventArgs e)
        {
            WPIsendCommand("L1;");
            WPIsendCommand("H");
            WPIsendCommand("?C");
            //timer1.Stop();
            // Need to read the current value on the volume counter and subtract this from the value in the GUI, otherwise they won't match up
        }

        private void buttonStop2_Click(object sender, EventArgs e)
        {

        }

        private void buttonStop3_Click(object sender, EventArgs e)
        {

        }

        private void buttonStop4_Click(object sender, EventArgs e)
        {

        }

        private void textBoxRate1_TextChanged(object sender, EventArgs e)
        {
            // Set the maximum infusion rate based on the syringe type
            int rate = Convert.ToInt32(textBoxRate1.Text);
            if (rate > WPI.maxrate[comboBoxType1.SelectedIndex]) textBoxRate1.Text = WPI.maxrate[comboBoxType1.SelectedIndex].ToString();
        }

        private void textBoxRate2_TextChanged(object sender, EventArgs e)
        {
            // Set the maximum infusion rate based on the syringe type
            int rate = Convert.ToInt32(textBoxRate2.Text);
            if (rate > WPI.maxrate[comboBoxType2.SelectedIndex]) textBoxRate1.Text = WPI.maxrate[comboBoxType2.SelectedIndex].ToString();
        }

        private void textBoxRate3_TextChanged(object sender, EventArgs e)
        {
            // Set the maximum infusion rate based on the syringe type
            int rate = Convert.ToInt32(textBoxRate3.Text);
            if (rate > WPI.maxrate[comboBoxType3.SelectedIndex]) textBoxRate1.Text = WPI.maxrate[comboBoxType3.SelectedIndex].ToString();
        }

        private void textBoxRate4_TextChanged(object sender, EventArgs e)
        {
            // Set the maximum infusion rate based on the syringe type
            int rate = Convert.ToInt32(textBoxRate4.Text);
            if (rate > WPI.maxrate[comboBoxType4.SelectedIndex]) textBoxRate1.Text = WPI.maxrate[comboBoxType4.SelectedIndex].ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //WPIbolus();
        }

        // --------------- USER FUNCTIONS ---------------

        public void WPIsendCommand(string command)
        {
            // WPIsendCommand("I");      // Infuse
            // WPIsendCommand("W");      // Withdraw
            // WPIsendCommand("G");      // Go
            // WPIsendCommand("H");      // Halt
            // WPIsendCommand("S");      // Set units to nl / sec
            // WPIsendCommand("M");      // Set units to nl / min
            // WPIsendCommand("L1;")     // Set line number
            // WPIsendCommand("N");      // Non grouped mode
            // WPIsendCommand("P");      // Grouped mode
            // WPIsendCommand("D");      // Disabled mode
            // WPIsendCommand("TG");     // Syringe type

            System.Threading.Thread.Sleep(WPI.pause);
            Console.WriteLine(command);
            if (SerialPortOpen) ComPort.Write(command);
            System.Threading.Thread.Sleep(WPI.pause);
        }

        public void WPIsetValue(string parameter, int value)
        {
            // WPIsendCommand("V",1500);    // Volume
            // WPIsendCommand("C",95000);   // Counter
            // WPIsendCommand("R",4000);    // Rate

            if (String.Equals(parameter, "V") | String.Equals(parameter, "C") | String.Equals(parameter, "R"))
            {
                // Send parameter (V, C, R) plus value
                string str = string.Format("{0:0.};", value);
                WPIsendCommand(parameter);
                foreach (char c in str) WPIsendCommand(c.ToString());       // Test to see whether each char needs to be sent individually
            }
        }

        public void WPIbolus()
        {
            // Stop any current infusions
            WPIsendCommand("H");

            // Get required values
            int vol = Convert.ToInt32(textBoxVol1.Text);
            int counter = Convert.ToInt32(textBoxCounter1.Text);
            int rate = Convert.ToInt32(textBoxRate1.Text);
            int capacity = WPI.capacity[comboBoxType1.SelectedIndex];
            int max = capacity * Convert.ToInt32(numericUpDownMax.Value) / 100;
            int min = capacity * Convert.ToInt32(numericUpDownMin.Value) / 100;

            // Check that the bolus volume is not too large
            if (vol > max - min) vol = max - min;

            // If the volume in the syringe is insufficient
            if (counter - vol < min)
            {
                // Reload if autoload box checked
                if (checkBoxAutoload1.Checked)
                {
                    WPIreload();
                    counter = Convert.ToInt32(textBoxCounter1.Text);
                }
                else
                {
                    MessageBox.Show("Insufficient volume remaining in syringe", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Setup infusion parameters
            comboBoxDirection1.SelectedIndex = 0;
            buttonRun1.ForeColor = Color.Red;                               // Need to add ability to do this for each row
            WPIsendCommand("I");
            WPIsendCommand(comboBoxUnits1.Text);
            WPIsendCommand("T");
            WPIsendCommand(comboBoxType1.Text);
            WPIsendCommand(comboBoxGrouping1.Text);
            WPIsetValue("R", Convert.ToInt32(rate));
            WPIsetValue("V", vol);
            WPIsetValue("C", 0);
            WPIsendCommand("G");

            // Wait for the pump to move
            //Console.WriteLine("Waiting for pump to move");
            //System.Threading.Thread.Sleep(250 + 1000 * vol / rate);         // Need to implement this as a timer

            // Check that the syringe moved correctly
            // counter = WPIgetValue('C');
            // if (counter / vol > 0.99),
            //    str = [datestr(now, 14), ' Delivered ', num2str(counter), 'nl'];
            //    disp(str)
            //    fprintf(WPI.logfileID, [str, '\n']);
            // else
            //    str = [datestr(now, 14), ' ERROR: Only ' num2str(counter), 'nl delivered!'];
            //    warning(str)
            //    fprintf(WPI.logfileID, [str, '\n']);
            // end

            string str = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff ") + " Delivered " + vol.ToString() + " nl";
            WPI.logfile.WriteLine(str);
            Console.WriteLine(str);

            // Update the counter
            counter -= vol;
            textBoxCounter1.Text = counter.ToString();
            buttonRun1.ForeColor = SystemColors.ControlText;   // Need to add ability to do this for each row
        }

        public void WPIwithdraw()
        {
            // Stop any current infusions
            WPIsendCommand("H");

            // Get required values
            int vol = Convert.ToInt32(textBoxVol1.Text);
            int counter = Convert.ToInt32(textBoxCounter1.Text);
            int rate = Convert.ToInt32(textBoxRate1.Text);
            int capacity = WPI.capacity[comboBoxType1.SelectedIndex];
            int max = capacity * Convert.ToInt32(numericUpDownMax.Value) / 100;

            // Check if the withdrawl volume is too large
            if (vol > max - counter) vol = max - counter;

            // Setup withdraw parameters
            comboBoxDirection1.SelectedIndex = 1;
            buttonRun1.ForeColor = Color.Red;
            WPIsendCommand("W");
            WPIsendCommand(comboBoxUnits1.Text);
            WPIsendCommand("T");
            WPIsendCommand(comboBoxType1.Text);
            WPIsendCommand(comboBoxGrouping1.Text);
            WPIsetValue("R", rate);
            WPIsetValue("V", vol);
            WPIsetValue("C", 0);
            WPIsendCommand("G");

            // Wait for the pump to move
            Console.WriteLine("Waiting for pump to move");
            System.Threading.Thread.Sleep(250 + 1000 * vol / Convert.ToInt32(textBoxRate1.Text));

            // Check that the syringe moved correctly
            // counter = WPIgetValue('C');logfile.Close();
            // if (counter / vol > 0.99),
            //    disp([datestr(now, 14), ' Withdrawn ', num2str(vol), 'nl'])
            // else
            //    warning([datestr(now, 14), ' ERROR RESETTING SYRINGE'])
            // end

            string str = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff ") + " Withdrawn " + vol.ToString() + " nl";
            WPI.logfile.WriteLine(str);
            Console.WriteLine(str);

            // Adjust the current volume
            counter += vol;
            textBoxCounter1.Text = counter.ToString();
            buttonRun1.ForeColor = SystemColors.ControlText;
        }

        public void WPIreload()
        {
            // Get required values
            int vol = Convert.ToInt32(textBoxVol1.Text);
            int counter = Convert.ToInt32(textBoxCounter1.Text);
            int capacity = WPI.capacity[comboBoxType1.SelectedIndex];
            int max = capacity * Convert.ToInt32(numericUpDownMax.Value) / 100;
            int reload = max - counter;

            // Reload
            textBoxVol1.Text = reload.ToString();
            WPIwithdraw();
            //textBoxCounter1.Text = max.ToString();
            textBoxVol1.Text = vol.ToString();




            // FIX THIS. THE COUNTER DOES NOT UPDATE AFTER RELOAD!!!!!




        }

        public void LoadSettings()
        {
            // Load all settings
            WPI.logfilename = Properties.Settings.Default.logfilename;
        }

        public void SaveSettings()
        {
            // Save all settings
            Properties.Settings.Default.logfilename = WPI.logfilename;
            Properties.Settings.Default.Save();
        }
    }
}