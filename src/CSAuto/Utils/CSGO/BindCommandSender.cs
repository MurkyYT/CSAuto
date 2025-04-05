﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Murky.Utils.CSGO
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
            if(File.Exists(_cfgPath))
                File.Delete(_cfgPath);

            File.WriteAllText(_cfgPath, command);

            Input.SendKey(Input.DirectXKeyStrokes.DIK_F13, false, Input.InputType.Keyboard);
            Input.SendKey(Input.DirectXKeyStrokes.DIK_F13, true, Input.InputType.Keyboard);

            Log.WriteLine($"|BindCommandSender.cs| Sent '{command}'");
        }
    }
}
