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
        protected bool bBoolFreezeActions;

        public void pollForActions(int now)
        {
            if (bBoolFreezeActions)
                return;

            Action.Priority nextPriority;

            nextPriority = shouldfireAtEnemy();
            if (nextPriority > Action.Priority.None)
                newAction(nextPriority, Action.Type.fireAtEnemy);

            nextPriority = shouldHealTeammates();
            if (nextPriority > Action.Priority.None)
                newAction(nextPriority, Action.Type.healTeam);

            nextPriority = shouldFollowTeammate();
            if (nextPriority > Action.Priority.None)
                newAction(nextPriority, Action.Type.followTeam);


        }

        public void newAction(Action.Priority priority, Action.Type type)
        {
            _actionQueue.Add(new Action(priority, type));
        }


        public Action.Priority shouldHealTeammates()
        {
            Action.Priority level = Action.Priority.None;

            IEnumerable<Player> playersInRange = injuredPlayersInRange();

            if (playersInRange.Count() > 0)
                return Action.Priority.Low;

            if (playersInRange.Count() >= 2)
                return Action.Priority.Medium;

            if (playersInRange.Count() >= 3)
                return Action.Priority.High;

            return level;
        }

        public Action.Priority shouldFollowTeammate()
        {
            Action.Priority level = Action.Priority.None;

            IEnumerable<Player> playersInRange = friendliesInRange();
            int injPlayersInRange = injuredPlayersInRange().Count();
            List<Player> currentTargets = getCurrentFriendlyTargets();
            int count = 0;
            foreach (Player p in playersInRange)
            {
                if (currentTargets.Contains(p))
                    continue;

                count++;
            }

            if (injPlayersInRange == 0 && count > 0)
                return Action.Priority.High;

            if (count < injPlayersInRange)
                return Action.Priority.None;

            if (count > injPlayersInRange && (count - injPlayersInRange) >= 1)
                return Action.Priority.Low;

            if (count > injPlayersInRange && (count - injPlayersInRange) >= 2)
                return Action.Priority.Medium;

            if (count > injPlayersInRange && (count - injPlayersInRange) >= 3)
                return Action.Priority.High;



            return level;
        }

        public Action.Priority shouldfireAtEnemy()
        {
            IEnumerable<Player> enemies = enemiesInRange();
            IEnumerable<Player> friendlies = friendliesInRange();

            int enemycount = 0;
            int friendlycount = 0;
            foreach (Player enemy in enemies)
            {
                if (Helpers.isInRange(c_playerRangeFire, _state, enemy._state))
                    enemycount++;
            }

            foreach (Player friendly in friendlies)
            {
                if (Helpers.isInRange(c_playerRangeSafety, _state, friendly._state))
                    friendlycount++;
            }

            friendlycount += getFriendlyBotsInRange().Count();
            enemycount += getEnemyBotsInRange().Count();

            //Meh
            if (enemycount > friendlycount && (enemycount - friendlycount) > 0)
                return Action.Priority.Low;

            //Now we're talking
            if (enemycount > friendlycount && (enemycount - friendlycount) >= 2)
                return Action.Priority.Medium;

            //Smoke em if ya got em
            if (enemycount > friendlycount && (enemycount - friendlycount) >= 3)
                return Action.Priority.High;

            return Action.Priority.None;
        }
    }
}
