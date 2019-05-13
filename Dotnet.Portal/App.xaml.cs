using System.Windows;
using System.Windows.Threading;
using Dotnet.Portal.Settings.Models;
using Dotnet.Portal.Settings.Services;

namespace Dotnet.Portal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ApplicationSettings _applicationSettings;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var applicationSettingsService = new ApplicationSettingsService();
            _applicationSettings = applicationSettingsService.Get();
            var mainWindow = new MainWindow(_applicationSettings);
            mainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode != 0) return;
            var applicationSettingsService = new ApplicationSettingsService();
            applicationSettingsService.Save(_applicationSettings);
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Error");
        }
    }
}