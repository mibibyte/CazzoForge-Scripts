using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using InfServer.Logic;
using InfServer.Game;
using InfServer.Scripting;
using InfServer.Bots;
using InfServer.Protocol;

using Assets;

namespace InfServer.Script.GameType_Multi
{ 	// Script Class
    /// Provides the interface between the script and arena
    ///////////////////////////////////////////////////////
    public partial class RTS
    {

        public bool tryProductionMenu(Player player, Computer computer, VehInfo.Computer.ComputerProduct product)
        {
            int idx = Convert.ToInt32(product.Title.Substring(0, 2));
            switch (idx)
            {
                //1 - Refinery - Iron (200 Iron)
                case 1:
                    {
                        if (player.getInventoryAmount(2027) < 200)
                        {
                            player.sendMessage(-1, "&You don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -200);
                        player.inventoryModify(197, 1);
                        player.sendMessage(0, "1 [RTS] Refinery - Iron has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //2 - Iron Mine (100 Iron)
                case 2:
                    {
                        if (player.getInventoryAmount(2027) < 100)
                        {
                            player.sendMessage(-1, "&You don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -100);
                        player.inventoryModify(205, 1);
                        player.sendMessage(0, "1 [RTS] Iron Mine Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //3 - Scrapyard (100 Iron)
                case 3:
                    {
                        if (player.getInventoryAmount(2027) < 100)
                        {
                            player.sendMessage(-1, "&You don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -100);
                        player.inventoryModify(206, 1);
                        player.sendMessage(0, "1 [RTS] Scrapyard Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //16 - Click For Info
                case 16:
                    {
                        player.sendMessage(0, "$ - Factory - Production");
                        player.sendMessage(0, "& --- Refinery - Iron (Refines scrap metal into Refined Iron)");
                        player.sendMessage(0, "& --- Iron Mine (Produces Refined Iron at hourly intervals)");
                        player.sendMessage(0, "& --- Scrapyard (Produces Scrap Metal at hourly intervals)");
                    }
                    break;
            }
            return false;
        }
    }
}
