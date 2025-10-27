using Rocket.API;
using Rocket.Unturned.Player;
using S1thK3nny.SWAT.Helpers;
using SDG.Unturned;
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

        // Clears the inventory of the player who issued the command
        // TODO: Fix non-existing weapon still showing
        public void Execute(IRocketPlayer caller, string[] command)
        {
            var uPlayer = caller as UnturnedPlayer;
            if (uPlayer == null || uPlayer.Player == null)
                return;

            clearinventory(uPlayer);

            var clothing = uPlayer.Player.clothing;

            // Remove all clothing by setting each slot to 0
            clothing.askWearBackpack(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearVest(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearShirt(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearPants(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearHat(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearMask(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearGlasses(0, 0, System.Array.Empty<byte>(), true);

            clearinventory(uPlayer); // Clear again to remove any items dropped from clothing. Yes, this is lazy but it works.

            ChatHelper.SendTo(caller, ChatLevel.INFO, "Your inventory has been cleared.");
        }

        private void clearinventory(UnturnedPlayer uPlayer)
        {
            // Unequip anything in hands first
            uPlayer.Player.equipment.dequip();

            // Clear all inventory pages (not clothing)
            PlayerInventory inv = uPlayer.Player.inventory;
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                Items items = inv.items[page];
                if (items == null) continue;

                for (int i = items.getItemCount() - 1; i >= 0; i--)
                {
                    items.removeItem((byte)i);
                }
            }
        }
    }
}
