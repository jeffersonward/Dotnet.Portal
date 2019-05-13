namespace Dotnet.Portal.Settings.Repositories
{
    public class ProjectSettingsDto
    {
        public string Name { get; set; }
        public string File { get; set; }
        public bool Watch { get; set; }
        public bool InheritHighlights { get; set; } = true;
        public HighlightSettingDto[] HighlightSettings { get; set; }
    }
}