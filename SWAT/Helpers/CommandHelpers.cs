using Rocket.API;
using Rocket.Unturned.Player;
using S1thK3nny.SWAT.Models.Teams;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using S1thK3nny.SWAT.Models.Databases;

namespace S1thK3nny.SWAT.Helpers
{
    public static class CommandHelpers
    {
        /// <summary>
        /// Ermittelt die Ziel-Steam64ID aus dem Command-Array (optional) oder dem Caller.
        /// Gibt bei Fehlern einen Übersetzungsschlüssel + Platzhalter zurück.
        /// </summary>
        public static bool TryResolveTargetSteam64ID(
            IRocketPlayer caller,
            string[] command,
            int argIndex,
            out ulong steam64,
            out string errorKey,
            out object[] errorArgs)
        {
            steam64 = 0;
            errorKey = null;
            errorArgs = null;

            // Fall 1: Steam64 via Argument
            if (command.Length > argIndex)
            {
                if (!ulong.TryParse(command[argIndex], out steam64))
                {
                    errorKey = "InvalidSteam64ID";
                    errorArgs = new object[] { command[argIndex] };
                    return false;
                }
                return true;
            }

            // Fall 2: Aus Caller ableiten
            if (caller is ConsolePlayer)
            {
                errorKey = "MustSpecifySteam64IDFromConsole";
                return false;
            }

            var player = (UnturnedPlayer)caller;
            steam64 = player.CSteamID.m_SteamID;
            return true;
        }

        /// <summary>
        /// Optional: Allegiance robust parsen.
        /// </summary>
        public static bool TryParseAllegiance(
            string token,
            out ALLEGIANCE allegiance,
            out string errorKey,
            out object[] errorArgs)
        {
            errorKey = null;
            errorArgs = null;

            if (Enum.TryParse(token, true, out allegiance) && allegiance != ALLEGIANCE.None)
                return true;

            allegiance = ALLEGIANCE.None;
            errorKey = "InvalidAllegiance";
            errorArgs = new object[] { token };
            return false;
        }

        /// <summary>
        /// Gets the position and rotation of the given player and returns them as a tuple.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static (Vector3, Vector3) GetPlayerPositionAndRotation(UnturnedPlayer player)
        {
            Vector3 position = player.Player.transform.position;
            Vector3 rotation = player.Player.transform.rotation.eulerAngles;
            return (position, rotation);
        }

        public static MapInfo ensureMapInfoExists(PerMapInfos db, string mapId)
        {
            db.Maps ??= new List<MapInfo>();

            var map = db.Maps.FirstOrDefault(m => string.Equals(m.Id, mapId, StringComparison.OrdinalIgnoreCase));
            if (map == null)
            {
                map = new MapInfo { Id = mapId };
                db.Maps.Add(map);
            }
            return map;
        }
    }
}
