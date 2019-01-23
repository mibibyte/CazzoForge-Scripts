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

        public bool tryCommandCenterMenu(Player player, Computer computer, VehInfo.Computer.ComputerProduct product)
        {
            int idx = Convert.ToInt32(product.Title.Substring(0, 1));
            switch (idx)
            {
                //1 - Factory - Production (125 Iron)
                case 1:
                    {
                        if (player.getInventoryAmount(2027) < 125)
                        {
                            player.sendMessage(-1, "I'm sorry, you don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -125);
                        player.inventoryModify(193, 1);
                        player.sendMessage(0, "1 [RTS] Factory - Production Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //2 - Factory - Defense (100 Iron)
                case 2:
                    {
                        if (player.getInventoryAmount(2027) < 100)
                        {
                            player.sendMessage(-1, "I'm sorry, you don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -100);
                        player.inventoryModify(194, 1);
                        player.sendMessage(0, "1 [RTS] Factory - Defense Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //3 - Factory - Housing (200 Iron)
                case 3:
                    {
                        if (player.getInventoryAmount(2027) < 200)
                        {
                            player.sendMessage(-1, "I'm sorry, you don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -200);
                        player.inventoryModify(204, 1);
                        player.sendMessage(0, "1 [RTS] Factory - Housing Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //4 - Power Station (25 Iron)
                case 4:
                    {
                        if (player.getInventoryAmount(2027) < 25)
                        {
                            player.sendMessage(-1, "I'm sorry, you don't have enough iron for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -25);
                        player.inventoryModify(200, 1);
                        player.sendMessage(0, "1 [RTS] Power Station Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
            }
            return false;
        }
    }
}
