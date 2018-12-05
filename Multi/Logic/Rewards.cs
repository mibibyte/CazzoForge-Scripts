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
{
    class Rewards
    {
        static public void calculatePlayerKillRewards(Player victim, Player killer)
        {
            CfgInfo cfg = victim._server._zoneConfig;

            int killerBounty = 0;
            int killerBountyIncrease = 0;
            int victimBounty = 0;
            int killerCash = 0;
            int killerExp = 0;
            int killerPoints = 0;

            //Fake it to make it
            CS_VehicleDeath update = new CS_VehicleDeath(0, new byte[0], 0, 0);
            update.killedID = victim._id;
            update.killerPlayerID = killer._id;
            update.positionX = victim._state.positionX;
            update.positionY = victim._state.positionY;
            update.type = Helpers.KillType.Player;

            if (killer._team != victim._team)
            {
                killerBounty = Convert.ToInt32(((double)killer.Bounty / 100) * Settings.c_percentOfOwn);
                killerBountyIncrease = Convert.ToInt32(((double)killer.Bounty / 100) * Settings.c_percentOfOwnIncrease);
                victimBounty = Convert.ToInt32(((double)victim.Bounty / 100) * Settings.c_percentOfVictim);

                killerPoints = Convert.ToInt32((Settings.c_baseReward + killerBounty + victimBounty) * Settings.c_pointMultiplier);
                killerCash = Convert.ToInt32((Settings.c_baseReward + killerBounty + victimBounty) * Settings.c_cashMultiplier);
                killerExp = Convert.ToInt32((Settings.c_baseReward + killerBounty + victimBounty) * Settings.c_expMultiplier);

            }
            else
            {
                foreach (Player p in victim._arena.Players)
                    Helpers.Player_RouteKill(p, update, victim, 0, 0, 0, 0);
                return;
            }


            //Inform the killer
            Helpers.Player_RouteKill(killer, update, victim, killerCash, killerPoints, killerPoints, killerExp);

            //Update some statistics
            killer.Cash += killerCash;
            killer.Experience += killerExp;
            killer.KillPoints += killerPoints;
            victim.DeathPoints += killerPoints;

            //Update his bounty
            killer.Bounty += (killerBountyIncrease + victimBounty);

            //Check for players in the share radius
            List<Player> sharedCash = victim._arena.getPlayersInRange(update.positionX, update.positionY, cfg.cash.shareRadius).Where(p => p._baseVehicle._type.Name.Contains("Medic")).ToList();
            List<Player> sharedExp = victim._arena.getPlayersInRange(update.positionX, update.positionY, cfg.experience.shareRadius).Where(p => p._baseVehicle._type.Name.Contains("Medic")).ToList();
            List<Player> sharedPoints = victim._arena.getPlayersInRange(update.positionX, update.positionY, cfg.point.shareRadius).Where(p => p._baseVehicle._type.Name.Contains("Medic")).ToList();
            Dictionary<int, int> cashRewards = new Dictionary<int, int>();
            Dictionary<int, int> expRewards = new Dictionary<int, int>();
            Dictionary<int, int> pointRewards = new Dictionary<int, int>();
            //Set up our shared math
            int CashShare = (int)((((float)killerCash) / 1000) * cfg.cash.sharePercent);
            int ExpShare = (int)((((float)killerExp) / 1000) * cfg.experience.sharePercent);
            int PointsShare = (int)((((float)killerPoints) / 1000) * cfg.point.sharePercent);
            int BtyShare = (int)((killerPoints * (((float)cfg.bounty.percentToAssistBounty) / 1000)));

            foreach (Player p in sharedCash)
            {
                if (p == killer || p._team != killer._team)
                    continue;

                cashRewards[p._id] = CashShare;
                expRewards[p._id] = 0;
                pointRewards[p._id] = 0;
            }

            foreach (Player p in sharedExp)
            {
                if (p == killer || p._team != killer._team)
                    continue;

                expRewards[p._id] = ExpShare;
                if (!cashRewards.ContainsKey(p._id))
                    cashRewards[p._id] = 0;
                if (!pointRewards.ContainsKey(p._id))
                    pointRewards[p._id] = 0;
            }

            foreach (Player p in sharedPoints)
            {
                if (p == killer || p._team != killer._team)
                    continue;

                pointRewards[p._id] = PointsShare;
                if (!cashRewards.ContainsKey(p._id))
                    cashRewards[p._id] = 0;
                if (!expRewards.ContainsKey(p._id))
                    expRewards[p._id] = 0;

                //Share bounty within the experience radius, Dunno if there is a sharebounty radius?
                p.Bounty += BtyShare;
            }

            //Sent reward notices to our lucky witnesses
            List<int> sentTo = new List<int>();
            foreach (Player p in sharedCash)
            {
                if (p == killer || p._team != killer._team)
                    continue;

                Helpers.Player_RouteKill(p, update, victim, cashRewards[p._id], killerPoints, pointRewards[p._id], expRewards[p._id]);
                p.Cash += cashRewards[p._id];
                p.Experience += expRewards[p._id];
                p.AssistPoints += pointRewards[p._id];

                sentTo.Add(p._id);
            }

            foreach (Player p in sharedExp)
            {
                if (p == killer || p._team != killer._team)
                    continue;

                if (!sentTo.Contains(p._id))
                {
                    Helpers.Player_RouteKill(p, update, victim, cashRewards[p._id], killerPoints, pointRewards[p._id], expRewards[p._id]);
                    p.Cash += cashRewards[p._id];
                    p.Experience += expRewards[p._id];
                    p.AssistPoints += pointRewards[p._id];

                    sentTo.Add(p._id);
                }
            }

            foreach (Player p in sharedPoints)
            {
                if (p == killer || p._team != killer._team)
                    continue;

                if (!sentTo.Contains(p._id))
                {	//Update the assist bounty
                    p.Bounty += BtyShare;

                    Helpers.Player_RouteKill(p, update, victim, cashRewards[p._id], killerPoints, pointRewards[p._id], expRewards[p._id]);
                    p.Cash += cashRewards[p._id];
                    p.Experience += expRewards[p._id];
                    p.AssistPoints += pointRewards[p._id];

                    sentTo.Add(p._id);
                }
            }

            //Shared kills anyone?
            Vehicle sharedveh = killer._occupiedVehicle;
            //are we in a vehicle?
            if (sharedveh != null)
            {
                //Was this a child vehicle? If so, re-route us to the parent
                if (sharedveh._parent != null)
                    sharedveh = sharedveh._parent;

                //Can we even share kills?
                if (sharedveh._type.SiblingKillsShared > 0)
                {   //Yep!
                    //Does this vehicle have any childs?
                    if (sharedveh._childs.Count > 0)
                    {
                        //Cycle through each child and reward them
                        foreach (Vehicle child in sharedveh._childs)
                        {
                            //Anyone home?
                            if (child._inhabitant == null)
                                continue;

                            //Can we share?
                            if (child._type.SiblingKillsShared == 0)
                                continue;

                            //Skip our killer
                            if (child._inhabitant == killer)
                                continue;

                            //Give them a kill!
                            child._inhabitant.Kills++;

                            //Show the message
                            child._inhabitant.triggerMessage(2, 500,
                                String.Format("Sibling Assist: Kills=1 (Points={0} Exp={1} Cash={2})",
                                CashShare, ExpShare, PointsShare));
                        }
                    }
                }
            }

            //Route the kill to the rest of the arena
            foreach (Player p in victim._arena.Players.ToList())
            {	//As long as we haven't already declared it, send
                if (p == null)
                    continue;

                if (p == killer)
                    continue;

                if (sentTo.Contains(p._id))
                    continue;

                Helpers.Player_RouteKill(p, update, victim, 0, killerPoints, 0, 0);
            }
        }
    }
}
