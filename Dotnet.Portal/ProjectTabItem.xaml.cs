using System;
using System.Windows;
using System.Windows.Controls;
using Dotnet.Portal.Settings.Models;

namespace Dotnet.Portal
{
    public partial class ProjectTabItem : TabItem
    {
        private readonly RunnerControl _runnerControl;

        public ProjectTabItem()
        {
            InitializeComponent();
        }

        public ProjectTabItem(ProjectSettings settings) : this()
        {
            Settings = settings;
            Dispatcher.ShutdownStarted += (sender, args) => Shutdown();

            Header.Text = Settings.Name;
            _runnerControl = new RunnerControl(settings);
            _runnerControl.RunningStateChanged += RunnerControl_OnRunningStateChanged;
            Content = _runnerControl;
        }

        public ProjectSettings Settings { get; }

        public bool Running => _runnerControl?.Running ?? false;

        public event EventHandler ProjectClosed;

        public event EventHandler RunningStateChanged;

        public void Start()
        {
            _runnerControl.Start();
        }

        public void Stop()
        {
            _runnerControl.Stop();
        }

        public void Shutdown()
        {
            _runnerControl.Shutdown();
            _runnerControl.RunningStateChanged -= RunnerControl_OnRunningStateChanged;
        }

        private void Header_OnNameChanged(object sender, EventArgs args)
        {
            Settings.Name = Header.Text;
        }

        private void Header_OnClosed(object sender, EventArgs args)
        {
            Shutdown();
            ProjectClosed?.Invoke(this, EventArgs.Empty);
        }

        private void RunnerControl_OnRunningStateChanged(object sender, EventArgs args)
        {
            Header.Running = Running;
            var menuItem = (MenuItem)Header?.ContextMenu?.Items[0];
            if (menuItem != null)
            {
                menuItem.Header = Running ? "Stop" : "Start";
            }
            RunningStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ProjectTabControl_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _runnerControl.FocusOnOutput();
        }

        private void StartStopMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (Running)
            {
                _runnerControl.Stop();
            }
            else
            {
                _runnerControl.Start();
            }
        }

        private void RenameMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Header.EditName();
        }

        private void RemoveMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Header_OnClosed(this, EventArgs.Empty);
        }
    }
}