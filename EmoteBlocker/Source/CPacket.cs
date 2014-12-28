using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace EmoteBlocker.Source
{
    class CPacket
    {
        Main main;
        public CPacket(Main main)
        {
            this.main = main;
        }

        public void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if(args.PacketData[0] == Packet.S2C.PlayEmote.Header)
            {
                Packet.S2C.PlayEmote.Struct packet = Packet.S2C.PlayEmote.Decoded(args.PacketData);

                // blocked?
                if (main.menuHandler.GetBlockedNetworkIDs().Contains(packet.NetworkId))
                {
                    args.Process = false;
                }
            }
        }
    }
}
