using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Dotnet.Portal.Settings.Models
{
    public class HighlightSetting
    {
        public Regex Regex { get; set; }
        public Color Color { get; set; }
    }
}