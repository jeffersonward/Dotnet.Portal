using System.Collections.Generic;

namespace Dotnet.Portal.Settings.Models
{
    public class SolutionSettings
    {
        public string Name { get; set; }
        public string File { get; set; }
        public bool InheritHighlights { get; set; } = true;
        public List<HighlightSetting> HighlightSettings { get; set; }
        public List<ProjectSettings> Projects { get; set; }
    }
}