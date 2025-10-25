using Rocket.API;
using Rocket.Unturned.Player;
using S1thK3nny.SWAT.Helpers;
using SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.UnityTypes;
using System.Collections.Generic;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandStartGame : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "start";

        public string Help => "Starts the SWAT game";

        public string Syntax => "[buildtime]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "swat.start" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var buildTime = 0;

            // Check if buildphase parameter is provided
            if (command.Length > 0)
            {
                if (int.TryParse(command[0], out int parsedBuildTime))
                {
                    buildTime = parsedBuildTime;
                }
                else
                {
                    ChatHelper.SendTo(caller, ChatLevel.ERROR, "Invalid parameter. Use: /start [buildtime]");
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
            GameStateManager.Instance.StartGame(buildTime);
        }
    }
}
