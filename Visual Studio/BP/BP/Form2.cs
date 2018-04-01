using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace BP
{
    partial class Form2 : Form
    {
        /*
        MOVABLE FORM
            */
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private static SensorServer server;
        private static Form2 _instance;

        public Form2()
        {
            InitializeComponent();
            _instance = this;

            this.Text = "Manažment serveru COM??";
        }


        public Form2(SensorServer srv)
        {
            InitializeComponent();
            server = srv;

            comLabel.Text = server.portName;
            UUIDLabel.Text = server.UUID;

            this.Text = "Manažment serveru "+server.portName;

            _instance = this;

        }
        public static Form2 Instance
        {
            get
            {
                return _instance;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            buttonPanel.Height = button1.Height;
            buttonPanel.Top = button1.Top;

            if (!contentPanel.Controls.Contains(SensorListControl.Instance))
            {
                contentPanel.Controls.Add(SensorListControl.Instance);
                SensorListControl.Instance.Dock = DockStyle.Fill;
                SensorListControl.Instance.BringToFront();
            }
            else
                SensorListControl.Instance.BringToFront();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            buttonPanel.Height = button2.Height;
            buttonPanel.Top = button2.Top;

            if (!contentPanel.Controls.Contains(PWM.Instance))
            {
                contentPanel.Controls.Add(PWM.Instance);
                PWM.Instance.Dock = DockStyle.Fill;
                PWM.Instance.BringToFront();
            }
            else
                PWM.Instance.BringToFront();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
            button1.PerformClick();
            try {
                server.sendCommand(new SerialCommand("list;",server.command_list));
            } catch (Exception ex)
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (server != null)
                server.closePort();

            this.Close();
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void UUIDLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void UUIDtextlabel_Click(object sender, EventArgs e)
        {
            
        }

        private void UUIDtextlabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null)
            {
                server.Clear();
            }
                
        }
    }
}
