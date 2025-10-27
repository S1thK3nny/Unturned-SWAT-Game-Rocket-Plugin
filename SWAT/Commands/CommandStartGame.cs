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
            if (command.Length > 1) {
                ChatHelper.SendTo(caller, "CommandStartInvalidParameter", ChatLevel.ERROR);
                return;
            }

            int buildTime = 0;
            if (command.Length == 1 && !int.TryParse(command[0], out buildTime)) {
                ChatHelper.SendTo(caller, "CommandStartInvalidParameter", ChatLevel.ERROR);
                return;
            }

            // Start the game. StartGame handles any sort of validation internally.
            GameStateManager.Instance.StartGame(out var error, buildTime);
        }
    }
}
