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

        public bool tryHousingProductionMenu(Player player, Computer computer, VehInfo.Computer.ComputerProduct product)
        {
            int idx = Convert.ToInt32(product.Title.Substring(0, 1));
            switch (idx)
            {
                //1 - Shack (75 Iron)
                case 1:
                    {
                        if (player.getInventoryAmount(2027) < 75)
                        {
                            player.sendMessage(-1, "&You don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -75);
                        player.inventoryModify(192, 1);
                        player.sendMessage(0, "1 [RTS] Shack Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //2 - House (125 Iron)
                case 2:
                    {
                        if (player.getInventoryAmount(2027) < 125)
                        {
                            player.sendMessage(-1, "&You don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -125);
                        player.inventoryModify(201, 1);
                        player.sendMessage(0, "1 [RTS] House Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //3 - Villa (175 Iron)
                case 3:
                    {
                        if (player.getInventoryAmount(2027) < 175)
                        {
                            player.sendMessage(-1, "&You don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -175);
                        player.inventoryModify(202, 1);
                        player.sendMessage(0, "1 [RTS] Villa Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
            }
            return false;
        }
    }
}
