using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InfServer.Logic;
using InfServer.Game;
using InfServer.Scripting;
using InfServer.Bots;
using InfServer.Protocol;

using Assets;

namespace InfServer.Script.GameType_Multi
{
    public class CapturePoint
    {
        public string name;
        public short posX;
        public short posY;
        public int height;
        public int width;
        public Arena.FlagState _flag;
        public bool active;
        public bool _isBeingCaptured;
        private Arena _arena;
        private Conquest _cq;
        private int tickLastWave;
        private int tickLastPointer;

        private List<Player> players;

        //Settings
        private int _flagCaptureRadius = 600;
        private int _flagCaptureTime = 5;


        public CapturePoint(Arena arena, Conquest cq, Arena.FlagState flag)
        {
            _flag = flag;
            _cq = cq;
            _arena = arena;
            players = new List<Player>();
            tickLastWave = 0;

            posX = _flag.posX;
            posY = _flag.posY;

            name = Helpers.posToLetterCoord(posX, posY);
        }

        public void poll(int now)
        {
            List<Player> playersInArea = new List<Player>();
            int attackers = 0;
            int defenders = 0;

            Arena.FlagState flag = _cq._flags.FirstOrDefault(f => f == _flag);

            playersInArea = _arena.getPlayersInRange(posX, posY, _flagCaptureRadius).Where(p => !p.IsDead).ToList();

            Team attacker = _arena.ActiveTeams.FirstOrDefault(t => t != flag.team);

            attackers = playersInArea.Count(p => p._team != flag.team);
            defenders = playersInArea.Count(p => p._team == flag.team);

            if (now - tickLastPointer >= 2000)
            {

                Helpers.ObjectState state = new Helpers.ObjectState();
                Helpers.ObjectState target = new Helpers.ObjectState();
                state.positionX = posX;
                state.positionY = posY;
                int index = _cq._flags.IndexOf(flag);

                if (flag.team == _cq.cqTeam1)
                {
                    if (index + 1 < _cq._flags.Count)
                    {
                        target.positionX = _cq._flags[index + 1].posX;
                        target.positionY = _cq._flags[index + 1].posY;
                    }
                }

                if (flag.team == _cq.cqTeam2)
                {
                    if (index - 1 >= 0)
                    {
                        target.positionX = _cq._flags[index - 1].posX;
                        target.positionY = _cq._flags[index - 1].posY;
                    }
                }
                if (flag.team != null)
                {
                    byte fireAngle = Helpers.computeLeadFireAngle(state, target, 6500 / 1000);
                    Helpers.Player_RouteExplosion(flag.team.ActivePlayers, 1128, posX, posY, 0, fireAngle, 0);
                }
                tickLastPointer = now;
            }

            if (attackers == 0 && defenders == 0)
            {
            }

            if (attackers > defenders)
            {
                if (now - tickLastWave >= 2500)
                {


                    Helpers.Player_RouteExplosion(_arena.Players, 3059, posX, posY, 0, 0, 0);
                    tickLastWave = now;
                    _arena.triggerMessage(0, 500, String.Format("{0} has taken control of the {1} capture point...", attacker._name, name));
                    _cq._flags.FirstOrDefault(f => f == flag).team = attacker;
                }
            }
            if (defenders > attackers)
            {
                if (now - tickLastWave >= 2500)
                {
                    //Helpers.Player_RouteExplosion(_arena.Players, 3059, posX, posY, 0, 0, 0);
                    tickLastWave = now;
                }
            }
            else
            {
                if (attackers == defenders && attackers > 0 && defenders > 0)
                {
                    if (now - tickLastWave >= 1500)
                    {
                        Helpers.Player_RouteExplosion(_arena.Players, 3060, posX, posY, 0, 0, 0);
                        tickLastWave = now;
                    }
                }
            }
        }
    }

}
