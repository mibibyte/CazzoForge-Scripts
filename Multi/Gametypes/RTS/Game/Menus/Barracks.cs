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

        public bool tryBarracksMenu(Player player, Computer computer, VehInfo.Computer.ComputerProduct product, DefenseProduction type)
        {
            int idx = Convert.ToInt32(product.Title.Substring(0, 1));
            switch (idx)
            {
                //1 - Train 1 Normal ($500)
                case 1:
                    {

                        if (player.Cash < 500)
                        {
                            player.sendMessage(-1, "Insufficient funds for this option");
                            return false;
                        }
                        player.Cash -= 500;
                        player.sendMessage(0, "Unit Ready");
                        player.syncState();
                        switch (type)
                        {
                            case DefenseProduction.Marine:
                                newUnit(BotType.Marine, null, null, BotLevel.Normal, player._state);
                                break;
                            case DefenseProduction.Ripper:
                                newUnit(BotType.Ripper, null, null, BotLevel.Normal, player._state);
                                break;
                        }
                    }
                    break;
                //2 - Train 1 Adept ($1000)
                case 2:
                    {
                        if (player.Cash < 1000)
                        {
                            player.sendMessage(-1, "Insufficient funds for this option");
                            return false;
                        }
                        player.Cash -= 1000;
                        player.sendMessage(0, "Unit Ready");
                        player.syncState();
                        switch (type)
                        {
                            case DefenseProduction.Marine:
                                newUnit(BotType.Marine, null, null, BotLevel.Adept, player._state);
                                break;
                            case DefenseProduction.Ripper:
                                newUnit(BotType.Ripper, null, null, BotLevel.Adept, player._state);
                                break;
                        }
                    }
                    break;
                //3 - Train 1 Elite ($1500)
                case 3:
                    {
                        if (player.Cash < 1500)
                        {
                            player.sendMessage(-1, "Insufficient funds for this option");
                            return false;
                        }
                        player.Cash -= 1500;
                        player.sendMessage(0, "Unit Ready");
                        player.syncState();
                        switch (type)
                        {
                            case DefenseProduction.Marine:
                                newUnit(BotType.EliteMarine, null, null, BotLevel.Elite, player._state);
                                break;
                            case DefenseProduction.Ripper:
                                newUnit(BotType.EliteHeavy, null, null, BotLevel.Elite, player._state);
                                break;
                        }
                    }
                    break;
            }
            return false;
        }
    }
}
