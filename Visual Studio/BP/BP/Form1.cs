using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace BP
{
    public partial class Form1 : Form
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


        private ArrayList avSerialPorts = new ArrayList();
        private List<String> autoPorts = new List<String>();
        private int autoPortsCount = 0;

        static SerialPort _serialPort;
        private int serialTimeOut = 3000;
        private static SensorServer server = null;

        private bool isSerialPortValid;

        delegate void UpdateStatusProgressDel(string val);
        delegate void UpdateStatusTextDel(string val);

        private bool runninAutoSearch = false;
        private bool runninSearch = false;

        public Form1()
        {
            InitializeComponent();

            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;

            foreach (string s in SerialPort.GetPortNames())
            {
                if (!avSerialPorts.Contains(s))
                {
                    avSerialPorts.Add(s);
                    serialPorts.Items.Add(s);
                }
            }
        }

        private void serialPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox slct = (ComboBox)sender;

            if (backgroundWorker1.IsBusy != true)
            {
                slct.Enabled = false;
                autoSearch.Enabled = false;
                progressBar1.Visible = true;
                progressBar1.Value = 0;

                label3.Text = "";
                label3.Visible = true;

                backgroundWorker1.RunWorkerAsync((string)slct.SelectedItem);
           }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void testSerialPort(object obj)
        {
            if (!(obj is SerialPort))
                return;

            SerialPort sp = obj as SerialPort;

            try
            {
                sp.Open();
            }
            catch (Exception)
            {
                // users don't want to experience this
                return;
            }

            if (sp.IsOpen)
            {
                isSerialPortValid = true;
            }
            sp.Close();
            return;

        }
        private Boolean testPort(SerialPort port)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            isSerialPortValid = false;
            Thread t = new Thread(new ParameterizedThreadStart(testSerialPort));
            t.Start(port);
            //Thread.Sleep(500); // wait and trink a tee for 500 ms
            
            while(stopWatch.ElapsedMilliseconds <= 500 && !isSerialPortValid)
            {

            }
            t.Abort();
            port.Close();
            stopWatch.Stop();
            // check wether the port was successfully opened
            return isSerialPortValid;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            autoPortsCount = 0;
            autoPorts.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                if (!autoPorts.Contains(s))
                {
                    autoPorts.Add(s);
                }
            }

            runninAutoSearch = true;
            serialPorts.Enabled = false;
            autoSearch.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Value = 0;

            label3.Text = "";
            label3.Visible = true;
            /*
            serialPorts.Enabled = false;

            foreach (string s in SerialPort.GetPortNames())
            {
                isSerialPortValid = false;
                Thread t = new Thread(new ParameterizedThreadStart(testSerialPort));
                t.Start(s);
                //Thread.Sleep(500); // wait and trink a tee for 500 ms
                t.Abort();

                // check wether the port was successfully opened
                if (isSerialPortValid)
                {
                    Console.WriteLine( "Serial Port " + s + " is OK !");
                }
                else
                {
                    Console.WriteLine("Serial Port " + s + " retards !");
                }
            }


            serialPorts.Enabled = true;
            */
            backgroundWorker2.RunWorkerAsync();
            /*
            Form f = new Form2();
            f.Show();
            */
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            runninSearch = true;
            int progress;
            string comName = (string)e.Argument;

            server = new SensorServer(comName);
            Console.WriteLine(comName);


            _serialPort = new SerialPort(comName);

            _serialPort.RtsEnable = true;
            
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            _serialPort.WriteTimeout = 200;


            e.Result = "";
            try
            {
                _serialPort.Open();

                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            } catch(Exception ee)
            {
                e.Result = "Port Nereaguje";
                return;
            }

            worker.ReportProgress(10);

            if (!_serialPort.IsOpen)
            {
                e.Result = "Port Nereaguje";
                return;
            }
            server.comPort = _serialPort;
            worker.ReportProgress(20);


            Thread.Sleep(100);
            server.flushInputBuffer();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            worker.ReportProgress(30);

            server.sendCommand(new SerialCommand("hi",server.command_hi));
            //server.sendCommand("list;");

            worker.ReportProgress(40);
            progress = 40;


            Console.WriteLine("Probe");
            while (true)
            {

                
                if (!_serialPort.IsOpen)
                {
                    Console.WriteLine("CLOSED");
                    break;
                }
                if (server.deviceFound())
                {
                    Console.WriteLine("FOUND");
                    break;
                }
                if (stopWatch.ElapsedMilliseconds > 0 && (stopWatch.ElapsedMilliseconds%100 == 0))
                {
                    worker.ReportProgress(40 + (int)(55 / ((serialTimeOut / stopWatch.ElapsedMilliseconds))));

                }
                if ((stopWatch.ElapsedMilliseconds >= serialTimeOut))
                {
                    Console.WriteLine("TIMEOUT");
                    break;
                }
                if (server.deviceWriteFailed())
                {
                    Console.WriteLine("WRITE TIMEOUT");
                    break;
                }
            }

            e.Result = comName;
            if(server.deviceFound())
            {
                Console.WriteLine("Najdene");

            } else
            {
                Console.WriteLine("TIMEOUT");
                _serialPort.Close();
            }
            
            stopWatch.Stop();

            worker.ReportProgress(100);

        }

        private static void DataReceivedHandler(
                        object sender,
                        SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();

            if(server != null)
            {
                server.recieveData(indata);
            }
            
        }
    

    private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //e.UserState.ToString()
            
            UpdateStatusProgress(e.ProgressPercentage.ToString());
            UpdateStatusText("Pripojenie portu " + server.portName + ":" + (e.ProgressPercentage.ToString() + "%"));

    
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                UpdateStatusText("Canceled!");
            }
            else if (e.Error != null)
            {
                UpdateStatusText("Error: " + e.Error.Message);
            }
            else
            {
                if(server.deviceFound())
                {
                    UpdateStatusText("Port " + server.portName + " pripojený!");
                    Form f = new Form2(server);
                    f.ShowDialog();
                }
                else
                {
                    UpdateStatusText("Port " + server.portName + " neodpovedá!");
                }
            }
            autoPortsCount++;
            try
            {

                if (!runninAutoSearch || (runninAutoSearch && autoPorts.Count == autoPortsCount))
                { 
                    serialPorts.Enabled = true;
                    autoSearch.Enabled = true;

                    progressBar1.Visible = false;
                    progressBar1.Value = 0;
                    Console.WriteLine("Enabled");
                }
            }
            catch(Exception ex)
            {

            }
            runninSearch = false;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            avSerialPorts.Clear();
            serialPorts.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                if (!avSerialPorts.Contains(s))
                {
                    avSerialPorts.Add(s);
                    serialPorts.Items.Add(s);
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                server.closePort();
            } catch(Exception ex)
            {

            }
            this.Close();
            Environment.Exit(Environment.ExitCode);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void label1_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            
            foreach (string s in autoPorts.ToList())
            {

                while (runninSearch == true)
                {
                    Thread.Sleep(100);
                }

                if(!backgroundWorker1.IsBusy)
                    backgroundWorker1.RunWorkerAsync(s);
            }
            Console.WriteLine("Konec");
        }

        private void UpdateStatusProgress(string val)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.progressBar1.InvokeRequired)
            {
                UpdateStatusProgressDel d = new UpdateStatusProgressDel(UpdateStatusProgress);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                try
                {
                    this.progressBar1.Value = Int32.Parse(val);
                }
                catch (Exception e)
                {

                }
            }
        }

        private void UpdateStatusText(string val)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.progressBar1.InvokeRequired)
            {
                UpdateStatusTextDel d = new UpdateStatusTextDel(UpdateStatusText);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                try
                {
                    this.label3.Text = (val);
                }
                catch (Exception e)
                {

                }
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {


            serialPorts.Enabled = true;
            autoSearch.Enabled = true;

            progressBar1.Visible = false;
            progressBar1.Value = 0;
            Console.WriteLine("Enabled");

        }   
    }

}
