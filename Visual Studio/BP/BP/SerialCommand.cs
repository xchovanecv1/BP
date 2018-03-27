using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP
{
    class SerialCommand
    {
        public enum CommandReturn { COMMAND_OK = 1, COMMAND_ERROR, COMMAND_TIMEOUT};
        public String cmd { get; }
        private Func<CommandReturn,String,bool> callback;
        public SerialCommand(String command, Func<CommandReturn, String, bool> callbac)
        {
            this.callback = callbac;
            this.cmd = command;
        }

        public void timeoutCommand()
        {
            callback(CommandReturn.COMMAND_TIMEOUT, "");
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
