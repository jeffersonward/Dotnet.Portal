using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Dotnet.Portal.Settings.Repositories
{
    public class ApplicationSettingsRepository
    {
        public ApplicationSettingsDto Load()
        {
            //TODO move away from setting
            var settings = Properties.Settings.Default.UserSettings ?? Properties.Settings.Default.DefaultSettings;

            if (string.IsNullOrWhiteSpace(settings))
            {
                Save(GenerateDefaultSettings());
                return Load();
            }

            var serializer = new XmlSerializer(typeof(ApplicationSettingsDto));
            using (var reader = new StringReader(settings))
            {
                return (ApplicationSettingsDto)serializer.Deserialize(reader);
            }
        }

        public void Save(ApplicationSettingsDto settings)
        {
            var serializer = new XmlSerializer(typeof(ApplicationSettingsDto));
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                serializer.Serialize(writer, settings);
            }
            Properties.Settings.Default.UserSettings = builder.ToString();
            Properties.Settings.Default.Save();
        }

        private static ApplicationSettingsDto GenerateDefaultSettings()
        {
            return new ApplicationSettingsDto
            {
                DefaultHighlightSettings = GetDefaultHighlightSettings()
            };
        }

        private static HighlightSettingDto[] GetDefaultHighlightSettings()
        {
            var settings = new[]
            {
                GetSerilogHighlight(Color.Gray, "VRB"),
                GetSerilogHighlight(Color.Green, "INF"),
                GetSerilogHighlight(Color.Cyan, "DBG"),
                GetSerilogHighlight(Color.Orange, "WRN"),
                GetSerilogHighlight(Color.Red, "ERR"),
                GetSerilogHighlight(Color.MediumPurple, "FTL")
            };

            return settings;
        }

        private static HighlightSettingDto GetSerilogHighlight(Color color, string level)
        {
            const string serilogPattern = @"^(\[\d{{2}}\:\d{{2}}\:\d{{2}}[ ]{0}\][ ]|\d{{2}}\:\d{{2}}\:\d{{2}}[ ]\[{0}\][ ])";

            var hex = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            var regex = string.Format(serilogPattern, level);

            return new HighlightSettingDto { Color = hex, Regex = regex };
        }
    }
}