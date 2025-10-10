using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Murky.Utils.CS
{
    public class BindCommandSender
    {
        private string _cfgPath;

        public BindCommandSender(string cfgPath)
        {
            _cfgPath = cfgPath;
        }

        public void SendCommand(string command) 
        {
            lock (this)
            {
                if (File.Exists(_cfgPath))
                    File.Delete(_cfgPath);

                File.WriteAllText(_cfgPath, command);

                Input.SendKey(Input.DirectXKeyStrokes.DIK_F13, false, Input.InputType.Keyboard);
                Thread.Sleep(15);
                Input.SendKey(Input.DirectXKeyStrokes.DIK_F13, true, Input.InputType.Keyboard);

                Log.WriteLine($"|BindCommandSender.cs| Sent '{command}'");
            }
        }
    }
}
