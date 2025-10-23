using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using S1thK3nny.SWAT.Models.Teams;
using S1thK3nny.SWAT.Models.Databases;
using S1thK3nny.SWAT.Helpers;
using SDG.Unturned;
using UnityEngine;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandRegisterPosition : IRocketCommand
    {
        private SWATPlugin pluginInstance => SWATPlugin.Instance;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            ulong steam64ID = ((UnturnedPlayer)caller).CSteamID.m_SteamID;
            ALLEGIANCE allegiance;

            if (command.Length > 0)
            {
                // Parse allegiance, check if there already is one
                if (!CommandHelpers.TryParseAllegiance(command[0], out allegiance, out var errKeyA, out var errArgsA))
                {
                    ChatHelper.SendTo(caller, errKeyA, ChatLevel.ERROR, errArgsA);
                    return;
                }
            }
            else
            {
                allegiance = pluginInstance.getPlayerAllegiance(steam64ID);
                if (allegiance == ALLEGIANCE.None)
                {
                    ChatHelper.SendTo(caller, "CommandRegisterPositionSyntax", ChatLevel.INFO);
                    return;
                }
            }

            (Vector3 unityPos, Vector3 unityRot) = CommandHelpers.GetPlayerPositionAndRotation((UnturnedPlayer)caller);

            var payload = new PlayerInfo
            {
                Steam64Id = steam64ID,
                Position = unityPos,
                Rotation = unityRot
            };

            string mapId = Provider.map ?? "Unknown";
            
            PerMapInfos db = pluginInstance.perMapInfosDatabase.Database ?? new PerMapInfos();
            var map = CommandHelpers.ensureMapInfoExists(db, mapId);

            map.Allegiances ??= new List<AllegianceInfo>();

            // Ensure AllegianceInfo exists
            string teamKey = allegiance.ToString();
            var team = map.Allegiances.FirstOrDefault(a => string.Equals(a.Team, teamKey, StringComparison.OrdinalIgnoreCase));
            if (team == null)
            {
                team = new AllegianceInfo { Team = teamKey };
                map.Allegiances.Add(team);
            }

            team.Players ??= new List<PlayerInfo>();

            // Upsert player record
            var existing = team.Players.FirstOrDefault(p => p.Steam64Id == steam64ID);
            if (existing != null)
            {
                existing.Position = payload.Position;
                existing.Rotation = payload.Rotation;
            }
            else
            {
                team.Players.Add(payload);
            }

            pluginInstance.perMapInfosDatabase.Save();

            Console.WriteLine($"[SWATPlugin] Registered position for player {steam64ID} at position {payload.Position} on map {mapId} for team {teamKey}.");
            ChatHelper.SendTo(caller, "CommandRegisterPositionSaved", ChatLevel.OK, new[] { Convert.ToString(steam64ID), teamKey, mapId });
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "SWATPosition";

        public string Help => "Register a position for a player. If they are in a team, it will save it under their allegiance. Otherwise, ensure the allegiance is specified.";

        public string Syntax => "[ALLEGIANCE]";

        public List<string> Aliases => [ "sposition" ];

        public List<string> Permissions => [ "swat.position" ];
    }
}
