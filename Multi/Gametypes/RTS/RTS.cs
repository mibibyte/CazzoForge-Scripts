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
{ 	// Script Class
    /// Provides the interface between the script and arena
    ///////////////////////////////////////////////////////
    public class RTS
    {	///////////////////////////////////////////////////
        // Member Variables
        ///////////////////////////////////////////////////
        private Arena _arena;					//Pointer to our arena class
        private CfgInfo _config;                //The zone config
        public Script_Multi _baseScript;
        private Random _rand;

        private int _lastGameCheck;				//The tick at which we last checked for game viability
        private Player _owner;
        private Team _ownerTeam;
        private Database _database;
      

        public class Position
        {
            public short positionX;
            public short positionY;
        }

        ///////////////////////////////////////////////////
        // Member Functions
        ///////////////////////////////////////////////////
        /// <summary>
        /// Performs script initialization
        /// </summary>
        public RTS(Arena arena, Script_Multi baseScript)
        {
            _baseScript = baseScript;
            _arena = arena;
            _config = arena._server._zoneConfig;
            _rand = new Random();
        }

        /// <summary>
        /// Allows the script to maintain itself
        /// </summary>
        public bool Poll(int now)
        {	//Should we check game state yet?
            // List<Player> crowns = _activeCrowns;

            if (now - _lastGameCheck <= Arena.gameCheckInterval)
                return true;

            _lastGameCheck = now;

            //Do we have enough players ingame?
            int playing = _arena.PlayerCount;

            return true;
        }

        public void init()
        {
            //Figure out who owns the arena
            string owner = _arena._name.Substring(5, _arena._name.Length - 5).TrimStart();
            _owner = _arena.getPlayerByName(owner);

            //Create our team
            _ownerTeam = new Team(_arena, _arena._server);

            //Assign some information to the team
            _ownerTeam._name = owner;
            _ownerTeam._isPrivate = true;
            _ownerTeam._password = "1234abc";
            _ownerTeam._id = (short)_arena.Teams.Count();

            
            _arena.createTeam(_ownerTeam);
            //Load our database
            _database = _baseScript._database;
            _database.loadBuildings(owner, _ownerTeam);
        }

        #region Events

        /// <summary>
        /// Called when a player enters the arena
        /// </summary>
        public void playerEnterArena(Player player)
        {

            //First player?
            if (_arena.TotalPlayerCount == 1)
                init();


            if (player == _owner)
                player.sendMessage(0, "Welcome home!");

            //Obtain the Co-Op skill..
            SkillInfo coopskillInfo = _arena._server._assets.getSkillByID(200);

            //Add the skill!
            if (player.findSkill(200) != null)
                player._skills.Remove(200);

            //Obtain the Powerup skill..
            SkillInfo powerupskillInfo = _arena._server._assets.getSkillByID(201);

            //Add the skill!
            if (player.findSkill(201) != null)
                player._skills.Remove(201);
        }

        public bool playerPortal(Player player, LioInfo.Portal portal)
        {
            return false;
        }

        /// <summary>
        /// Called when a player enters the game
        /// </summary>
        public void playerEnter(Player player)
        {
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        public void playerLeave(Player player)
        {
        }

        /// <summary>
        /// Triggered when a player tries to heal
        /// </summary>
        public void PlayerRepair(Player from, ItemInfo.RepairItem item)
        {
        }


        public void playerLeaveArena(Player player)
        {
        }

        /// <summary>
        /// Handles the spawn of a player
        /// </summary>
        public bool playerSpawn(Player player, bool bDeath)
        {
            return true;
        }

        /// <summary>
        /// Triggered when a player wants to unspec and join the game
        /// </summary>
        public bool playerJoinGame(Player player)
        {
            return true;
        }

        /// <summary>
        /// Triggered when a player wants to spec and leave the game
        /// </summary>
        public bool playerLeaveGame(Player player)
        {
            return true;
        }

        /// <summary>
        /// Triggered when a player has died, by any means
        /// </summary>
        /// <remarks>killer may be null if it wasn't a player kill</remarks>
        public bool playerDeath(Player victim, Player killer, Helpers.KillType killType, CS_VehicleDeath update)
        {   
            return true;
        }

        /// <summary>
        /// Triggered when one player has killed another
        /// </summary>
        public bool playerPlayerKill(Player victim, Player killer)
        {
            return true;
        }

        /// <summary>
        /// Triggered when a vehicle dies
        /// </summary>
        public bool vehicleDeath(Vehicle dead, Player killer)
        {
            return true;
        }

        #endregion

        #region Command Handlers
        public bool playerModcommand(Player player, Player recipient, string command, string payload)
        {
            return true;
        }

        public bool playerChatCommand(Player player, Player recipient, string command, string payload)
        {
            return true;
        }
        #endregion

    }
}