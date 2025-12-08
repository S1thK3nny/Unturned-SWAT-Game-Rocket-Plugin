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

        public (List<ulong> swat, List<ulong> terrorists) GetTeamsInfo()
        {
            List<ulong> swat = pluginInstance.AllegianceDatabase.Allegiances
                .Where(a => a.Team == ALLEGIANCE.SWAT)
                .Select(a => a.Steam64ID)
                .ToList();

            List<ulong> terrorists = pluginInstance.AllegianceDatabase.Allegiances
                .Where(a => a.Team == ALLEGIANCE.TERRORIST)
                .Select(a => a.Steam64ID)
                .ToList();

            swat = swat.Where(IsPlayerOnline).Distinct().ToList();
            terrorists = terrorists.Where(IsPlayerOnline).Distinct().ToList();

            Console.WriteLine($"{ScriptTag.GetScriptName()} SWAT Team: {string.Join(", ", swat)}");
            Console.WriteLine($"{ScriptTag.GetScriptName()} Terrorist Team: {string.Join(", ", terrorists)}");

            return (swat, terrorists);
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
            (swat, terrorists) = GetTeamsInfo();

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
                    .Select(id => PlayerNameHelper.GetDisplayName(id, stripTags: true))
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
        /// <param name="buildPhaseTime">Build phase duration in minutes (0 = skip build phase)</param>
        public bool StartGame(out string errorMessage, int buildPhaseTime = 0)
        {
            // Validate start conditions
            if (!CanStartGame(out var validSwat, out var validTerrorists, out errorMessage))
            {
                ChatHelper.Broadcast(ChatLevel.ERROR, $"Cannot start game: {errorMessage}");
                return false;
            }

            Console.WriteLine($"{ScriptTag.GetScriptName()} Starting game...");
            
            CurrentState = GameState.Preparing;

            SwatPlayers = validSwat;
            TerroristPlayers = validTerrorists;
            AlivePlayers = [.. SwatPlayers.Concat(TerroristPlayers)];

            ChatHelper.Broadcast(ChatLevel.INFO, "=== SWAT GAME STARTING ===");
            ChatHelper.Broadcast(ChatLevel.INFO, $"SWAT: {SwatPlayers.Count} players");
            ChatHelper.Broadcast(ChatLevel.INFO, $"TERRORISTS: {TerroristPlayers.Count} players");

            TeleportPlayersToSpawns();
            SpawnSwatVehicles();

            if (buildPhaseTime > 0)
            {
                // Start build phase
                CurrentState = GameState.BuildPhase;
                ChatHelper.Broadcast(ChatLevel.INFO, $"=== {buildPhaseTime} MINUTE BUILD PHASE STARTED ===");
                gameTimerCoroutine = pluginInstance.StartCoroutine(BuildPhaseTimer(buildPhaseTime));
            }
            else
            {
                // No build phase - give kits immediately and start combat
                PreparePlayersForCombat();
                BeginCombatPhase();
            }

            // Start UI updates
            StartUIUpdates();
            return true;
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
            // Guard the plugin & DB
            var db = pluginInstance?.AllegianceDatabase?.Allegiances;
            if (db == null) return;

            var mapAllegiance = mapData?.Allegiances?
                .FirstOrDefault(a => string.Equals(a.Team, allegiance.ToString(), StringComparison.OrdinalIgnoreCase));

            if (mapAllegiance?.Players == null) return;

            foreach (var pInfo in mapAllegiance.Players)
            {
                if (pInfo == null) continue;

                try
                {
                    var steamId = pInfo.Steam64Id;

                    // Find authoritative allegiance entry. If missing, skip to avoid NRE.
                    var dbEntry = db.FirstOrDefault(a => a.Steam64ID == steamId);
                    if (dbEntry == null) continue;                        // <- important
                    if (dbEntry.Team != allegiance) continue;

                    var player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(steamId));
                    if (player == null || player.Player == null) continue;

                    player.Player.teleportToLocationUnsafe(pInfo.Position, pInfo.Rotation.y);
                    ChatHelper.SendTo(player, ChatLevel.OK, $"Teleported {player.DisplayName} to {allegiance} spawn position!");
                }
                catch (Exception ex)
                {
                    // Helpful log to pinpoint bad map entries
                    Console.WriteLine($"{ScriptTag.GetScriptName()} Teleport error for allegiance {allegiance}: " +
                                    $"Steam64Id={pInfo?.Steam64Id} Pos={pInfo?.Position} " +
                                    $"Err={ex.GetType().Name}: {ex.Message}");
                }
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
                ushort vehicleId = mapData.SwatVehicleInfos.VehicleID;
                Vector3 position = mapData.SwatVehicleInfos.SpawnPosition;
                Quaternion rotation = Quaternion.Euler(mapData.SwatVehicleInfos.SpawnRotation);

                VehicleManager.spawnVehicleV2(vehicleId, position, rotation);
                ChatHelper.Broadcast(ChatLevel.OK, $"SWAT vehicle spawned!");
            }
        }

        /// <summary>
        /// Prepares players for combat by optionally clearing inventories/items and giving kits.
        /// </summary>
        private void PreparePlayersForCombat()
        {
            ClearHelper.ClearAllInventories(AlivePlayers);
            ClearHelper.ClearItems();
            GivePlayerKits();
        }

        /// <summary>
        /// Gives kits to all players using /kit command.
        /// Uses player display names instead of Steam64IDs.
        /// </summary>
        private void GivePlayerKits()
        {
            foreach (var steamId in SwatPlayers)
            {
                var player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(steamId));
                if (player != null)
                {
                    // Execute /kit <PlayerName> <PlayerName>
                    // This assumes you have RestoreMonarchys Kits plugin installed
                    // If names contain spaces, wrap in quotes
                    if (!KitGiver.TryGiveKitToPlayer(player, ALLEGIANCE.SWAT, out string error))
                    {
                        ChatHelper.SendTo(player, ChatLevel.ERROR, $"Failed to give kit to {player.DisplayName}: {error}");
                    }
                }
            }

            foreach (var steamId in TerroristPlayers)
            {
                var player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(steamId));
                if (player != null)
                {
                    if (!KitGiver.TryGiveKitToPlayer(player, ALLEGIANCE.TERRORIST, out string error))
                    {
                        ChatHelper.SendTo(player, ChatLevel.ERROR, $"Failed to give kit to {player.DisplayName}: {error}");
                    }
                }
            }
        }

        /// <summary>
        /// Build phase timer (30 minutes)
        /// </summary>
        private IEnumerator BuildPhaseTimer(int buildTimeInMinutes)
        {
            yield return new WaitForSeconds(buildTimeInMinutes * 60);

            EndBuildPhase();
        }

        /// <summary>
        /// Ends the build phase immediately
        /// </summary>
        public void EndBuildPhase()
        {
            if (CurrentState != GameState.BuildPhase)
                return;

            // Stop the build timer coroutine if running
            if (gameTimerCoroutine != null)
            {
                pluginInstance.StopCoroutine(gameTimerCoroutine);
                gameTimerCoroutine = null;
            }

            Console.WriteLine($"{ScriptTag.GetScriptName()} Ending build phase...");

            ChatHelper.Broadcast(ChatLevel.INFO, "=== BUILD PHASE ENDED ===");
            ChatHelper.Broadcast(ChatLevel.INFO, "Teleporting players back to spawn positions...");

            // Teleport players again
            TeleportPlayersToSpawns();

            // Clear build phase items and give combat kits
            PreparePlayersForCombat();

            ChatHelper.Broadcast(ChatLevel.INFO, "Combat phase starting in 5 seconds...");

            // Start a short coroutine for the delay before combat
            pluginInstance.StartCoroutine(StartCombatAfterDelay());
        }

        /// <summary>
        /// Waits 5 seconds then starts combat phase
        /// </summary>
        private IEnumerator StartCombatAfterDelay()
        {
            yield return new WaitForSeconds(5);
            BeginCombatPhase();
        }

        /// <summary>
        /// Sets random time and weather for variety in each match
        /// </summary>
        public void RandomizeEnvironment()
        {
            System.Random random = new System.Random(Environment.TickCount + DateTime.Now.Millisecond);

            // Define specific time periods with weights for more interesting variety
            float bias = LevelLighting.bias;
            uint cycle = LightingManager.cycle == 0 ? 3600u : LightingManager.cycle;

            // Weighted time period selection for more dramatic variety
            int timeChoice = random.Next(100);
            uint randomTimeSeconds;
            string timeOfDay;

            if (timeChoice < 20) // 20% - Early Morning (dawn)
            {
                randomTimeSeconds = (uint)(cycle * random.NextDouble() * 0.1f); // First 10% of day
                timeOfDay = "Dawn";
            }
            else if (timeChoice < 45) // 25% - Midday (bright)
            {
                randomTimeSeconds = (uint)(cycle * (0.3f + random.NextDouble() * 0.2f)); // Middle of day
                timeOfDay = "Midday";
            }
            else if (timeChoice < 60) // 15% - Late Afternoon
            {
                randomTimeSeconds = (uint)(cycle * (bias - 0.1f + random.NextDouble() * 0.1f)); // Just before dusk
                timeOfDay = "Late Afternoon";
            }
            else if (timeChoice < 75) // 15% - Dusk (transition)
            {
                randomTimeSeconds = (uint)(cycle * (bias + random.NextDouble() * 0.05f)); // Just after bias
                timeOfDay = "Dusk";
            }
            else if (timeChoice < 90) // 15% - Deep Night (dark)
            {
                float nightMid = bias + (1f - bias) * 0.5f; // Middle of night
                randomTimeSeconds = (uint)(cycle * (nightMid - 0.1f + random.NextDouble() * 0.2f));
                timeOfDay = "Night";
            }
            else // 10% - Pre-dawn (very dark)
            {
                randomTimeSeconds = (uint)(cycle * (1f - 0.05f + random.NextDouble() * 0.05f)); // Very end of cycle
                timeOfDay = "Pre-Dawn";
            }

            LightingManager.time = randomTimeSeconds;

            // Random weather
            string[] weatherOptions = ["none", "none", "none", "storm", "blizzard"];
            string selectedWeather = weatherOptions[random.Next(weatherOptions.Length)];
            
            if (selectedWeather == "none")
            {
                LightingManager.ResetScheduledWeather();
            }
            else if (selectedWeather == "storm")
            {
                WeatherAssetBase rainWeather = WeatherAssetBase.DEFAULT_RAIN.Find();
                if (rainWeather != null)
                {
                    LightingManager.ForecastWeatherImmediately(rainWeather);
                }
            }
            else if (selectedWeather == "blizzard")
            {
                WeatherAssetBase snowWeather = WeatherAssetBase.DEFAULT_SNOW.Find();
                if (snowWeather != null)
                {
                    LightingManager.ForecastWeatherImmediately(snowWeather);
                }
            }

            string weatherDesc = GetWeatherString(selectedWeather);
            ChatHelper.Broadcast(ChatLevel.INFO, $"Environment: {timeOfDay}, {weatherDesc}");
            Console.WriteLine($"{ScriptTag.GetScriptName()} Environment randomized: Time={randomTimeSeconds}s ({timeOfDay}), Weather={weatherDesc}");
        }

        /// <summary>
        /// Gets a friendly description of the weather
        /// </summary>
        private string GetWeatherString(string weather)
        {
            return weather switch
            {
                "none" => "Clear Skies",
                "storm" => "Rainy",
                "blizzard" => "Snowy",
                _ => "Clear"
            };
        }

        /// <summary>
        /// Begins the combat phase
        /// </summary>
        private void BeginCombatPhase()
        {
            CurrentState = GameState.InProgress;

            // Randomize time and weather
            RandomizeEnvironment();

            foreach (var steamId in AlivePlayers)
            {
                var player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(steamId));
                if (player == null) continue;

                // Fully heal
                player.Heal(100);
                player.Player.life.serverModifyFood(100);
                player.Player.life.serverModifyWater(100);
                player.Player.life.serverModifyVirus(100);
                player.Player.life.serverModifyStamina(100);
                player.Player.skills.ServerUnlockAllSkills();
            }

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