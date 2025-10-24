using Rocket.API;
using S1thK3nny.SWAT.Helpers;
using S1thK3nny.SWAT.Models.Teams;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandShuffle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "shuffle";

        public string Help => "Randomly assigns all online players to SWAT or TERRORIST teams";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "swat.shuffle" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var plugin = SWATPlugin.Instance;

            // Get all currently online players
            var onlinePlayers = Provider.clients
                .Select(c => c.playerID.steamID.m_SteamID)
                .ToList();

            if (onlinePlayers.Count == 0)
            {
                ChatHelper.SendTo(caller, ChatLevel.ERROR, "No players online to shuffle!");
                return;
            }

            if (onlinePlayers.Count < 2)
            {
                ChatHelper.SendTo(caller, ChatLevel.ERROR, "Need at least 2 players to shuffle teams!");
                return;
            }

            // Clear existing registrations
            plugin.AllegianceDatabase.Allegiances.Clear();

            // Shuffle the list
            var random = new Random();
            var shuffledPlayers = onlinePlayers.OrderBy(x => random.Next()).ToList();

            // Split in half - first half SWAT, second half TERRORIST
            int halfCount = shuffledPlayers.Count / 2;
            
            List<ulong> swatPlayers = new List<ulong>();
            List<ulong> terroristPlayers = new List<ulong>();

            for (int i = 0; i < shuffledPlayers.Count; i++)
            {
                ALLEGIANCE team = i < halfCount ? ALLEGIANCE.SWAT : ALLEGIANCE.TERRORIST;
                
                plugin.AllegianceDatabase.Allegiances.Add(new Models.Databases.AllegianceData
                {
                    Steam64ID = shuffledPlayers[i],
                    Team = team
                });

                if (team == ALLEGIANCE.SWAT)
                    swatPlayers.Add(shuffledPlayers[i]);
                else
                    terroristPlayers.Add(shuffledPlayers[i]);

                // Update player name tag
                PlayerNameHelper.SetPlayerName(shuffledPlayers[i], team);
            }

            // Save to database
            plugin.AllegianceDatabase.Save();

            // Broadcast results
            ChatHelper.Broadcast(ChatLevel.OK, "=== TEAMS SHUFFLED ===");
            ChatHelper.Broadcast(ChatLevel.INFO, $"[[b]]SWAT Team[[/b]] ({swatPlayers.Count} players)");
            
            foreach (var steamId in swatPlayers)
            {
                var player = Provider.clients.FirstOrDefault(c => c.playerID.steamID.m_SteamID == steamId);
                if (player != null)
                {
                    ChatHelper.Broadcast(ChatLevel.INFO, $"  - {player.playerID.playerName}");
                }
            }

            ChatHelper.Broadcast(ChatLevel.INFO, $"[[b]]TERRORIST Team[[/b]] ({terroristPlayers.Count} players)");
            
            foreach (var steamId in terroristPlayers)
            {
                var player = Provider.clients.FirstOrDefault(c => c.playerID.steamID.m_SteamID == steamId);
                if (player != null)
                {
                    ChatHelper.Broadcast(ChatLevel.INFO, $"  - {player.playerID.playerName}");
                }
            }

            ChatHelper.SendTo(caller, ChatLevel.OK, $"Successfully shuffled {onlinePlayers.Count} players into teams!");
        }
    }
}
