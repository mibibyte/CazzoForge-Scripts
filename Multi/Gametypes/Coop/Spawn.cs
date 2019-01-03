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
        public void spawnMedic(Team team)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_team, true);

            if (!newBot(team, BotType.Medic, null, null, warpPoint))
                Log.write(TLog.Warning, "Unable to spawn medic bot");

        }

        public void spawnEliteHeavy(Team team)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_team, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            Random rand = new Random();
            short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);

            if (!newBot(team, BotType.EliteHeavy, null, null, openPoint))
                Log.write(TLog.Warning, "Unable to spawn bot");

        }
        public void spawnEliteHeavy(Player player, Team team)
        {
            if (!newBot(team, BotType.EliteHeavy, null, null, player._state))
                Log.write(TLog.Warning, "Unable to spawn bot");
        }

        public void spawnEliteMarine(Player player, Team team)
        {
            if (!newBot(team, BotType.EliteMarine, null, null, player._state))
                Log.write(TLog.Warning, "Unable to spawn bot");
        }

        public void spawnEliteMarine(Team team)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_team, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            Random rand = new Random();

            short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);

            if (!newBot(team, BotType.EliteMarine, null, null, openPoint))
                Log.write(TLog.Warning, "Unable to spawn bot");

        }

        public void spawnDropShip(Team team)
        {
            Helpers.ObjectState warpPoint = new Helpers.ObjectState();
            warpPoint.positionX = 400;
            warpPoint.positionY = 1744;
            warpPoint.positionZ = 299;

            if (!newBot(team, BotType.Dropship, null, null, warpPoint))
                Log.write(TLog.Warning, "Unable to spawn medic bot");

        }

        public void spawnRandomWave(Team team, int count)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            Random rand = new Random();

            for (int i = 0; i < count; i++)
            {
                if (_bots.Count >= _botMax)
                    break;

                BotType type = BotType.Marine;
                bool bRipper = (rand.Next(0, 100) <= 35);

                if (bRipper)
                    type = BotType.Ripper;


                short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
                warpPoint = _baseScript.findFlagWarp(_botTeam, true);
                openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);


                if (!newBot(team, type, null, null, openPoint))
                    Log.write(TLog.Warning, "Unable to spawn bot");
            }

            _arena.triggerMessage(0, 500, "Enemy reinforcements have arrived!");
        }

        public void spawnMedicWave(Team team, int count)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            Random rand = new Random();

            for (int i = 0; i < count; i++)
            {
                short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
                warpPoint = _baseScript.findFlagWarp(team, true);
                openPoint = _baseScript.findOpenWarp(team, _arena, randomoffset, warpPoint.positionY, 1200);


                if (!newBot(team, BotType.Medic, null, null, openPoint))
                    Log.write(TLog.Warning, "Unable to spawn medic bot");
            }
        }

        public void spawnMarineWave(Team team, int count)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();
            Random rand = new Random();

            for (int i = 0; i < count; i++)
            {
                short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
                warpPoint = _baseScript.findFlagWarp(_botTeam, true);
                openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);

                if (!newBot(team, BotType.Marine, null, null, openPoint))
                    Log.write(TLog.Warning, "Unable to spawn marine bot");
            }

            _arena.triggerMessage(0, 500, "Enemy reinforcements have arrived!");
        }


        public void spawnRipperWave(Team team, int count)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();
            Random rand = new Random();

            for (int i = 0; i < count; i++)
            {
                short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
                warpPoint = _baseScript.findFlagWarp(_botTeam, true);
                openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);

                if (!newBot(team, BotType.Ripper, null, null, openPoint))
                    Log.write(TLog.Warning, "Unable to spawn ripper bot");
            }

            _arena.triggerMessage(0, 500, "Enemy reinforcements have arrived!");
        }

        public void spawnGunship(Team team, Vehicle marker, Player owner)
        {
            Random rand = new Random();

            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);

            short randomoffset = (short)(warpPoint.positionX - rand.Next(0, 600));
            warpPoint.positionZ = 199;
            warpPoint.positionX = randomoffset;

            if (!newBot(team, BotType.Gunship, marker, owner, warpPoint))
                Log.write(TLog.Warning, "Unable to spawn bot");
        }


        public void spawnExoLight(Team team)
        {
            Random rand = new Random();

            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(team, true);

            short randomoffset = (short)(warpPoint.positionX - rand.Next(0, 600));
            warpPoint.positionX = randomoffset;

            if (!newBot(team, BotType.ExoLight, null, null, warpPoint))
                Log.write(TLog.Warning, "Unable to spawn bot");
        }

        public void spawnExoHeavy(Team team)
        {
            Random rand = new Random();

            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(team, true);

            short randomoffset = (short)(warpPoint.positionX - rand.Next(0, 600));
            warpPoint.positionX = randomoffset;

            if (!newBot(team, BotType.ExoHeavy, null, null, warpPoint))
                Log.write(TLog.Warning, "Unable to spawn bot");
        }

        public void checkForWaves(int now, int flagcount)
        {
            switch (flagcount)
            {
                case 10:
                    {
                        if (!_firstRushWave)
                        {
                            _firstRushWave = true;

                            _arena.sendArenaMessage("!The enemy has sent extra reinforcements in a last ditch attempt to stop you!", 4);
                            int playercount = _team.ActivePlayerCount;
                            int max = playercount * 1;
                            spawnRandomWave(_botTeam, max);
                            spawnExoLight(_botTeam);
                        }
                    }
                    break;
                case 19:
                    {
                        if (!_firstBoss)
                        {
                            _firstBoss = true;

                            _arena.sendArenaMessage("!The enemy has sent some enhanced Soldiers, Look out!", 4);
                            spawnEliteHeavy(_botTeam);
                            spawnEliteMarine(_botTeam);
                            int playercount = _team.ActivePlayerCount;
                            int max = Convert.ToInt32(playercount * 1.25);
                            spawnRandomWave(_botTeam, max);
                            spawnExoHeavy(_botTeam);
                        }
                    }
                    break;
                case 27:
                    {
                        if (!_secondRushWave)
                        {
                            _secondRushWave = true;

                            _arena.sendArenaMessage("!The enemy has sent extra reinforcements in a last ditch attempt to stop you!", 4);
                            int playercount = _team.ActivePlayerCount;
                            int max = Convert.ToInt32(playercount * 1.50);
                            spawnRandomWave(_botTeam, max);
                            spawnEliteMarine(_botTeam);
                        }
                    }
                    break;
                case 32:
                    {
                        if (!_secondBoss)
                        {
                            _secondBoss = true;

                            _arena.sendArenaMessage("!The enemy has sent some enhanced Soldiers, Look out!", 4);
                            spawnEliteHeavy(_botTeam);
                            spawnEliteMarine(_botTeam);
                            int playercount = _team.ActivePlayerCount;
                            int max = Convert.ToInt32(playercount * 1.75);
                            spawnRandomWave(_botTeam, max);
                            spawnExoLight(_botTeam);
                        }
                    }
                    break;
                case 35:
                    {
                        if (!_thirdRushWave)
                        {
                            _thirdRushWave = true;

                            _arena.sendArenaMessage("!The enemy has sent extra reinforcements in a last ditch attempt to stop you!", 4);
                            int playercount = _team.ActivePlayerCount;
                            int max = Convert.ToInt32(playercount * 3);
                            spawnRandomWave(_botTeam, max);
                            spawnEliteHeavy(_botTeam);
                            spawnEliteMarine(_botTeam);
                            spawnExoHeavy(_botTeam);
                        }
                    }
                    break;
            }
        }
    }
}