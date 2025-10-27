using SDG.Unturned;
using S1thK3nny.SWAT.Models.Teams;
using Steamworks;
using System.Linq;
using Rocket.Unturned.Player;

namespace S1thK3nny.SWAT.Helpers
{
    public static class PlayerNameHelper
    {
        public static void SetPlayerName(ulong steamID, ALLEGIANCE allegiance)
        {
            CSteamID csteamID = new CSteamID(steamID);
            UnturnedPlayer uPlayer = UnturnedPlayer.FromCSteamID(csteamID);

            if (uPlayer == null) return;
            
            string originalName = GetDisplayName(steamID, stripTags: true);
            string tag = GetTag(allegiance);
            
            uPlayer.Player.channel.owner.playerID.characterName = $"{tag} {originalName}";
            uPlayer.Player.channel.owner.playerID.nickName = $"{tag} {originalName}";

            // Test: Set player color based on allegiance
            uPlayer.Color = allegiance == ALLEGIANCE.SWAT ? UnityEngine.Color.blue : UnityEngine.Color.red;
        }

        private static string GetTag(ALLEGIANCE allegiance)
        {
            switch (allegiance)
            {
                case ALLEGIANCE.SWAT:
                    return "[SWAT]";
                case ALLEGIANCE.TERRORIST:
                    return "[TERRORIST]";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Gets the display name of a player by their Steam64ID.
        /// Unified method to resolve player names across the entire plugin.
        /// </summary>
        /// <param name="steam64ID">The player's Steam64ID</param>
        /// <param name="stripTags">If true, removes [SWAT] or [TERRORIST] tags from the name</param>
        /// <returns>Player's display name, or Steam64ID as string if player not found</returns>
        public static string GetDisplayName(ulong steam64ID, bool stripTags = false)
        {
            // Try Rocket wrapper first (most reliable for online players)
            var unturnedPlayer = UnturnedPlayer.FromCSteamID(new CSteamID(steam64ID));
            if (unturnedPlayer != null && !string.IsNullOrEmpty(unturnedPlayer.DisplayName))
            {
                return stripTags ? StripTeamTags(unturnedPlayer.DisplayName) : unturnedPlayer.DisplayName;
            }

            // Fallback: Provider clients (direct SDG access)
            var client = Provider.clients.FirstOrDefault(c => c.playerID.steamID.m_SteamID == steam64ID);
            if (client != null && !string.IsNullOrEmpty(client.playerID.characterName))
            {
                return stripTags ? StripTeamTags(client.playerID.characterName) : client.playerID.characterName;
            }

            // Last resort: return Steam64ID as string
            return steam64ID.ToString();
        }

        /// <summary>
        /// Strips [SWAT] or [TERRORIST] tags from a player name.
        /// </summary>
        private static string StripTeamTags(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return System.Text.RegularExpressions.Regex.Replace(
                name,
                @"\[(SWAT|TERRORIST)\]\s*",
                "").Trim();
        }

        public static void RemoveTag(ulong steamID)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromCSteamID((CSteamID)steamID);
            if (uPlayer == null) return;

            string cleanName = GetDisplayName(uPlayer.CSteamID.m_SteamID, true);

            uPlayer.Player.channel.owner.playerID.characterName = cleanName;
            uPlayer.Player.channel.owner.playerID.nickName = cleanName;
            uPlayer.Color = UnityEngine.Color.white;
        }
    }
}