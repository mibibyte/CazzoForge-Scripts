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

        //Delay Variables
        private int _marineSpawnDelay = 3000;
        private int _ripperSpawnDelay = 8000;
        private int _medicSpawnDelay = 7000;
        private int _supplyDropDelay = 140000;

        //Wave Variables
        private int _lastFriendlyMedic;
        private int _lastWaveModifier;
        private int _lastMedicWave;
        private int _lastSupplyDrop;
        private int _lastMarineWave;
        private int _lastRipperWave;
        private bool _firstRushWave;
        private bool _secondRushWave;
        private bool _thirdRushWave;

        private bool _firstBoss;
        private bool _secondBoss;
        public bool _dropSupplies;

        public bool spawnBots;

        private int _botMax = 45;

    }
}
