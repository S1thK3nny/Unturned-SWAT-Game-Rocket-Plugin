using Rocket.API;

namespace S1thK3nny.SWAT
{
    public class SWATConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor { get; set; }
        public string MessageIconUrl { get; set; }

        public void LoadDefaults()
        {
            MessageColor = "yellow";
            MessageIconUrl = "https://cdn-icons-png.flaticon.com/512/387/387456.png";
        }
    }
}
