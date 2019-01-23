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

        public bool tryResidentialMenu(Player player, Computer computer, VehInfo.Computer.ComputerProduct product, ProductionBuilding buildingType)
        {
            int idx = Convert.ToInt32(product.Title.Substring(0, 1));
            Structure structure = getStructureByVehicleID(computer._id);

            //Sanity
            if (structure == null)
                return false;

            int level = structure._productionLevel;
            int cash = calculateCashProduction(level, buildingType);
            int cashNextLevel = calculateCashProduction(level + 1, buildingType);
            int upgradeNextLevel = calculateUpgradeCost(structure._upgradeCost, buildingType);


            switch (idx)
            {
                //1 - Collect
                case 1:
                    {
                        //Are we ready?
                        if (!IsProductionReady(structure))
                        {
                            player.sendMessage(0, "&This building is not ready for collection yet.");
                            return false;
                        }

                        //Produce!
                        structure.produce(player);

                    }
                    break;
                //2 - Upgrade (100 Iron)
                case 2:
                    {
                        if (player.getInventoryAmount(2027) < structure._upgradeCost)
                        {
                            player.sendMessage(-1, "&You do not have sufficient iron to upgrade this building");
                            return false;
                        }

                        player.inventoryModify(2027, -structure._upgradeCost);
                        player.syncState();

                        //Up the cost for the next level
                        structure._upgradeCost = upgradeNextLevel;
                        structure._productionLevel++;
                        structure._productionQuantity = cashNextLevel;

                        player.sendMessage(0, String.Format("&Structure upgraded, Next collection: ${0}", structure._productionQuantity));
                        
                    }
                    break;
                //3 More Info
                case 3:
                    {
                        player.sendMessage(0, String.Format("&Building Info:"));
                        player.sendMessage(0, String.Format("Current Level: {0}", structure._productionLevel));
                        player.sendMessage(0, String.Format("Upgrade Cost for next level: {0} Iron", structure._upgradeCost));
                        player.sendMessage(0, String.Format("Next Collection amount: ${0}", structure._productionQuantity));
                        player.sendMessage(0, String.Format("Next Level Collection amount: ${0}", cashNextLevel));

                        TimeSpan remaining = _baseScript.timeTo(structure._nextProduction.TimeOfDay);
                        player.sendMessage(0, String.Format("Next collection ready in {0} Hour(s) & {1} minute(s)", remaining.Hours, remaining.Minutes));

                    }
                    break;
            }
            return false;
        }
    }
}
