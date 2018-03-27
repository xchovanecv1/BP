using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BP
{
     partial class DHTControlls : UserControl, SensorUX
    {
        delegate void UpdateTemperatureDel(string val);
        delegate void UpdateHumidityDel(string val);
        delegate void UpdateTimeUpdateDel(string val);
        delegate void UpdateRefreshTimeDel(string val);

        public DHTSensor dhtInstance;

        private int sensorID;

        public DHTControlls()
        {
            InitializeComponent();
        }

        public DHTControlls(int id, DHTSensor inst)
        {
            InitializeComponent();
            sensorID = id;
            dhtInstance = inst;
        }

        private void UpdateTimeUpdate(string val)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label6.InvokeRequired)
            {
                UpdateTimeUpdateDel d = new UpdateTimeUpdateDel(UpdateTimeUpdate);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                try
                {
                    this.label6.Text = val;
                }
                catch (Exception e)
                {

                }
            }
        }
        private void UpdateHumidity(string val)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label4.InvokeRequired)
            {
                UpdateHumidityDel d = new UpdateHumidityDel(UpdateHumidity);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                try
                {
                    this.label4.Text = val + " %";
                }
                catch (Exception e)
                {

                }
            }
        }
        private void UpdateTemperature(string val)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label3.InvokeRequired)
            {
                UpdateTemperatureDel d = new UpdateTemperatureDel(UpdateTemperature);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                try
                {
                    this.label3.Text = val + " °C";
                }
                catch (Exception e)
                {

                }
            }
        }

        public void UpdateRefreshTime(string val)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label5.InvokeRequired)
            {
                UpdateRefreshTimeDel d = new UpdateRefreshTimeDel(UpdateRefreshTime);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                try
                {
                    this.label5.Text = val + " ms";
                }
                catch (Exception e)
                {

                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public void UpdateGUI(object[] data)
        {
            if(data.Length == 2)
            {
                UpdateHumidity(data[1].ToString());
                UpdateTemperature(data[0].ToString());

                UpdateTimeUpdate(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SensorServer.Instance.removesensor(this.sensorID);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = " Default.csv";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                dhtInstance.createCSV(saveFileDialog1.FileName);

            }
        }
    }
}
