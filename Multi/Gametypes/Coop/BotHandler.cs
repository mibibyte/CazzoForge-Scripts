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
        private int _lastMedicWave;
        private int _lastMarineWave;
        private int _lastFriendlyMedic;
        private int _marineSpawnDelay = 25000;
        private int _medicSpawnDelay = 30000;
        public List<Bot> _bots;
        public bool spawnBots;

        public void pollBots(int now)
        {
            if (spawnBots && _arena._bGameRunning)
            {

                //Does team1 need a medic?
                if (now - _lastFriendlyMedic > _medicSpawnDelay)
                {
                    int botcount = _bots.Where(b => b._type.Id == 128 && b._team == _team).Count();

                    if (botcount < 2)
                    {
                        _lastFriendlyMedic = now;
                        spawnMedic(_team);
                    }
                }

                //Does team1 need a medic?
                if (now - _lastMedicWave > _medicSpawnDelay)
                {
                    int botcount = _bots.Where(b => b._type.Id == 301 && b._team == _botTeam).Count();
                    int playercount = _team.ActivePlayerCount;
                    int max = playercount * 1;

                    if (botcount < max)
                    {
                        int add = (max - botcount);
                        _lastMedicWave = now;
                        spawnMedicWave(_botTeam, add);
                    }
                }
              
                //Does team1 need a marine?
                if (now - _lastMarineWave > _marineSpawnDelay)
                {
                    int botcount = _bots.Where(b => b._type.Id == 131 && b._team == _botTeam).Count();
                    int playercount = _team.ActivePlayerCount;
                    int max = playercount * 2;

                    if (botcount < max)
                    {
                        int add = (max - botcount);
                        _lastMarineWave = now;
                        spawnMarineWave(_botTeam, add);
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
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_team, true);

            if (!newBot(team, BotType.Medic, warpPoint))
                Log.write(TLog.Warning, "Unable to spawn medic bot");

        }

        public void spawnMedicWave(Team team, int count)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            for (int i = 0; i < count; i++)
            {
                warpPoint = _baseScript.findFlagWarp(_botTeam, true);
                openPoint = _baseScript.findOpenWarp(_botTeam, _arena, warpPoint.positionX, warpPoint.positionY, 800);


                if (!newBot(team, BotType.Medic, openPoint))
                    Log.write(TLog.Warning, "Unable to spawn medic bot");
            }

            _arena.triggerMessage(0, 500, "Enemy reinforcements have arrived!");
        }

        public void spawnMarineWave(Team team, int count)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            for (int i = 0; i < count; i++)
            {
                warpPoint = _baseScript.findFlagWarp(_botTeam, true);
                openPoint = _baseScript.findOpenWarp(_botTeam, _arena, warpPoint.positionX, warpPoint.positionY, 800);

                if (!newBot(team, BotType.Marine, openPoint))
                    Log.write(TLog.Warning, "Unable to spawn marine bot");
            }

            _arena.triggerMessage(0, 500, "Enemy reinforcements have arrived!");
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

                        Medic medic = _arena.newBot(typeof(Medic), vehid, team, null, state, null) as Medic;

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

                        Marine marine = _arena.newBot(typeof(Marine), vehid, team, null, state, null) as Marine;

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
            return true;
        }
    }
}
