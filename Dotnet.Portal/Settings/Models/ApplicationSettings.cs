using System.Collections.Generic;

namespace Dotnet.Portal.Settings.Models
{
    public class ApplicationSettings
    {
        public List<HighlightSetting> DefaultHighlightSettings { get; set; }
        public List<SolutionSettings> Solutions { get; set; }
    }
}