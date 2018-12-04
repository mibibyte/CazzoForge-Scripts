using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfServer.Script.GameType_Multi
{
    public abstract class Settings
    {
        public static bool Events = false;			//Are events enabled? (mini-maps/etc)
        public static bool Voting = true;	        //Can we vote?
        public static int VotingTime = 30;         //How long our voting time is   
        public static int LeagueSeason;
        public static int demoRounds = 5;          //Max SnD rounds

        public enum GameTypes
        {
            NULL,
            Conquest,
            Coop
        }

        public enum Maps
        {
            RedBlue,
            KaiserPass,
            GreenYellow,
            WhiteBlack,
            PinkPurple,
            GoldSilver,
            BronzeDiamond,
            OrangeGray
        }
    }
}