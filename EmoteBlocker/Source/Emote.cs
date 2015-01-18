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
            if(args.PacketData[0] == 36)
            {
                Packet.S2C.PlayEmote.Struct packet = Packet.S2C.PlayEmote.Decoded(args.PacketData);

                if (Core.DebugMode)
                    Game.PrintChat("EmoteID: {0}", packet.EmoteId);

                // blocked?
                if (Config.GetBlockedNetworkIDs().Contains(packet.NetworkId))
                {
                    args.Process = false;
                    if(Core.DebugMode)
                        Game.PrintChat("Blocked emote");
                }
            }
        }

        internal static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Config.SpamEnabled || _nextEmoteSpam > (int) Game.Time || !CanPlayEmote())
                return;

            PlayEmote();
            _nextEmoteSpam = (int)Game.Time + Config.SpamInterval;
        }

        static Boolean CanPlayEmote()
        {
            // is there any condition where emote should not be used?
            if (Core.Hero.IsDead || Core.Hero.Spellbook.IsChanneling || Core.Hero.Spellbook.IsCharging || Core.Hero.HasBuff("Recall")
                || !Core.Hero.IsVisible || Core.Hero.IsWindingUp || Core.Hero.Spellbook.IsAutoAttacking || Core.Hero.IsMoving)
                return false;

            Obj_AI_Hero nearestEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => (!h.IsDead && h.IsEnemy))
                    .OrderBy(h => h.Distance(Core.Hero))
                    .FirstOrDefault();

            // there is no enemy?
            if (nearestEnemy == null || nearestEnemy.Distance(Core.Hero) > 1000)
                return false;

            // otherwise
            return true;
        }

        static void PlayEmote()
        {
            //Packet.C2S.Emote.Encoded(new Packet.C2S.Emote.Struct((byte)Packet.Emotes.Laugh, main.Hero.NetworkId)).Send();

            // temp solution until the related packet implemented by Joduskame
            String emoteCmd = Config.GetRandomEmoteCommand();
            if (emoteCmd != String.Empty)
            {
                Game.Say(emoteCmd);
                if (Core.DebugMode)
                    Game.PrintChat("Spam: {0}", emoteCmd);
            }
        }
    }
}
