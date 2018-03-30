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
    public partial class SetInterval : Form
    {
        public Decimal interval { get; set; }
        public SetInterval()
        {
            InitializeComponent();
        }

        public SetInterval(int interval)
        {
            InitializeComponent();
            this.interval = interval;
            this.numericUpDown1.Value = interval;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            interval = this.numericUpDown1.Value;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
