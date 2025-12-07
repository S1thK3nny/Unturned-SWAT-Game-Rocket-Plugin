using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Player;
using S1thK3nny.SWAT.Helpers;
using System;
using System.Linq;

namespace S1thK3nny.SWAT
{
    public class CommandInterceptor
    {
        private static CommandInterceptor _instance;

        // Commands allowed ONLY during Idle and BuildPhase
        private static readonly string[] allowedDuringBuildCommands = { "give", "vehicle", "v", "heal", "god", "kit", "tpa", "tp", "teleport" };
        
        // Commands blocked at all times during games
        private static readonly string[] alwaysBlockedCommands = { "spy", "home" };

        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new CommandInterceptor();
                R.Commands.OnExecuteCommand += _instance.OnCommandExecuted;
            }
        }

        public static void Shutdown()
        {
            if (_instance != null)
            {
                R.Commands.OnExecuteCommand -= _instance.OnCommandExecuted;
                _instance = null;
            }
        }

        private void OnCommandExecuted(IRocketPlayer player, IRocketCommand command, ref bool cancel)
        {
            string cmdName = command.Name.ToLower();

            if (player is ConsolePlayer) return; // Never block console commands
            
            // Check if this is an allowed build command (give, vehicle, v)
            bool isAllowedBuildCommand = allowedDuringBuildCommands.Contains(cmdName) || 
                                          command.Aliases.Any(alias => allowedDuringBuildCommands.Contains(alias.ToLower()));
            
            // Check if this is an always blocked command (spy, tpa, tp, home)
            bool isAlwaysBlocked = alwaysBlockedCommands.Contains(cmdName) || 
                                    command.Aliases.Any(alias => alwaysBlockedCommands.Contains(alias.ToLower()));
            
            GameState currentState = GameStateManager.Instance.CurrentState;
            
            // Get player's Steam64ID for alive check
            UnturnedPlayer uPlayer = player as UnturnedPlayer;
            ulong playerSteamId = uPlayer?.CSteamID.m_SteamID ?? 0;
            bool isPlayerAlive = GameStateManager.Instance.AlivePlayers.Contains(playerSteamId);
            
            // Block always-blocked commands during any non-Idle state (only for alive players)
            if (isAlwaysBlocked && currentState != GameState.Idle && isPlayerAlive)
            {
                cancel = true;
                
                // Penalize the player for attempting to use a blocked command
                if (uPlayer != null && uPlayer.Player != null)
                {
                    uPlayer.Player.life.serverModifyHealth(-50);
                }
                
                ChatHelper.SendTo(player, ChatLevel.ERROR, $"The /{command.Name} command is disabled during SWAT games!");
                ChatHelper.Broadcast(ChatLevel.WARNING, $"{player.DisplayName} attempted to use the /{command.Name} command! Shame on you! Tsk tsk tsk.");
                
                Console.WriteLine($"{ScriptTag.GetScriptName()} Blocked /{command.Name} command from {player.DisplayName} during game state: {currentState}");
                return;
            }
            
            // Block allowed-build commands during InProgress state (only for alive players)
            if (isAllowedBuildCommand && currentState == GameState.InProgress && isPlayerAlive)
            {
                cancel = true;
                
                // Penalize the player for attempting to use a blocked command
                if (uPlayer != null && uPlayer.Player != null)
                {
                    uPlayer.Player.life.serverModifyHealth(-50);
                }
                
                ChatHelper.SendTo(player, ChatLevel.ERROR, $"The /{command.Name} command is disabled during combat!");
                ChatHelper.Broadcast(ChatLevel.WARNING, $"{player.DisplayName} attempted to use the /{command.Name} command during combat! Shame on you! Tsk tsk tsk.");
                
                Console.WriteLine($"{ScriptTag.GetScriptName()} Blocked /{command.Name} command from {player.DisplayName} during game state: {currentState}");
                return;
            }
        }
    }
}
