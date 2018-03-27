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

        private void button1_Click(object sender, EventArgs e)
        {
            SensorServer.Instance.refreshSensors();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SensorServer.Instance.sendCommand(new SerialCommand("load;", SensorServer.Instance.command_load));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SensorServer.Instance.sendCommand(new SerialCommand("save;", SensorServer.Instance.command_save));

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
