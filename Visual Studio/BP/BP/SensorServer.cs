using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BP
{
    class SensorServer
    {
        public string UUID { get; set; }
        public int FlashSize { get; set; }
        public string portName { get; set; }
        public SerialPort comPort { get; set; }
        private ArrayList options;

        private ArrayList dataIn = new ArrayList();
        private ArrayList cmdIn = new ArrayList();
        private ArrayList cmdOut = new ArrayList();
        private Queue<SerialCommand> cmdPending = new Queue<SerialCommand>();

        private SerialCommand pendingCommand = null;

        private Dictionary<int, DHTSensor> DHTSensors = new Dictionary<int, DHTSensor>();

        private Thread comPortThread;


        private bool authenticated = false;
        private bool writeTimeOut = false;

        private static SensorServer _instance;

        public static SensorServer Instance
        {
            get
            {
                return _instance;
            }
        }

        private string inputLine = "";

        public SensorServer()
        {
            _instance = this;
        }

        public SensorServer(string COM)
        {
            this.portName = COM;
            _instance = this;

            this.comPortThread = new Thread(this.performCommandSend);
            this.comPortThread.Start();
            //this.comPort = new SerialPort(COM);
        }
        public SensorServer(string COM, SerialPort serial)
        {
            this.portName = COM;
            this.comPort = serial;
            _instance = this;

            this.comPortThread = new Thread(this.performCommandSend);
            this.comPortThread.Start();
        }

        public SensorServer(string COM, string UUID, int FlashSize, ArrayList options)
        {
            this.portName = COM;
            this.options = options;
            this.UUID = UUID;
            this.FlashSize = FlashSize;
            _instance = this;

            this.comPortThread = new Thread(this.performCommandSend);
            this.comPortThread.Start();
        }

        public void closePort()
        {
            this.comPort.Close();
        }


        public string toString()
        {
            return "Server: " + portName + " UUID:" + UUID + " FlashSize:" + FlashSize + " Options:" + options.ToString();
        }

        public bool deviceFound()
        {
            return this.authenticated;
        }

        public bool deviceWriteFailed()
        {
            return this.writeTimeOut;
        }

        public void performCommandSend()
        {
            SerialCommand scomand;
            while (true)
            {
                if(pendingCommand != null)
                {
                    if (pendingCommand.commandTimeout())
                    {
                        pendingCommand = null;
                    }
                }
                if (comPort != null && comPort.IsOpen && comPort.CtsHolding && pendingCommand == null)
                {

                    if (cmdPending.Count > 0)
                    {
                        scomand = cmdPending.Dequeue();
                        //Console.WriteLine("IDE VEN:"+cmd);
                        string[] command = scomand.cmd.Split(';');
                        if (command.Length > 0)
                        {
                            //;
                            try
                            {
                                comPort.WriteLine(scomand.cmd);
                                scomand.commandSend();
                                pendingCommand = scomand;

                                cmdOut.Add(command[0]);
                            }
                            catch (TimeoutException e)
                            {
                                this.writeTimeOut = true;
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        public bool sendCommand(String d)
        {
            return true;
        }
        public bool sendCommand(SerialCommand cmd)
        {
            if (comPort.IsOpen)
            {
                if (cmd != null)
                {
                    cmdPending.Enqueue(cmd);
                }

            }
            return false;
        }

        private void addSensor(string data)
        {
            var regex = new Regex(@"\[([0-9]+)\]");
            if (regex.IsMatch(data))
            {
                string[] spl = regex.Split(data);

                if (spl.Length == 3)
                {
                    if (spl[2].Contains("DHT"))
                    {
                        int ID = Int32.Parse(spl[1]);

                        string[] sensData = spl[2].Split(';');
                        Console.WriteLine("SENS CREATE:" + spl[2]);

                        DHTSensor s = new DHTSensor(ID, Int32.Parse(sensData[1]), Int32.Parse(sensData[2]), Int32.Parse(sensData[3]));

                        s.form.UpdateRefreshTime(sensData[3]);
                        //s.form.Dock = DockStyle.Fill;
                        TabPage myTabPage = new TabPage();//Create new tabpage
                        myTabPage.Text = s.tabName;

                        myTabPage.Controls.Add(s.form);

                        s.tab = myTabPage;


                        DHTSensors.Add(ID, s);


                        SensorListControl.Instance.addSensorTab(myTabPage);

                    }
                }
            }
        }

        public void removesensor(int id, bool CMD = false)
        {
            DHTSensor s;
            if (DHTSensors.TryGetValue(id, out s))
            {
                if (!CMD)
                {
                    sendCommand(new SerialCommand("del;" + id + ";",this.command_del));
                }
                else
                {
                    SensorListControl.Instance.removeSensorTab(s.tab);
                    s.tab = null;
                    s.form = null;
                    DHTSensors.Remove(id);
                }
            }
        }

        public void clearSensors()
        {
            foreach (KeyValuePair<int, DHTSensor> s in DHTSensors)
            {
                SensorListControl.Instance.removeSensorTab(s.Value.tab);
                s.Value.tab = null;
                s.Value.form = null;
            }

            DHTSensors.Clear();
            Console.WriteLine("Clear: " + DHTSensors.Count);

        }

        public void refreshSensors()
        {

            clearSensors();
            sendCommand(new SerialCommand("list;", this.command_list));
        }

        public bool command_hi(SerialCommand.CommandReturn ret, String data)
        {
            string[] prm = data.Split(';');
            if (prm.Length > 3 && prm[0].Equals("hi") && prm[1].Length == 23)
            {
                Console.WriteLine(prm[1]);
                ArrayList opt = new ArrayList();
                for (int i = 3; i < prm.Length; i++)
                {
                    if (prm[i].Contains("+") && !opt.Contains(prm[i]))
                    {
                        opt.Add(prm[i]);
                    }
                }

                this.UUID = prm[1];
                this.FlashSize = Int32.Parse(prm[2]);
                this.options = opt;

                this.authenticated = true;

                Console.WriteLine(this.toString());
            }

            return true;
        }

        public bool command_list(SerialCommand.CommandReturn ret, String data)
        {
            if (ret == SerialCommand.CommandReturn.COMMAND_OK)
            {
                string[] subPars = data.Split(
                        new[] { "\r\n" }, //, "\r", "\n", "\n\r"
                        StringSplitOptions.None
                    );
                for (int i = 1; i < subPars.Length; i++)
                {
                    addSensor(subPars[i]);
                    Console.WriteLine("Sensor: " + subPars[i]);
                }
            } else if(ret == SerialCommand.CommandReturn.COMMAND_TIMEOUT)
            {
                
            }
            return true;
        }

        public bool command_del(SerialCommand.CommandReturn ret, String data)
        {
            if (ret == SerialCommand.CommandReturn.COMMAND_OK)
            {
                string sresultString = Regex.Match(data, @"\d+").Value;
                int index;
                if (Int32.TryParse(sresultString, out index))
                {
                    removesensor(index, true);
                    Console.WriteLine("Del sensor:" + index);
                }
            }
            return true;
        }
        public bool command_dhtadd(SerialCommand.CommandReturn ret, String data)
        {
            if (ret == SerialCommand.CommandReturn.COMMAND_OK)
            {
                refreshSensors();
            }
            return true;
        }

        public bool command_save(SerialCommand.CommandReturn ret, String data)
        {
            
            return true;
        }

        public bool command_load(SerialCommand.CommandReturn ret, String data)
        {
            return true;
        }

        private void invokeCommand(string data)
        {
            Console.WriteLine("CALL:" + data);

            if (data.Contains("hi;"))
            {
                string[] prm = data.Split(';');
                if (prm.Length > 3 && prm[0].Equals("hi") && prm[1].Length == 23)
                {
                    Console.WriteLine(prm[1]);
                    ArrayList opt = new ArrayList();
                    for (int i = 3; i < prm.Length; i++)
                    {
                        if (prm[i].Contains("+") && !opt.Contains(prm[i]))
                        {
                            opt.Add(prm[i]);
                        }
                    }

                    this.UUID = prm[1];
                    this.FlashSize = Int32.Parse(prm[2]);
                    this.options = opt;

                    this.authenticated = true;

                    Console.WriteLine(this.toString());
                }
            }
            else if (data.Contains("list"))
            {
                if (data.Contains("OK"))
                {
                    string[] subPars = data.Split(
                        new[] { "\r\n" }, //, "\r", "\n", "\n\r"
                        StringSplitOptions.None
                    );
                    for (int i = 1; i < subPars.Length; i++)
                    {
                        addSensor(subPars[i]);
                        Console.WriteLine("Sensor: "+subPars[i]);
                    }
                }
            }
            else if (data.Contains("del"))
            {
                if (data.Contains("OK"))
                {
                    string sresultString = Regex.Match(data, @"\d+").Value;
                    int index;
                    if(Int32.TryParse(sresultString, out index))
                    {
                        removesensor(index, true);
                        Console.WriteLine("Del sensor:" + index);
                    }
                   

                }
            }
            else if (data.Contains("load"))
            {
                if (data.Contains("OK"))
                {
                  //  MessageBox.Show("Načíanie prebehlo úspešne");
                }
            }
            else if (data.Contains("save"))
            {
                if (data.Contains("OK"))
                {
                //    MessageBox.Show("Uloženie prebehlo úspešne");
                }
            }
            else if (data.Contains("dhtadd"))
            {
                if (data.Contains("OK"))
                {
                    refreshSensors();
                }
            }
        }
        private void processData()
        {
            /*int cO = 0;
            int cI = 0;*/
            ArrayList dataC = (ArrayList)dataIn.Clone();

            for(int i=0; i < dataC.Count; i++)
            {
                string data = (string)dataC[i];

                var regex = new Regex(@"\[([0-9]+)\]");
                string[] splt = regex.Split(data);
                if(splt.Length == 3)
                {
                    DHTSensor s = null;
                    int ID = Int32.Parse(splt[1]);
                    try
                    {
                        s = (DHTSensor)DHTSensors[ID];
                    }
                    catch(Exception e)
                    {

                    }

                    if (s != null)
                    {
                        s.updateData(s.ParseInputData(splt[2]));           
                     }

                }
                dataIn.RemoveAt(i);
                //cO++;
            }
        }
        private void processCommands()
        {
            /*int cO = 0;
            int cI = 0;*/
            foreach (string cmdO in cmdOut)
            {
                foreach(string cmdI in cmdIn)
                {
                    if(cmdI.Contains(cmdO))
                    {
                        invokeCommand(cmdI);
                        cmdIn.Remove(cmdI);
                        cmdOut.Remove(cmdO);

                        return;
                    }
                    //cI++;
                }
                //cO++;
            }
        }

        public void Clear()
        {
            this.closePort();
            clearSensors();
            this.comPortThread.Abort();
        }

        public void flushInputBuffer()
        {
            this.inputLine = "";
        }

        public void recieveData(string input)
        {
            inputLine += input;

            //Console.WriteLine("Data:"+inputLine);

            string[] lines = inputLine.Split(
                new[] { "*\r\n" }, //, "\r", "\n", "\n\r"
                StringSplitOptions.None
            );

            inputLine = lines[lines.Length - 1];

            if(lines.Length > 1)
            {
                var regex = new Regex(@"\[([0-9]+)\]");

                for (int i=0; i <lines.Length-1; i++)
                {
                    string l = lines[i];

                    string[] subPars = l.Split(
                        new[] { "\r\n" }, //, "\r", "\n", "\n\r"
                        StringSplitOptions.None
                    );

                    Console.WriteLine("L:"+l);
                    if(subPars[0].Length > 0)
                    {
                        if (regex.IsMatch(subPars[0]))
                        {
                            Console.WriteLine("Data:" + l);
                            dataIn.Add(l);
                        }
                        else
                        {
                            /*   if(l[l.Length-1] == '*')  
                               {*/
                            if(pendingCommand != null)
                            {
                                if(pendingCommand.checkResponse(l))
                                {
                                    pendingCommand = null;
                                }
                            }
                               // cmdIn.Add();
                                Console.WriteLine("CMD:" + l);
                            //}
                        }
                    }
                }
            }
            processCommands();
            processData();

            /*

            

            if (inputLine.Contains('\n'))
            {
                int pos = inputLine.IndexOf('\n');
                string cmd = inputLine.Substring(0, pos);
                string rest = inputLine.Substring(pos, inputLine.Length - cmd.Length-1);
                inputLine = rest;
                Console.WriteLine("CMD IN:"+cmd);

            }*/
        }

    }
}
