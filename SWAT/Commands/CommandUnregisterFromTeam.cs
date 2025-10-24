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
    public class CommandUnregisterFromTeam : IRocketCommand
    {
        private SWATPlugin pluginInstance => SWATPlugin.Instance;

        // TODO: Ensure early return if ran during match
        public void Execute(IRocketPlayer caller, string[] command)
        {

            if (!CommandHelpers.TryResolveTargetSteam64ID(caller, command, 0, out var targetSteam64ID, out var errKey, out var errArgs))
            {
                ChatHelper.SendTo(caller, errKey, ChatLevel.ERROR, errArgs ?? Array.Empty<object>());
                return;
            }

            // Check if registered
            var existingData = pluginInstance.AllegianceDatabase.Allegiances
                .FirstOrDefault(x => x.Steam64ID == targetSteam64ID);

            if (existingData == null)
            {
                ChatHelper.SendTo(caller, "PlayerNotRegistered", ChatLevel.ERROR, new[] { targetSteam64ID });
                return;
            }

            pluginInstance.AllegianceDatabase.Allegiances.Remove(existingData);
            pluginInstance.AllegianceDatabase.Save();

            PlayerNameHelper.RemoveTag(targetSteam64ID);
            ChatHelper.SendTo(caller, "PlayerUnregisteredFromTeam", ChatLevel.OK, new[] { Convert.ToString(targetSteam64ID), existingData.Team.ToString() });
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "SWATUnregister";

        public string Help => "Unregister a player from a team.";

        public string Syntax => "[Steam64ID or PlayerName]";

        public List<string> Aliases => [ "sunregister" ];

        public List<string> Permissions => [ "swat.unregister" ];
    }
}
