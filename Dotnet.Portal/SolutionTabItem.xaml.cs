using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dotnet.Portal.Settings.Models;

namespace Dotnet.Portal
{
    public partial class SolutionTabItem : TabItem
    {
        private readonly List<ProjectTabItem> _projectTabControls;

        public SolutionTabItem()
        {
            InitializeComponent();
        }

        public SolutionTabItem(SolutionSettings settings) : this()
        {
            Settings = settings;
            Dispatcher.ShutdownStarted += (sender, args) => Shutdown();
            Projects.TabItemDropped += TabItemDropped;

            Header.Text = Settings.Name;
            _projectTabControls = new List<ProjectTabItem>();
            foreach (var project in Settings.Projects)
            {
                var projectTabControl = new ProjectTabItem(project);
                projectTabControl.ProjectClosed += ProjectControl_ProjectClosed;
                projectTabControl.RunningStateChanged += ProjectControl_RunningStateChanged;
                _projectTabControls.Add(projectTabControl);
                Projects.Items.Add(projectTabControl);
            }
        }

        private void TabItemDropped(object sender, EventArgs e)
        {
            var settings = Projects.Items.OfType<ProjectTabItem>().Select(x => x.Settings);
            Settings.Projects.Clear();
            Settings.Projects.AddRange(settings);
        }

        public SolutionSettings Settings { get; }

        public event EventHandler SolutionClosed;

        public void Shutdown()
        {
            foreach (var projectTabControl in _projectTabControls)
            {
                projectTabControl.ProjectClosed -= ProjectControl_ProjectClosed;
                projectTabControl.RunningStateChanged -= ProjectControl_RunningStateChanged;
                projectTabControl.Shutdown();
            }
        }

        private void Header_OnNameChanged(object sender, EventArgs args)
        {
            Settings.Name = Header.Text;
        }

        private void Header_OnClosed(object sender, EventArgs args)
        {
            Shutdown();
            SolutionClosed?.Invoke(this, EventArgs.Empty);
        }

        private void ProjectControl_RunningStateChanged(object sender, EventArgs e)
        {
            Header.Running = _projectTabControls.Any(x => x.Running);
        }

        private void ProjectControl_ProjectClosed(object sender, EventArgs e)
        {
            var projectTabControl = (ProjectTabItem)sender;
            projectTabControl.ProjectClosed -= ProjectControl_ProjectClosed;
            projectTabControl.RunningStateChanged -= ProjectControl_RunningStateChanged;
            _projectTabControls.Remove(projectTabControl);

            Settings.Projects.Remove(projectTabControl.Settings);

            if (Settings.Projects.Count == 0)
            {
                SolutionClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void StartAllMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var projectTabControl in _projectTabControls.Where(x => !x.Running))
            {
                projectTabControl.Start();
            }
        }

        private void StopAllMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var projectTabControl in _projectTabControls.Where(x => x.Running))
            {
                projectTabControl.Stop();
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