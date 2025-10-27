using Rocket.API;
using S1thK3nny.SWAT.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace S1thK3nny.SWAT.Commands
{
    public class CommandShowTeams : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "showteams";

        public string Help => "Displays the current teams and their members";

        public string Syntax => "";

        public List<string> Aliases => new List<string> { "teams" };

        public List<string> Permissions => new List<string> { "swat.showteams" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var (swatIds, terroristIds) = GameStateManager.Instance.GetTeamsInfo();

            var swatNames = swatIds.Select(id => PlayerNameHelper.GetDisplayName(id, stripTags: true))
                                   .Where(name => !ulong.TryParse(name, out _)) // Filter out Steam64IDs (offline players)
                                   .OrderBy(name => name, System.StringComparer.OrdinalIgnoreCase)
                                   .ToList();

            var terroristNames = terroristIds.Select(id => PlayerNameHelper.GetDisplayName(id, stripTags: true))
                                             .Where(name => !ulong.TryParse(name, out _)) // Filter out Steam64IDs (offline players)
                                             .OrderBy(name => name, System.StringComparer.OrdinalIgnoreCase)
                                             .ToList();

            string swatMembers = swatNames.Count > 0 ? string.Join(", ", swatNames) : "No members";
            string terroristsMembers = terroristNames.Count > 0 ? string.Join(", ", terroristNames) : "No members";

            ChatHelper.SendTo(caller, ChatLevel.INFO, $"SWAT Team: {swatMembers}", UnityEngine.Color.blue);
            ChatHelper.SendTo(caller, ChatLevel.INFO, $"Terrorists Team: {terroristsMembers}", UnityEngine.Color.red);
        }
    }
}
