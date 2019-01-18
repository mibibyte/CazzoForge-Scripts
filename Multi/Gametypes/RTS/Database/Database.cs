using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;


using InfServer.Game;
using InfServer.Protocol;
using InfServer.Scripting;
using InfServer.Bots;

using Assets;

namespace InfServer.Script.GameType_Multi
{
    public class Database
    {
        public Dictionary<ushort, Building> _buildings;
        private Arena _arena;

        public Database(Arena arena)
        {
            _arena = arena;
        }

        public void loadBuildings(string player, Team team)
        {
            _buildings = new Dictionary<ushort, Building>();
            Building building;
            XmlDocument Doc = new XmlDocument();
            Doc.Load(System.Environment.CurrentDirectory + "/Data/RTS/Buildings/" + player + ".xml");

            XmlNode header = Doc.SelectSingleNode("buildingTable");

            foreach (XmlNode Node in Doc.SelectNodes("buildingTable/building"))
            {
                foreach (XmlNode child in Node.ChildNodes)
                {
                    building = new Building();
                    building._id = Convert.ToUInt16(child.Attributes["id"].Value);
                    building._name = child.Attributes["name"].Value;
                    building._type = AssetManager.Manager.getVehicleByID(Convert.ToInt32(child.Attributes["vehicleID"].Value));

                    building._state = new Helpers.ObjectState();
                    building._state.positionX = Convert.ToInt16(child.Attributes["positionX"].Value);
                    building._state.positionY = Convert.ToInt16(child.Attributes["positionY"].Value);
                    building._state.health = Convert.ToInt16(child.Attributes["health"].Value);

                    //Add it
                    _buildings.Add(building._id, building);
                }
            }



            foreach (Building b in _buildings.Values)
            {
                Vehicle newVeh = _arena.newVehicle(b._type, team, null, b._state);

                if (newVeh == null)
                    Log.write("[RTS] Could not spawn vehicle");
            }
        }
    }
}
