using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using S1thK3nny.SWAT.Models.Teams;
using S1thK3nny.SWAT.Helpers;
using S1thK3nny.SWAT.Models.Databases;

namespace S1thK3nny.SWAT
{
    public enum GameState
    {
        Idle,           // No game running
        Preparing,      // Game starting, moving players to positions
        BuildPhase,     // 30-minute build timer (optional)
        InProgress,     // Active combat
        Ended           // Game over, awaiting cleanup
    }

    public class GameStateManager
    {
        // Singleton instance
        public static GameStateManager Instance { get; private set; }

        // Current game state
        public GameState CurrentState { get; private set; } = GameState.Idle;

        // Players in this match (Steam64IDs)
        public List<ulong> SwatPlayers { get; private set; } = new List<ulong>();
        public List<ulong> TerroristPlayers { get; private set; } = new List<ulong>();

        // Alive tracking
        public HashSet<ulong> AlivePlayers { get; private set; } = new HashSet<ulong>();

        // Timer references
        private Coroutine gameTimerCoroutine;
        private Coroutine uiUpdateCoroutine;

        private SWATPlugin pluginInstance => SWATPlugin.Instance;

        // Initialize singleton
        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new GameStateManager();
            }
        }

        // Clean up singleton
        public static void Shutdown()
        {
            if (Instance != null)
            {
                Instance.CancelGame();
                Instance = null;
            }
        }

        /// <summary>
        /// Validates if a game can be started
        /// </summary>
        public bool CanStartGame(out List<ulong> swat, out List<ulong> terrorists, out string errorMessage)
        {
            errorMessage = string.Empty;
            swat = terrorists = null;

            // Check if game is already running
            if (CurrentState != GameState.Idle)
            {
                errorMessage = "A game is already in progress!";
                return false;
            }

            // Check if map data exists first and foremost
            string currentMap = Provider.map;
            var mapData = pluginInstance.perMapInfosDatabase.Maps
                .FirstOrDefault(m => m.Id == currentMap);

            if (mapData == null)
            {
                errorMessage = $"No spawn positions configured for map '{currentMap}'!";
                return false;
            }

            // Get all registered players (authoritative from Allegiance.xml)
            swat = pluginInstance.AllegianceDatabase.Allegiances
                .Where(a => a.Team == ALLEGIANCE.SWAT)
                .Select(a => a.Steam64ID)
                .ToList();

            terrorists = pluginInstance.AllegianceDatabase.Allegiances
                .Where(a => a.Team == ALLEGIANCE.TERRORIST)
                .Select(a => a.Steam64ID)
                .ToList();

            // Only online players are legitimate participants
            swat = swat.Where(IsPlayerOnline).Distinct().ToList();
            terrorists = terrorists.Where(IsPlayerOnline).Distinct().ToList();

            if (!CanStartGameAllegianceCheck(swat, ALLEGIANCE.SWAT, out errorMessage))
                return false;

            if (!CanStartGameAllegianceCheck(terrorists, ALLEGIANCE.TERRORIST, out errorMessage))
                return false;


            return true;
        }

        /// <summary>
        /// Checks if the game can be started for a specific allegiance. Do this so we don't have to repeat code.
        /// </summary>
        private bool CanStartGameAllegianceCheck(List<ulong> players, ALLEGIANCE team, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Ensure there's at least one ONLINE player for this team
            if (players == null || players.Count == 0)
            {
                errorMessage = $"No online players found for team {team}!";
                return false;
            }

            // Check if all players have assigned teams
            foreach (var steamId in players)
            {
                var allegiance = pluginInstance.AllegianceDatabase.Allegiances
                    .FirstOrDefault(a => a.Steam64ID == steamId);

                if (allegiance == null)
                {
                    errorMessage = $"Player {steamId} is not assigned to a team!";
                    return false;
                }
            }

            // Check if all online players have spawn positions configured on this map
            string currentMap = Provider.map;
            var mapData = pluginInstance.perMapInfosDatabase.Maps
                .FirstOrDefault(m => m.Id == currentMap);

            if (mapData?.Allegiances == null)
            {
                errorMessage = $"No allegiance data configured for map '{currentMap}'!";
                return false;
            }

            var teamMapData = mapData.Allegiances
                .FirstOrDefault(a => string.Equals(a.Team, team.ToString(), StringComparison.OrdinalIgnoreCase));

            if (teamMapData == null || teamMapData.Players == null)
            {
                errorMessage = $"No spawn positions configured for team {team} on map '{currentMap}'!";
                return false;
            }

            // Verify each online player has a spawn position
            var playersWithSpawns = teamMapData.Players.Select(p => p.Steam64Id).ToHashSet();
            var missingSpawns = players.Where(steamId => !playersWithSpawns.Contains(steamId)).ToList();

            if (missingSpawns.Count > 0)
            {
                var missingPlayerNames = missingSpawns
                    .Select(id => UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(id))?.DisplayName ?? id.ToString())
                    .ToList();
                
                errorMessage = $"Team {team} players missing spawn positions: {string.Join(", ", missingPlayerNames)}";
                return false;
            }

            return true;
        }

        private bool IsPlayerOnline(ulong steam64ID)
        {
            try
            {
                // Prefer Provider.clients for fast lookup
                return Provider.clients.Any(c => c.playerID.steamID.m_SteamID == steam64ID);
            }
            catch
            {
                // Fallback to Rocket wrapper
                var player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(steam64ID));
                return player != null;
            }
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        public void StartGame(bool includeBuildPhase = false)
        {
            if (!CanStartGame(out var swat, out var terrorists, out string errorMessage))
            {
                ChatHelper.Broadcast(ChatLevel.ERROR, $"Cannot start game: {errorMessage}");
                return;
            }

            CurrentState = GameState.Preparing;

            SwatPlayers = swat;
            TerroristPlayers = terrorists;

            // Initialize alive players
            AlivePlayers = [.. SwatPlayers.Concat(TerroristPlayers)];

            ChatHelper.Broadcast(ChatLevel.INFO, "=== SWAT GAME STARTING ===");
            ChatHelper.Broadcast(ChatLevel.INFO, $"SWAT: {SwatPlayers.Count} players");
            ChatHelper.Broadcast(ChatLevel.INFO, $"TERRORISTS: {TerroristPlayers.Count} players");

            TeleportPlayersToSpawns();
            SpawnSwatVehicles();
            GivePlayerKits();

            if (includeBuildPhase)
            {
                // Start build phase
                CurrentState = GameState.BuildPhase;
                ChatHelper.Broadcast(ChatLevel.INFO, "=== 30 MINUTE BUILD PHASE STARTED ===");
                gameTimerCoroutine = pluginInstance.StartCoroutine(BuildPhaseTimer());
            }
            else
            {
                BeginCombatPhase();
            }

            // Start UI updates
            StartUIUpdates();
        }

        /// <summary>
        /// Teleports all registered players to their spawn positions
        /// </summary>
        private void TeleportPlayersToSpawns()
        {
            string currentMap = Provider.map;
            var mapData = pluginInstance.perMapInfosDatabase.Maps
                .FirstOrDefault(m => m.Id == currentMap);

            if (mapData == null) return;

            // Only teleport players to the spawn that matches their current allegiance
            TeleportPlayersFromAllegianceToSpawns(mapData, ALLEGIANCE.SWAT);
            TeleportPlayersFromAllegianceToSpawns(mapData, ALLEGIANCE.TERRORIST);
        }

        private void TeleportPlayersFromAllegianceToSpawns(MapInfo mapData, ALLEGIANCE allegiance)
        {
            // Map may contain positions for a player under multiple allegiances.
            // Only teleport if the player's current allegiance (from Allegiance.xml) matches the requested allegiance.
            var mapAllegiance = mapData.Allegiances?
                .FirstOrDefault(a => string.Equals(a.Team, allegiance.ToString(), StringComparison.OrdinalIgnoreCase));

            if (mapAllegiance == null)
                return;

            foreach (var pInfo in mapAllegiance.Players ?? Enumerable.Empty<PlayerInfo>())
            {
                var steamId = pInfo.Steam64Id;

                // Cross-check against authoritative allegiance database
                var currentAllegiance = pluginInstance.getPlayerAllegiance(steamId);
                if (currentAllegiance != allegiance) continue;

                var player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(steamId));
                if (player == null)
                    continue; // not online

                player.Player.teleportToLocationUnsafe(pInfo.Position, pInfo.Rotation.y);
                ChatHelper.SendTo(player, ChatLevel.OK, $"Teleported {player.DisplayName} to {allegiance} spawn position!");
            }
        }

        /// <summary>
        /// Spawns SWAT vehicles at configured positions
        /// </summary>
        private void SpawnSwatVehicles()
        {
            string currentMap = Provider.map;
            var mapData = pluginInstance.perMapInfosDatabase.Maps
                .FirstOrDefault(m => m.Id == currentMap);

            if (mapData?.SwatVehicleInfos != null)
            {
                ushort vehicleId = (ushort)mapData.SwatVehicleInfos.VehicleID;
                Vector3 position = mapData.SwatVehicleInfos.SpawnPosition;
                Quaternion rotation = Quaternion.Euler(mapData.SwatVehicleInfos.SpawnRotation);

                VehicleManager.spawnVehicleV2(vehicleId, position, rotation);
                ChatHelper.Broadcast(ChatLevel.OK, $"SWAT vehicle spawned!");
            }
        }

        /// <summary>
        /// Gives kits to all players using /kit command.
        /// Uses player display names instead of Steam64IDs.
        /// </summary>
        private void GivePlayerKits()
        {
            foreach (var steamId in SwatPlayers.Concat(TerroristPlayers))
            {
                var player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(steamId));
                if (player != null)
                {
                    // Execute /kit <PlayerName> <PlayerName>
                    // This assumes you have RestoreMonarchys Kits plugin installed
                    // If names contain spaces, wrap in quotes
                    if (!KitGiver.TryGiveKitToPlayer(player, out string error))
                    {
                        ChatHelper.SendTo(player, ChatLevel.ERROR, $"Failed to give kit to {player.DisplayName}: {error}");
                    }
                }
            }
        }

        /// <summary>
        /// Build phase timer (30 minutes)
        /// </summary>
        private IEnumerator BuildPhaseTimer()
        {
            float buildTime = 30 * 60; // 30 minutes in seconds
            yield return new WaitForSeconds(buildTime);

            ChatHelper.Broadcast(ChatLevel.INFO, "=== BUILD PHASE ENDED ===");
            ChatHelper.Broadcast(ChatLevel.INFO, "Teleporting players back to spawn positions...");

            // Teleport players again
            TeleportPlayersToSpawns();

            yield return new WaitForSeconds(5);

            BeginCombatPhase();
        }

        /// <summary>
        /// Begins the combat phase
        /// </summary>
        private void BeginCombatPhase()
        {
            CurrentState = GameState.InProgress;
            ChatHelper.Broadcast(ChatLevel.OK, "=== COMBAT PHASE STARTED ===");
            ChatHelper.Broadcast(ChatLevel.OK, "Good luck!");
        }

        /// <summary>
        /// Starts the UI update coroutine
        /// </summary>
        private void StartUIUpdates()
        {
            if (uiUpdateCoroutine != null)
            {
                pluginInstance.StopCoroutine(uiUpdateCoroutine);
            }
            uiUpdateCoroutine = pluginInstance.StartCoroutine(UpdateUICoroutine());
        }

        /// <summary>
        /// Updates UI every second
        /// </summary>
        private IEnumerator UpdateUICoroutine()
        {
            while (CurrentState != GameState.Idle)
            {
                UpdateUI();
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Updates the UI for all players          TODO: Implement proper UI
        /// </summary>
        private void UpdateUI()
        {
            var stats = GetTeamStats();
        }

        /// <summary>
        /// Gets team statistics
        /// </summary>
        public (int swatAlive, int swatTotal, int terroristAlive, int terroristTotal) GetTeamStats()
        {
            int swatAlive = SwatPlayers.Count(s => AlivePlayers.Contains(s));
            int terroristAlive = TerroristPlayers.Count(s => AlivePlayers.Contains(s));

            return (swatAlive, SwatPlayers.Count, terroristAlive, TerroristPlayers.Count);
        }

        /// <summary>
        /// Handles player death
        /// </summary>
        public void OnPlayerDeath(ulong steam64ID)
        {
            if (CurrentState != GameState.InProgress)
                return;

            // Remove from alive players
            if (AlivePlayers.Contains(steam64ID))
            {
                AlivePlayers.Remove(steam64ID);

                var allegiance = pluginInstance.getPlayerAllegiance(steam64ID);
                ChatHelper.Broadcast(ChatLevel.WARNING, $"A {allegiance} player has been eliminated!");

                // Check win condition
                CheckWinCondition();
            }
        }

        /// <summary>
        /// Checks if a team has won
        /// </summary>
        private void CheckWinCondition()
        {
            var stats = GetTeamStats();

            if (stats.swatAlive == 0)
            {
                EndGame(ALLEGIANCE.TERRORIST);
            }
            else if (stats.terroristAlive == 0)
            {
                EndGame(ALLEGIANCE.SWAT);
            }
        }

        /// <summary>
        /// Ends the game with a winner
        /// </summary>
        public void EndGame(ALLEGIANCE winner)
        {
            if (CurrentState == GameState.Idle)
                return;

            CurrentState = GameState.Ended;

            ChatHelper.Broadcast(ChatLevel.OK, "===================");
            ChatHelper.Broadcast(ChatLevel.OK, $"=== [[b]]{winner} WINS![[/b]] ===");
            ChatHelper.Broadcast(ChatLevel.OK, "===================");

            var stats = GetTeamStats();
            ChatHelper.Broadcast(ChatLevel.INFO, $"Final Score - SWAT: {stats.swatAlive}/{stats.swatTotal} | TERRORISTS: {stats.terroristAlive}/{stats.terroristTotal}");

            // Cleanup
            CleanupGame();
        }

        /// <summary>
        /// Cancels the current game
        /// </summary>
        public void CancelGame()
        {
            if (CurrentState == GameState.Idle)
                return;

            ChatHelper.Broadcast(ChatLevel.WARNING, "=== GAME CANCELLED ===");
            CleanupGame();
        }

        /// <summary>
        /// Cleans up game state
        /// </summary>
        private void CleanupGame()
        {
            // Stop coroutines
            if (gameTimerCoroutine != null)
            {
                pluginInstance.StopCoroutine(gameTimerCoroutine);
                gameTimerCoroutine = null;
            }

            if (uiUpdateCoroutine != null)
            {
                pluginInstance.StopCoroutine(uiUpdateCoroutine);
                uiUpdateCoroutine = null;
            }

            // Reset state
            CurrentState = GameState.Idle;
            SwatPlayers.Clear();
            TerroristPlayers.Clear();
            AlivePlayers.Clear();
        }

        /// <summary>
        /// Gets a status message for the current game
        /// </summary>
        public string GetStatusMessage()
        {
            if (CurrentState == GameState.Idle)
            {
                return "NoGameIsCurrentlyRunning";
            }

            var stats = GetTeamStats();
            return $"Game Status: {CurrentState}\n" +
                   $"SWAT: {stats.swatAlive}/{stats.swatTotal}\n" +
                   $"TERRORISTS: {stats.terroristAlive}/{stats.terroristTotal}";
        }
    }
}