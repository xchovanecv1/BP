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
    public partial class SettingsControls : UserControl
    {
        private static SettingsControls _instance;
        public static SettingsControls Instance
        {
            get
            {
                if (_instance == null) _instance = new SettingsControls();
                return _instance;
            }
        }
        public SettingsControls()
        {
            InitializeComponent();
        }
    }
}
