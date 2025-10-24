using Rocket.API;
using S1thK3nny.SWAT.Helpers;
using System.Collections.Generic;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandGameStatus : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "status";

        public string Help => "Shows the current game status";

        public string Syntax => "";

        public List<string> Aliases => new List<string> { "info" };

        public List<string> Permissions => new List<string> { "swat.status" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            string statusMessage = GameStateManager.Instance.GetStatusMessage();
            ChatHelper.SendTo(caller, statusMessage, ChatLevel.INFO);
        }
    }
}
