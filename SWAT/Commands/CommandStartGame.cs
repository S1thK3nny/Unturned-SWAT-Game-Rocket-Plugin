using Rocket.API;
using Rocket.Unturned.Player;
using S1thK3nny.SWAT.Helpers;
using System.Collections.Generic;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandStartGame : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "start";

        public string Help => "Starts the SWAT game";

        public string Syntax => "[buildphase]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "swat.start" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            bool includeBuildPhase = false;

            // Check if buildphase parameter is provided
            if (command.Length > 0)
            {
                if (command[0].ToLower() == "buildphase")
                {
                    includeBuildPhase = true;
                }
                else
                {
                    ChatHelper.SendTo(caller, ChatLevel.ERROR, "Invalid parameter. Use: /swat start [buildphase]");
                    return;
                }
            }

            // Check if game can start
            if (!GameStateManager.Instance.CanStartGame(out var swat, out var terrorists, out string errorMessage))
            {
                ChatHelper.SendTo(caller, ChatLevel.ERROR, $"Cannot start game: {errorMessage}");
                return;
            }

            // Start the game
            GameStateManager.Instance.StartGame(includeBuildPhase);

            string phaseMessage = includeBuildPhase ? " with 30-minute build phase" : "";
            ChatHelper.SendTo(caller, ChatLevel.OK, $"Game started{phaseMessage}!");
        }
    }
}
