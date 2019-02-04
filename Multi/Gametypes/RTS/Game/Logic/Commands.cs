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
        #region Command Handlers
        public bool playerModcommand(Player player, Player recipient, string command, string payload)
        {
            return true;
        }

        public bool playerChatCommand(Player player, Player recipient, string command, string payload)
        {
            string cmd = command.ToLower();

            switch (cmd)
            {
                case "productions":
                    {
                        player.sendMessage(0, "$-Upcoming Productions:");
                        List<Structure> producers = _structures.Values.Where(str => str._productionQuantity > 0).ToList();

                        foreach (Structure producer in producers)
                        {
                            TimeSpan remaining = _baseScript.timeTo(producer._nextProduction.TimeOfDay);

                            player.sendMessage(0, String.Format("&--- {0} (Quantity={1} Time Remaining={2}H{3}M{4}S",
                                producer._type.Name,
                                producer._productionQuantity,
                                remaining.Hours,
                                remaining.Minutes,
                                remaining.Seconds));
                        }

                    }
                    break;
            }
            return true;
        }
        #endregion
    }
}
