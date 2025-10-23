using SDG.Unturned;
using S1thK3nny.SWAT.Models.Teams;
using UnityEngine;
using Steamworks;

namespace S1thK3nny.SWAT.Helpers
{
    public static class PlayerNameHelper
    {
        public static void SetPlayerName(CSteamID steamID, ALLEGIANCE allegiance)
        {
            Player player = PlayerTool.getPlayer(steamID);
            if (player == null) return;
            
            string originalName = GetOriginalName(player);
            string tag = GetTag(allegiance);
            
            // Setze den Namen mit dem Tag (ohne Farbe, da Unturned das nicht unterstützt)
            player.channel.owner.playerID.characterName = $"{tag} {originalName}";
            player.channel.owner.playerID.nickName = $"{tag} {originalName}";
        }

        public static void SetPlayerName(ulong steamID, ALLEGIANCE allegiance)
        {
            SetPlayerName(new CSteamID(steamID), allegiance);
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

        private static string GetOriginalName(Player player)
        {
            string currentName = player.channel.owner.playerID.characterName;
            
            // Entferne existierende Tags
            return System.Text.RegularExpressions.Regex.Replace(
                currentName, 
                @"\[(SWAT|TERRORIST)\]\s*", 
                "").Trim();
        }

        public static void RemoveTag(CSteamID steamID)
        {
            Player player = PlayerTool.getPlayer(steamID);
            if (player == null) return;
            
            string cleanName = GetOriginalName(player);
            
            player.channel.owner.playerID.characterName = cleanName;
            player.channel.owner.playerID.nickName = cleanName;
        }

        public static void RemoveTag(ulong steamID)
        {
            RemoveTag(new CSteamID(steamID));
        }
    }
}