using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using dotnet.runner.Settings.Models;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace dotnet.runner
{
    public partial class RunnerControl
    {
        private const int MaxLineCount = 200;
        private const int LinesToCut = 50;

        private readonly IDictionary<int, Color> _highlightColors;

        private readonly Runner _runner;
        private readonly ProjectSettings _settings;

        public RunnerControl(ProjectSettings settings) : this()
        {
            _settings = settings;
            IDictionary<int, Regex> highlights = new Dictionary<int, Regex>();
            _highlightColors = new Dictionary<int, Color>();

            for (var i = 0; i < settings.HighlightSettings.Count; i++)
            {
                var highlightSetting = settings.HighlightSettings[i];
                highlights.Add(i, highlightSetting.Regex);
                _highlightColors.Add(i, highlightSetting.Color);
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

            richTextBoxOutput.BackColor = Color.Black;
            richTextBoxOutput.ForeColor = Color.White;
            richTextBoxOutput.BorderStyle = BorderStyle.None;
            richTextBoxOutput.ShowSelectionMargin = true;
            richTextBoxOutput.TabIndex = 4;
            richTextBoxOutput.LinkClicked += RichTextBoxOutput_LinkClicked;
            richTextBoxOutput.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
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
            if (richTextBoxOutput.InvokeRequired)
            {
                var d = new AppendTextCallback(AppendLine);
                Dispatcher.Invoke(d, text, highlights);
            }
            else
            {
                richTextBoxOutput.SuspendLayout();

                if (richTextBoxOutput.Lines.Length > MaxLineCount)
                {
                    var index = 0;
                    for (var i = 0; i < LinesToCut; i++)
                    {
                        index = richTextBoxOutput.Text.IndexOf('\n', index) + 1;
                    }
                    richTextBoxOutput.Select(0, index);
                    richTextBoxOutput.Cut();
                }

                var start = richTextBoxOutput.TextLength;

                richTextBoxOutput.AppendText(text);

                Highlight(start, highlights);

                richTextBoxOutput.AppendText("\r\n");

                richTextBoxOutput.ResumeLayout();
            }
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

        private void Highlight(int start, HighlightMatches highlights)
        {
            if (highlights == null) return;

            var matches = highlights.Matches;
            var color = _highlightColors[highlights.Level];

            if (matches.Count == 0) return;

            var originalSelectionStart = richTextBoxOutput.SelectionStart;
            var originalSelectionLength = richTextBoxOutput.SelectionLength;

            foreach (var match in matches)
            {
                richTextBoxOutput.Select(start + match.Index, match.Length);
                richTextBoxOutput.SelectionColor = color;
            }

            richTextBoxOutput.SelectionStart = originalSelectionStart;
            richTextBoxOutput.SelectionLength = originalSelectionLength;
        }

        public void Stop()
        {
            _runner.Stop();
        }

        public void Start()
        {
            richTextBoxOutput.Text = "";
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
            if (richTextBoxOutput.InvokeRequired)
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

        private static void RichTextBoxOutput_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void CheckBoxWatch_OnClick(object sender, RoutedEventArgs e)
        {
            _settings.Watch = checkBoxWatch.IsChecked == true;
        }
    }
}