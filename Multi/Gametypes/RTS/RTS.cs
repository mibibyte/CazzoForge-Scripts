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
    public partial class RTS
    {	///////////////////////////////////////////////////
        // Member Variables
        ///////////////////////////////////////////////////
        private Arena _arena;					//Pointer to our arena class
        private CfgInfo _config;                //The zone config
        public Script_Multi _baseScript;
        private Random _rand;

        private int _lastGameCheck;				//The tick at which we last checked for game viability
        public string _owner;
        private Team _ownerTeam;
        public Database _database;

        private Team _titan;
        private Team _collective;

        private int _tickLastUpdate;
        private int _tickLastBotUpdate;

        //Stored Data
        public Dictionary<ushort, Structure> _structures;
        public Dictionary<ushort, Unit> _units;
        public Dictionary<ushort, StoredItem> _items;
      

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

            _arena._bIsPublic = true;

            _titan = _arena.getTeamByName("Titan Militia");
            _collective = _arena.getTeamByName("Collective Military");
            _bots = new List<Bot>();
            _units = new Dictionary<ushort, Unit>();
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


            //Time to update our structures?
            if (now - _tickLastUpdate >= 10000)
            {
                if (_structures != null)
                {
                    foreach (Structure structure in _structures.Values)
                        _database.updateStructure(structure, _owner);

                }

                if (_units != null)
                {
                    foreach (Unit unit in _units.Values)
                        _database.updateBot(unit, _owner);

                }

                _tickLastUpdate = now;
            }

            //Check for productions
            checkForProductions(now);

            maintainBots(now);

            return true;
        }

        public void maintainBots(int now)
        {
            if (now - _tickLastBotUpdate < 5000 && _tickLastBotUpdate != 0)
                return;

            bool bUpdated = (_bots.Count == _units.Count);


            foreach (Unit bot in _units.Values)
            {
                if (bUpdated)
                    break;

                BotLevel level = BotLevel.Normal;
                BotType type = BotType.Marine;

                switch (bot._vehicleID)
                {
                    case 152:
                        level = BotLevel.Adept;
                        type = BotType.Ripper;
                        break;
                    case 151:
                        level = BotLevel.Adept;
                        type = BotType.Marine;
                        break;
                    case 131:
                        level = BotLevel.Normal;
                        type = BotType.Marine;
                        break;
                    case 145:
                        level = BotLevel.Normal;
                        type = BotType.Ripper;
                        break;
                    case 146:
                        level = BotLevel.Elite;
                        type = BotType.EliteMarine;
                        break;
                    case 148:
                        level = BotLevel.Elite;
                        type = BotType.EliteHeavy;
                        break;


                }

                //Avoids spawning bots under computer vehicles/buildings
                if (_arena.getVehiclesInRange(bot._state.positionX, bot._state.positionY, 150).Count(veh => veh._type.Type == VehInfo.Types.Computer) > 0)
                    Helpers.randomPositionInArea(_arena, 200, ref bot._state.positionX, ref bot._state.positionY);

                Bot newUnit = newBot(_titan, type, null, null, level, bot._state);

                if (newUnit == null)
                    Log.write("[RTS] Could not spawn bot");
                else
                    bot._bot = newUnit;


            }
        }

        public void init()
        {
            _arena.gameStart();

            //Load our database
            _database = new Database(_arena, this);
            _database.loadXML();

            //Figure out who owns the arena
            _owner = _arena._name.Substring(5, _arena._name.Length - 5).TrimStart().ToLower();

            if (!_database.tableExists(_owner))
                _database.createTable(_owner);

            _structures = _database.loadStructures(_owner);
            _units = _database.loadBots(_owner);
            _items = _database.loadItems(_owner);


            foreach (Structure b in _structures.Values)
            {
                Vehicle newVeh = _arena.newVehicle(b._type, _titan, null, b._state);

                if (newVeh == null)
                    Log.write("[RTS] Could not spawn vehicle");
                else
                {
                    newVeh._state.health = b._state.health;
                    b._vehicle = newVeh;
                    b.initState(newVeh);
                }
            }

            foreach (StoredItem item in _items.Values)
            {
                item._drop = _arena.itemSpawn(AssetManager.Manager.getItemByID(item._itemID), (ushort)item._quantity, item._posX, item._posY, null);
                item._key = _owner;
            }


            _tickLastUpdate = Environment.TickCount;
            _tickLastBotUpdate = Environment.TickCount;
        }

        #region Events

        /// <summary>
        /// Called when a player enters the arena
        /// </summary>
        public void playerEnterArena(Player player)
        {

            //First player?
            if (_arena.TotalPlayerCount == 1)
            {
                if (_arena.TotalPlayerCount == 1 && player._permissionStatic < Data.PlayerPermission.Mod && _arena._bIsPublic)
                    player._permissionTemp = Data.PlayerPermission.Normal;

                //Turn the lights on
                init();
            }

            if (player._alias.ToLower() == _owner)
                player.sendMessage(0, "Welcome to your city!");

            //Obtain the Co-Op skill..
            SkillInfo coopskillInfo = _arena._server._assets.getSkillByID(200);
            SkillInfo powerupskillInfo = _arena._server._assets.getSkillByID(201);
            SkillInfo royaleskillInfo = _arena._server._assets.getSkillByID(203);
            SkillInfo rtsMode = _arena._server._assets.getSkillByID(202);

            //Add the skill!
            if (player.findSkill(200) != null)
                player._skills.Remove(200);

            if (player.findSkill(203) != null)
                player._skills.Remove(203);

            //Add the skill!
            if (player.findSkill(201) != null)
                player._skills.Remove(201);

            //Add the skill!
            if (player.findSkill(202) == null)
                player.skillModify(rtsMode, 1);
        }

        public bool playerPortal(Player player, LioInfo.Portal portal)
        {
            if (portal.GeneralData.Name.Contains("DS Portal"))
            {
                if (player._alias.ToLower() == _owner)
                {
                    Vehicle command = _arena.Vehicles.FirstOrDefault(v => v._type.Id == 414);
                    if (command != null)
                        player.warp(Helpers.ResetFlags.ResetNone, command._state, 200, -1, 0);
                    else
                        player.warp(12260, 7460);
                }
                else
                {
                    player.warp(12260, 7460);
                }

            }
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
            if (player._alias.ToLower() == _owner)
            {
                player.unspec(_titan);
                return true;
            }
            else
               player.sendMessage(-1, "You are not the owner of this city. Offensive RTS Gameplay coming soon(tm)");

            return false;
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


        public bool playerProduce(Player player, Computer computer, VehInfo.Computer.ComputerProduct product)
        {
            switch (computer._type.Name)
            {
                case "[RTS] Command Center":
                    return tryCommandCenterMenu(player, computer, product);
                case "[RTS] Shack":
                    return tryResidentialMenu(player, computer, product, ProductionBuilding.Shack);
                case "[RTS] House":
                    return tryResidentialMenu(player, computer, product, ProductionBuilding.House);
                case "[RTS] Villa":
                    return tryResidentialMenu(player, computer, product, ProductionBuilding.Villa);
                case "[RTS] Marine Barracks":
                    return tryBarracksMenu(player, computer, product, DefenseProduction.Marine);
                case "[RTS] Ripper Barracks":
                    return tryBarracksMenu(player, computer, product, DefenseProduction.Ripper);
                case "[RTS] Factory - Housing":
                    return tryHousingProductionMenu(player, computer, product);
                case "[RTS] Refinery - Iron":
                    return tryIronRefinery(player, computer, product);
                case "[RTS] Factory - Defense":
                    return tryDefenseMenu(player, computer, product);
                case "[RTS] Factory - Production":
                    return tryProductionMenu(player, computer, product);
            }
            return true;
        }

        /// <summary>
        /// Triggered when a vehicle dies
        /// </summary>
        public bool vehicleDeath(Vehicle dead, Player killer)
        {
            if (!dead._bBotVehicle)
            {
                Structure structure = _structures.Values.FirstOrDefault(st => st._vehicle == dead);

                if (structure == null)
                    return true;

                //Destroy it
                structure.destroyed(dead, killer);
            }
            return true;
        }

        public bool botDeath(Bot dead, Player killer)
        {
            Unit unit = _units.Values.FirstOrDefault(st => st._bot == dead);

            if (unit == null)
                return true;

            //Destroy it
            unit.destroyed(dead, killer);

            return true;
        }

        public bool playerMakeVehicle(Player player, ItemInfo.VehicleMaker item, short posX, short posY)
        {
            int count = _arena.Vehicles.Count(veh => veh._type.Id == item.vehicleID);
            bool bSuccess = false;
            VehInfo vehicle = AssetManager.Manager.getVehicleByID(item.vehicleID);

            switch (item.vehicleID)
            {
                //Command Center
                case 414:
                    {
                        if (count > 0)
                        {
                            player.sendMessage(-1, "You may only have one active Command Center.");
                            player.syncState();
                            bSuccess = false;
                        }
                        else if (!canBuildInArea(posX, posY, vehicle.PhysicalRadius))
                        {
                            player.sendMessage(-1, "Cannot construct building, Cannot construct buildings that close to eachother.");
                            bSuccess = false;
                        }
                        else
                        {
                            //Building!
                            bSuccess = true;
                        }
                    }
                    break;
                //Power Station
                case 423:
                    {
                        if (!canBuildInArea(posX, posY, vehicle.PhysicalRadius))
                        {
                            player.sendMessage(-1, "Cannot construct building, Cannot construct buildings that close to eachother.");
                            bSuccess = false;
                        }
                        else
                        {
                            //Building!
                            bSuccess = true;
                        }
                    }
                    break;
                //Catch all
                default:
                    {
                        if (!canBuildInArea(posX, posY, vehicle.PhysicalRadius))
                        {
                            player.sendMessage(-1, "Cannot construct building, Cannot construct buildings that close to eachother.");
                            bSuccess = false;
                        }
                        else if (!powerStationInArea(posX, posY))
                        {
                            player.sendMessage(-1, "Cannot construct building, too far from a power station.");
                            bSuccess = false;
                        }
                        else
                        {
                            //Building...
                            bSuccess = true;
                        }
                    }
                    break;
            }
            //Give them back their kit if it failed.
            if (!bSuccess)
                player.inventoryModify(item, 1);

            return bSuccess;
        }

        /// <summary>
        /// Triggered when a vehicle is created
        /// </summary>
        /// <remarks>Doesn't catch spectator or dependent vehicle creation</remarks>
        public bool vehicleCreation(Vehicle created, Team team, Player creator)
        {
            if (creator != null && !created._bBotVehicle)
            newStructure(created, creator);

            return true;
        }


        public bool playerItemPickup(Player player, Arena.ItemDrop drop, ushort quantity)
        {
            StoredItem item = _items.Values.FirstOrDefault(itm => itm._drop.id == drop.id);
            if (item != null)
            {
                if (drop.quantity == 0)
                    item.remove();
                else
                {
                    item._quantity -= (short)quantity;
                    _database.updateItem(item, _owner);
                }
            }
            return true;
        }

        public bool playerItemDrop(Player player, ItemInfo item, ushort quantity)
        {
            Arena.ItemDrop newDrop = null;

            //Droppable?
            if (!item.droppable)
                return false;

            if (player.inventoryModify(item, -quantity))
            {   //Create an item spawn
                newDrop = _arena.itemSpawn(item, quantity, player._state.positionX, player._state.positionY, 0, (int)player._team._id, player);
                newDrop.tickExpire = 0;
            }

            if (player._alias.ToLower() == _owner)
                if (newDrop != null)
                newItem(newDrop);

            return false;
        }

            #endregion

            #region Command Handlers
            public bool playerModcommand(Player player, Player recipient, string command, string payload)
        {
            return true;
        }

        public bool playerChatCommand(Player player, Player recipient, string command, string payload)
        {
            if (command.Equals("test"))
            {
                newBot(_titan, BotType.Marine, null, player, BotLevel.Normal, player._state);
            }
            return true;
        }
        #endregion

        #region Private Routines
        public void newStructure(Vehicle veh, Player player)
        {

            Structure newStruct = new Structure(this);
            newStruct._vehicle = veh;
            newStruct._state = veh._state;
            newStruct._productionLevel = 1;
            newStruct._id = (ushort)(_database.getLastStructureID(_owner) + 1);
            newStruct._key = _owner;

            switch (veh._type.Name)
            {
                case "[RTS] Shack":
                    {
                        newStruct._upgradeCost = c_baseShackUpgrade;
                        newStruct._productionQuantity = c_baseShackProduction;
                        newStruct._nextProduction = DateTime.Now.AddHours(c_shackProductionInterval);
                    }
                    break;
                case "[RTS] House":
                    {
                        newStruct._upgradeCost = c_baseHouseUpgrade;
                        newStruct._productionQuantity = c_baseHouseProduction;
                        newStruct._nextProduction = DateTime.Now.AddHours(c_houseProductionInterval);
                    }
                    break;
                case "[RTS] Villa":
                    {
                        newStruct._upgradeCost = c_basevillaUpgrade;
                        newStruct._productionQuantity = c_baseVillaProduction;
                        newStruct._nextProduction = DateTime.Now.AddHours(c_villaProductionInterval);
                    }
                    break;
                default:
                    {
                        newStruct._productionItem = AssetManager.Manager.getItemByID(323);
                        newStruct._productionQuantity = 0;
                    }
                    break;
            }


            newStruct.initState(veh);

             _database.addStructure(newStruct, _owner);
            _structures.Add(newStruct._id, newStruct);
        }

        public void newUnit(BotType type, Vehicle target, Player owner, BotLevel level, Helpers.ObjectState state = null)
        {
            Unit newUnit = new Unit(this);

            Bot bot = newBot(_titan, type, target, owner, level, state);
            newUnit._vehicleID = (ushort)bot._type.Id;
            newUnit._state = state;
            newUnit._id = (ushort)(_database.getLastBotID(_owner) + 1);
            newUnit._bot = bot;
            _database.addBot(bot, _owner);
            _units.Add(bot._id, newUnit);
        }

        public void newItem(Arena.ItemDrop drop)
        {
            ushort id = (ushort)(_database.getLastItemID(_owner) + 1);

            StoredItem newItem = new StoredItem(this);
            newItem._id = id;
            newItem._itemID = drop.item.id;
            newItem._quantity = drop.quantity;
            newItem._posX = drop.positionX;
            newItem._posY = drop.positionY;
            newItem._key = _owner;
            newItem._drop = drop;

            _database.addItem(drop.item.id, drop.positionX, drop.positionY, drop.quantity, _owner);
            _items.Add(id, newItem);
        }

        #endregion

        #region Helpers
        public bool canBuildInArea(short posX, short posY, int radius)
        {
            return (_arena.getVehiclesInRange(posX, posY, radius + 200).Count(veh => veh._type.Name.StartsWith("[RTS]")) == 0);
        }

        public bool powerStationInArea(short posX, short posY)
        {
            return (_arena.getVehiclesInRange(posX, posY, 500).Count(veh => veh._type.Name == "[RTS] Power Station") > 0);
        }
        #endregion

    }
}