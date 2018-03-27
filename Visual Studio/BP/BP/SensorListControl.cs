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
    public partial class SensorListControl : UserControl
    {
        private static SensorListControl _instance;

        delegate void AddControlTab(TabPage userControl);
        delegate void DeleteControlTab(TabPage userControl);

        public static SensorListControl Instance
        {
            get
            {
                if (_instance == null) _instance = new SensorListControl();
                return _instance;
            }
        }
        public SensorListControl()
        {
            InitializeComponent();
        }
        private void AddTab(TabPage tab)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.tabControl1.InvokeRequired)
            {
                AddControlTab d = new AddControlTab(AddTab);
                this.Invoke(d, new object[] { tab });
            }
            else
            {
                try { 
                    this.tabControl1.TabPages.Add(tab);
                }catch(Exception e)
                {

                }
            }
        }

        private void DeleteTab(TabPage tab)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.tabControl1.InvokeRequired)
            {
                DeleteControlTab d = new DeleteControlTab(DeleteTab);
                this.Invoke(d, new object[] { tab });
            }
            else
            {
                try
                {
                    if(this.tabControl1.TabPages.Contains(tab))
                        this.tabControl1.TabPages.Remove(tab);
                }
                catch (Exception e)
                {

                }
            }
        }



        public void addSensorTab(TabPage myTabPage)
        {
            AddTab(myTabPage);
        }

        public void removeSensorTab(TabPage myTabPage)
        {
            DeleteTab(myTabPage);
        }



        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        public void clearTabs()
        {
            tabControl1.TabPages.Clear();
        }

        private void SensorListControl_Load(object sender, EventArgs e)
        {
            //tabControl1.TabPages.Clear();
        }
    }
}
