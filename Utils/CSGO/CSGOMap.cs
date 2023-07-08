using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAuto.Utils.CSGO
{
    public static class CSGOMap
    {
        public static bool IsOfficial(string mapName) 
        {
            string mapExtention = mapName.Substring(0, 3);
            return
                mapExtention == "de_" ||
                mapExtention == "dz_" ||
                mapExtention == "gd_" ||
                mapExtention == "cs_" ||
                mapExtention == "ar_";
        }
    }
}
