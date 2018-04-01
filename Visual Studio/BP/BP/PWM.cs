using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace BP
{
    partial class PWM : UserControl
    {
        private int pwmStart;
        private int pwmEnd;

        private int hours;
        private int mins;
        private int secs;

        private bool restart;
        private int totalTimeSec;

        private int actualPWM = 0;

        private Thread pwmThread;
        private Stopwatch watch;

        private bool threadStop = false;

        delegate void UpdateTimeDel(string val);
        delegate void UpdatePWMValueDel(string val);


        public List<PWMValue> measuredValues = new List<PWMValue>();

        private static PWM _instance;
        public static PWM Instance
        {
            get
            {
                if (_instance == null) _instance = new PWM();
                return _instance;
            }
        }
        public PWM()
        {
            InitializeComponent();
        }

        private long map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public bool pwmCommandSet(SerialCommand.CommandReturn ret, String data)
        {
            UpdatePWMValue(actualPWM.ToString());
            return true;
        }

        public static String formateTime(long seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds);
        }

        public void pwmThreadExec()
        {
            this.watch.Start();
            long tm = 0;
            long tmLast = 0;
            int pwmVal = 0;
            int pwmValBuf = 0;

            measuredValues.Clear();

            UpdateTime(formateTime(tm));
            SensorServer.Instance.sendCommand(new SerialCommand("pwms;" + pwmStart + ";", this.pwmCommandSet));
            this.measuredValues.Add(new PWMValue(pwmStart));

            while (true)
            {
                if(threadStop)
                {
                    threadStop = false;
                    return;
                }
                try
                {
                    tm = watch.ElapsedMilliseconds / 1000;
                } catch(Exception ex)
                {

                }

                // Up
                if (totalTimeSec >= tm)
                {
                    if (tmLast != tm)
                    {
                        tmLast = tm;
                        UpdateTime(formateTime(tm));
                    }
                    pwmValBuf = (int)map(tm, (long)0, (long)totalTimeSec, (long)pwmStart, (long)pwmEnd);
                    if(pwmVal != pwmValBuf)
                    {
                        pwmVal = pwmValBuf;
                        actualPWM = pwmVal;
                        this.measuredValues.Add(new PWMValue(pwmVal));

                        SensorServer.Instance.sendCommand(new SerialCommand("pwms;" + pwmVal+";", this.pwmCommandSet));
                    }
                } else // Down
                {
                    if (tm >= 2 * totalTimeSec)
                    {

                        UpdateTime(formateTime(tm));
                        SensorServer.Instance.sendCommand(new SerialCommand("pwms;" + pwmStart + ";", this.pwmCommandSet));
                        UpdatePWMValue(pwmStart.ToString());
                        this.measuredValues.Add(new PWMValue(pwmStart));
                        return;
                    }
                    if (tmLast != tm)
                    {
                        tmLast = tm;
                        UpdateTime(formateTime(tm));
                    }
                    pwmValBuf = (int)map((long)(totalTimeSec-(tm-totalTimeSec)), (long)0, (long)totalTimeSec, (long)pwmStart, (long)pwmEnd);
                    if (pwmVal != pwmValBuf)
                    {
                        pwmVal = pwmValBuf;
                        actualPWM = pwmVal;
                        this.measuredValues.Add(new PWMValue(pwmVal));
                        SensorServer.Instance.sendCommand(new SerialCommand("pwms;" + pwmVal + ";", this.pwmCommandSet));
                    }
                }
                Thread.Sleep(500);
            }
        }

        private void UpdateTime(string val)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label11.InvokeRequired)
            {
                UpdateTimeDel d = new UpdateTimeDel(UpdateTime);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                try
                {
                    this.label11.Text = val;
                }
                catch (Exception e)
                {

                }
            }
        }

        private void UpdatePWMValue(string val)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label10.InvokeRequired)
            {
                UpdatePWMValueDel d = new UpdatePWMValueDel(UpdatePWMValue);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                try
                {
                    this.label10.Text = val;
                }
                catch (Exception e)
                {

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pwmStart = (int)numericUpDown1.Value;
            pwmEnd = (int)numericUpDown2.Value;

            hours = (int)numericUpDown3.Value;
            mins = (int)numericUpDown4.Value;
            secs = (int)numericUpDown5.Value;


            restart = checkBox1.Checked;

            totalTimeSec = (hours * 3600) + (mins * 60) + secs;

            if (pwmStart < pwmEnd)
            {
                if(totalTimeSec > 0)
                {

                    this.watch = new Stopwatch();

                    this.pwmThread = new Thread(this.pwmThreadExec);
                    this.pwmThread.Start();

                    panel1.Enabled = false;
                    panel2.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Zle zadaný polčas testu!");
                }
            }
            else
            {
                MessageBox.Show("Štartovacia hodnota PWM musí byť menšia ako konečná!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            threadStop = true;
        }

        public void createCSV(string path)
        {
            try { 
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@path))
            {
                file.WriteLine("Time,PWM");
                foreach (PWMValue val in measuredValues)
                {
                    file.WriteLine(val.CSV());
                }
            }
            } catch(Exception ex)
            {

            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = "PWM.csv";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                createCSV(saveFileDialog1.FileName);

            }
        }
    }

    class PWMValue
    {
        public long Value { set; get; }
        public DateTime Time { set; get; }

        public PWMValue(long val)
        {
            Value = val;
            Time = DateTime.Now;
        }
        public string CSV()
        {
            return this.Time.ToString("dd.MM.yyyy HH:mm:ss") + "," + Value;
        }
    }
}
