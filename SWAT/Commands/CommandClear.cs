using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using S1thK3nny.SWAT.Helpers;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandClear : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "clear";

        public string Help => "Clears items, vehicles, buildings, or inventory";

        public string Syntax => "<all|a|buildings|b|inventory|inv|items|i|vehicles|v>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "clear" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.SendTo(caller, "CommandClearUsage", ChatLevel.ERROR);
                return;
            }

            string property = command[0].ToLower();

            switch (property)
            {
                case "items":
                case "i":
                    int itemsRemoved = ClearHelper.ClearItems();
                    ChatHelper.SendTo(caller, "CommandClearItemsSuccess", ChatLevel.INFO, itemsRemoved);
                    break;

                case "vehicles":
                case "v":
                    int vehiclesRemoved = ClearHelper.ClearVehicles();
                    ChatHelper.SendTo(caller, "CommandClearVehiclesSuccess", ChatLevel.INFO, vehiclesRemoved);
                    break;

                case "buildings":
                case "b":
                    var buildingResult = ClearHelper.ClearBuildings();
                    ChatHelper.SendTo(caller, "CommandClearBuildingsSuccess", ChatLevel.INFO, buildingResult.structures, buildingResult.barricades);
                    break;

                case "inventory":
                case "inv":
                    UnturnedPlayer uPlayer = caller as UnturnedPlayer;
                    if (uPlayer == null)
                    {
                        ChatHelper.SendTo(caller, "CommandClearInventoryError", ChatLevel.ERROR);
                        return;
                    }
                    ClearHelper.ClearInventory(uPlayer);
                    ChatHelper.SendTo(caller, "CommandClearInventorySuccess", ChatLevel.INFO, PlayerNameHelper.GetDisplayName(uPlayer.CSteamID.m_SteamID));
                    break;

                case "all":
                case "a":
                    int totalItemsRemoved = ClearHelper.ClearItems();
                    int totalVehiclesRemoved = ClearHelper.ClearVehicles();
                    var totalBuildingResult = ClearHelper.ClearBuildings();
                    ChatHelper.SendTo(caller, "CommandClearAllSuccess", ChatLevel.INFO, totalItemsRemoved, totalVehiclesRemoved, totalBuildingResult.structures, totalBuildingResult.barricades);
                    break;

                default:
                    ChatHelper.SendTo(caller, "CommandClearInvalidProperty", ChatLevel.ERROR);
                    break;
            }
        }
    }
}
