using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace S1thK3nny.SWAT.Helpers
{
    public static class ClearHelper
    {
        /// <summary>
        /// Clears all items from the map
        /// </summary>
        /// <returns>Number of items removed</returns>
        public static int ClearItems()
        {
            int itemsRemoved = 0;

            // Count items before clearing
            for (byte x = 0; x < Regions.WORLD_SIZE; x++)
            {
                for (byte y = 0; y < Regions.WORLD_SIZE; y++)
                {
                    ItemRegion region = ItemManager.regions[x, y];
                    if (region != null)
                    {
                        itemsRemoved += region.items.Count;
                    }
                }
            }

            // Clear all items from the map
            ItemManager.askClearAllItems();

            return itemsRemoved;
        }

        /// <summary>
        /// Clears all vehicles from the map
        /// </summary>
        /// <returns>Number of vehicles removed</returns>
        public static int ClearVehicles()
        {
            int vehiclesRemoved = 0;
            List<InteractableVehicle> vehiclesToRemove = new List<InteractableVehicle>();

            // Collect all vehicles
            foreach (InteractableVehicle vehicle in VehicleManager.vehicles)
            {
                if (vehicle != null)
                {
                    vehiclesToRemove.Add(vehicle);
                }
            }

            // Remove all collected vehicles
            foreach (InteractableVehicle vehicle in vehiclesToRemove)
            {
                VehicleManager.askVehicleDestroy(vehicle);
                vehiclesRemoved++;
            }

            return vehiclesRemoved;
        }

        /// <summary>
        /// Clears all player-built structures and barricades from the map
        /// </summary>
        /// <returns>Tuple with number of structures and barricades removed</returns>
        public static (int structures, int barricades) ClearBuildings()
        {
            int structuresRemoved = 0;
            int barricadesRemoved = 0;

            // Remove all structures
            for (byte x = 0; x < Regions.WORLD_SIZE; x++)
            {
                for (byte y = 0; y < Regions.WORLD_SIZE; y++)
                {
                    StructureRegion structureRegion = StructureManager.regions[x, y];
                    if (structureRegion == null) continue;

                    for (int i = structureRegion.drops.Count - 1; i >= 0; i--)
                    {
                        StructureManager.destroyStructure(structureRegion.drops[i], x, y, Vector3.zero);
                        structuresRemoved++;
                    }
                }
            }

            // Remove all barricades
            for (byte x = 0; x < Regions.WORLD_SIZE; x++)
            {
                for (byte y = 0; y < Regions.WORLD_SIZE; y++)
                {
                    BarricadeRegion barricadeRegion = BarricadeManager.regions[x, y];
                    if (barricadeRegion == null) continue;

                    for (int i = barricadeRegion.drops.Count - 1; i >= 0; i--)
                    {
                        BarricadeManager.destroyBarricade(barricadeRegion.drops[i], x, y, ushort.MaxValue);
                        barricadesRemoved++;
                    }
                }
            }

            return (structuresRemoved, barricadesRemoved);
        }

        /// <summary>
        /// Clears the inventory and clothing of a player
        /// </summary>
        /// <param name="player">The player whose inventory to clear</param>
        public static void ClearInventory(UnturnedPlayer player)
        {
            if (player == null || player.Player == null)
                return;

            // Unequip anything in hands first
            player.Player.equipment.dequip();

            // Clear all inventory pages (not clothing)
            PlayerInventory inv = player.Player.inventory;
            ClearInventoryHelper(inv);

            var clothing = player.Player.clothing;

            // Remove all clothing by setting each slot to 0
            clothing.askWearBackpack(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearVest(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearShirt(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearPants(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearHat(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearMask(0, 0, System.Array.Empty<byte>(), true);
            clothing.askWearGlasses(0, 0, System.Array.Empty<byte>(), true);

            // Clear again to remove any items dropped from clothing
            ClearInventoryHelper(inv);
        }

        public static void ClearAllInventories(HashSet<ulong> Players)
        {
            foreach (var steamId in Players)
            {
                var uPlayer = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(steamId));
                ClearInventory(uPlayer);
            }
        }

        private static void ClearInventoryHelper(PlayerInventory inv)
        {
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
