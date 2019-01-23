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

        public bool tryIronRefinery(Player player, Computer computer, VehInfo.Computer.ComputerProduct product)
        {
            int idx = Convert.ToInt32(product.Title.Substring(0, 1));

            switch (idx)
            {
                //1 - 1 Iron (2 Scrap Metal)
                case 1:
                    {
                        if (player.getInventoryAmount(2026) < 2)
                        {
                            player.sendMessage(-1, "Refinery Worker> I'm sorry, you don't have enough scrap metal for this option");
                            return false;
                        }

                        player.inventoryModify(2027, 1);
                        player.inventoryModify(2026, -2);
                        player.sendMessage(0, "1 Iron has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //2 - 5 Iron (10 Scrap Metal)
                case 2:
                    {
                        if (player.getInventoryAmount(2026) < 10)
                        {
                            player.sendMessage(-1, "Refinery Worker> I'm sorry, you don't have enough scrap metal for this option");
                            return false;
                        }

                        player.inventoryModify(2027, 5);
                        player.inventoryModify(2026, -10);
                        player.sendMessage(0, "5 Iron has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //3 - 10 Iron (18 Scrap Metal)
                case 3:
                    {
                        if (player.getInventoryAmount(2026) < 18)
                        {
                            player.sendMessage(-1, "Refinery Worker> I'm sorry, you don't have enough scrap metal for this option");
                            return false;
                        }
                        player.inventoryModify(2027, 10);
                        player.inventoryModify(2026, -18);
                        player.sendMessage(0, "10 Iron has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //4 - 35 Iron (25 Scrap Metal)
                case 4:
                    {
                        if (player.getInventoryAmount(2026) < 25)
                        {
                            player.sendMessage(-1, "Refinery Worker> I'm sorry, you don't have enough scrap metal for this option");
                            return false;
                        }

                        player.inventoryModify(2027, 35);
                        player.inventoryModify(2026, -25);
                        player.sendMessage(0, "35 Iron has been added to your inventory");
                        player.syncState();
                    }
                    break;
                //5 - 50 Iron (30 Scrap Metal)
                case 5:
                    {
                        if (player.getInventoryAmount(2026) < 30)
                        {
                            player.sendMessage(-1, "Refinery Worker> I'm sorry, you don't have enough scrap metal for this option");
                            return false;
                        }

                        player.inventoryModify(2027, 50);
                        player.inventoryModify(2026, -30);
                        player.sendMessage(0, "20 Iron has been added to your inventory");
                        player.syncState();
                    }
                    break;

            }

            return false;
        }
    }
}
