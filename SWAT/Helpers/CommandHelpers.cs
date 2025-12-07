using Rocket.API;
using Rocket.Unturned.Player;
using S1thK3nny.SWAT.Models.Teams;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using S1thK3nny.SWAT.Models.Databases;
using SDG.Unturned;

namespace S1thK3nny.SWAT.Helpers
{
    public static class CommandHelpers
    {
        /// <summary>
        /// Resolves the target Steam64ID from the command array (optional) or from the caller.
        /// Accepts either a Steam64ID or a player name.
        /// Returns an error key and placeholders on failure.
        /// </summary>
        /// <param name="caller">The command caller.</param>
        /// <param name="command">The command arguments.</param>
        /// <param name="argIndex">The index of the argument to check for Steam64ID or player name.</param>
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

            // Case 1: Steam64ID or PlayerName via Argument
            if (command.Length > argIndex)
            {
                string input = command[argIndex];
                
                // First, try to parse it as a Steam64ID
                if (ulong.TryParse(input, out steam64))
                {
                    return true;
                }
                
                // If it's not a Steam64ID, try to find it as a player name
                Player foundPlayer = null;
                
                // Search for exact name match (case-insensitive)
                foreach (var client in Provider.clients)
                {
                    if (client.player == null) continue;
                    
                    string playerName = client.player.channel.owner.playerID.characterName;
                    if (string.Equals(playerName, input, StringComparison.OrdinalIgnoreCase))
                    {
                        foundPlayer = client.player;
                        break;
                    }
                }
                
                // If no exact match, search for partial match
                if (foundPlayer == null)
                {
                    foreach (var client in Provider.clients)
                    {
                        if (client.player == null) continue;
                        
                        string playerName = client.player.channel.owner.playerID.characterName;
                        if (playerName.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foundPlayer = client.player;
                            break;
                        }
                    }
                }
                
                if (foundPlayer != null)
                {
                    steam64 = foundPlayer.channel.owner.playerID.steamID.m_SteamID;
                    return true;
                }
                
                // Neither Steam64ID nor player name found
                errorKey = "InvalidSteam64IDOrPlayerName";
                errorArgs = new object[] { input };
                return false;
            }

            // Case 2: Derive from caller
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
        /// Parses allegiance from string input robustly.
        /// Supports abbreviations: S for SWAT, T for TERRORIST.
        /// </summary>
        public static bool TryParseAllegiance(
            string token,
            out ALLEGIANCE allegiance,
            out string errorKey,
            out object[] errorArgs)
        {
            errorKey = null;
            errorArgs = null;
            token = token.ToUpper();

            // Try direct enum parse first (SWAT, TERRORIST, etc.)
            if (Enum.TryParse(token, true, out allegiance) && allegiance != ALLEGIANCE.None)
                return true;

            // Check for abbreviations
            switch (token.ToUpper())
            {
                case "S":
                    allegiance = ALLEGIANCE.SWAT;
                    return true;
                case "T":
                    allegiance = ALLEGIANCE.TERRORIST;
                    return true;
            }

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
