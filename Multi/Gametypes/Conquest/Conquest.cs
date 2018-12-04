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
{
    public partial class Conquest
    {
        private Arena _arena;
        public Team _team1;
        public Team _team2;
        private CfgInfo _config;                //The zone config
        private int _lastTickerUpdate;
        private int _lastKillStreakUpdate;
        private bool _isScrim = false;
        public List<Arena.FlagState> _flags;
        private List<CapturePoint> _activePoints;
        private List<CapturePoint> _allPoints;
        private int _lastFlagCheck;
        public Script_Multi _baseScript;

        private int _flagCaptureRadius = 250;
        private int _flagCaptureTime = 5;


        public Team cqTeam1; 
        public Team cqTeam2;


        #region Stat Recording
        private string FileName;
        private List<Team> activeTeams = null;
        private Team lastTeam1, lastTeam2;  //Records the previous team before overtime starts(We get team stats from this since ot is not recorded)
        public Team victoryTeam = null;
        DateTime startTime;

        /// <summary>
        /// Current game player stats
        /// </summary>
        private Dictionary<string, PlayerStat> _savedPlayerStats;
        /// <summary>
        /// Only used when overtime is about to be initiated
        /// </summary>
        private Dictionary<string, PlayerStat> _lastSavedStats;

        /// <summary>
        /// Stores our player information
        /// </summary>
        private class PlayerStat
        {
            public Player player { get; set; }
            public string teamname { get; set; }
            public string alias { get; set; }
            public string squad { get; set; }
            public long points { get; set; }
            public int kills { get; set; }
            public int deaths { get; set; }
            public int killPoints { get; set; }
            public int assistPoints { get; set; }
            public int bonusPoints { get; set; }
            public int playSeconds { get; set; }
            public bool hasPlayed { get; set; }
            public string classType { get; set; }

            //Kill stats
            public ItemInfo.Projectile lastUsedWep { get; set; }
            public int lastUsedWepKillCount { get; set; }
            public long lastUsedWepTick { get; set; }
            public int lastKillerCount { get; set; }

            //Medic stats
            public int potentialHealthHealed { get; set; }

            public bool onPlayingField { get; set; }
        }
        #endregion

        #region Misc Gameplay Pointers
        private Player lastKiller;

        public Conquest(Arena arena, Script_Multi baseScript)
        {
            _baseScript = baseScript;
            _arena = arena;
            _config = arena._server._zoneConfig;
            _savedPlayerStats = new Dictionary<string, PlayerStat>();
            _activePoints = new List<CapturePoint>();
            _allPoints = new List<CapturePoint>();
            _bots = new List<Bot>();

            cqTeam1 = _arena.getTeamByName("Titan Militia");
            cqTeam2 = _arena.getTeamByName("Collective Military");

        }

        public void setTeams(Team team1, Team team2, bool isScrim)
        {
            if (_team1 != team1)
            {
                _team1 = team1;
                _team2 = team2;
                SwitchTeams();
            }
        }


        public void Poll(int now)
        {

            if (now - _lastTickerUpdate >= 1000)
            {
                UpdateTickers();
                _lastTickerUpdate = now;
            }

            if (now - _lastKillStreakUpdate >= 500)
            {
                UpdateKillStreaks();
                _lastKillStreakUpdate = now;
            }

            if (now - _lastFlagCheck >= 500 && _arena._bGameRunning)
            {
                _lastFlagCheck = now;

                int team1count = _arena._flags.Values.Where(f => f.team == cqTeam1).Count();
                int team2count = _arena._flags.Values.Where(f => f.team == cqTeam2).Count();

                //Has anyone won?
                if (team1count == 0 || team2count == 0)
                    _arena.gameEnd();

                Arena.FlagState team1Flag = _arena._flags.Values.OrderByDescending(f => f.posX).Where(f => f.team == cqTeam1).First();
                Arena.FlagState team2Flag = _arena._flags.Values.OrderBy(f => f.posX).Where(f => f.team == cqTeam2).First();

                int unowned = _arena._flags.Values.Where(f => f.team != cqTeam1 && f.team != cqTeam2).Count();

                _activePoints.Clear();

                if (unowned > 0)
                {
                    _activePoints.Add(_allPoints.FirstOrDefault(p => p._flag.team != cqTeam1 && p._flag.team != cqTeam2));
                }
                else
                {
                    _activePoints.Add(_allPoints.FirstOrDefault(p => p._flag == team1Flag));
                    _activePoints.Add(_allPoints.FirstOrDefault(p => p._flag == team2Flag));
                }

                foreach (Player p in _arena.Players)
                    Helpers.Object_Flags(p, _flags);
            }

            foreach (CapturePoint point in _activePoints)
                point.poll(now);

            if (cqTeam1.ActivePlayerCount > 0 && _arena._bGameRunning)
                pollBots(now);
        }
        #endregion

        #region Gametype Events
        public void gameStart()
        {
            if (_arena.ActiveTeams.Count() == 0)
                return;


            _flags = _arena._flags.Values.OrderBy(f => f.posX).ToList();
            _allPoints = new List<CapturePoint>();



            int flagcount = 1;
            foreach (Arena.FlagState flag in _flags)
            {
                if (flagcount <= 19)
                    flag.team = cqTeam1;
                if (flagcount >= 21)
                    flag.team = cqTeam2;
                flagcount++;

                _allPoints.Add(new CapturePoint(_arena, this, flag));
            }

            foreach (Player p in _arena.Players)
                Helpers.Object_Flags(p, _flags);


            Arena.FlagState team1Flag = _flags.OrderByDescending(f => f.posX).Where(f => f.team == cqTeam1).First();
            Arena.FlagState team2Flag = _flags.OrderBy(f => f.posX).Where(f => f.team == cqTeam2).First();

            _activePoints = new List<CapturePoint>();
            _activePoints.Add(_allPoints.FirstOrDefault(p => p._flag == team1Flag));
            _activePoints.Add(_allPoints.FirstOrDefault(p => p._flag == team2Flag));
            _activePoints.Add(_allPoints.FirstOrDefault(p => p._flag.team == null));

            ResetKiller(null);

            _savedPlayerStats.Clear();
            foreach (Player p in _arena.Players)
            {
                PlayerStat temp = new PlayerStat();
                temp.teamname = p._team._name;
                temp.alias = p._alias;
                temp.points = 0;
                temp.assistPoints = 0;
                temp.bonusPoints = 0;
                temp.killPoints = 0;
                temp.playSeconds = 0;
                temp.squad = p._squad;
                temp.kills = 0;
                temp.deaths = 0;
                temp.player = p;
                temp.hasPlayed = p.IsSpectator ? false : true;
                if (!p.IsSpectator)
                {
                    if (p._baseVehicle != null)
                        temp.classType = p._baseVehicle._type.Name;
                }

                temp.lastKillerCount = 0;
                temp.lastUsedWep = null;
                temp.lastUsedWepKillCount = 0;
                temp.lastUsedWepTick = -1;
                temp.potentialHealthHealed = 0;
                temp.onPlayingField = false;
                _savedPlayerStats.Add(p._alias, temp);
            }


            UpdateTickers();

            int timer = 1800 * 100;


            //Let everyone know
            _arena.sendArenaMessage("Game has started! The team with control of the most flags at the end of the game wins.");
            _arena.setTicker(1, 3, timer, "Time Left: ",
                delegate ()
                {   //Trigger game end
                    _arena.gameEnd();
                }
            );
        }

        public bool playerFlagAction(Player player, bool bPickup, bool bInPlace, LioInfo.Flag flag)
        {
           
            return true;
        }

            /// <summary>
            /// Called when the statistical breakdown is displayed
            /// </summary>
            public void individualBreakdown(Player from, bool bCurrent)
        {
            //Allows additional "custom" breakdown information
            int team1count = _arena._flags.Values.Where(f => f.team == cqTeam1).Count();
            int team2count = _arena._flags.Values.Where(f => f.team == cqTeam2).Count();
            if (team1count > team2count)
                from.sendMessage(0, String.Format("{0} is Victorious with {1} flags", cqTeam1._name, team1count));
            if (team2count > team1count)
                from.sendMessage(0, String.Format("{0} is Victorious with {1} flags", cqTeam2._name, team2count));

            from.sendMessage(0, "#Team Statistics Breakdown");

            IEnumerable<Team> activeTeams = _arena.Teams.OrderByDescending(entry => entry._currentGameKills).ToList();
            int idx = 3;	//Only display top three teams
            foreach (Team t in activeTeams)
            {
                if (t == null)
                    continue;

                if (idx-- == 0)
                    break;

                string format = "!3rd (K={0} D={1}): {2}";
                switch (idx)
                {
                    case 2:
                        format = "!1st (K={0} D={1}): {2}";
                        break;
                    case 1:
                        format = "!2nd (K={0} D={1}): {2}";
                        break;
                }

                from.sendMessage(0, string.Format(format,
                    t._currentGameKills, t._currentGameDeaths,
                    t._name));
            }

            from.sendMessage(0, "#Individual Statistics Breakdown");
            idx = 3;        //Only display the top 3 players
            List<Player> rankers = new List<Player>();
            foreach (Player p in _arena.Players.ToList())
            {
                if (p == null)
                    continue;
                if (_savedPlayerStats.ContainsKey(p._alias) && _savedPlayerStats[p._alias].hasPlayed)
                    rankers.Add(p);
            }

            if (rankers.Count > 0)
            {
                var rankedPlayerGroups = rankers.Select(player => new
                {
                    Alias = player._alias,
                    Kills = _savedPlayerStats[player._alias].kills,
                    Deaths = _savedPlayerStats[player._alias].deaths
                })
                .GroupBy(pl => pl.Kills)
                .OrderByDescending(k => k.Key)
                .Take(idx)
                .Select(g => g.OrderBy(plyr => plyr.Deaths));

                foreach (var group in rankedPlayerGroups)
                {
                    if (idx <= 0)
                        break;

                    string placeWord = "";
                    string format = " (K={0} D={1}): {2}";
                    switch (idx)
                    {
                        case 3:
                            placeWord = "!1st";
                            break;
                        case 2:
                            placeWord = "!2nd";
                            break;
                        case 1:
                            placeWord = "!3rd";
                            break;
                    }

                    idx -= group.Count();
                    if (group.First() != null)
                        from.sendMessage(0, string.Format(placeWord + format, group.First().Kills,
                            group.First().Deaths, string.Join(", ", group.Select(g => g.Alias))));
                }

                IEnumerable<Player> specialPlayers = rankers.OrderByDescending(player => _savedPlayerStats[player._alias].deaths);
                int topDeaths = (specialPlayers.First() != null ? _savedPlayerStats[specialPlayers.First()._alias].deaths : 0), deaths = 0;
                if (topDeaths > 0)
                {
                    from.sendMessage(0, "Most Deaths");
                    int i = 0;
                    List<string> mostDeaths = new List<string>();
                    foreach (Player p in specialPlayers)
                    {
                        if (p == null)
                            continue;

                        if (_savedPlayerStats.ContainsKey(p._alias))
                        {
                            deaths = _savedPlayerStats[p._alias].deaths;
                            if (deaths == topDeaths)
                            {
                                if (i++ >= 1)
                                    mostDeaths.Add(p._alias);
                                else
                                    mostDeaths.Add(string.Format("(D={0}): {1}", deaths, p._alias));
                            }
                        }
                    }
                    if (mostDeaths.Count > 0)
                    {
                        string s = string.Join(", ", mostDeaths.ToArray());
                        from.sendMessage(0, s);
                    }
                }

                IEnumerable<Player> Healed = rankers.Where(player => _savedPlayerStats[player._alias].potentialHealthHealed > 0);
                if (Healed.Count() > 0)
                {
                    IEnumerable<Player> mostHealed = Healed.OrderByDescending(player => _savedPlayerStats[player._alias].potentialHealthHealed);
                    idx = 3;
                    from.sendMessage(0, "&Most HP Healed");
                    foreach (Player p in mostHealed)
                    {
                        if (p == null) continue;
                        if (_savedPlayerStats[p._alias] != null)
                        {
                            if (idx-- <= 0)
                                break;

                            string placeWord = "&3rd";
                            string format = " (HP Total={0}): {1}";
                            switch (idx)
                            {
                                case 2:
                                    placeWord = "&1st";
                                    break;
                                case 1:
                                    placeWord = "&2nd";
                                    break;
                            }
                            from.sendMessage(0, string.Format(placeWord + format, _savedPlayerStats[p._alias].potentialHealthHealed, p._alias));
                        }
                    }
                }
            }

            //Are they on the list?
            if (_savedPlayerStats.ContainsKey(from._alias))
            {
                string personalFormat = "!Personal Score: (K={0} D={1})";
                from.sendMessage(0, string.Format(personalFormat,
                    _savedPlayerStats[from._alias].kills,
                    _savedPlayerStats[from._alias].deaths));
            }
            //If not, give them the generic one
            else
            {
                string personalFormat = "!Personal Score: (K=0 D=0)";
                from.sendMessage(0, personalFormat);
            }
        }

        public void gameEnd()
        {
            if (_isScrim)
            _isScrim = false;
        }

        public void gameReset()
        {

        }
        #endregion

        #region Player Events
        public bool playerUnspec(Player player)
        {

            //Add them to the list if not in it
            if (!_savedPlayerStats.ContainsKey(player._alias))
            {
                PlayerStat temp = new PlayerStat();
                temp.alias = player._alias;
                temp.squad = player._squad;
                temp.assistPoints = 0;
                temp.bonusPoints = 0;
                temp.killPoints = 0;
                temp.points = 0;
                temp.playSeconds = 0;
                temp.deaths = 0;
                temp.kills = 0;
                temp.player = player;
                temp.onPlayingField = false;
                _savedPlayerStats.Add(player._alias, temp);
            }
            _savedPlayerStats[player._alias].hasPlayed = true;
            if (player._baseVehicle != null)
                _savedPlayerStats[player._alias].classType = player._baseVehicle._type.Name;

            pickTeam(player);

            return true;
        }


        private void pickTeam(Player player)
        {

            if (_team1.ActivePlayerCount <= _team2.ActivePlayerCount)
            {
                if (player._team != _team1)
                {
                    player.unspec(_team1);
                }
            }
            else
            {
                if (player._team != _team2)
                {
                    player.unspec(_team2);
                }
            }
        }

        public void playerSpec(Player player)
        {


        }

        public void playerSpawn(Player player, bool death)
        {
        }

            public void playerEnterArena(Player player)
        {
            //Add them to the list if not in it
            if (!_savedPlayerStats.ContainsKey(player._alias))
            {
                PlayerStat temp = new PlayerStat();
                temp.squad = player._squad;
                temp.assistPoints = 0;
                temp.bonusPoints = 0;
                temp.killPoints = 0;
                temp.points = 0;
                temp.playSeconds = 0;
                temp.alias = player._alias;
                temp.deaths = 0;
                temp.kills = 0;
                temp.player = player;
                temp.onPlayingField = false;
                _savedPlayerStats.Add(player._alias, temp);
            }
            _savedPlayerStats[player._alias].teamname = player._team._name;
            _savedPlayerStats[player._alias].hasPlayed = player.IsSpectator ? false : true;
            if (player._baseVehicle != null)
                _savedPlayerStats[player._alias].classType = player._baseVehicle._type.Name;
        }

        public void playerLeaveArena(Player player)
        {

        }

        /// <summary>
        /// Triggered when a player tries to heal
        /// </summary>
        public void PlayerRepair(Player from, ItemInfo.RepairItem item)
        {
            if (!_savedPlayerStats.ContainsKey(from._alias))
                return;
            if (item.repairType == 0 && item.repairDistance < 0)
            {   //Get all players near
                List<Player> players = _arena.getPlayersInRange(from._state.positionX, from._state.positionY, -item.repairDistance);
                int totalHealth = 0;
                foreach (Player p in players)
                {
                    if (p == null || p == from || p._state.health >= 100 || p._state.health <= 0)
                        continue;
                    totalHealth += (p._baseVehicle._type.Hitpoints - p._state.health);
                }
                _savedPlayerStats[from._alias].potentialHealthHealed += totalHealth;
            }
        }


        public void playerExplosion(Player player, ItemInfo.Projectile weapon, short posX, short posY, short posZ)
        {

        }

        public void playerPlayerKill(Player victim, Player killer)
        {   //Update our kill streak
             UpdateKiller(killer);
             UpdateDeath(victim, killer);

            if (_savedPlayerStats.ContainsKey(killer._alias))
            {
                _savedPlayerStats[killer._alias].kills++;
                long wepTick = _savedPlayerStats[killer._alias].lastUsedWepTick;
                if (wepTick != -1)
                    UpdateWeaponKill(killer);
            }
            if (_savedPlayerStats.ContainsKey(victim._alias))
                _savedPlayerStats[victim._alias].deaths++;

            if (killer != null && victim != null && victim._bounty >= 300)
                _arena.sendArenaMessage(string.Format("{0} has ended {1}'s bounty.", killer._alias, victim._alias), 8);
        }

        public void playerDeath(Player victim, Player killer, Helpers.KillType killType, CS_VehicleDeath update)
        {
         
        }
        #endregion


        #region Updation Calls

        /// <summary>
        /// Switches the teams before a game start.
        /// </summary>
        private void SwitchTeams()
        {
            foreach (var player in _arena.PlayersIngame)
            {
                //Sanity checks
                if (_team1 == null || _team2 == null)
                    break;

                if (_team1.ActivePlayerCount <= _team2.ActivePlayerCount)
                {
                    if (player._team != _team1)
                        _team1.addPlayer(player);
                }
                else
                {
                    if (player._team != _team2)
                        _team2.addPlayer(player);
                }

            }
        }

        /// <summary>
        /// Updates our players kill streak timer
        /// </summary>
        private void UpdateKillStreaks()
        {
            foreach (PlayerStat p in _savedPlayerStats.Values)
            {
                if (p.lastUsedWepTick == -1)
                    continue;

                if (Environment.TickCount - p.lastUsedWepTick <= 0)
                    ResetWeaponTicker(p.player);
            }
        }

        /// <summary>
        /// Updates our tickers
        /// </summary>
        public void UpdateTickers()
        {
            if (!_arena._bGameRunning)
            { return; }

            IEnumerable<Team> active = _arena.ActiveTeams;
            if (activeTeams != null && activeTeams.Count() > 0)
            {
                active = activeTeams;
            }

            Team collie = active.Count() > 1 ? active.ElementAt(1) : _arena.getTeamByName(_config.teams[0].name);
            Team titan = active.Count() > 0 ? active.ElementAt(0) : _arena.getTeamByName(_config.teams[1].name);

            string format = string.Format("{0}={1} - {2}={3}", titan._name, titan._currentGameKills, collie._name, collie._currentGameKills);
            //We playing more events at the same time?
            if (active.Count() > 3)
            {
                Team third = active.ElementAt(2);
                Team fourth = active.ElementAt(3);
                format = string.Format("{0}={1} - {2}={3} | {4}={5} - {6}={7}", titan._name, titan._currentGameKills, collie._name, collie._currentGameKills,
                    third._name, third._currentGameKills, fourth._name, fourth._currentGameKills);
            }
            _arena.setTicker(1, 2, 0, format);

            //Personal Scores
            _arena.setTicker(2, 1, 0, delegate (Player p)
            {
                //Update their ticker
                if (_savedPlayerStats.ContainsKey(p._alias))
                    return string.Format("HP={0}          Personal Score: Kills={1} - Deaths={2}",
                        p._state.health,
                        _savedPlayerStats[p._alias].kills,
                        _savedPlayerStats[p._alias].deaths);
                return "";
            });

            //1st and 2nd place
            List<Player> ranked = new List<Player>();
            foreach (Player p in _arena.Players)
            {
                if (p == null)
                    continue;
                if (_savedPlayerStats.ContainsKey(p._alias) && _savedPlayerStats[p._alias].hasPlayed)
                    ranked.Add(p);
            }

            IEnumerable<Player> ranking = ranked.OrderBy(player => _savedPlayerStats[player._alias].deaths).OrderByDescending(player => _savedPlayerStats[player._alias].kills);
            int idx = 3; format = "";
            foreach (Player rankers in ranking)
            {
                if (idx-- == 0)
                    break;

                switch (idx)
                {
                    case 2:
                        format = string.Format("1st: {0}(K={1} D={2})", rankers._alias,
                          _savedPlayerStats[rankers._alias].kills, _savedPlayerStats[rankers._alias].deaths);
                        break;
                    case 1:
                        format = (format + string.Format(" 2nd: {0}(K={1} D={2})", rankers._alias,
                          _savedPlayerStats[rankers._alias].kills, _savedPlayerStats[rankers._alias].deaths));
                        break;
                }
            }
            if (!_arena.recycling)
                _arena.setTicker(2, 0, 0, format);
        }

        /// <summary>
        /// Resets the last killer object
        /// </summary>
        public void ResetKiller(Player killer)
        {
            lastKiller = killer;
        }

        /// <summary>
        /// Resets the weapon ticker to default (Time Expired)
        /// </summary>
        public void ResetWeaponTicker(Player target)
        {
            if (_savedPlayerStats.ContainsKey(target._alias))
            {
                _savedPlayerStats[target._alias].lastUsedWep = null;
                _savedPlayerStats[target._alias].lastUsedWepKillCount = 0;
                _savedPlayerStats[target._alias].lastUsedWepTick = -1;
            }
        }

        /// <summary>
        /// Updates the killer and their counter
        /// </summary>
        public void UpdateKiller(Player killer)
        {
            if (_savedPlayerStats.ContainsKey(killer._alias))
            {
                _savedPlayerStats[killer._alias].lastKillerCount++;
                switch (_savedPlayerStats[killer._alias].lastKillerCount)
                {
                    case 6:
                        _arena.sendArenaMessage(string.Format("{0} is on fire!", killer._alias), 17);
                        break;
                    case 8:
                        _arena.sendArenaMessage(string.Format("Someone kill {0}!", killer._alias), 18);
                        break;
                    case 10:
                        _arena.sendArenaMessage(string.Format("{0} is dominating!", killer._alias), 19);
                        break;
                    case 12:
                        _arena.sendArenaMessage(string.Format("DEATH TO {0}!", killer._alias), 30);
                        break;
                }
            }

            //Is this first blood?
            if (lastKiller == null)
            {
                //It is, lets make the sound
                _arena.sendArenaMessage(string.Format("{0} has drawn first blood.", killer._alias), 9);
            }
            lastKiller = killer;
        }

        /// <summary>
        /// Updates the victim's kill streaks
        /// </summary>
        public void UpdateDeath(Player victim, Player killer)
        {
            if (_savedPlayerStats.ContainsKey(victim._alias))
            {
                if (_savedPlayerStats[victim._alias].lastKillerCount >= 6)
                {
                    _arena.sendArenaMessage(string.Format("{0}", killer != null ? killer._alias + " has ended " + victim._alias + "'s kill streak." :
                        victim._alias + "'s kill streak has ended."), 7);
                }
                _savedPlayerStats[victim._alias].lastKillerCount = 0;
            }
        }

        /// <summary>
        /// Updates the last fired weapon and the ticker
        /// </summary>
        public void UpdateWeapon(Player from, ItemInfo.Projectile usedWep)
        {
            if (_savedPlayerStats.ContainsKey(from._alias))
            {
                _savedPlayerStats[from._alias].lastUsedWep = usedWep;
                //500 = Alive time for the schrapnel after main weap explosion
                _savedPlayerStats[from._alias].lastUsedWepTick = DateTime.Now.AddTicks(500).Ticks;
            }
        }

        /// <summary>
        /// Updates the last weapon kill counter
        /// </summary>
        public void UpdateWeaponKill(Player from)
        {
            if (_savedPlayerStats.ContainsKey(from._alias))
            {
                _savedPlayerStats[from._alias].lastUsedWepKillCount++;
                ItemInfo.Projectile lastUsedWep = _savedPlayerStats[from._alias].lastUsedWep;
                if (lastUsedWep == null)
                    return;

                if (lastUsedWep.name.Contains("Combat Knife"))
                    _arena.sendArenaMessage(string.Format("{0} is throwing out the knives.", from._alias), 6);

                switch (_savedPlayerStats[from._alias].lastUsedWepKillCount)
                {
                    case 2:
                        _arena.sendArenaMessage(string.Format("{0} just got a double {1} kill.", from._alias, lastUsedWep.name), 13);
                        break;
                    case 3:
                        _arena.sendArenaMessage(string.Format("{0} just got a triple {1} kill.", from._alias, lastUsedWep.name), 14);
                        break;
                    case 4:
                        _arena.sendArenaMessage(string.Format("A 4 {0} kill by {0}?!?", lastUsedWep.name, from._alias), 15);
                        break;
                    case 5:
                        _arena.sendArenaMessage(string.Format("Unbelievable! {0} with the 5 {1} kill?", from._alias, lastUsedWep.name), 16);
                        break;
                }
            }
        }
    }
}
#endregion