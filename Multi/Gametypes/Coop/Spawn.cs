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
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            /*
            Random rand = new Random();
            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);
            */

            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, warpPoint.positionX, warpPoint.positionY, 200);

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
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            /*
            Random rand = new Random();
            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);
            */

            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, warpPoint.positionX, warpPoint.positionY, 200);

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

            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();

            /*
            Random rand = new Random();
            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);
            */

            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, warpPoint.positionX, warpPoint.positionY, 400);
            
            /*
            Random rand = new Random();
            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            short randomoffsetX = (short)(warpPoint.positionX + rand.Next(0, 100));
            short randomoffsetY = (short)(warpPoint.positionX + rand.Next(0, 100));
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffsetX, randomoffsetY, 200);
            */
            if (!newBot(team, BotType.ExoLight, null, null, openPoint))
                Log.write(TLog.Warning, "Unable to spawn bot");
        }

        public void spawnExoHeavy(Team team)
        {
            Helpers.ObjectState warpPoint = _baseScript.findFlagWarp(_botTeam, true);
            Helpers.ObjectState openPoint = new Helpers.ObjectState();
            /*
            Random rand = new Random();
            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            short randomoffset = (short)(warpPoint.positionX + rand.Next(0, 800));
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, randomoffset, warpPoint.positionY, 1200);
            */

            warpPoint = _baseScript.findFlagWarp(_botTeam, false);
            openPoint = _baseScript.findOpenWarp(_botTeam, _arena, warpPoint.positionX, warpPoint.positionY, 400);

            if (!newBot(team, BotType.ExoHeavy, null, null, openPoint))
                Log.write(TLog.Warning, "Unable to spawn bot");
        }

        public void checkForWaves(int now, int flagcount)
        {
            switch (flagcount)
            {
                case 3:
                    {
                        if (!_firstLightExoWave)
                        {
                            _firstLightExoWave = true;

                            _arena.sendArenaMessage("!The enemy has sent a Light ExoSuit to stop you!", 4);                        
                            spawnExoLight(_botTeam);
                            spawnExoLight(_botTeam);
                        }
                    }
                    break;

                case 6:
                    {
                        if (!_secondLightExoWave)
                        {
                            _secondLightExoWave = true;

                            _arena.sendArenaMessage("!The enemy has sent a Light ExoSuit to stop you!", 4);
                            spawnExoLight(_botTeam);
                            
                        }
                    }
                    break;

                case 9:
                    {
                        if (!_firstRushWave) // problems with spawns
                        {
                            _firstRushWave = true;

                            _arena.sendArenaMessage("!The enemy has sent extra reinforcements in a last ditch attempt to stop you!", 4);
                            spawnExoLight(_botTeam);
                            spawnEliteHeavy(_botTeam);
                            int playercount = _team.ActivePlayerCount;

                            if (playercount > 6) //  Hard limit the wave size peaks to 6 players
                            playercount = 6;

                            int max = playercount * 1;
                            spawnRandomWave(_botTeam, max);
                            
                        }
                    }
                    break;

                case 13:
                    {
                        if (!_firstHeavyExoWave)
                        {
                            _firstHeavyExoWave = true;

                            _arena.sendArenaMessage("!The enemy has sent a Heavy ExoSuit to stop you!", 4);
                            spawnExoHeavy(_botTeam);
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
                            spawnExoHeavy(_botTeam);
                            int playercount = _team.ActivePlayerCount;

                            if (playercount > 6) //  Hard limit the wave size peaks to 6 players
                                playercount = 6;

                            int max = Convert.ToInt32(playercount * 1.25);
                            spawnRandomWave(_botTeam, max);
                            
                        }
                    }
                    break;

                case 21:
                    {
                        if (!_secondHeavyExoWave)
                        {
                            _secondHeavyExoWave = true;

                            _arena.sendArenaMessage("!The enemy has sent a Heavy ExoSuit to stop you!", 4);
                            spawnExoHeavy(_botTeam);
                        }
                    }
                    break;

                case 24:
                    {
                        if (!_thirdLightExoWave)
                        {
                            _thirdLightExoWave = true;

                            _arena.sendArenaMessage("!The enemy has sent a Light ExoSuit to stop you!", 4);
                            spawnExoLight(_botTeam);
                           

                        }
                    }
                    break;

                case 26:
                    {
                        if (!_secondRushWave)
                        {
                            _secondRushWave = true;

                            _arena.sendArenaMessage("!The enemy has sent extra reinforcements in a last ditch attempt to stop you!", 4);
                            spawnEliteMarine(_botTeam);
                            int playercount = _team.ActivePlayerCount;

                            if (playercount > 6) //  Hard limit the wave size peaks to 6 players
                                playercount = 6;

                            int max = Convert.ToInt32(playercount * 1.50);
                            spawnRandomWave(_botTeam, max);
                            
                        }
                    }
                    break;     

                case 33:
                    {
                        if (!_secondBoss)
                        {
                            _secondBoss = true;

                            _arena.sendArenaMessage("!The enemy has sent some enhanced Soldiers, Look out!", 4);
                            spawnEliteHeavy(_botTeam);
                            spawnEliteMarine(_botTeam);
                            spawnExoLight(_botTeam);
                            int playercount = _team.ActivePlayerCount;

                            if (playercount > 6) //  Hard limit the wave size peaks to 6 players
                                playercount = 6;

                            int max = Convert.ToInt32(playercount * 1.75);
                            spawnRandomWave(_botTeam, max);
                            
                        }
                    }
                    break;

                case 34:
                    {
                        if (!_thirdRushWave)
                        {
                            _thirdRushWave = true;

                            _arena.sendArenaMessage("!The enemy has sent extra reinforcements in a last ditch attempt to stop you!", 4);
                            spawnEliteHeavy(_botTeam);
                            spawnEliteMarine(_botTeam);
                            spawnExoHeavy(_botTeam);
                            int playercount = _team.ActivePlayerCount;

                            if (playercount > 6) //  Hard limit the wave size peaks to 6 players
                                playercount = 6;

                            int max = Convert.ToInt32(playercount * 3);
                            spawnRandomWave(_botTeam, max);
                            
                        }
                    }
                    break;

                case 36:
                    {
                        if (!_thirdHeavyExoWave)
                        {
                            _thirdHeavyExoWave = true;

                            _arena.sendArenaMessage("!The enemy has sent a Heavy ExoSuit to stop you!", 4);
                            spawnExoHeavy(_botTeam);
                        }
                    }
                    break;
            }
        }
    }
}