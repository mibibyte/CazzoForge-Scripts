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
    public static class TeamHelpers
    { 

        public static void scrambleTeams(this Arena arena, IEnumerable<Player> unorderedPlayers, List<Team> teams, int maxPerTeam)
        {
            Random _rand = new Random();
            List<Player> players = unorderedPlayers.OrderBy(plyr => _rand.Next(0, 500)).ToList();

            //gets the minimum number of teams we need to fit our players
            int numTeams = players.Count / maxPerTeam + (players.Count % maxPerTeam == 0 ? 0 : 1);

            //adds our players to these teams in team-order
            for (int i = 0; i < players.Count; i++)
            {
                Player p = players[i];
                teams[i % numTeams].addPlayer(p);
            }
        }

        public static bool inArea(this Player player, int xMin, int yMin, int xMax, int yMax)
        {
            Helpers.ObjectState state = player.getState();
            int px = state.positionX;
            int py = state.positionY;
            return (xMin <= px && px <= xMax && yMin <= py && py <= yMax);
        }

        public static void resetSkills(this Player player)
        {
                for (int i = 0; i < 100; i++) // we gotta remove any class skills that already got
                {
                    if (player.findSkill(i) != null)
                        player._skills.Remove(i);
                }
        }
    }
}
