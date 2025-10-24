using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Linq;
using Rocket.Unturned;
using S1thK3nny.SWAT.Database;
using S1thK3nny.SWAT.Models.Teams;
using S1thK3nny.SWAT.Helpers;
using Rocket.Unturned.Events;

namespace S1thK3nny.SWAT
{
    public class SWATPlugin : RocketPlugin<SWATConfiguration>
    {
        public static SWATPlugin Instance { get; private set; }
        public UnityEngine.Color MessageColor { get; set; }
        public AllegianceXmlDatabase AllegianceDatabase { get; private set; } = new();
        public PerMapInfosXmlDatabase perMapInfosDatabase = new();

        // This appears whenever Rocket loads the plugin
        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, UnityEngine.Color.green);
            AllegianceDatabase = new AllegianceXmlDatabase();
            AllegianceDatabase.Load();

            perMapInfosDatabase = new PerMapInfosXmlDatabase();
            perMapInfosDatabase.Load();

            // Initialize GameStateManager
            GameStateManager.Initialize();

            U.Events.OnPlayerConnected += OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;

            Logger.Log($"{Name} {Assembly.GetName().Version.ToString(3)} has been loaded!", ConsoleColor.Yellow);
        }

        // This appears whenever Rocket unloads the plugin
        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;

            // Shutdown GameStateManager
            GameStateManager.Shutdown();

            AllegianceDatabase.Save();
            perMapInfosDatabase.Save();

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        public override TranslationList DefaultTranslations => new()
        {
            { "SwatTestMessage", "[[b]]SWAT Plugin[[/b]] is working! Welcome to the team, operative!" },
            { "CommandRegisterToTeamSyntax", "Usage: /sregister <Allegiance> [Steam64ID or PlayerName]" },
            { "InvalidAllegiance", "Invalid allegiance '{0}'. Use SWAT or TERRORIST." },
            { "InvalidSteam64IDOrPlayerName", "Invalid Steam64ID or PlayerName: {0}" },
            { "MustSpecifySteam64IDFromConsole", "You must specify a Steam64ID when using this command from console." },

            { "PlayerSwitchedTeams", "Player {0} has switched to team [[b]]{1}[[/b]]!" },
            { "PlayerRegisteredToTeam", "Player {0} has been registered to team [[b]]{1}[[/b]]!" },

            { "PlayerNotRegistered", "Player {0} is not registered to any team." },
            { "PlayerUnregisteredFromTeam", "Player {0} has been unregistered from team [[b]]{1}[[/b]]!" },

            { "CommandRegisterPositionSyntax", "Usage: /sposition [Allegiance]" },
            { "CommandRegisterPositionSaved", "Position registered for player [[b]]{0}[[/b]] for team [[b]]{1}[[/b]] on map [[b]]{2}[[/b]]!" },

            { "CommandRegisterSWATVehicleSyntax", "Usage: /svehicle <vehicleID>" },
            { "CommandRegisterSWATVehicleSaved", "SWAT vehicle [[b]]{0}[[/b]] spawn registered on map [[b]]{1}[[/b]]!" },

            { "NoGameIsCurrentlyRunning", "No game is currently running!" },
            { "GameIsCurrentlyRunning", "A game is currently running. You cannot use this command right now." }
        };

        public ALLEGIANCE getPlayerAllegiance(ulong steam64ID)
        {
            var existingData = Instance.AllegianceDatabase.Allegiances
                .FirstOrDefault(x => x.Steam64ID == steam64ID);

            if (existingData != null)
            {
                return existingData.Team;
            }

            return ALLEGIANCE.None;
        }

        // Event handler for player connections
        // Set player name tag based on allegiance on connect
        public void OnPlayerConnected(UnturnedPlayer unturnedPlayer)
        {
            var allegiance = getPlayerAllegiance(unturnedPlayer.CSteamID.m_SteamID);
            if (allegiance != ALLEGIANCE.None)
            {
                PlayerNameHelper.SetPlayerName(unturnedPlayer.CSteamID.m_SteamID, allegiance);
            }
            Console.WriteLine($"[SWATPlugin] Player connected: {unturnedPlayer.DisplayName} ({unturnedPlayer.CSteamID.m_SteamID})");
            Console.WriteLine($"[SWATPlugin] Allegiance data: {allegiance}");
        }

        // Event handler for player deaths
        public void OnPlayerDeath(UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, Steamworks.CSteamID murderer)
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnPlayerDeath(player.CSteamID.m_SteamID);
            }
        }
    }
}