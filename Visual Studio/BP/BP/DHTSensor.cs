using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BP
{
    class DHTSensor: SensorBackend
    {
        public int id { get; set; }
        public int pin { get; set; }
        public int type { get; set; }
        public int updateTime { get; set; }
        public string tabName { get; set; }
        public DHTControlls form{ get; set; }
        public TabPage tab { get; set; }

        public List<DHTValue> measuredValues = new List<DHTValue>();

        public DHTSensor(int id,int p, int t, int u)
        {
            this.id = id;
            pin = p;
            type = t;
            updateTime = u;

            form = new DHTControlls(id,this);

            tabName = "[" + id + "]DHT" + type;
        }

        public void updateData(object[] data)
        {
            if (data.Length == 2)
            { 
                this.measuredValues.Add(new DHTValue((Double)data[0], (Double)data[1]));

                this.form.UpdateGUI(data);
            }
        }

        public void createCSV(string path)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@path))
            {
                file.WriteLine("Time,Temperature,Humidity");
                foreach (DHTValue val in measuredValues)
                {
                    file.WriteLine(val.CSV());
                }
            }
            
        }
        public object[] ParseInputData(string input)
        {
            string vals = input.Substring(input.IndexOf(':'));
            string[] hod = vals.Split(',');
            string tmp, hm;

            Double temp = Double.NaN, hum = Double.NaN;

            if (hod.Length == 2)
            {
                int index = hod[0].IndexOfAny("0123456789-".ToCharArray());
                if(index >= 0)
                {
                    tmp = hod[0].Substring(index);
                    if (Double.TryParse(tmp, out temp))
                    {
                        Console.WriteLine("Temp: " + temp);
                    }
                }
                index = hod[1].IndexOfAny("0123456789-".ToCharArray());
                if (index >= 0)
                {
                    tmp = hod[1].Substring(index);
                    if (Double.TryParse(tmp, out hum))
                    {
                        Console.WriteLine("Parametre: " + hum);
                    }
                }
            }
            return new object[] { temp, hum};
        }
    }

    class DHTValue
    {
        public Double Temperature { set; get; }
        public Double Humidity { get; set; }
        public DateTime Time { set; get; }

        public DHTValue(Double tmp, Double hum)
        {
            Temperature = tmp;
            Humidity = hum;
            Time = DateTime.Now;
        }
        public string CSV()
        {
            return this.Time.ToString("dd.MM.yyyy HH:mm:ss") + ","+Temperature+","+Humidity; 
        }
    }
}
