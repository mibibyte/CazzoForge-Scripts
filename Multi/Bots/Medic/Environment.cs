using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;


using InfServer.Game;
using InfServer.Protocol;
using InfServer.Scripting;
using InfServer.Bots;

using Assets;
using Axiom.Math;
using Bnoerj.AI.Steering;

namespace InfServer.Script.GameType_Multi
{   // Script Class
    /// Provides the interface between the script and bot
    ///////////////////////////////////////////////////////
    public partial class Medic : Bot
    {
        /// <summary>
        /// Checks to see if we're too far from the team
        /// </summary>
        protected bool checkTeamDistance()
        {
            if (_team == null)
                return false;

            //sanity check
            if (_team.ActivePlayerCount == 0)
                return true;

            int minDist = int.MaxValue;

            //finds minimum distance from any one of the activeplayers
            foreach (Player player in _team.ActivePlayers)
            {
                //uses the max{dx,dy} metric for distance estimate
                int dist = Math.Max(Math.Abs(_state.positionX - player._state.positionX), Math.Abs(_state.positionY - player._state.positionY));

                if (dist < minDist)
                    minDist = dist;
            }

            return minDist < c_MaxRespawnDist + c_DistanceLeeway;
        }

        protected IEnumerable<Player> injuredPlayersInRange()
        {
            IEnumerable<Player> players = _arena.getPlayersInRange(_state.positionX, _state.positionY, c_playerMaxRangeHeals)
                .Where(p => p._team == _team && !p.IsDead && ((int)(p._state.health / p.ActiveVehicle._type.Hitpoints) * 100) <= c_lowHealth)
                .OrderBy(p => p._state.health);


            return players;
        }

        protected IEnumerable<Player> friendliesInRange()
        {
            IEnumerable<Player> players = _arena.getPlayersInRange(_state.positionX, _state.positionY, c_playerMaxRangeHeals)
                .Where(p => p._team == _team && !p.IsDead && p._state.health >= c_lowHealth)
                .OrderBy(p => p._state.health);

            return players;
        }

        protected IEnumerable<Player> enemiesInRange()
        {
            IEnumerable<Player> sorted = _arena.getPlayersInRange(_state.positionX, _state.positionY, c_playerMaxRangeEnemies)
                .Where(p => p._team != _team && !p.IsDead);

            return sorted;
        }


        protected Player getInjuredTeammate(ref bool bInSight)
        {
            Player target = null;
            double lastDist = double.MaxValue;
            bInSight = false;

            IEnumerable<Player> sorted = _team.ActivePlayers.Where(p => !p.IsDead)
                .OrderBy(p => p._state.health);

            foreach (Player p in sorted)
            {   //Find the closest player
                if (_arena.getTerrain(p._state.positionX, p._state.positionY).safety)
                    continue;

                int healthPercentage = ((p._state.health / p.ActiveVehicle._type.Hitpoints) * 100);


                //If they're over 75 percent, ignore them. Prioritize!
                if (healthPercentage >= 75)
                    continue;

                double dist = Helpers.distanceSquaredTo(_state, p._state);
                bool bClearPath = Helpers.calcBresenhemsPredicate(_arena, _state.positionX, _state.positionY, p._state.positionX, p._state.positionY,
                    delegate (LvlInfo.Tile t)
                    {
                        return !t.Blocked;
                    }
                );

                if ((!bInSight || (bInSight && bClearPath)) && lastDist > dist)
                {
                    bInSight = bClearPath;
                    lastDist = dist;
                    target = p;
                }
            }


            return target;

        }

        protected List<Player> getCurrentFriendlyTargets()
        {
            List<Vehicle> medics = _arena.getVehiclesInRange(_state.positionX, _state.positionY, 1000,
                                        delegate (Vehicle v)
                                        { return (v is Medic); }).Where(b => b._team == _team).ToList();


            //Is this player currently being targeted?
            List<Player> currentTargets = new List<Player>();

            foreach (Medic medic in medics)
            {
                if (medic._team == _team)
                {
                    if (medic._target != null)
                        currentTargets.Add(medic._target);
                }
            }

            return currentTargets;
        }

        protected IEnumerable<Vehicle> getFriendlyBotsInRange()
        {
            List<Vehicle> bots = _arena.getVehiclesInRange(_state.positionX, _state.positionY, 1000,
                                        delegate (Vehicle v)
                                        { return (v is Bot); }).Where(b => b._team == _team).ToList();
            return bots;
        }

        protected IEnumerable<Vehicle> getEnemyBotsInRange()
        {
            List<Vehicle> bots = _arena.getVehiclesInRange(_state.positionX, _state.positionY, 1000,
                                        delegate (Vehicle v)
                                        { return (v is Bot); }).Where(b => b._team != _team).ToList();
            return bots;
        }


        /// <summary>
        /// Obtains a suitable target player
        /// </summary>
        protected Player getNearestTeammate(ref bool bInSight, Team targetTeam)
        {
            //Look at the players on the target team
            if (targetTeam == null)
                return null;

            Player target = null;
            double lastDist = double.MaxValue;
            bInSight = false;

            IEnumerable<Player> playersInRange = friendliesInRange();
            List<Player> currentTargets = getCurrentFriendlyTargets();

            foreach (Player p in targetTeam.ActivePlayers.ToList())
            {   //Find the closest player
                if (p.IsDead)
                    continue;

                if (_arena.getTerrain(p._state.positionX, p._state.positionY).safety)
                    continue;
                
                //Ignore them if they're over 80hp
                if (p._state.health >= c_lowHealth)
                    continue;

                if (currentTargets.Contains(p))
                    continue;

                double dist = Helpers.distanceSquaredTo(_state, p._state);
                bool bClearPath = Helpers.calcBresenhemsPredicate(_arena, _state.positionX, _state.positionY, p._state.positionX, p._state.positionY,
                    delegate (LvlInfo.Tile t)
                    {
                        return !t.Blocked;
                    }
                );

                if ((!bInSight || (bInSight && bClearPath)) && lastDist > dist)
                {
                    bInSight = bClearPath;
                    lastDist = dist;
                    target = p;
                }
            }
            //Still no target? Let's loop again but with different circumstances
            if (target == null)
            {
                foreach (Player p in playersInRange)
                {
                    if (p.IsDead)
                        continue;

                    if (_arena.getTerrain(p._state.positionX, p._state.positionY).safety)
                        continue;

                    if (currentTargets.Contains(p))
                        continue;

                    double dist = Helpers.distanceSquaredTo(_state, p._state);
                    bool bClearPath = Helpers.calcBresenhemsPredicate(_arena, _state.positionX, _state.positionY, p._state.positionX, p._state.positionY,
                        delegate (LvlInfo.Tile t)
                        {
                            return !t.Blocked;
                        }
                    );

                    if ((!bInSight || (bInSight && bClearPath)) && lastDist > dist)
                    {
                        bInSight = bClearPath;
                        lastDist = dist;
                        target = p;
                    }
                }
            }


            return target;
        }


        /// <summary>
        /// Obtains a suitable target player
        /// </summary>
        protected Player getTargetPlayer(ref bool bInSight, Team targetTeam)
        {
            //Look at the players on the target team
            if (targetTeam == null)
                return null;

            Player target = null;
            double lastDist = double.MaxValue;
            bInSight = false;

            foreach (Player p in targetTeam.ActivePlayers.ToList())
            {   //Find the closest player
                if (p.IsDead)
                    continue;

                if (_arena.getTerrain(p._state.positionX, p._state.positionY).safety)
                    continue;

                double dist = Helpers.distanceSquaredTo(_state, p._state);
                bool bClearPath = Helpers.calcBresenhemsPredicate(_arena, _state.positionX, _state.positionY, p._state.positionX, p._state.positionY,
                    delegate (LvlInfo.Tile t)
                    {
                        return !t.Blocked;
                    }
                );

                if ((!bInSight || (bInSight && bClearPath)) && lastDist > dist)
                {
                    bInSight = bClearPath;
                    lastDist = dist;
                    target = p;
                }
            }


            return target;
        }
    }
}
