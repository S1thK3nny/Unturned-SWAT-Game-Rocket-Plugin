using Rocket.API;
using Rocket.Unturned.Player;
using S1thK3nny.SWAT.Helpers;
using System.Collections.Generic;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandClearInventory : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "clearinventory";

        public string Help => "Clears the player's inventory";

        public string Syntax => "";

        public List<string> Aliases => new List<string> { "cinv" };

        public List<string> Permissions => new List<string> { "swat.clearinventory" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var uPlayer = caller as UnturnedPlayer;
            if (uPlayer == null || uPlayer.Player == null)
            {
                ChatHelper.SendTo(caller, ChatLevel.ERROR, "Failed to clear inventory.");
                return;
            }

            // Delegate to the centralized helper
            ClearHelper.ClearInventory(uPlayer);

            ChatHelper.SendTo(caller, ChatLevel.INFO, "Your inventory has been cleared.");
        }
    }
}
