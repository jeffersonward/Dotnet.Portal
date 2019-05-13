namespace Dotnet.Portal.Settings.Repositories
{
    public class SolutionSettingsDto
    {
        public string Name { get; set; }
        public string File { get; set; }
        public bool InheritHighlights { get; set; } = true;
        public HighlightSettingDto[] HighlightSettings { get; set; }
        public ProjectSettingsDto[] Projects { get; set; }
    }
}