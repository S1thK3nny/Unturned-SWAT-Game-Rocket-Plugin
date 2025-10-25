using Rocket.API;
using Rocket.Unturned.Player;
using S1thK3nny.SWAT.Helpers;
using SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.UnityTypes;
using System.Collections.Generic;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandSkipBuildTime : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "skip";

        public string Help => "Skips the build time phase";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "swat.skip" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            // Check if game is in build phase
            if (GameStateManager.Instance.CurrentState != GameState.BuildPhase)
            {
                ChatHelper.SendTo(caller, ChatLevel.ERROR, "Cannot skip build time: Game is not in build phase!");
                return;
            }

            // Start the game
            GameStateManager.Instance.EndBuildPhase();
        }
    }
}
