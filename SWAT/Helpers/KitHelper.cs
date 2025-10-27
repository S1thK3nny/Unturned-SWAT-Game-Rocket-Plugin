using System;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using S1thK3nny.SWAT.Models.Teams;
using UnturnedPlayer = Rocket.Unturned.Player.UnturnedPlayer;

namespace S1thK3nny.SWAT.Helpers
{
    public static class KitGiver
    {
        private static IRocketCommand GetKitCommand()
        {
            var meta = R.Commands.Commands.FirstOrDefault(c =>
            c.Name.Equals("kit", StringComparison.OrdinalIgnoreCase) ||
            c.Aliases.Any(a => a.Equals("kit", StringComparison.OrdinalIgnoreCase)));

            return meta?.Command;
        }

        // Give one kit to one player, running as console.
        public static bool TryGiveKitToPlayer(UnturnedPlayer target, ALLEGIANCE allegiance, out string error)
        {
            error = null;

            var steam64ID = target.CSteamID.m_SteamID;
            // Check if the player has assigned themselves a kit to use for the given allegiance. If not, default to a kit named after themselves.
            var kitName   = SWATPlugin.Instance.KitInfoDatabase.GetKit(allegiance, steam64ID) ?? PlayerNameHelper.GetDisplayName(steam64ID, stripTags: true);
            var playerArg = steam64ID.ToString();

            var kitCmd = GetKitCommand();
            if (kitCmd == null)
            {
                error = "Kits command not found (is the Kits plugin loaded?).";
                return false;
            }

            // Console has all permissions.
            IRocketPlayer console = new ConsolePlayer();

            try
            {
                // Executes: /kit <kitName> <steam64>
                kitCmd.Execute(console, new[] { kitName, playerArg });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}