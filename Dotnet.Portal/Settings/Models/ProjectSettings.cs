using System.Collections.Generic;

namespace Dotnet.Portal.Settings.Models
{
    public class ProjectSettings
    {
        public string Name { get; set; }
        public string File { get; set; }
        public bool Watch { get; set; }
        public bool InheritHighlights { get; set; } = true;
        public List<HighlightSetting> HighlightSettings { get; set; }
    }
}