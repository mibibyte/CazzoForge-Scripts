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
    public partial class Coop
    {
        private int _lastMedicSpawnTeam;
        private int _lastMarineSpawnTeam;
        private int _marineSpawnDelay = 20000;
        private int _medicSpawnDelay = 30000;
        public List<Bot> _bots;
        public bool spawnBots;

        public void pollBots(int now)
        {
            if (spawnBots)
            {
                //Does team1 need a medic?
                if (now - _lastMedicSpawnTeam > _medicSpawnDelay)
                {
                    int botcount = _bots.Where(b => b._type.Id == 128 && b._team == _botTeam).Count();

                    if (botcount < 2)
                    {
                        _lastMedicSpawnTeam = now;
                        spawnMedic(_botTeam);
                    }
                }
              
                //Does team1 need a marine?
                if (now - _lastMarineSpawnTeam > _marineSpawnDelay)
                {
                    int botcount = _bots.Where(b => b._type.Id == 133 && b._team == _botTeam).Count();

                    if (botcount < 1)
                    {
                        _lastMarineSpawnTeam = now;
                        spawnMarine(_botTeam);
                    }
                }
            }

            foreach (Bot bot in _bots)
            {


                switch (bot._type.Name)
                {
                    case ("Titan Medic (B)"):
                        {
                            IEnumerable<Player> enemies = _arena.Players.Where(p => p._team != bot._team);
                            if (!bot.IsDead)
                            {
                                Helpers.Player_RouteExplosion(bot._team.ActivePlayers, 1131, bot._state.positionX, bot._state.positionY, 0, 0, 0);
                                Helpers.Player_RouteExplosion(enemies, 1130, bot._state.positionX, bot._state.positionY, 0, 0, 0);
                            }
                        }
                        break;

                    case ("Collective Medic (B)"):
                        {
                            IEnumerable<Player> enemies = _arena.Players.Where(p => p._team != bot._team);
                       
                            if (!bot.IsDead)
                            {
                                Helpers.Player_RouteExplosion(bot._team.ActivePlayers, 1131, bot._state.positionX, bot._state.positionY, 0, 0, 0);
                                Helpers.Player_RouteExplosion(enemies, 1130, bot._state.positionX, bot._state.positionY, 0, 0, 0);
                            }
                        }
                        break;

                    case ("Collective Marine (B)"):
                        {

                            IEnumerable<Player> enemies = _arena.Players.Where(p => p._team != bot._team);

                            if (!bot.IsDead)
                            {
                                Helpers.Player_RouteExplosion(bot._team.ActivePlayers, 1131, bot._state.positionX, bot._state.positionY, 0, 0, 0);
                                Helpers.Player_RouteExplosion(enemies, 1130, bot._state.positionX, bot._state.positionY, 0, 0, 0);
                            }
                        }
                        break;

                    case ("Titan Marine (B)"):
                        { 

                            IEnumerable<Player> enemies = _arena.Players.Where(p => p._team != bot._team);

                            if (!bot.IsDead)
                            {
                                Helpers.Player_RouteExplosion(bot._team.ActivePlayers, 1131, bot._state.positionX, bot._state.positionY, 0, 0, 0);
                                Helpers.Player_RouteExplosion(enemies, 1130, bot._state.positionX, bot._state.positionY, 0, 0, 0);
                            }
                        }
                        break;
                }

            }
        }

        public void spawnMedic(Team team)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(team);

            if (!newBot(team, BotType.Medic, warpPoint))
                Log.write(TLog.Warning, "Unable to spawn medic bot");

        }

        public void spawnMarine(Team team)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(team);

            if (!newBot(team, BotType.Marine, warpPoint))
                Log.write(TLog.Warning, "Unable to spawn marine bot");

        }

        private bool newBot(Team team, BotType type, Helpers.ObjectState state = null)
        {
            if (_bots == null)
                _bots = new List<Bot>();

            switch (type)
            {
                case BotType.Medic:
                    {
                        //Collective vehicle
                        ushort vehid = 301;

                        //Titan vehicle?
                        if (team._name == "Titan Militia")
                            vehid = 128;

                        Medic medic = _arena.newBot(typeof(Medic), vehid, team, null, state) as Medic;

                        if (medic == null)
                            return false;

                        medic._team = team;
                        medic.type = BotType.Medic;
                        medic.init();

                        medic.Destroyed += delegate (Vehicle bot)
                        {
                            _bots.Remove((Bot)bot);

                        };

                        _bots.Add(medic);
                    }
                    break;

                case BotType.Marine:
                    {
                        //Collective vehicle
                        ushort vehid = 131;

                        //Titan vehicle?
                        if (team._name == "Titan Militia")
                            vehid = 133;

                        Marine marine = _arena.newBot(typeof(Marine), vehid, team, null, state) as Marine;

                        if (marine == null)
                            return false;

                        marine._team = team;
                        marine.type = BotType.Marine;
                        marine.init();

                        marine.Destroyed += delegate (Vehicle bot)
                        {
                            _bots.Remove((Bot)bot);

                        };

                        _bots.Add(marine);
                    }
                    break;


            }
            _arena.triggerMessage(0, 500, "Enemy reinforcements have arrived!");
            return true;
        }
    }
}
