using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAuto.Shared
{
    internal class NetworkTypes
    {
        public enum Commands
        {
            None,
            KeepAlive,
            AcceptedMatch,
            LoadedOnMap,
            LoadedInLobby,
            Connected,
            Crashed,
            Bomb,
            Clear,
            GameState
        }
    }
}
