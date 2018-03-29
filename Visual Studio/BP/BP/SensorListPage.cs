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
    public partial class SensorListPage : UserControl
    {
        private static SensorListPage _instance;


        delegate bool ChangeButton1Del(bool enabled);
        delegate bool ChangeButton2Del(bool enabled);
        delegate bool ChangeButton3Del(bool enabled);


        public SensorListPage()
        {
            InitializeComponent();
            _instance = this;
        }

        public static SensorListPage Instance
        {
            get
            {
                if (_instance == null) _instance = new SensorListPage();
                return _instance;
            }
        }

        private bool ChangeButton1(bool enabled)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.button1.InvokeRequired)
            {
                ChangeButton1Del d = new ChangeButton1Del(ChangeButton1);
                this.Invoke(d, new object[] { enabled });
            }
            else
            {
                try
                {
                    this.button1.Enabled = enabled;
                }
                catch (Exception e)
                {

                }
            }
            return true;
        }

        private bool ChangeButton2(bool enabled)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.button2.InvokeRequired)
            {
                ChangeButton2Del d = new ChangeButton2Del(ChangeButton2);
                this.Invoke(d, new object[] { enabled });
            }
            else
            {
                try
                {
                    this.button2.Enabled = enabled;
                }
                catch (Exception e)
                {

                }
            }
            return true;
        }

        private bool ChangeButton3(bool enabled)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.button3.InvokeRequired)
            {
                ChangeButton3Del d = new ChangeButton3Del(ChangeButton3);
                this.Invoke(d, new object[] { enabled });
            }
            else
            {
                try
                {
                    this.button3.Enabled = enabled;
                }
                catch (Exception e)
                {

                }
            }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SensorServer.Instance.refreshSensors(ChangeButton1);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            SensorServer.Instance.sendCommand(new SerialCommand("load;", SensorServer.Instance.command_load,ChangeButton2));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SensorServer.Instance.sendCommand(new SerialCommand("save;", SensorServer.Instance.command_save,ChangeButton3));

        }

        private void button4_Click(object sender, EventArgs e)
        {
            DHTadd frm = new DHTadd();

            frm.ShowDialog();

            if(frm.pin != -1 && frm.type != -1 && frm.update != -1)
            {
                Console.WriteLine("dhtadd;" + frm.pin + ";" + frm.type + ";" + (frm.update) + ";");
                SensorServer.Instance.sendCommand(new SerialCommand("dhtadd;" + frm.pin + ";" + frm.type + ";" + (frm.update) + ";", SensorServer.Instance.command_dhtadd));
            }

        }
    }
}
