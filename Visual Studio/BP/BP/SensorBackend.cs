using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP
{
    interface SensorBackend
    {
        object[] ParseInputData(string input);

        bool updateTimeGUI(SerialCommand.CommandReturn res, String data);

        int updateTime { get; set; }
        int id { get; set; }
    }
}
