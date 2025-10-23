using Rocket.API;
using S1thK3nny.SWAT.Helpers;
using System.Collections.Generic;

namespace S1thK3nny.SWAT.Commands
{
    public class TestCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "swat"; // The name of the command

        public string Help => "Test command for SWAT plugin"; // A brief description of the command

        public string Syntax => ""; // Something something variables for the command?

        public List<string> Aliases => new List<string>(); // Just more names for the command

        public List<string> Permissions => new List<string> { "swat.test" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            ChatHelper.SendTo(caller, "SwatTestMessage", ChatLevel.INFO); // Send a message to the player who called the command. Look up the dictionary key in the Translations section of the Plugin.cs        }
        }
    }
}