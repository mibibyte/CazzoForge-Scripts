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

        private List<Action> _actionQueue;

        public void fireAtEnemy(int now)
        {
            //Get the closest player
            bool bClearPath = false;
            _target = getTargetPlayer(ref bClearPath, _targetTeam);


            if (_target != null)
            {
                if (bClearPath)
                {   //What is our distance to the target?
                    double distance = (_state.position() - _target._state.position()).Length;
                    bool bFleeing = false;

                    //Too far?
                    if (distance > farDist)
                        steering.steerDelegate = steerForPersuePlayer;

                    //Too short?
                    else if (distance < runDist)
                    {
                        bFleeing = true;
                        steering.steerDelegate = delegate (InfantryVehicle vehicle)
                        {
                            if (_target != null)
                                return vehicle.SteerForFlee(_target._state.position());
                            else
                                return Vector3.Zero;
                        };
                    }
                    //Just right
                    else
                        steering.steerDelegate = null;
                        

                    //Can we shoot?
                    if (!bFleeing && _weapon.ableToFire() && distance < farDist)
                    {

                        int aimResult = _weapon.getAimAngle(_target._state);

                        if (_weapon.isAimed(aimResult))
                        {   //Spot on! Fire?
                            _itemUseID = _weapon.ItemID;
                            _weapon.shotFired();
                        }

                        steering.bSkipAim = true;
                        steering.angle = aimResult;
                    }
                    else
                        steering.bSkipAim = false;
                        
                }
                else
                {
                    updatePath(now);

                    //Navigate to him
                    if (_path == null)
                        //If we can't find out way to him, just mindlessly walk in his direction for now
                        steering.steerDelegate = steerForPersuePlayer;
                    else
                        steering.steerDelegate = steerAlongPath;
                }


            }
        }


        public void healTeamates(int now)
        {
            //Get the closest player
            bool bClearPath = false;
            _target = getInjuredTeammate(ref bClearPath);

            if (_target != null)
            {
                if (bClearPath)
                {   //What is our distance to the target?
                    double distance = (_state.position() - _target._state.position()).Length;

                    //Too far?
                    if (distance > healfarDist)
                        steering.steerDelegate = steerForPersuePlayer;
                    //Just right
                    else
                        steering.steerDelegate = null;

                    //Can we shoot?
                    if (distance < healfarDist && _energy >= 120)
                        fireMedkit(now);
                    else
                        steering.steerDelegate = steerForPersuePlayer;
                }
                else
                {
                    updatePath(now);

                    //Navigate to him
                    if (_path == null)
                        //If we can't find out way to him, just mindlessly walk in his direction for now
                        steering.steerDelegate = steerForPersuePlayer;
                    else
                        steering.steerDelegate = steerAlongPath;
                }
            }
        }

        public void followTeamMate(int now)
        {
            //Get the closest player
            bool bClearPath = false;
            _target = getNearestTeammate(ref bClearPath, _team);

            if (_target != null)
            {
                if (bClearPath)
                {   //What is our distance to the target?
                    double distance = (_state.position() - _target._state.position()).Length;

                    //Too far?
                    if (distance > healfarDist)
                        steering.steerDelegate = steerForPersuePlayer;
                    //Just right
                    else
                        steering.steerDelegate = null;

                    //Are we there yet?
                    if (distance < healfarDist)
                    {
                        steering.bSkipRotate = false;
                    }
                    else
                        steering.steerDelegate = steerForPersuePlayer;

                }
                else
                {
                    updatePath(now);

                    //Navigate to him
                    if (_path == null)
                        //If we can't find out way to him, just mindlessly walk in his direction for now
                        steering.steerDelegate = steerForPersuePlayer;
                    else
                        steering.steerDelegate = steerAlongPath;
                }
            }
            else
            {
            }
        }

        public void patrolBetweenFlags(int now)
        {
            Arena.FlagState friendlyflag;
            Arena.FlagState enemyFlag;
            if (_team._name == "Titan Militia")
            {
                friendlyflag = _arena._flags.Values.OrderBy(f => f.posX).Where(f => f.team == _team).Last();
                enemyFlag = _arena._flags.Values.OrderBy(f => f.posX).Where(f => f.team != _team).First();
            }
            else
            {
                friendlyflag = _arena._flags.Values.OrderBy(f => f.posX).Where(f => f.team == _team).First();
                enemyFlag = _arena._flags.Values.OrderBy(f => f.posX).Where(f => f.team != _team).Last();
            }
            Helpers.ObjectState target = new Helpers.ObjectState();
            if (_bPatrolEnemy)
            {
                target.positionX = enemyFlag.posX;
                target.positionY = enemyFlag.posY;
            }
            else
            {
                target.positionX = friendlyflag.posX;
                target.positionY = friendlyflag.posY;
            }


            //What is our distance to the target?
            double distance = (_state.position() - target.position()).Length;

            //Are we there yet?
            if (distance < patrolDist)
            {
                //change our direction
                _bPatrolEnemy = !_bPatrolEnemy;
            }

            //Does our path need to be updated?
            if (now - _tickLastPath > c_pathUpdateInterval)
            {
                _arena._pathfinder.queueRequest(
                           (short)(_state.positionX / 16), (short)(_state.positionY / 16),
                           (short)(target.positionX / 16), (short)(target.positionY / 16),
                           delegate (List<Vector3> path, int pathLength)
                           {
                               if (path != null)
                               {   //Is the path too long?
                                   if (pathLength > c_MaxPath)
                                   {   //Destroy ourself and let another zombie take our place
                                       _path = null;
                                       destroy(true);
                                   }
                                   else
                                   {
                                       _path = path;
                                       _pathTarget = 1;
                                   }
                               }

                               _tickLastPath = now;
                           }
                );
            }

            //Navigate to him
            if (_path == null)
                //If we can't find out way to him, just mindlessly walk in his direction for now
                steering.steerDelegate = steerForPersuePlayer;
            else
                steering.steerDelegate = steerAlongPath;
        }


        public class Action
        {
            public Priority priority;
            public Type type;

            public Action(Priority pr, Type ty)
            {
                type = ty;
                priority = pr;
            }

            public enum Priority
            {
                None,
                Low,
                Medium,
                High
            }

            public enum Type
            {
                fireAtEnemy,
                healTeam,
                healSelf,
                followTeam
            }
        }
    }
}
