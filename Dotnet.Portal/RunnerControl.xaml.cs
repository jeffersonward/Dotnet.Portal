using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dotnet.Portal.Settings.Models;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Dotnet.Portal
{
    public partial class RunnerControl
    {
        private const int MaxLineCount = 200;
        private const int LinesToCut = 50;

        private readonly IDictionary<int, Brush> _highlightColors;

        private readonly Runner _runner;
        private readonly ProjectSettings _settings;

        public RunnerControl(ProjectSettings settings) : this()
        {
            _settings = settings;
            IDictionary<int, Regex> highlights = new Dictionary<int, Regex>();
            _highlightColors = new Dictionary<int, Brush>();

            for (var i = 0; i < settings.HighlightSettings.Count; i++)
            {
                var highlightSetting = settings.HighlightSettings[i];
                highlights.Add(i, highlightSetting.Regex);
                _highlightColors.Add(i, new SolidColorBrush(highlightSetting.Color));
            }

            textBoxWorkingDirectory.Text = settings.File;
            checkBoxWatch.IsChecked = settings.Watch;

            _runner = new Runner(new FileInfo(settings.File).DirectoryName, highlights);
            _runner.OutputReceived += RunnerOnOutputReceived;
            _runner.RunningStateChanged += RunnerOnRunningStateChanged;
        }

        public RunnerControl()
        {
            Dispatcher.ShutdownStarted += (sender, args) => Shutdown();

            InitializeComponent();

            richTextBoxOutput.Document.LineHeight = 10;
        }

        private delegate void AppendTextCallback(string text, HighlightMatches highlights);

        public event EventHandler RunningStateChanged;

        public bool Running { get; private set; }

        public void FocusOnOutput()
        {
            richTextBoxOutput.Focus();
        }

        private void AppendLine(string text, HighlightMatches highlights = null)
        {
            if (!richTextBoxOutput.Dispatcher.CheckAccess())
            {
                var d = new AppendTextCallback(AppendLine);
                richTextBoxOutput.Dispatcher.Invoke(d, text, highlights);
            }
            else
            {
                var autoScroll = ShouldAutoScroll();

                if (richTextBoxOutput.Document.Blocks.Count > MaxLineCount)
                {
                    for (var i = 0; i < LinesToCut; i++)
                    {
                        richTextBoxOutput.Document.Blocks.Remove(richTextBoxOutput.Document.Blocks.FirstBlock);
                    }
                }

                var range = new TextRange(richTextBoxOutput.Document.ContentEnd.DocumentEnd, richTextBoxOutput.Document.ContentEnd.DocumentEnd)
                {
                    Text = text + "\r\n"
                };

                range.ApplyPropertyValue(TextElement.ForegroundProperty, richTextBoxOutput.Foreground);

                Highlight(range, highlights);

                if (autoScroll)
                {
                    richTextBoxOutput.ScrollToEnd();
                    richTextBoxOutput.Selection.Select(richTextBoxOutput.Document.ContentEnd, richTextBoxOutput.Document.ContentEnd);
                }
            }
        }

        private bool ShouldAutoScroll()
        {
            var currentSelection = richTextBoxOutput.Selection;
            if (!currentSelection.IsEmpty) return false;

            var textRange = new TextRange(richTextBoxOutput.CaretPosition, richTextBoxOutput.Document.ContentEnd);
            return textRange.IsEmpty || string.IsNullOrWhiteSpace(textRange.Text);
        }

        private void ButtonStartStop_OnClick(object sender, RoutedEventArgs e)
        {
            if (Running)
            {
                Stop();
            }
            else
            {
                Start();
            }
        }

        public void Shutdown()
        {
            if (_runner == null) return;

            _runner.OutputReceived -= RunnerOnOutputReceived;
            _runner.RunningStateChanged -= RunnerOnRunningStateChanged;
            _runner?.Dispose();

            AppendLine("");
            AppendLine("");
            AppendLine("Disposed");
        }

        private void Highlight(TextRange range, HighlightMatches highlights)
        {
            if (highlights == null) return;

            var matches = highlights.Matches;
            var color = _highlightColors[highlights.Level];

            if (matches.Count == 0) return;

            foreach (var match in matches)
            {
                var start = range.Start.GetPositionAtOffset(match.Index);
                if (start == null) continue;

                var end = start.GetPositionAtOffset(match.Length);

                var highlightRange = new TextRange(start, end);

                highlightRange.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            }
        }

        public void Stop()
        {
            _runner.Stop();
        }

        public void Start()
        {
            richTextBoxOutput.Document.Blocks.Clear();
            buttonStartStop.ToolTip = "Stop";
            imageStartStop.Source = new BitmapImage(new Uri("images/blue-cross.png", UriKind.Relative));
            checkBoxWatch.IsEnabled = false;

            _runner.Start(checkBoxWatch.IsChecked ?? false);
        }

        private void TextBoxWorkingDirectory_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isCopy = e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            e.Handled = !isCopy;
        }

        private void VisualStopped()
        {
            if (!buttonStartStop.Dispatcher.CheckAccess())
            {
                buttonStartStop.Dispatcher.Invoke(VisualStopped);
            }
            else
            {
                AppendLine("");
                AppendLine("");
                buttonStartStop.ToolTip = "Start";
                imageStartStop.Source = new BitmapImage(new Uri("images/orange-tick.png", UriKind.Relative));
                checkBoxWatch.IsEnabled = true;
            }
        }

        private delegate void EventCallback(object sender, RunningStateChangeEventArgs args);

        private void RunnerOnRunningStateChanged(object sender, RunningStateChangeEventArgs args)
        {
            if (!richTextBoxOutput.Dispatcher.CheckAccess())
            {
                var d = new EventCallback(RunnerOnRunningStateChanged);
                Dispatcher.Invoke(d, sender, args);
                return;
            }

            Running = args.Running;
            if (!Running)
            {
                VisualStopped();
            }
            RunningStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RunnerOnOutputReceived(object sender, OutputReceivedEventArgs args)
        {
            AppendLine(args.Text, args.Highlights);
        }

        private void CheckBoxWatch_OnClick(object sender, RoutedEventArgs e)
        {
            _settings.Watch = checkBoxWatch.IsChecked == true;
        }
    }
}