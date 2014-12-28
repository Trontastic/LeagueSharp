using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace EmoteBlocker.Source
{
    class Main
    {
        public CMenu menuHandler;
        public CPacket packetHandler;

        public Obj_AI_Hero Hero;

        public Main()
        {
            if (Game.Mode == GameMode.Running)
            {
                Game_OnGameStart(new EventArgs());
            }
            else
            {
                Game.OnGameStart += Game_OnGameStart;
            }
        }

        void Game_OnGameStart(EventArgs args)
        {
            Hero = ObjectManager.Player;

            packetHandler = new CPacket(this);
            menuHandler = new CMenu(this);

            Game.OnGameProcessPacket += packetHandler.Game_OnGameProcessPacket;
        }
    }
}
