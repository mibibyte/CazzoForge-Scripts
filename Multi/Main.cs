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
{   // Script Class
    /// Provides the interface between the script and arena
    ///////////////////////////////////////////////////////
    partial class Script_Multi : Scripts.IScript
    {   ///////////////////////////////////////////////////
        // Member Variables
        ///////////////////////////////////////////////////
        private Arena _arena;                   //Pointer to our arena class
        private CfgInfo _config;				//The zone config
        private Settings.GameTypes _gameType;


        //Poll variables
        private int _lastGameCheck;         //Tick at which we checked for game availability
        private int _tickGameStarting;      //Tick at which the game began starting (0 == not initiated)
        private int _tickGameStarted;       //Tick at which the game actually started (0 == stopped)
      

        //Misc variables
        private int _tickStartDelay;
        private int _minPlayers;            //Do we have the # of min players to start a game?
        private bool _bMiniMapsEnabled;

        private List<Player> _fakePlayers;


        /// <summary>
        /// Our gametype handlers
        /// </summary>
        private Conquest _cq;
        private Coop _coop;

        ///////////////////////////////////////////////////
        // Member Functions
        ///////////////////////////////////////////////////
        /// <summary>
        /// Performs script initialization
        /// </summary>
        public bool init(IEventObject invoker)
        {   //Populate our variables
            _arena = invoker as Arena;
            _config = _arena._server._zoneConfig;

            //Load up our gametype handlers
            _cq = new Conquest(_arena, this);
            _coop = new Coop(_arena, this);

            //Default to Conquest
            _gameType = Settings.GameTypes.Conquest;
            _minPlayers = 1;

            if (_arena._name == "[Public] Co-Op")
                _gameType = Settings.GameTypes.Coop;
            else
            {
                Team team1 = _arena.getTeamByName("Titan Militia");
                Team team2 = _arena.getTeamByName("Collective Military");
                _cq.setTeams(team1, team2, false);
            }

            _fakePlayers = new List<Player>();

            _lastSpawn = new Dictionary<string, Helpers.ObjectState>();
            _bMiniMapsEnabled = true;
            return true;
        }

        /// <summary>
        /// Allows the script to maintain itself
        /// </summary>
        public bool poll()
        {	//Should we check game state yet?
            int now = Environment.TickCount;

            if (now - _lastGameCheck <= Arena.gameCheckInterval)
                return true;
            _lastGameCheck = now;

            //Do we have enough players?
            int playing = _arena.PlayerCount;
            if (_arena._bGameRunning && playing < _minPlayers && _arena._bIsPublic)
            {
                //Stop the game and reset voting
                _arena.gameEnd();
            }


            if (playing < _minPlayers && _arena._bIsPublic)
            {
                _tickGameStarting = 0;
                _arena.setTicker(1, 3, 0, "Not Enough Players");
            }

            if (playing < _minPlayers && !_arena._bIsPublic && !_arena._bGameRunning)
            {
                _tickGameStarting = 0;
                _arena.setTicker(1, 3, 0, "Private arena, Waiting for arena owner to start the game!");
            }

            //Do we have enough to start a game?
            if (!_arena._bGameRunning && _tickGameStarting == 0 && playing >= _minPlayers && _arena._bIsPublic)
            {
                _tickGameStarting = now;


                _arena.setTicker(1, 3, _config.deathMatch.startDelay * 100, "Next game: ",
                    delegate ()
                    {   //Trigger the game start
                        _arena.gameStart();
                    });
            }

            //Poll our current gametype!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.Poll(now);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.Poll(now);
                    break;

                default:
                    //Do nothing
                    break;
            }

            return true;
        }

        #region Events

        [Scripts.Event("Player.Portal")]
        public bool playerPortal(Player player, LioInfo.Portal portal)
        {
            if (portal.GeneralData.Name.Contains("DS Portal"))
            {
                Helpers.ObjectState flagPoint = findFlagWarp(player);
                Helpers.ObjectState warpPoint;

                if (flagPoint == null)
                {
                    Log.write(TLog.Normal, String.Format("Could not find suitable player warp for {0}", player._alias));

                    if (!_lastSpawn.ContainsKey(player._alias))
                    {
                        player.sendMessage(-1, "Could not find suitable warp, warped to landing ship!");
                        return true;
                    }
                    else
                        warpPoint = _lastSpawn[player._alias];
                }
                else
                {
                    warpPoint = findOpenWarp(player, _arena, flagPoint.positionX, 1744, _playerWarpRadius);
                }

                if (warpPoint == null)
                {
                    Log.write(TLog.Normal, String.Format("Could not find open warp for {0} (Warp Blocked)", player._alias));
                    player.sendMessage(-1, "Warp was blocked, please try again");
                    return false;
                }

                warp(player, warpPoint);

                if (_lastSpawn.ContainsKey(player._alias))
                    _lastSpawn[player._alias] = warpPoint;
                else
                    _lastSpawn.Add(player._alias, warpPoint);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Triggered when a player attempts to use a warp item
        /// </summary>
        [Scripts.Event("Player.MakeVehicle")]
        public bool playerMakeVehicle(Player player, ItemInfo.VehicleMaker item, short posX, short posY)
        {

            return true;
        }

        [Scripts.Event("Player.WarpItem")]
        public bool playerWarpItem(Player player, ItemInfo.WarpItem item, ushort targetVehicle, short posX, short posY)
        {
            return false;
        }

        /// <summary>
        /// Triggered when a player notifies the server of an explosion
        /// </summary>
        [Scripts.Event("Player.DamageEvent")]
        public bool playerDamageEvent(Player player, ItemInfo.Projectile weapon, short posX, short posY, short posZ)
        {
            
            return true;
        }

        /// <summary>
        /// Triggered when a player notifies the server of an explosion
        /// </summary>
        [Scripts.Event("Player.Explosion")]
        public bool playerExplosion(Player player, ItemInfo.Projectile weapon, short posX, short posY, short posZ)
        {


            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.playerExplosion(player, weapon, posX, posY, posZ);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.playerExplosion(player, weapon, posX, posY, posZ);
                    break;

                default:
                    //Do nothing
                    break;
            }
          
            return true;
        }

        /// <summary>
        /// Triggered when a player wants to unspec and join the game
        /// </summary>
        [Scripts.Event("Player.JoinGame")]
        public bool playerJoinGame(Player player)
        {
            bool handler = true;
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    handler = _cq.playerUnspec(player);
                    break;
                case Settings.GameTypes.Coop:
                    handler = _coop.playerUnspec(player);
                    break;

                default:
                    //Do nothing
                    break;
            }
            return handler;
        }

        /// <summary>
        /// Called when a player enters the game
        /// </summary>
        [Scripts.Event("Player.Enter")]
        public void playerEnter(Player player)
        {

        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        [Scripts.Event("Player.Leave")]
        public bool playerLeave(Player player)
        {

            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.playerSpec(player);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.playerSpec(player);
                    break;


                default:
                    //Do nothing
                    break;
            }
            return false;
        }

        /// <summary>
        /// Called when a player enters the arena
        /// </summary>
        [Scripts.Event("Player.EnterArena")]
        public void playerEnterArena(Player player)
        {
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.playerEnterArena(player);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.playerEnterArena(player);
                    break;

                default:
                    //Do nothing
                    break;
            }
        }

        /// <summary>
        /// Called when a player leaves the arena
        /// </summary>
        [Scripts.Event("Player.LeaveArena")]
        public void playerLeaveArena(Player player)
        {
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.playerLeaveArena(player);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.playerLeaveArena(player);
                    break;


                default:
                    //Do nothing
                    break;
            }
        }

        /// <summary>
        /// Triggered when a player has spawned
        /// </summary>
        [Scripts.Event("Player.Spawn")]
        public bool playerSpawn(Player player, bool death)
        {
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.playerSpawn(player, death);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.playerSpawn(player, death);
                    break;

                default:
                    //Do nothing
                    break;
            }
            return true;
        }

        /// <summary>
        /// Called when the game begins
        /// </summary>
        [Scripts.Event("Game.Start")]
        public bool gameStart()
        {
            _tickGameStarting = 0;
            _tickGameStarted = Environment.TickCount;

            if (_arena.ActiveTeams.Count() == 0)
                return false;

            _arena.initialHideSpawns();

            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.gameStart();
                    break;
                case Settings.GameTypes.Coop:
                    _coop.gameStart();
                    break;

                default:
                    //Do nothing
                    break;
            }
            return true;
        }

        /// <summary>
        /// Called when the game ends
        /// </summary>
        [Scripts.Event("Game.End")]
        public bool gameEnd()
        {   //Game finished, perhaps start a new one
            _tickGameStarted = 0;
            _tickGameStarting = 0;

            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.gameEnd();
                    break;
                case Settings.GameTypes.Coop:
                    _coop.gameEnd();
                    break;


                default:
                    //Do nothing
                    break;
            }
            return true;
        }

        /// <summary>
        /// Called when the statistical breakdown is displayed
        /// </summary>
        [Scripts.Event("Game.Breakdown")]
        public bool gameBreakdown()
        {	//Allows additional "custom" breakdown information

            //Always return true;
            return true;
        }

        /// <summary>
        /// Called to reset the game state
        /// </summary>
        [Scripts.Event("Game.Reset")]
        public bool gameReset()
        {
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.gameReset();
                    break;
                case Settings.GameTypes.Coop:
                    _coop.gameReset();
                    break;

                default:
                    //Do nothing
                    break;
            }
            return true;
        }

        /// <summary>
        /// Handles a player's flag request
        /// </summary>
        [Scripts.Event("Player.FlagAction")]
        public bool playerFlagAction(Player player, bool bPickup, bool bInPlace, LioInfo.Flag flag)
        {
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    return _cq.playerFlagAction(player, bPickup, bInPlace, flag);
                case Settings.GameTypes.Coop:
                    return _coop.playerFlagAction(player, bPickup, bInPlace, flag);
                default:
                    //Do nothing
                    break;
            }
            return true;
        }

        /// <summary>
        /// Called when the statistical breakdown is displayed
        /// </summary>
        [Scripts.Event("Player.Breakdown")]
        public bool individualBreakdown(Player from, bool bCurrent)
        {
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.individualBreakdown(from, bCurrent);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.individualBreakdown(from, bCurrent);
                    break;

                default:
                    //Do nothing
                    break;
            }
            return true;
        }

        /// <summary>
        /// Handles a player's produce request
        /// </summary>
        [Scripts.Event("Player.Produce")]
        public bool playerProduce(Player player, Computer computer, VehInfo.Computer.ComputerProduct product)
        {
            return true;
        }

        /// <summary>
        /// Triggered when a player has died, by any means
        /// </summary>
        /// <remarks>killer may be null if it wasn't a player kill</remarks>
        [Scripts.Event("Player.Death")]
        public bool playerDeath(Player victim, Player killer, Helpers.KillType killType, CS_VehicleDeath update)
        {
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.playerDeath(victim, killer, killType, update);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.playerDeath(victim, killer, killType, update);
                    break;
                default:
                    //Do nothing
                    break;
            }
            return true;
        }

        /// <summary>
        /// Triggered when one player has killed another
        /// </summary>
        [Scripts.Event("Player.PlayerKill")]
        public bool playerPlayerKill(Player victim, Player killer)
        {
            //Don't reward for teamkills
            if (victim._team == killer._team)
                Logic_Assets.RunEvent(victim, _arena._server._zoneConfig.EventInfo.killedTeam);
            else
            {
                Logic_Assets.RunEvent(victim, _arena._server._zoneConfig.EventInfo.killedEnemy);
                //Calculate rewards
                Rewards.calculatePlayerKillRewards(victim, killer);
            }

            //Update stats
            killer.Kills++;
            victim.Deaths++;


            //Now defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.playerPlayerKill(victim, killer);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.playerPlayerKill(victim, killer);
                    break;

                default:
                    //Do nothing
                    break;
            }
            return false;
        }

        /// <summary>
        /// Triggered when a bot has killed a player
        /// </summary>
        [Scripts.Event("Player.BotKill")]
        public bool playerBotKill(Player victim, Bot bot)
        {
            //Now defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.playerDeathBot(victim, bot);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.playerDeathBot(victim, bot);
                    break;

                default:
                    //Do nothing
                    break;
            }

            return true;
        }


        /// <summary>
        /// Triggered when a vehicle dies
        /// </summary>
        [Scripts.Event("Bot.Death")]
        public bool botDeath(Bot dead, Player killer, int weaponID)
        {
            killer.Kills++;


            //Now defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.botDeath(dead, killer);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.botDeath(dead, killer);
                    break;

                default:
                    //Do nothing
                    break;
            }

            return true;
        }

        /// <summary>
        /// Triggered when a player requests to buy a skill
        /// </summary>
        [Scripts.Event("Shop.SkillRequest")]
        public bool PlayerShopSkillRequest(Player from, SkillInfo skill)
        {

            if (!_arena._bIsPublic)
            {
            }
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                   // _cq.PlayerRepair(from, item);
                    break;

                default:
                    //Do nothing
                    break;
            }

            return true;
        }

        /// <summary>
        /// Triggers when a repair item is used
        /// </summary>
        [Scripts.Event("Player.Repair")]
        public bool playerPlayerRepair(Player player, ItemInfo.RepairItem item, UInt16 target, short posX, short posY)
        { 
            //Defer to our current gametype handler!
            switch (_gameType)
            {
                case Settings.GameTypes.Conquest:
                    _cq.PlayerRepair(player, item);
                    break;
                case Settings.GameTypes.Coop:
                    _coop.PlayerRepair(player, item);
                    break;

                default:
                    //Do nothing
                    break;
            }
            return true;
        }

        /// <summary>
        /// Triggered only when a special communication command is created here that isn't a server command.
        /// </summary>
        [Scripts.Event("Player.CommCommand")]
        public bool playerCommCommand(Player player, Player recipient, string command, string payload)
        {

                return true;
        }

        /// <summary>
        /// Triggered only when a special chat command is created here that isn't a server command.
        /// </summary>
        [Scripts.Event("Player.ChatCommand")]
        public bool playerChatCommand(Player player, Player recipient, string command, string payload)
        {
            if (command.Equals("trigger"))
            {

                int id;
                bool isNumeric = int.TryParse(payload, out id);

                if (!isNumeric)
                    return false;

                player.triggerMessage((byte)id, 500, "Trigger message test");

            }
            return true;
        }

        /// <summary>
        /// Triggered only when a special mod command created here that isn't a server command.
        /// </summary>
        [Scripts.Event("Player.ModCommand")]
        public bool playerModcommand(Player player, Player recipient, string command, string payload)
        {
            if (command.Equals("pop"))
            {

                int id;
                bool isNumeric = int.TryParse(payload, out id);

                if (!isNumeric)
                    return false;

                for (int i = 0; i < id; i++)
                {
                    Client<Player> client = new Client<Player>(true);
                    client._ipe = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 1337);
                    Player p = _arena._server.newPlayer(client, String.Format("Guest{0}", i));

                    if (p != null)
                    _fakePlayers.Add(p);
                }

                return false;
            }

            if (command.Equals("depop"))
            {
                if (_fakePlayers == null)
                    return false;


                foreach (Player p in _fakePlayers)
                {
                    _arena._server.lostPlayer(p);
                }

                _fakePlayers.Clear();

                return false;
            }


            if (command.Equals("bots"))
            {
                if (String.IsNullOrEmpty(payload))
                {
                    player.sendMessage(0, String.Format("Bots are {0}", ((_cq.spawnBots) ? "enabled" : "disabled")));
                    return false;
                }

                if (payload.Equals("off"))
                {
                    _cq.spawnBots = false;
                    _arena.sendArenaMessage("Bots have been disabled for this arena");
                    return false;
                }

                if (payload.Equals("on"))
                {
                    _cq.spawnBots = true;
                    _arena.sendArenaMessage("Bots have been enabled for this arena");
                    return false;
                }
            }

            if (command.Equals("minimaps"))
            {
                if (String.IsNullOrEmpty(payload))
                {
                    player.sendMessage(0, String.Format("Minimaps are {0}", ((_bMiniMapsEnabled) ? "enabled" : "disabled")));
                    return false;
                }

                if (payload.Equals("off"))
                {
                    _bMiniMapsEnabled = false;

                    if (_arena._bGameRunning)
                        _arena.gameEnd();

                    Team team1 = _arena.getTeamByName("Titan Militia");
                    Team team2 = _arena.getTeamByName("Collective Military");
                    _cq.setTeams(team1, team2, false);

                    _arena.sendArenaMessage("Minimaps have been disabled for this arena");
                    return false;
                }

                if (payload.Equals("on"))
                {
                    _bMiniMapsEnabled = true;
                    _arena.sendArenaMessage("Minimaps have been enabled for this arena");
                    return false;
                }
            }

            if (command.Equals("map"))
            {
                if (String.IsNullOrEmpty(payload))
                {
                    player.sendMessage(0, "Payload cannot be empty. Ex: *map redblue");
                    return false;
                }

                if (payload.Equals("cazzo"))
                {
                    Team team1 = _arena.getTeamByName("Titan Militia");
                    Team team2 = _arena.getTeamByName("Collective Military");
                    _cq.setTeams(team1, team2, false);
                    _arena.gameStart();
                }

                if (payload.Equals("redblue"))
                {
                    Team team1 = _arena.getTeamByName("Red");
                    Team team2 = _arena.getTeamByName("Blue");
                    _cq.setTeams(team1, team2, false);
                    _arena.gameStart();
                }

                if (payload.Equals("greenyellow"))
                {
                    Team team1 = _arena.getTeamByName("Green");
                    Team team2 = _arena.getTeamByName("Yellow");
                    _cq.setTeams(team1, team2, false);
                    _arena.gameStart();
                }
            }

            if (command.Equals("poweradd"))
            {
                if (player.PermissionLevelLocal < Data.PlayerPermission.SMod)
                {
                    player.sendMessage(-1, "Nice try.");
                    return false;
                }

                int level = (int)Data.PlayerPermission.ArenaMod;
                //Pm'd?
                if (recipient != null)
                {
                    //Check for a possible level
                    if (!String.IsNullOrWhiteSpace(payload))
                    {
                        try
                        {
                            level = Convert.ToInt16(payload);
                        }
                        catch
                        {
                            player.sendMessage(-1, "Invalid level. Level must be either 1 or 2.");
                            return false;
                        }

                        if (level < 1 || level > (int)player.PermissionLevelLocal
                            || level == (int)Data.PlayerPermission.SMod)
                        {
                            player.sendMessage(-1, ":alias:*poweradd level(optional), :alias:*poweradd level (Defaults to 1)");
                            player.sendMessage(0, "Note: there can only be 1 admin level.");
                            return false;
                        }

                        switch (level)
                        {
                            case 1:
                                recipient._permissionStatic = Data.PlayerPermission.ArenaMod;
                                break;
                            case 2:
                                recipient._permissionStatic = Data.PlayerPermission.Mod;
                                break;
                        }
                        recipient._developer = true;
                        recipient.sendMessage(0, String.Format("You have been powered to level {0}. Use *help to familiarize with the commands and please read all rules.", level));
                        player.sendMessage(0, String.Format("You have promoted {0} to level {1}.", recipient._alias, level));
                    }
                    else
                    {
                        recipient._developer = true;
                        recipient._permissionStatic = Data.PlayerPermission.ArenaMod;
                        recipient.sendMessage(0, String.Format("You have been powered to level {0}. Use *help to familiarize with the commands and please read all rules.", level));
                        player.sendMessage(0, String.Format("You have promoted {0} to level {1}.", recipient._alias, level));
                    }

                    //Lets send it to the database
                    //Send it to the db
                    CS_ModQuery<Data.Database> query = new CS_ModQuery<Data.Database>();
                    query.queryType = CS_ModQuery<Data.Database>.QueryType.mod;
                    query.sender = player._alias;
                    query.query = recipient._alias;
                    query.level = level;
                    //Send it!
                    player._server._db.send(query);
                    return true;
                }
                else
                {
                    //We arent
                    //Get name and possible level
                    Int16 number;
                    if (String.IsNullOrEmpty(payload))
                    {
                        player.sendMessage(-1, "Syntax: *poweradd alias:level(optional) Note: if using a level, put : before it otherwise defaults to arena mod");
                        player.sendMessage(0, "Note: there can only be 1 admin.");
                        return false;
                    }
                    if (payload.Contains(':'))
                    {
                        string[] param = payload.Split(':');
                        try
                        {
                            number = Convert.ToInt16(param[1]);
                            if (number >= 0)
                                level = number;
                        }
                        catch
                        {
                            player.sendMessage(-1, "That is not a valid level. Possible powering levels are 1 or 2.");
                            return false;
                        }
                        if (level < 1 || level > (int)player.PermissionLevelLocal
                            || level == (int)Data.PlayerPermission.SMod)
                        {
                            player.sendMessage(-1, String.Format("Syntax: *poweradd alias:level(optional) OR :alias:*poweradd level(optional) possible levels are 1-{0}", ((int)player.PermissionLevelLocal).ToString()));
                            player.sendMessage(0, "Note: there can be only 1 admin level.");
                            return false;
                        }
                        payload = param[0];
                    }
                    player.sendMessage(0, String.Format("You have promoted {0} to level {1}.", payload, level));
                    if ((recipient = player._server.getPlayer(payload)) != null)
                    { //They are playing, lets update them
                        switch (level)
                        {
                            case 1:
                                recipient._permissionStatic = Data.PlayerPermission.ArenaMod;
                                break;
                            case 2:
                                recipient._permissionStatic = Data.PlayerPermission.Mod;
                                break;
                        }
                        recipient._developer = true;
                        recipient.sendMessage(0, String.Format("You have been powered to level {0}. Use *help to familiarize with the commands and please read all rules.", level));
                    }

                    //Lets send it off
                    CS_ModQuery<Data.Database> query = new CS_ModQuery<Data.Database>();
                    query.queryType = CS_ModQuery<Data.Database>.QueryType.dev;
                    query.sender = player._alias;
                    query.query = payload;
                    query.level = level;
                    //Send it!
                    player._server._db.send(query);
                    return true;
                }
            }

            if (command.Equals("powerremove"))
            {
                if (player.PermissionLevelLocal < Data.PlayerPermission.SMod)
                {
                    player.sendMessage(-1, "Nice try.");
                    return false;
                }

                int level = (int)Data.PlayerPermission.Normal;
                //Pm'd?
                if (recipient != null)
                {
                    //Check for a possible level
                    if (!String.IsNullOrWhiteSpace(payload))
                    {
                        try
                        {
                            level = Convert.ToInt16(payload);
                        }
                        catch
                        {
                            player.sendMessage(-1, "Invalid level. Levels must be between 0 and 2.");
                            return false;
                        }

                        if (level < 0 || level > (int)player.PermissionLevelLocal
                            || level == (int)Data.PlayerPermission.SMod)
                        {
                            player.sendMessage(-1, ":alias:*powerremove level(optional), :alias:*powerremove level (Defaults to 0)");
                            return false;
                        }

                        switch (level)
                        {
                            case 0:
                                recipient._permissionStatic = Data.PlayerPermission.Normal;
                                recipient._developer = false;
                                break;
                            case 1:
                                recipient._permissionStatic = Data.PlayerPermission.ArenaMod;
                                break;
                            case 2:
                                recipient._permissionStatic = Data.PlayerPermission.Mod;
                                break;
                        }
                        recipient.sendMessage(0, String.Format("You have been demoted to level {0}.", level));
                        player.sendMessage(0, String.Format("You have demoted {0} to level {1}.", recipient._alias, level));
                    }
                    else
                    {
                        recipient._developer = false;
                        recipient._permissionStatic = Data.PlayerPermission.Normal;
                        recipient.sendMessage(0, String.Format("You have been demoted to level {0}.", level));
                        player.sendMessage(0, String.Format("You have demoted {0} to level {1}.", recipient._alias, level));
                    }

                    //Lets send it to the database
                    //Send it to the db
                    CS_ModQuery<Data.Database> query = new CS_ModQuery<Data.Database>();
                    query.queryType = CS_ModQuery<Data.Database>.QueryType.dev;
                    query.sender = player._alias;
                    query.query = recipient._alias;
                    query.level = level;
                    //Send it!
                    player._server._db.send(query);
                    return true;
                }
                else
                {
                    //We arent
                    //Get name and possible level
                    Int16 number;
                    if (String.IsNullOrEmpty(payload))
                    {
                        player.sendMessage(-1, "Syntax: *powerremove alias:level(optional) Note: if using a level, put : before it otherwise defaults to arena mod");
                        return false;
                    }
                    if (payload.Contains(':'))
                    {
                        string[] param = payload.Split(':');
                        try
                        {
                            number = Convert.ToInt16(param[1]);
                            if (number >= 0)
                                level = number;
                        }
                        catch
                        {
                            player.sendMessage(-1, "That is not a valid level. Possible depowering levels are between 0 and 2.");
                            return false;
                        }
                        if (level < 0 || level > (int)player.PermissionLevelLocal
                            || level == (int)Data.PlayerPermission.SMod)
                        {
                            player.sendMessage(-1, String.Format("Syntax: *powerremove alias:level(optional) OR :alias:*powerremove level(optional) possible levels are 0-{0}", ((int)player.PermissionLevelLocal).ToString()));
                            return false;
                        }
                        payload = param[0];
                    }
                    player.sendMessage(0, String.Format("You have demoted {0} to level {1}.", payload, level));
                    if ((recipient = player._server.getPlayer(payload)) != null)
                    { //They are playing, lets update them
                        switch (level)
                        {
                            case 0:
                                recipient._permissionStatic = Data.PlayerPermission.Normal;
                                recipient._developer = false;
                                break;
                            case 1:
                                recipient._permissionStatic = Data.PlayerPermission.ArenaMod;
                                break;
                            case 2:
                                recipient._permissionStatic = Data.PlayerPermission.Mod;
                                break;
                        }
                        recipient.sendMessage(0, String.Format("You have been depowered to level {0}.", level));
                    }

                    //Lets send it off
                    CS_ModQuery<Data.Database> query = new CS_ModQuery<Data.Database>();
                    query.queryType = CS_ModQuery<Data.Database>.QueryType.dev;
                    query.sender = player._alias;
                    query.query = payload;
                    query.level = level;
                    //Send it!
                    player._server._db.send(query);
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region Custom Calls
        private Player findSquadWarp(IEnumerable<Player> squadmates, Player player)
        {
            Player warpTo = null;

            List<Player> potentialTargets = new List<Player>();

            //Start shuffling through looking for potential targets
            foreach (Player squadmate in squadmates)
            {
                int enemycount = 0;
                double distance = 0;

                //Any enemies?
                enemycount = _arena.getPlayersInRange
                    (squadmate._state.positionX, squadmate._state.positionY, 625).Where
                    (p => p._team != squadmate._team && !p.IsDead).Count();

                //Decent distance away? Don't want to warp ourselves super sort distances
                distance = Helpers.distanceTo(squadmate._state, player._state);

                //Is he a match!?
                if (distance >= 2000 && enemycount == 0)
                {
                    potentialTargets.Add(squadmate);
                }
            }

            //Randomize!
            if (potentialTargets.Count > 0)
            {
                warpTo = potentialTargets[new Random().Next(0, potentialTargets.Count)];
            }


            return warpTo;
        }

        private bool warpToSquad(Player player)
        {
            //Public arena?
            if (!_arena._bIsPublic)
            {
                player.sendMessage(-1, "Only allowed in public arenas!");
                return false;
            }

            //No squad no laundry
            if (player._squad == "")
            {
                player.sendMessage(-1, "You don't have a squad to warp to! Join or create a squad.");
                return false;
            }

            //Lets find some squaddies
            IEnumerable<Player> squadmates = player._team.ActivePlayers.Where(p => p._squad == player._squad && p.IsDead == false && p != player);

            //No squadmates online on his team?
            if (squadmates.Count() == 0)
            {
                player.sendMessage(-1, "You don't have any squadmates online on your team!");
                return false;
            }

            Player warpTo = findSquadWarp(squadmates, player);

            //Can we find an appropriate target?
            if (warpTo == null)
            {
                player.sendMessage(-1, "Your squadmates are in battle or dead! Try again soon");
                return false;
            }

            //Warp him!
            player.warp(warpTo);
            warpTo.sendMessage(0, String.Format("!{0} has joined you in battle.", player._alias));

            return true;
        }
        #endregion
    }
}