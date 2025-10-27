using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using UnityEngine;
using RLogger = Rocket.Core.Logging.Logger;

namespace S1thK3nny.SWAT.Helpers
{
    public enum ChatLevel
    {
        INFO,
        OK,
        WARNING,
        ERROR
    }

    /// Examples:
    /// Player-targeted (localized): ChatHelper.SendTo(caller, "PlayerRegisteredToTeam", ChatLevel.OK, playerName, teamName);
    /// Player-targeted (raw): ChatHelper.SendTo(caller, ChatLevel.ERROR, "That ID is not valid.");
    /// Global (localized): ChatHelper.Broadcast("SwatTestMessage", ChatLevel.INFO);
    /// Global (raw + rich tags): ChatHelper.Broadcast(ChatLevel.OK, "[[b]]Operation ready[[/b]] — gear up!");

    public static class ChatHelper
    {
        /// <summary>
        /// Map each ChatLevel to a color. Adjust to your taste or wire to config later.
        /// </summary>
        public static Color GetColor(ChatLevel level)
        {
            switch (level)
            {
                case ChatLevel.OK: return Color.green;            // success
                case ChatLevel.ERROR: return new Color(1f, 0.25f, 0.25f); // soft red
                case ChatLevel.WARNING: return new Color(1f, 1f, 0.25f); // soft yellow
                case ChatLevel.INFO:
                default: return new Color(0.95f, 0.85f, 0.35f); // amber
            }
        }

        /// <summary>
        /// Send a localized (TranslationList) message to a specific player with level color.
        /// </summary>
        public static void SendTo(IRocketPlayer player, string translationKey, ChatLevel level, params object[] placeholders)
            => SendInternal(player, level, TranslateAndFormat(translationKey, placeholders));

        /// <summary>
        /// Send a raw (already formatted) message to a specific player with level color or custom color.
        /// </summary>
        public static void SendTo(IRocketPlayer player, ChatLevel level, string message, Color? color = null)
            => SendInternal(player, level, FormatRich(message), color);

        /// <summary>
        /// Broadcast a localized (TranslationList) message to everyone with level color.
        /// </summary>
        public static void Broadcast(string translationKey, ChatLevel level, params object[] placeholders)
            => BroadcastInternal(level, TranslateAndFormat(translationKey, placeholders));

        /// <summary>
        /// Broadcast a raw (already formatted) message to everyone with level color.
        /// </summary>
        public static void Broadcast(ChatLevel level, string message)
            => BroadcastInternal(level, FormatRich(message));

        // ----------------------------
        // Internals
        // ----------------------------

        private static void SendInternal(IRocketPlayer player, ChatLevel level, string richMsg, Color? color = null)
        {
            var plugin = SWATPlugin.Instance;
            if (plugin == null)
            {
                RLogger.Log($"[SWAT/ChatHelper] Plugin instance not ready. Message: {StripRichTags(richMsg)}", ConsoleColor.DarkYellow);
                return;
            }

            // Console-safe path
            if (player is ConsolePlayer)
            {
                LogToConsole(level, richMsg);
                return;
            }

            // In-game player
            var unturnedPlayer = player as UnturnedPlayer;
            if (unturnedPlayer?.SteamPlayer() == null)
            {
                RLogger.Log($"[SWAT/ChatHelper] Target player not available. Message: {StripRichTags(richMsg)}", ConsoleColor.DarkYellow);
                return;
            }

            ChatManager.serverSendMessage(
                richMsg,
                color ?? GetColor(level),
                null,
                unturnedPlayer.SteamPlayer(),
                EChatMode.GLOBAL,
                plugin.Configuration.Instance.MessageIconUrl,
                true // rich text
            );
        }

        private static void BroadcastInternal(ChatLevel level, string richMsg)
        {
            var plugin = SWATPlugin.Instance;
            if (plugin == null)
            {
                RLogger.Log($"[SWAT/ChatHelper] Plugin instance not ready. Broadcast: {StripRichTags(richMsg)}", ConsoleColor.DarkYellow);
                return;
            }

            // Pass 'toPlayer: null' to broadcast globally
            ChatManager.serverSendMessage(
                richMsg,
                GetColor(level),
                null,
                null,
                EChatMode.GLOBAL,
                plugin.Configuration.Instance.MessageIconUrl,
                true // rich text
            );

            // Also mirror to server console for visibility
            LogToConsole(level, richMsg);
        }

        private static string TranslateAndFormat(string key, params object[] args)
        {
            var plugin = SWATPlugin.Instance;
            string msg = plugin != null
                ? plugin.Translate(key, args)
                : key; // fallback if plugin is null

            return FormatRich(msg);
        }

        /// <summary>
        /// Your translations use [[b]]...[[/b]] style tags; convert to Unity rich tags.
        /// Extend here if you add more custom tags.
        /// </summary>
        private static string FormatRich(string message)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            return message
                .Replace("[[b]]", "<b>")
                .Replace("[[/b]]", "</b>")
                .Replace("[[i]]", "<i>")
                .Replace("[[/i]]", "</i>")
                .Replace("[[u]]", "<u>")
                .Replace("[[/u]]", "</u>")
                .Replace("[[", "<")
                .Replace("]]", ">");
        }

        private static string StripRichTags(string message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;
            // Cheap strip: remove <...> segments for console fallback text.
            // Keeps content readable if rich breaks.
            return System.Text.RegularExpressions.Regex.Replace(message, "<.*?>", string.Empty);
        }

        private static void LogToConsole(ChatLevel level, string richMsg)
        {
            var plain = StripRichTags(richMsg);
            switch (level)
            {
                case ChatLevel.OK:
                    RLogger.Log($"[OK] {plain}", ConsoleColor.Green);
                    break;
                case ChatLevel.ERROR:
                    RLogger.Log($"[ERROR] {plain}", ConsoleColor.Red);
                    break;
                case ChatLevel.INFO:
                case ChatLevel.WARNING:
                    RLogger.Log($"[WARNING] {plain}", ConsoleColor.Yellow);
                    break;
                default:
                    RLogger.Log($"[INFO] {plain}", ConsoleColor.Yellow);
                    break;
            }
        }
    }
}
