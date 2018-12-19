using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfServer.Script.GameType_Multi
{
    public abstract class Settings
    {

        //KillRewards
        public const int c_baseReward = 25;
        public const double c_pointMultiplier = 2;
        public const double c_cashMultiplier = 1;
        public const double c_expMultiplier = 0.5;
        public const int c_percentOfVictim = 50;
        public const int c_percentOfOwn = 3;
        public const int c_percentOfOwnIncrease = 5;

        //SupplyDrops
        public const int c_supply_openRadius = 150;

        //Jackpot Conquest
        public const int c_jackpot_CQ_Fixed = 4000;
        public const int c_jackpot_CQ_PointsPerKill = 20;
        public const int c_jackpot_CQ_PointsPerDeath = 3;
        public const int c_jackpot_CQ_PointsPerFlag = 5;
        public const double c_jackpot_CQ_PointsPerHP = 0.035;
        public const double c_jackpot_CQ_WinnerPointsPerSecond = 0.2;
        public const double c_jackpot_CQ_LoserPointsPerSecond = 0.1;

        //Jackpot CO-OP
        public const int c_jackpot_Co_Fixed = 8000;
        public const int c_jackpot_Co_PointsPerKill = 2;
        public const int c_jackpot_Co_PointsPerDeath = 4;
        public const int c_jackpot_Co_PointsPerFlag = 5;
        public const double c_jackpot_Co_PointsPerHP = 0.035;
        public const double c_jackpot_Co_WinnerPointsPerSecond = 0.2;
        public const double c_jackpot_Co_LoserPointsPerSecond = 0.1;


        //JackpotRewards
        public const double c_jackpot_CashMultiplier = 0.5;
        public const double c_jackpot_ExpMultiplier = 0.25;

        public enum GameTypes
        {
            NULL,
            Conquest,
            Coop
        }
    }
}