using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BP
{
    public partial class DHTadd : Form
    {
        public int type { get; set; }
        public int pin { get; set; }
        public int update { get; set; }

        public DHTadd()
        {
            InitializeComponent();
            type = -1;
            pin = -1;
            update = -1;

            comboBox1.Items.Add(new ComboItem(11, "DHT11"));
            comboBox1.Items.Add(new ComboItem(22, "DHT22"));

            comboBox2.Items.Add(new ComboItem(0, "PA0"));
            comboBox2.Items.Add(new ComboItem(1, "PA1"));
            comboBox2.Items.Add(new ComboItem(2, "PA2"));
            comboBox2.Items.Add(new ComboItem(3, "PA3"));
            comboBox2.Items.Add(new ComboItem(4, "PA4"));
            comboBox2.Items.Add(new ComboItem(5, "PA5"));
            comboBox2.Items.Add(new ComboItem(6, "PA6"));
            comboBox2.Items.Add(new ComboItem(7, "PA7"));
            comboBox2.Items.Add(new ComboItem(16, "PB0"));
            comboBox2.Items.Add(new ComboItem(17, "PB1"));
            comboBox2.Items.Add(new ComboItem(19, "PB3"));
            comboBox2.Items.Add(new ComboItem(20, "PB4"));

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ComboItem bf = comboBox1.SelectedItem as ComboItem;
            this.type = bf.Key;

            bf = comboBox2.SelectedItem as ComboItem;
            this.pin = bf.Key;

            this.update = (int)numericUpDown1.Value;

            this.Close();
        }

        class ComboItem
        {
            public int Key { get; set; }
            public string Value { get; set; }
            public ComboItem(int key, string value)
            {
                Key = key; Value = value;
            }
            public override string ToString()
            {
                return Value;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
