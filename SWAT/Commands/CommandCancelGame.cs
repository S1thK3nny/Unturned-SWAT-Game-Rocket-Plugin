using Rocket.API;
using S1thK3nny.SWAT.Helpers;
using System.Collections.Generic;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandCancelGame : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "cancel";

        public string Help => "Cancels the current SWAT game";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "swat.cancel" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            // Check if a game is running
            if (GameStateManager.Instance.CurrentState == GameState.Idle)
            {
                ChatHelper.SendTo(caller, "NoGameIsCurrentlyRunning", ChatLevel.ERROR);
                return;
            }

            // Cancel the game
            GameStateManager.Instance.CancelGame();
            ChatHelper.SendTo(caller, ChatLevel.OK, "Game cancelled successfully!");
        }
    }
}
