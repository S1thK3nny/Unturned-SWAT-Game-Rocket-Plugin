using Rocket.API;
using S1thK3nny.SWAT.Helpers;
using S1thK3nny.SWAT.Models.Teams;
using System;
using System.Collections.Generic;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandSetKit : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "setkit";

        public string Help => "Sets the player's kit depending on their team";

        public string Syntax => "<kitname> [allegiance] [Steam64ID or PlayerName]";

        public List<string> Aliases => new List<string> { "skit" };

        public List<string> Permissions => new List<string> { "swat.setkit" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                ChatHelper.SendTo(caller, "CommandSetKitSyntax", ChatLevel.INFO);
                return;
            }

            string kitName = command[0];
            ALLEGIANCE givenAllegiance = ALLEGIANCE.None;

            string errKey = null;
            object[] errArgs = null;

            if (command.Length > 1 && command[1] != null)
            {
                if (!CommandHelpers.TryParseAllegiance(command[1], out givenAllegiance, out errKey, out errArgs))
                {
                    ChatHelper.SendTo(caller, errKey, ChatLevel.ERROR, errArgs);
                    return;
                }
            }

            ulong steam64ID = 0;
            if (command.Length == 3 && !CommandHelpers.TryResolveTargetSteam64ID(caller, command, 2, out steam64ID, out errKey, out errArgs))
            {
                ChatHelper.SendTo(caller, errKey, ChatLevel.ERROR, errArgs ?? Array.Empty<object>());
                return;
            }

            if (givenAllegiance == ALLEGIANCE.None)
            {
                // Set kit for both allegiances
                SWATPlugin.Instance.KitInfoDatabase.SetKit(ALLEGIANCE.SWAT, steam64ID, kitName);
                SWATPlugin.Instance.KitInfoDatabase.SetKit(ALLEGIANCE.TERRORIST, steam64ID, kitName);
            }
            else
            {
                SWATPlugin.Instance.KitInfoDatabase.SetKit(givenAllegiance, steam64ID, kitName);
            }
        }
    }
}
