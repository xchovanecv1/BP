using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP
{
    class PinManagement
    {
        private PinManagement _instance;
        public PinManagement Instance
        {
            get
            {
                if (_instance == null) _instance = new PinManagement();
                return _instance;
            }
        }
        public PinManagement()
        {

        }
    }

    class Pin
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool used { get; set; }

        public Pin(int i, string n)
        {
            id = i;
            name = n;
        }
    }
}
