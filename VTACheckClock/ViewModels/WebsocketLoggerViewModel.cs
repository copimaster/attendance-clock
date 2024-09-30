﻿using Avalonia.Threading;
using NLog;
using NLog.Targets;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using VTA_Clock;
using VTACheckClock.Models;
using VTACheckClock.Services;
using VTACheckClock.Services.Libs;

namespace VTACheckClock.ViewModels
{
    class WebsocketLoggerViewModel : ViewModelBase
    {
        private readonly WSClient _WSClient = new();
        //private readonly Logger log = LogManager.GetLogger("app_logger");
        private FileSystemWatcher? _watcher;
        readonly string logFilePath = Path.Combine(GlobalVars.AppWorkPath, "logs");
        private string _logText = "Waiting for log changes...";
        private string? _searchText;

        public ObservableCollection<LogEntry> LogEntries { get; } = new();
        public ObservableCollection<LogEntry> SearchResults { get; } = new();
        public ObservableCollection<LogFile> LogFiles { get; } = new();
        private int _selectedIndex = 0, _SelectedLogFile = -1;

        public WebsocketLoggerViewModel()
        {
            ReadLogFile(logFilePath + "\\AppLog.txt");
            ConfigFileSystemWatcher();

            ReloadWSCommand = ReactiveCommand.CreateFromTask(ReloadWS);
            CancelCommand = ReactiveCommand.Create(() => { });
            GetLogFiles();
            this.WhenAnyValue(x => x.SelectedLogFileIndex)
            .Where(index => index != -1)
            .Subscribe(OnChangeLogFile);

            this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);

            LogEntries.CollectionChanged += LogEntries_CollectionChanged;
        }

        public string LogText
        {
            get => _logText;
            set => this.RaiseAndSetIfChanged(ref _logText, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }

        public int SelectedLogFileIndex
        {
            get => _SelectedLogFile;
            set => this.RaiseAndSetIfChanged(ref _SelectedLogFile, value);
        }

        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ICommand ReloadWSCommand { get; }

        private void validateMainDirectory()
        {
            bool exists = Directory.Exists(logFilePath);
            if (!exists) Directory.CreateDirectory(logFilePath);

            var fileName = logFilePath + "\\AppLog.txt";
            if (!File.Exists(fileName)) {
                // Create a file to write to.
                using StreamWriter sw = File.CreateText(fileName);
                sw.WriteLine("");
                sw.Close();
            }
        }

        /// <summary>
        /// Configura el FileSystemWatcher para monitorear cambios en el archivo de registro generado por NLog
        /// </summary>
        private void ConfigFileSystemWatcher()
        {
            try {
                //validateMainDirectory();
                _watcher = new FileSystemWatcher {
                    Path = logFilePath, // Replace with your log folder path
                    Filter = "AppLog.txt", // Replace with your log file name
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName
                };
                _watcher.Changed += LogFileChanged;
                _watcher.Created += LogFileChanged;
                _watcher.Deleted += (o, e) => {
                    LogText = "";
                    LogEntries.Clear();
                };

            } catch (Exception ex) {
                //log.Warn(ex);
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// NLog - Allow other processes to read log file
        /// </summary>
        private void ReadLogFile(string fullFilePath)
        {
            try {
                LogEntries.Clear();

                using var f = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
                using var s = new StreamReader(f);
                var newLines = s.ReadToEnd().Split('\n');

                var fileInfo = new FileInfo(fullFilePath);

                if(fileInfo.Name.StartsWith("AppLog")) { 
                    foreach (var line in newLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line)) {
                            //Debug.WriteLine(line.TrimEnd('\r') + "\n");
                            LogEntries.Add(new LogEntry() {
                                Timestamp = DateTime.Now,
                                Message = line
                            });
                        }
                    }
                } else {
                    var cacheInfo = File.ReadLines(fullFilePath).Skip(1).ToList();

                    foreach (string infoItem in cacheInfo)
                    {
                        LogEntries.Add(new LogEntry() {
                            Timestamp = DateTime.Now,
                            Message = GetCacheMessage(infoItem)
                        });
                    }
                }

                //Dispatcher.UIThread.InvokeAsync(() => LogText += newLines);
            } catch (Exception ex) {
                LogText = ex.ToString();
            }
        }

        private void LogFileChanged(object o, FileSystemEventArgs e)
        {
            try {
                if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed) {
                    ReadLogFile(e.FullPath);
                    if(SelectedLogFileIndex == LogFiles.Count - 1) {
                        Dispatcher.UIThread.InvokeAsync(async() => {
                            await UpdateSearchResults(SearchText);
                        });
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine("Error while reading WebSocket Log Connection File: " + ex);
            }
        }

        private void LogMemoryChanged(object o, FileSystemEventArgs e)
        {
            var memoryTarget = LogManager.Configuration.FindTargetByName<MemoryTarget>("logViewer");
            if (memoryTarget != null) {
                //LogText = memoryTarget.Logs.Aggregate(new StringBuilder(), (sb, l) => sb.AppendLine(l + "\n")).ToString();
            }
        }

        private async Task ReloadWS()
        {
            await _WSClient.ReloadConnection();
            //using StreamWriter sw = File.AppendText(logFilePath + "\\logss.txt");
            //sw.WriteLine("This is the new text");
        }

        private void GetLogFiles()
        {
            try {
                DirectoryInfo archiveDirectory = new(Path.Combine(logFilePath, "archive"));
                DirectoryInfo rootDirect = new(logFilePath);
                
                if(archiveDirectory.Exists) {
                    FileInfo[] fileList = archiveDirectory.GetFiles("*.txt");

                    foreach (FileInfo file in fileList) {
                        LogFiles.Add(new LogFile() {
                            directory = "archive",
                            filename = file.Name
                        });
                    }
                }

                if(rootDirect.Exists) {
                    FileInfo[] rooFileList = rootDirect.GetFiles("*.txt");
                
                    foreach (FileInfo file in rooFileList) {
                        LogFiles.Add(new LogFile() {
                            directory = "",
                            filename = file.Name
                        });
                    }
                }

                var cacheRoot = GlobalVars.DefWorkPath + @"\" + ((CacheMan.CacheDirs)0).ToString();
                var currentCacheSubdir = cacheRoot + @"\" + ((CacheMan.CacheDirs)1).ToString();
                var oldCacheSubdir = cacheRoot + @"\" + ((CacheMan.CacheDirs)2).ToString();
                string[] ignore_files = new string[] { "vtattcache_456D7012" };

                DirectoryInfo curCacheInfo = new(currentCacheSubdir);
                if (curCacheInfo.Exists)
                {
                    FileInfo[] curFiles = curCacheInfo.GetFiles("*.vcch");
                   

                    foreach (FileInfo file in curFiles)
                    {
                        bool shouldIgnore = ignore_files.Any(ignorePattern =>
                            file.Name.IndexOf(ignorePattern, StringComparison.OrdinalIgnoreCase) >= 0);

                        if (!shouldIgnore)
                        {
                            LogFiles.Add(new LogFile() {
                                directory = currentCacheSubdir,
                                filename = file.Name
                            });
                        }
                    }
                }

                DirectoryInfo oldCacheInfo = new(oldCacheSubdir);
                if (curCacheInfo.Exists)
                {
                    FileInfo[] oldCacheFiles = oldCacheInfo.GetFiles("*.vcch");

                    foreach (FileInfo file in oldCacheFiles)
                    {
                        bool shouldIgnore = ignore_files.Any(ignorePattern =>
                            file.Name.IndexOf(ignorePattern, StringComparison.OrdinalIgnoreCase) >= 0);

                        if (!shouldIgnore) { 
                            LogFiles.Add(new LogFile() {
                                directory = oldCacheSubdir,
                                filename = file.Name
                            });
                        }
                    }
                }

                if (LogFiles.Count == 1) SelectedLogFileIndex = 0;
            } catch (Exception ex) {
                Console.WriteLine($"Error al listar archivos .txt: {ex.Message}");
            }
        }

        private async void OnChangeLogFile(int index)
        {
            try {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    await Task.Delay(100);
                    var fileFound = LogFiles[index];
                    var FullPath = "";

                    if (string.IsNullOrEmpty(fileFound.directory) || fileFound.directory == "archive") {
                        FullPath = logFilePath + "\\" + Path.Combine(fileFound.directory!, fileFound.filename!);
                    }
                    else
                    {
                        FullPath = Path.Combine(fileFound.directory, fileFound.filename!);
                    }

                    ReadLogFile(FullPath);
                    await UpdateSearchResults(SearchText);
                });
            } catch(Exception) { 
            
            }
        }

        private async void DoSearch(string s)
        {
            await UpdateSearchResults(s);
        }

        private void LogEntries_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        private async Task UpdateSearchResults(string? s)
        {
            SearchResults.Clear();

            if (!string.IsNullOrWhiteSpace(s))
            {
                // Filtrar LogEntries según el criterio de búsqueda
                var filteredEntries = await Task.Run(() => {
                    return LogEntries.Where(entry => entry.Message.ToLower().Contains(s.ToLower(), StringComparison.OrdinalIgnoreCase)).ToList();
                });

                foreach (var entry in filteredEntries)
                {
                    SearchResults.Add(entry);
                }
            } else {
                foreach (var entry in LogEntries)
                {
                    SearchResults.Add(entry);
                }
            }

            SelectedIndex = SearchResults.Count - 1;
        }
    
        private static string GetCacheMessage(string cacheLog)
        {
            var currentInfoLog = CommonProcs.EnDeCapsulateTxt(cacheLog, false);
            var cachePart = currentInfoLog.Split(new char[] { '|' });

            var newMessage = "";
            //var tester = new MainWindowViewModel().FingerPrints;

            DateTime v = CommonProcs.FromFileString(cachePart[2].ToString());
            newMessage = $"El empleado { cachePart[0]} ha registrado {CommonObjs.EvTypes[int.Parse(cachePart[1].ToString() ?? "0")]} a las {v.ToString("HH:mm:ss")} horas.";

            return newMessage;
        }
    }
}
