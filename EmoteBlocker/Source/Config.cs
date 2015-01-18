using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace EmoteBlocker.Source
{
    class Config
    {
        public enum Emote { Taunt, Joke, Laugh, Dance }

        internal static Dictionary<string, MenuWrapper.SubMenu> Menus = new Dictionary<string, MenuWrapper.SubMenu>();
        internal static Dictionary<string, MenuWrapper.BoolLink> BoolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
        internal static Dictionary<string, MenuWrapper.SliderLink> SliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();

        internal static void CreateMenu()
        {
            MenuWrapper.SubMenu mainMenu = new MenuWrapper("[Kirito] Emote Blocker", false, false).MainMenu;

            Menus.Add("_", mainMenu);

            // spammer
            MenuWrapper.SubMenu spammerMenu = mainMenu.AddSubMenu("Emote Spammer");
            BoolLinks.Add("spamEnabled", spammerMenu.AddLinkedBool("Enabled", false));
            BoolLinks.Add("spamTaunt", spammerMenu.AddLinkedBool("Spam Taunt"));
            BoolLinks.Add("spamJoke", spammerMenu.AddLinkedBool("Spam Joke"));
            BoolLinks.Add("spamLaugh", spammerMenu.AddLinkedBool("Spam Laugh"));
            BoolLinks.Add("spamDance", spammerMenu.AddLinkedBool("Spam Dance", false));
            SliderLinks.Add("spamInterval", spammerMenu.AddLinkedSlider("Interval (seconds)", 3, 1, 15));

            // there are any ally champions?
            IEnumerable<Obj_AI_Hero> allies = ObjectManager.Get<Obj_AI_Hero>().Where(hero => (hero.IsAlly && !hero.IsMe)).ToList();
            if (allies.Any())
            {
                MenuWrapper.SubMenu allySubMenu = mainMenu.AddSubMenu("Ally");
                Menus.Add("ally", allySubMenu);

                foreach(Obj_AI_Hero ally in allies)
                    BoolLinks.Add("ally." + ally.NetworkId, allySubMenu.AddLinkedBool(ally.ChampionName, false));
            }

            // and enemies?
            IEnumerable<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(hero => (hero.IsEnemy)).ToList();
            if (enemies.Any())
            {
                MenuWrapper.SubMenu enemySubMenu = mainMenu.AddSubMenu("Enemy");
                Menus.Add("enemy", enemySubMenu);

                foreach (Obj_AI_Hero enemy in enemies)
                    BoolLinks.Add("enemy." + enemy.NetworkId, enemySubMenu.AddLinkedBool(enemy.ChampionName, false));
            }

            // other menus
            if (allies.Any())
                BoolLinks.Add("blockAlly", mainMenu.AddLinkedBool("Block all ally emotes", false));

            if (enemies.Any())
                BoolLinks.Add("blockEnemy", mainMenu.AddLinkedBool("Block all enemy emotes", false));

            if (allies.Any() && enemies.Any())
                BoolLinks.Add("blockAll", mainMenu.AddLinkedBool("Block all emotes (except mine)", false));

            BoolLinks.Add("blockMy", mainMenu.AddLinkedBool("Block my emotes (only for me)", false));
            mainMenu.MenuHandle.AddItem(new MenuItem("credits", "Created by Kirito"));
        }

        internal static bool SpamEnabled
        {
            get { return BoolLinks["spamEnabled"].Value; }
        }

        internal static int SpamInterval
        {
            get { return SliderLinks["spamInterval"].Value.Value; }
        }

        internal static String GetRandomEmoteCommand()
        {
            List<Emote> emotes = new List<Emote>();

            if (BoolLinks["spamTaunt"].Value)
                emotes.Add(Emote.Taunt);

            if (BoolLinks["spamJoke"].Value)
                emotes.Add(Emote.Joke);

            if (BoolLinks["spamLaugh"].Value)
                emotes.Add(Emote.Laugh);

            if (BoolLinks["spamDance"].Value)
                emotes.Add(Emote.Dance);

            if (!emotes.Any())
                return String.Empty;

            switch (emotes[new Random().Next(emotes.Count)])
            {
                case Emote.Taunt:
                    return "/t";

                case Emote.Joke:
                    return "/j";

                case Emote.Laugh:
                    return "/l";

                case Emote.Dance:
                    return "/d";
            }

            return String.Empty;
        }

        internal static HashSet<long> GetBlockedNetworkIDs()
        {
            HashSet<long> blockedNetworkIDs = new HashSet<long>();

            bool allBlocked = false;

            // all? - except me
            if (BoolLinks.ContainsKey("blockAll") && BoolLinks["blockAll"].Value)
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => !hero.IsMe))
                    blockedNetworkIDs.Add(hero.NetworkId);

                allBlocked = true;
            }

            // me?
            if (BoolLinks["blockMy"].Value)
                blockedNetworkIDs.Add(Core.Hero.NetworkId);

            // all blocked so ally/enemy checks isn't needed
            if (allBlocked)
                return blockedNetworkIDs;

            // all ally
            if (BoolLinks.ContainsKey("blockAlly") && BoolLinks["blockAlly"].Value)
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => (!hero.IsMe && hero.IsAlly)))
                    blockedNetworkIDs.Add(hero.NetworkId);
            }

            // all enemy
            if (BoolLinks.ContainsKey("blockEnemy") && BoolLinks["blockEnemy"].Value)
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
                    blockedNetworkIDs.Add(hero.NetworkId);
            }
            
            // specified allies/enemies
            foreach(KeyValuePair<string, MenuWrapper.BoolLink> link in BoolLinks)
            {
                if (!link.Value.Value)
                    continue;

                if (link.Key.Contains("ally.") || link.Key.Contains("enemy."))
                {
                    string[] split = link.Key.Split('.');
                    blockedNetworkIDs.Add(Convert.ToInt64(split[1]));
                }
            }

            return blockedNetworkIDs;
        }
    }
}
