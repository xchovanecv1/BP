using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP
{
    class SerialCommand
    {
        public enum CommandReturn { COMMAND_OK = 1, COMMAND_ERROR, COMMAND_TIMEOUT };
        public String cmd { get; }
        private Func<CommandReturn, String, bool> callback;

        private Stopwatch sw;

        private Int32 timeout { get; set; }

        public SerialCommand(String command, Func<CommandReturn, String, bool> callbac)
        {
            this.timeout = 1000;
            this.callback = callbac;
            this.cmd = command;
        }

        public void commandSend()
        {
            sw = new Stopwatch();
            sw.Start();
        }

        public bool commandTimeout()
        {
            if(sw != null)
            {
                if(sw.ElapsedMilliseconds >= timeout)
                {
                    sw.Stop();
                    sw = null;
                    callback(CommandReturn.COMMAND_TIMEOUT, "");
                    return true;
                }
            }
            return false;
        }

        public bool checkResponse(String input)
        {
            String[] prs = cmd.Split(';');
            CommandReturn ret;
            if(prs.Length > 0)
            {
                if(input.Contains(prs[0]))
                {
                    if(input.Contains(" OK"))
                    {
                        ret = CommandReturn.COMMAND_OK;
                    } else
                    {
                        ret = CommandReturn.COMMAND_ERROR;
                    }


                    callback(ret, input);

                    return true;
                }
            }
            return false;
        }
    }
}
