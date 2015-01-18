using System;
using LeagueSharp;

namespace EmoteBlocker.Source
{
    class Core
    {
        internal static bool DebugMode = false;

        internal static Obj_AI_Hero Hero;

        internal static void Init()
        {
            if (Game.Mode == GameMode.Running)
                Game_OnGameStart(new EventArgs());
            else
                Game.OnGameStart += Game_OnGameStart;
        }

        static void Game_OnGameStart(EventArgs args)
        {
            Hero = ObjectManager.Player;

            Game.OnGameProcessPacket += Emote.Game_OnGameProcessPacket;
            //Game.OnGameSendPacket += Emote.Game_OnGameSendPacket;
            Game.OnGameUpdate += Emote.Game_OnGameUpdate;

            Config.CreateMenu();
        }
    }
}
