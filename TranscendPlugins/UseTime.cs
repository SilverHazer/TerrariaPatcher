﻿using System;
using System.Windows.Forms;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class UseTime : MarshalByRefObject, IPluginItemSetDefaults, IPluginPlayerUpdateBuffs, IPluginChatCommand
    {
        private int initialTileRangeX, initialTileRangeY, initialDefaultItemGrabRange;
        private bool maxTileSpeed, maxWallSpeed, maxPickSpeed, maxReachRange, maxItemPickupRange;
        private bool builderBuffWarning = false;
        
        public UseTime()
        {
            initialTileRangeX = Player.tileRangeX;
            initialTileRangeY = Player.tileRangeY;
            initialDefaultItemGrabRange = Player.defaultItemGrabRange;

            maxPickSpeed = bool.Parse(IniAPI.ReadIni("UseTime", "MaxPickSpeed", "true", writeIt: true)); // Pick / Hammer / Axe
            maxTileSpeed = bool.Parse(IniAPI.ReadIni("UseTime", "MaxTileSpeed", "true", writeIt: true)); // Placing blocks / wire
            maxWallSpeed = bool.Parse(IniAPI.ReadIni("UseTime", "MaxWallSpeed", "true", writeIt: true)); // Placing wall
            maxReachRange = bool.Parse(IniAPI.ReadIni("UseTime", "MaxReachRange", "true", writeIt: true)); // Block reach
            maxItemPickupRange = bool.Parse(IniAPI.ReadIni("UseTime", "MaxItemPickupRange", "true", writeIt: true)); // Item pickup range
        }

        public void OnItemSetDefaults(Item item)
        {
            if (maxPickSpeed && (item.axe > 0 ||
                                 item.pick > 0 ||
                                 item.hammer > 0))
                item.useTime = 0;

            if (maxTileSpeed &&
                (item.createTile >= 0 ||
                 item.type == ItemID.Wrench ||
                 item.type == ItemID.BlueWrench ||
                 item.type == ItemID.GreenWrench ||
                 item.type == ItemID.WireCutter ||
                 item.type == ItemID.Actuator))
                item.useTime = 1;

            if (maxWallSpeed && item.createWall > 0)
                item.useTime = 1;
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (!builderBuffWarning)
                {
                    for (int k = 0; k < 22; k++)
                    {
                        if (player.buffType[k] == BuffID.Builder && player.buffTime[k] > 0)
                        {
                            builderBuffWarning = true;
                            MessageBox.Show("Please disable the Builder buff, it causes issues with the UseTime plugin.");
                        }
                    }
                }

                if (maxReachRange)
                {
                    Player.tileRangeX = 100;
                    Player.tileRangeY = 100;
                }
                else
                {
                    Player.tileRangeX = initialTileRangeX;
                    Player.tileRangeY = initialTileRangeY;
                }

                Player.defaultItemGrabRange = maxItemPickupRange ? 700 : initialDefaultItemGrabRange;
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "usetime" && command != "autoreuse" && command != "range") return false;

            if (!(command == "usetime" && args.Length == 1) &&
                !(command == "autoreuse" && args.Length == 0) &&
                !(command == "range" && args.Length == 0))
            {
                Main.NewText("Usage:");
                Main.NewText("   /autoreuse");
                Main.NewText("   /usetime num");
                Main.NewText("   /range");
                Main.NewText("Example:");
                Main.NewText("   /usetime 0");
                return true;
            }

            // get item on cursor, if nothing there, get hotbar item
            var item = Main.mouseItem;
            if (item.type == 0)
            {
                var player = Main.player[Main.myPlayer];
                item = player.inventory[player.selectedItem];
                if (item.type == 0)
                {
                    Main.NewText("No item selected.");
                    return true;
                }
            }

            switch (command)
            {
                case "usetime":
                    int num;
                    if (!int.TryParse(args[0], out num))
                    {
                        Main.NewText("Invalid num.");
                        break;
                    }

                    item.useTime = num;
                    Main.NewText("UseTime = " + num);
                    break;
                case "autoreuse":
                    item.autoReuse = !item.autoReuse;
                    Main.NewText("AutoReuse = " + item.autoReuse);
                    break;
                case "range":
                    IniAPI.WriteIni("UseTime", "MaxReachRange", (maxReachRange = !maxReachRange).ToString());
                    IniAPI.WriteIni("UseTime", "MaxItemPickupRange", (maxItemPickupRange = maxReachRange /* this is not a typo */).ToString());
                    Main.NewText("Block reach and item pickup range is " + (maxReachRange ? "enhanced" : "back to normal") + ".");
                    break;
            }
            return true;
        }
    }
}
