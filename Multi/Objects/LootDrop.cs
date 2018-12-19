using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;


using InfServer.Game;
using InfServer.Protocol;
using InfServer.Scripting;
using InfServer.Bots;

using Assets;

namespace InfServer.Script.GameType_Multi
{
    public class LootDrop
    {
        private List<ItemInfo> _loot;           //The loot in our drop
        public Player _owner;                   //The player that owns this loot (Only visible to him)

        /// <summary>
        /// Constructor
        /// </summary>
        public LootDrop()
        {

        }
    }
}
