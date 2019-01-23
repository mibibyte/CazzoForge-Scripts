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
            int idx = Convert.ToInt32(product.Title.Substring(0, 1));
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
                        player.sendMessage(0, "1 [RTS] Refinery - Iron Kit has been added to your inventory");
                        player.syncState();
                    }
                    break;
            }
            return false;
        }
    }
}
