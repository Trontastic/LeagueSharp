using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace EmoteBlocker.Source
{
    class Emote
    {
        internal static class EmoteId
        {
            internal static byte[] Joke = { 0, 3, 4, 7 };
            internal static byte[] Taunt = { 2 };
            internal static byte[] Dance = { 5 };
            internal static byte[] Laugh = { 1 };
        }

        private static int _nextEmoteSpam = 0;

        internal static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if(args.PacketData[0] == Packet.S2C.PlayEmote.Header)
            {
                Packet.S2C.PlayEmote.Struct packet = Packet.S2C.PlayEmote.Decoded(args.PacketData);

                if (Core.DebugMode)
                    Game.PrintChat("EmoteID: {0}", packet.EmoteId);

                // blocked?
                if (Config.GetBlockedNetworkIDs().Contains(packet.NetworkId))
                    args.Process = false;
            }
        }

        internal static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Config.SpamEnabled || _nextEmoteSpam < (int) Game.Time)
                return;

            if (!CanPlayEmote())
                return;

            PlayEmote();
            _nextEmoteSpam = (int)Game.Time + Config.SpamInterval;

            if (Core.DebugMode)
                Game.PrintChat("Spam");
        }

        static Boolean CanPlayEmote()
        {
            Obj_AI_Hero nearestEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => (!h.IsDead && h.IsEnemy))
                    .OrderBy(h => h.Distance(Core.Hero))
                    .FirstOrDefault();

            // there is no enemy?
            if (nearestEnemy == null || nearestEnemy.Distance(Core.Hero) > 1000)
                return false;

            // is there any other condition where emote should not be used?
            if (Core.Hero.IsDead || Core.Hero.IsChanneling || Core.Hero.IsCharging || Core.Hero.HasBuff("Recall")
                || !Core.Hero.IsVisible || Core.Hero.IsWindingUp || Core.Hero.IsAutoAttacking)
                return false;

            return true;
        }

        static void PlayEmote()
        {
            //Packet.C2S.Emote.Encoded(new Packet.C2S.Emote.Struct((byte)Packet.Emotes.Laugh, main.Hero.NetworkId)).Send();

            // temp solution until the related packet implemented by Joduskame
            String emoteCmd = Config.GetRandomEmoteCommand();
            if (emoteCmd != String.Empty)
                Game.Say(emoteCmd);
        }

        internal static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (Core.DebugMode)
                Game.PrintChat("Header: {0}", args.PacketData[0]);
        }
    }
}
