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
    {

        public bool _bHappyHour;

        public bool isHappyHour(TimeSpan start, TimeSpan end)
        {
                // convert datetime to a TimeSpan
                TimeSpan now = DateTime.Now.TimeOfDay;
                // see if start comes before end
                if (start < end)
                    return start <= now && now <= end;
                // start is after end, so do the inverse comparison
                return !(end < now && now < start);
        }


    }
}