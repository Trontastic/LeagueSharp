using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace DisableEmote.Source
{
    class Main
    {
        public Main()
        {
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.S2C.PlayEmote.Header)
            {
                args.Process = false;
            }
        }
    }
}
