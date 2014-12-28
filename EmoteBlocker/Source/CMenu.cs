using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace EmoteBlocker.Source
{
    class CMenu
    {
        Main main;
        Dictionary<string, MenuWrapper.SubMenu> menus = new Dictionary<string, MenuWrapper.SubMenu>();
        Dictionary<string, MenuWrapper.BoolLink> menuBools = new Dictionary<string, MenuWrapper.BoolLink>();

        public CMenu(Main main)
        {
            this.main = main;
            CreateMenu();
        }

        void CreateMenu()
        {
            MenuWrapper.SubMenu mainMenu = new MenuWrapper("[Kirito] Emote Blocker", false, false).MainMenu;

            menus.Add("_", mainMenu);

            // there are any ally champions?
            IEnumerable<Obj_AI_Hero> allys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => (hero.Team == main.Hero.Team && !hero.IsMe));
            if (allys.Count() > 0)
            {
                MenuWrapper.SubMenu allySubMenu = mainMenu.AddSubMenu("Ally");
                menus.Add("ally", allySubMenu);

                foreach(Obj_AI_Hero ally in allys)
                {
                    menuBools.Add("ally." + ally.NetworkId, allySubMenu.AddLinkedBool(ally.ChampionName, false));
                }
            }

            // and enemies?
            IEnumerable<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(hero => (hero.Team != main.Hero.Team));
            if (enemies.Count() > 0)
            {
                MenuWrapper.SubMenu enemySubMenu = mainMenu.AddSubMenu("Enemy");
                menus.Add("enemy", enemySubMenu);

                foreach (Obj_AI_Hero enemy in enemies)
                {
                    menuBools.Add("enemy." + enemy.NetworkId, enemySubMenu.AddLinkedBool(enemy.ChampionName, false));
                }
            }

            // other menus
            menuBools.Add("blockAlly", mainMenu.AddLinkedBool("Block all ally emotes", false));
            menuBools.Add("blockEnemy", mainMenu.AddLinkedBool("Block all enemy emotes", false));
            menuBools.Add("blockAll", mainMenu.AddLinkedBool("Block all emotes (except mine)", false));
            menuBools.Add("blockMy", mainMenu.AddLinkedBool("Block my emotes (only for me)", false));
            mainMenu.MenuHandle.AddItem(new MenuItem("credits", "Created by Kirito"));
        }

        public List<long> GetBlockedNetworkIDs()
        {
            List<long> blockedNetworkIDs = new List<long>();

            bool allBlocked = false;

            // all?
            if (menuBools["blockAll"].Value)
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => !hero.IsMe))
                {
                    blockedNetworkIDs.Add(hero.NetworkId);
                }

                allBlocked = true;
            }

            // me?
            if (menuBools["blockMy"].Value)
            {
                blockedNetworkIDs.Add(main.Hero.NetworkId);
            }

            // all blocked so ally/enemy checks isn't needed
            if (allBlocked)
            {
                return blockedNetworkIDs;
            }
            
            // ally/enemy
            foreach(KeyValuePair<string, MenuWrapper.BoolLink> link in menuBools)
            {
                if (!link.Value.Value)
                {
                    continue;
                }

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
