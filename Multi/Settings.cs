using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfServer.Script.GameType_Multi
{
    public abstract class Settings
    {

        //Rewards
        public const int c_baseReward = 25;
        public const double c_pointMultiplier = 2;
        public const double c_cashMultiplier = 1;
        public const double c_expMultiplier = 0.5;
        public const int c_percentOfVictim = 50;
        public const int c_percentOfOwn = 3;
        public const int c_percentOfOwnIncrease = 5;


        public enum GameTypes
        {
            NULL,
            Conquest,
            Coop
        }
    }
}