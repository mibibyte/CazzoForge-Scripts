using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;

using InfServer.Logic;
using InfServer.Game;
using InfServer.Scripting;
using InfServer.Bots;
using InfServer.Protocol;

using Assets;

namespace InfServer.Script.GameType_Multi
{
    partial class Script_Multi : Scripts.IScript
    {
        public bool tryEngineer(Player player, Computer computer, VehInfo.Computer.ComputerProduct product)
        {
            int idx = Convert.ToInt32(product.Title.Substring(0, 1));

            switch (idx)
            {
                //Build Exosuit Kit (10 Iron + $1000)
                case 1:
                    {
                        if (player.getInventoryAmount(2027) < 10)
                        {
                            player.sendMessage(-1, "Engineer> I'm sorry, you don't have enough iron for this option");
                            return false;
                        }

                        if (player.Cash < 1000)
                        {
                            player.sendMessage(-1, "Engineer> I'm sorry, you don't have enough cash for this option");
                            return false;
                        }

                        player.inventoryModify(2027, -10);
                        player.inventoryModify(159, 1);
                        player.Cash -= 1000;
                        player.sendMessage(0, "1 Deploy ExoSuit has been added to your inventory");
                        player.syncState();
                    }
                    break;
            }
            return false;
        }
    }
}
