namespace Dotnet.Portal.Settings.Repositories
{
    public class ApplicationSettingsDto
    {
        public HighlightSettingDto[] DefaultHighlightSettings { get; set; }
        public SolutionSettingsDto[] Solutions { get; set; }
    }
}