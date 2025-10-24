using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using S1thK3nny.SWAT.Models.Teams;
using S1thK3nny.SWAT.Models.Databases;
using S1thK3nny.SWAT.Helpers;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandRegisterToTeam : IRocketCommand
    {
        private SWATPlugin pluginInstance => SWATPlugin.Instance;

        // TODO: Ensure early return if ran during match
        public void Execute(IRocketPlayer caller, string[] command)
        {
            // Check if allegiance argument is provided
            if (command.Length < 1)
            {
                ChatHelper.SendTo(caller, "CommandRegisterToTeamSyntax", ChatLevel.INFO);
                return;
            }

            if (GameStateManager.Instance.CurrentState != GameState.Idle)
            {
                ChatHelper.SendTo(caller, "GameIsCurrentlyRunning", ChatLevel.ERROR);
                return;
            }

            // Parse allegiance, check if there already is one
            if (!CommandHelpers.TryParseAllegiance(command[0], out ALLEGIANCE allegiance, out var errKeyA, out var errArgsA))
            {
                ChatHelper.SendTo(caller, errKeyA, ChatLevel.ERROR, errArgsA);
                return;
            }

            // Resolve target Steam64ID and check who called the command
            if (!CommandHelpers.TryResolveTargetSteam64ID(caller, command, 1, out var targetSteam64ID, out var errKey, out var errArgs))
            {
                ChatHelper.SendTo(caller, errKey, ChatLevel.ERROR, errArgs ?? Array.Empty<object>());
                return;
            }

            // Check if already registered
            var existingData = pluginInstance.AllegianceDatabase.Allegiances
                .FirstOrDefault(x => x.Steam64ID == targetSteam64ID);
            if (existingData != null)
            {
                existingData.Team = allegiance;

                PlayerNameHelper.SetPlayerName(targetSteam64ID, allegiance);
                ChatHelper.SendTo(caller, "PlayerSwitchedTeams", ChatLevel.OK, new[] { Convert.ToString(targetSteam64ID), allegiance.ToString() });

                pluginInstance.AllegianceDatabase.Save();
                return;
            }

            // Register
            var newData = new AllegianceData { Steam64ID = targetSteam64ID, Team = allegiance };
            pluginInstance.AllegianceDatabase.Allegiances.Add(newData);
            pluginInstance.AllegianceDatabase.Save();

            PlayerNameHelper.SetPlayerName(targetSteam64ID, allegiance);
            ChatHelper.SendTo(caller, "PlayerRegisteredToTeam", ChatLevel.OK, new[] { Convert.ToString(targetSteam64ID), allegiance.ToString() });
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "SWATRegister";

        public string Help => "Register a player to a team. Options are SWAT or TERRORIST.";

        public string Syntax => "<Allegiance> [Steam64ID or PlayerName]";
    
        public List<string> Aliases => [ "sregister", "steam" ];

        public List<string> Permissions => [ "swat.register" ];
    }
}
