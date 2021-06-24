using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Dotnet.Portal
{
    public class RunningStateChangeEventArgs : EventArgs
    {
        public RunningStateChangeEventArgs(bool running)
        {
            Running = running;
        }

        public bool Running { get; }
    }

    public class OutputReceivedEventArgs : EventArgs
    {
        public OutputReceivedEventArgs(string text, HighlightMatches highlights)
        {
            Text = text;
            Highlights = highlights;
        }

        public string Text { get; }
        public HighlightMatches Highlights { get; }
    }

    public class Runner : IDisposable
    {
        private readonly IDictionary<int, Regex> _highlights;
        private readonly FileSystemWatcher _watcher;
        private readonly ProcessStartInfo _startInfo;

        private bool _running;
        private Process _process;

        public Runner(string workingDirectory, IDictionary<int, Regex> highlights)
        {
            _highlights = highlights;
            _watcher = new FileSystemWatcher
            {
                IncludeSubdirectories = true
            };

            _watcher.Changed += Compiling;
            _watcher.Created += Compiling;
            _watcher.Deleted += Compiling;
            _watcher.Renamed += Compiling;

            _startInfo = new ProcessStartInfo("dotnet")
            {
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var watchPath = Path.Combine(workingDirectory, "obj");
            _watcher.Path = Directory.Exists(watchPath) ? watchPath : workingDirectory;
        }

        public delegate void RunningStateChangedDelegate(object sender, RunningStateChangeEventArgs args);

        public event RunningStateChangedDelegate RunningStateChanged;

        public delegate void OutputReceivedDelegate(object sender, OutputReceivedEventArgs args);

        public event OutputReceivedDelegate OutputReceived;

        private void Compiling(object sender, FileSystemEventArgs e)
        {
            if (_running)
            {
                if (Path.GetDirectoryName(e.FullPath) == e.FullPath) return;

                Stop($"Detected compilation in progress: {e.Name} {e.ChangeType}");
            }
        }

        public void Dispose()
        {
            if (_process != null)
            {
                StopProcess();
            }

            if (_watcher == null) return;

            _watcher.Changed -= Compiling;
            _watcher.Created -= Compiling;
            _watcher.Deleted -= Compiling;
            _watcher.Renamed -= Compiling;
            _watcher.Dispose();
        }

        private void ReadError()
        {
            ReadOutput(_process.StandardError.ReadLine);
        }

        private void ReadOutput()
        {
            ReadOutput(_process.StandardOutput.ReadLine);
        }

        private void ReadOutput(Func<string> output)
        {
            while (_running)
            {
                string text;
                while (_process != null && (text = output()) != null)
                {
                    var highlights = GetHighlights(text);
                    FireOutputReceived(text, highlights);
                }
            }
        }

        public void Start(bool watch)
        {
            _startInfo.Arguments = watch ? "watch run" : "run --no-build";

            _process = Process.Start(_startInfo);

            _watcher.EnableRaisingEvents = !watch;

            _running = true;

            new Thread(ReadOutput) { IsBackground = true }.Start();
            new Thread(ReadError) { IsBackground = true }.Start();
            new Thread(MonitorProcess) { IsBackground = true }.Start();

            FireRunningStateChanged();
        }

        public void Stop()
        {
            Stop("User requested");
        }

        private void Stop(string message)
        {
            StopProcess();
            FireRunningStateChanged();
            FireOutputReceived("Server stopped: " + message);
        }

        private void StopProcess()
        {
            if (!_process.HasExited)
            {
                var startInfo = new ProcessStartInfo("taskkill", $"/pid {_process.Id} /f /t")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(startInfo);
            }

            _process?.WaitForExit();
            _process?.Dispose();
            _process = null;
            _running = false;
            _watcher.EnableRaisingEvents = false;
        }

        private void MonitorProcess()
        {
            while (_running)
            {
                if (_process.HasExited)
                {
                    Stop("Process was terminated");
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        private HighlightMatches GetHighlights(string text)
        {
            foreach (var highlight in _highlights)
            {
                var level = highlight.Key;
                var pattern = highlight.Value;

                var matches = pattern.Matches(text);

                if (matches.Count == 0) continue;

                return new HighlightMatches(level, matches);
            }

            return null;
        }

        private void FireRunningStateChanged()
        {
            RunningStateChanged?.Invoke(this, new RunningStateChangeEventArgs(_running));
        }

        private void FireOutputReceived(string text, HighlightMatches highlights = null)
        {
            OutputReceived?.Invoke(this, new OutputReceivedEventArgs(text, highlights));
        }
    }

    public class HighlightMatches
    {
        public HighlightMatches(int level, MatchCollection matches)
        {
            Level = level;
            var list = new List<Match>();

            for (var i = 0; i < matches.Count; i++)
            {
                list.Add(matches[i]);
            }

            Matches = list;
        }

        public int Level { get; }

        public IReadOnlyCollection<Match> Matches { get; }
    }
}