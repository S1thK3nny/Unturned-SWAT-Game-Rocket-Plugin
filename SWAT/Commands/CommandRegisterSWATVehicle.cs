using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using S1thK3nny.SWAT.Models.Databases;
using S1thK3nny.SWAT.Helpers;
using SDG.Unturned;
using UnityEngine;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandRegisterSWATVehicle : IRocketCommand
    {
        private SWATPlugin pluginInstance => SWATPlugin.Instance;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            (Vector3 unityPos, Vector3 unityRot) = CommandHelpers.GetPlayerPositionAndRotation((UnturnedPlayer)caller);

            if (command.Length == 0)
            {
                ChatHelper.SendTo(caller, "CommandRegisterSWATVehicleSyntax", ChatLevel.INFO);
                return;
            }

            if (GameStateManager.Instance.CurrentState != GameState.Idle)
            {
                ChatHelper.SendTo(caller, "GameIsCurrentlyRunning", ChatLevel.ERROR);
                return;
            }

            var payload = new SwatVehicleInfos
            {
                VehicleID = ushort.Parse(command[0]),
                SpawnPosition = unityPos,
                SpawnRotation = unityRot
            };

            string mapId = Provider.map ?? "Unknown";

            PerMapInfos db = pluginInstance.perMapInfosDatabase.Database ?? new PerMapInfos();
            var map = CommandHelpers.ensureMapInfoExists(db, mapId);

            map.SwatVehicleInfos ??= new SwatVehicleInfos();

            var existing = map.SwatVehicleInfos; ;
            if (existing != null)
            {
                // Overwrite existing
                existing.VehicleID = payload.VehicleID;
                existing.SpawnPosition = payload.SpawnPosition;
                existing.SpawnRotation = payload.SpawnRotation;
            }
            else
            {
                // New entry
                map.SwatVehicleInfos = payload;
            }

            pluginInstance.perMapInfosDatabase.Save();

            Console.WriteLine($"{ScriptTag.GetScriptName()} Registered SWAT vehicle {payload.VehicleID} at position {payload.SpawnPosition} on map {mapId}.");
            ChatHelper.SendTo(caller, "CommandRegisterSWATVehicleSaved", ChatLevel.OK, new[] { $"{payload.VehicleID}", mapId });
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "SWATVehicle";

        public string Help => "Register a vehicle and its spawn point for the SWAT team on the current map.";

        public string Syntax => "<vehicleID>";

        public List<string> Aliases => ["svehicle"];

        public List<string> Permissions => ["swat.vehicle"];
    }
}
