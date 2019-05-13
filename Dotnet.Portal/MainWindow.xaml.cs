using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dotnet.Portal.Settings.Models;
using Dotnet.Portal.SolutionReader.Services;
using Microsoft.Win32;

namespace Dotnet.Portal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private readonly ApplicationSettings _settings;

        internal MainWindow(ApplicationSettings settings) : this()
        {
            _settings = settings;

            foreach (var solution in settings.Solutions)
            {
                TabControlSolutions.Items.Add(CreateSolutionTabItem(solution));
            }

            var settingsHeader = new TabHeader
            {
                Content = new Image { Source = new BitmapImage(new Uri("images/settings.png", UriKind.Relative)) }
            };

            var settingsTabItem = new TabItem
            {
                Header = settingsHeader,
                Background = Brushes.Black,
                Foreground = Brushes.White,
                Height = 36,
                AllowDrop = false
            };

            settingsTabItem.MouseDown += OpenSettings;
            TabControlSolutions.Sticky = settingsTabItem;

            TabControlSolutions.TabItemDropped += TabItemDropped;

            if (settings.Solutions.Count > 0)
            {
                TabControlSolutions.SelectedIndex = 1;
            }
        }

        private TabItem CreateSolutionTabItem(SolutionSettings solution)
        {
            var solutionTabControl = new SolutionTabItem(solution);
            solutionTabControl.SolutionClosed += SolutionTabControl_SolutionClosed;
            return solutionTabControl;
        }

        private void SolutionTabControl_SolutionClosed(object sender, EventArgs e)
        {
            var solutionTabControl = (SolutionTabItem)sender;
            solutionTabControl.Shutdown();
            TabControlSolutions.Items.Remove(solutionTabControl);
            _settings.Solutions.Remove(solutionTabControl.Settings);
        }

        private void TabItemDropped(object sender, EventArgs e)
        {
            var settings = TabControlSolutions.Items.OfType<SolutionTabItem>().Select(x => x.Settings);
            _settings.Solutions.Clear();
            _settings.Solutions.AddRange(settings);
        }

        private void OnMinimise(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximise(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            ImageMaximiseRestore.Source = WindowState == WindowState.Maximized ? new BitmapImage(new Uri(@"pack://application:,,,/images/restore.png")) : new BitmapImage(new Uri(@"pack://application:,,,/images/maximise.png"));
        }

        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var openFileDialog = new OpenFileDialog { Filter = "Solution Files (*.sln)|*.sln" };
            if (openFileDialog.ShowDialog() == true)
            {
                var solutionReaderService = new SolutionReaderService();
                var solution = solutionReaderService.Read(openFileDialog.FileName);

                var solutionSettings = new SolutionSettings
                {
                    Name = solution.Name,
                    File = solution.File.FullName,
                    InheritHighlights = true,
                    HighlightSettings = new List<HighlightSetting>(_settings.DefaultHighlightSettings),
                    Projects = new List<ProjectSettings>()
                };

                foreach (var project in solution.Projects)
                {
                    var projectSettings = new ProjectSettings
                    {
                        Name = project.Name,
                        File = project.File,
                        Watch = false,
                        InheritHighlights = true,
                        HighlightSettings = new List<HighlightSetting>(_settings.DefaultHighlightSettings)
                    };

                    solutionSettings.Projects.Add(projectSettings);
                }

                _settings.Solutions.Add(solutionSettings);

                TabControlSolutions.Items.Insert(TabControlSolutions.Items.Count - 1, CreateSolutionTabItem(solutionSettings));
            }
        }
    }
}