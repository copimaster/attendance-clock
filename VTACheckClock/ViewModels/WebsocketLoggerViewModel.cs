using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Targets;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using VTACheckClock.Models;
using VTACheckClock.Services;
using VTACheckClock.Services.Libs;

namespace VTACheckClock.ViewModels
{
    class WebsocketLoggerViewModel : ViewModelBase
    {
        private readonly IRealtimeService _realtime;
        private readonly Logger log = LogManager.GetLogger("app_logger");
        private FileSystemWatcher? _watcher;
        readonly string logFilePath = Path.Combine(GlobalVars.AppWorkPath, "logs");
        private string _logText = "Waiting for log changes...";
        private string? _searchText;

        public ObservableCollection<LogEntry> LogEntries { get; } = [];
        public ObservableCollection<LogEntry> SearchResults { get; } = [];
        public ObservableCollection<LogFile> LogFiles { get; } = [];
        private int _selectedIndex = 0, _SelectedLogFile = -1;

        public WebsocketLoggerViewModel()
        {
            ConfigFileSystemWatcher();

            ReloadWSCommand = ReactiveCommand.CreateFromTask(ReloadWS);
            CancelCommand = ReactiveCommand.Create(() => { });

            this.WhenAnyValue(x => x.SelectedLogFileIndex)
            .Where(index => index != -1)
            .Subscribe(OnChangeLogFile);

            this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);

            LogEntries.CollectionChanged += LogEntries_CollectionChanged;
            _realtime = App.ServiceProvider.GetRequiredService<IRealtimeService>();
        }

        public async Task InitializeAsync()
        {
            await Task.Run(async () => {
                var files = GetLogFiles();
                var entries = await ReadLogFileAsync(logFilePath + "\\AppLog.txt");

                await Dispatcher.UIThread.InvokeAsync(() => {
                    LogFiles.Clear();
                    foreach(var f in files) LogFiles.Add(f);

                    LogEntries.Clear();
                    foreach(var e in entries) LogEntries.Add(e);
                });
            });
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

        private void ValidateMainDirectory()
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
        private static async Task<List<LogEntry>> ReadLogFileAsync(string fullFilePath)
        {
            var entries = new List<LogEntry>();
            try
            {
                using var f = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true);
                using var s = new StreamReader(f);
                var content = await s.ReadToEndAsync();
                var newLines = content.Split('\n');

                var fileInfo = new FileInfo(fullFilePath);

                if (fileInfo.Name.StartsWith("AppLog"))
                {
                    foreach (var line in newLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            entries.Add(new LogEntry()
                            {
                                Timestamp = DateTime.Now,
                                Message = line
                            });
                        }
                    }
                }
                else
                {
                    var cacheInfo = File.ReadLines(fullFilePath).Skip(1).ToList(); // This is still synchronous for cache files, might need optimization later
                    var emp_dt = GlobalVars.AppCache.RetrieveEmployees();

                    foreach (string infoItem in cacheInfo)
                    {
                        entries.Add(new LogEntry()
                        {
                            Timestamp = DateTime.Now,
                            Message = GetCacheMessage(infoItem, emp_dt)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                //LogText = ex.ToString();
                Console.WriteLine(ex);
            }
            return entries;
        }

        private void ReadLogFile(string fullFilePath)
        {
            // Mantener compatibilidad con FileSystemWatcher (que corre en otro hilo)
            // pero con cuidado. Idealmente deberíamos llamar a InitializeAsync o similar.
            // Para simplificar, usaremos la versión asíncrona y bloquearemos en Task.Run para no bloquear UI si se llama desde UI thread por error,
            // o simplemente disparar y olvidar.

            Dispatcher.UIThread.InvokeAsync(async () => {
                var entries = await ReadLogFileAsync(fullFilePath);
                LogEntries.Clear();
                foreach (var e in entries) LogEntries.Add(e);
            });
        }

        private void LogFileChanged(object o, FileSystemEventArgs e)
        {
            try {
                if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed) {
                    ReadLogFile(e.FullPath);
                    //if(SelectedLogFileIndex == LogFiles.Count - 1) {
                        Dispatcher.UIThread.InvokeAsync(async() => {
                            await UpdateSearchResults(SearchText);
                        });
                    //}
                }
            } catch (Exception ex) {
                Debug.WriteLine("Error while reading WebSocket Log Connection File: " + ex);
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
            await _realtime.ReloadAsync();
            //using StreamWriter sw = File.AppendText(logFilePath + "\\logss.txt");
            //sw.WriteLine("This is the new text");
        }

        private List<LogFile> GetLogFiles()
        {
            var files = new List<LogFile>();
            try {
                DirectoryInfo archiveDirectory = new(Path.Combine(logFilePath, "archive"));
                DirectoryInfo rootDirect = new(logFilePath);
                
                if(archiveDirectory.Exists) {
                    FileInfo[] fileList = archiveDirectory.GetFiles("*.txt");

                    foreach (FileInfo file in fileList) {
                        files.Add(new LogFile() {
                            Directory = "archive",
                            Filename = file.Name,
                            CustomName = file.Name
                        });
                    }
                }

                if(rootDirect.Exists) {
                    FileInfo[] rooFileList = rootDirect.GetFiles("*.txt");
                
                    foreach (FileInfo file in rooFileList) {
                        files.Add(new LogFile() {
                            Directory = "",
                            Filename = file.Name,
                            CustomName = file.Name
                        });
                    }
                }

                var cacheRoot = GlobalVars.DefWorkPath + @"\" + ((CacheMan.CacheDirs)0).ToString();
                var currentCacheSubdir = cacheRoot + @"\" + ((CacheMan.CacheDirs)1).ToString();
                var oldCacheSubdir = cacheRoot + @"\" + ((CacheMan.CacheDirs)2).ToString();
                string[] ignore_files = ["vtattcache_456D7012"]; // Ignorar la cache de Huellas

                DirectoryInfo curCacheInfo = new(currentCacheSubdir);
                if (curCacheInfo.Exists)
                {
                    FileInfo[] curFiles = curCacheInfo.GetFiles("*.vcch");
                   
                    foreach (FileInfo file in curFiles)
                    {
                        bool shouldIgnore = ignore_files.Any(ignorePattern => file.Name.Contains(ignorePattern, StringComparison.OrdinalIgnoreCase));

                        if (!shouldIgnore)
                        {
                            files.Add(new LogFile() {
                                Directory = currentCacheSubdir,
                                Filename = file.Name,
                                CustomName = CacheMan.ReplaceFileName(file.Name) + "_CURRENT"
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
                        bool shouldIgnore = ignore_files.Any(ignorePattern => file?.Name.IndexOf(ignorePattern, StringComparison.OrdinalIgnoreCase) >= 0);

                        if (!shouldIgnore) { 
                            files.Add(new LogFile() {
                                Directory = oldCacheSubdir,
                                Filename = file.Name,
                                CustomName = CacheMan.ReplaceFileName(file.Name) + "_OLD"
                            });
                        }
                    }
                }

                // if (files.Count == 1) SelectedLogFileIndex = 0; // Move to UI thread logic if needed
            } catch (Exception ex) {
                Console.WriteLine($"Error al listar archivos .txt: {ex.Message}");
            }
            return files;
        }

        private async void OnChangeLogFile(int index)
        {
            try {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    await Task.Delay(100);
                    var fileFound = LogFiles[index];
                    var FullPath = "";

                    if (string.IsNullOrEmpty(fileFound.Directory) || fileFound.Directory == "archive") {
                        FullPath = logFilePath + "\\" + Path.Combine(fileFound.Directory!, fileFound.Filename!);
                    }
                    else
                    {
                        FullPath = Path.Combine(fileFound.Directory, fileFound.Filename!);
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
    
        private static string GetCacheMessage(string cacheLog, DataTable? emp_dt)
        {
            var currentInfoLog = CommonProcs.EnDeCapsulateTxt(cacheLog, false);
            var cachePart = currentInfoLog.Split(['|']);
            DataRow? employee = emp_dt?.AsEnumerable().FirstOrDefault(row => row.Field<string>("EmpID") == cachePart[0]);
            string? empName = employee?.Field<string>("EmpName") ?? cachePart[0];

            DateTime v = DateTime.MinValue;
            if (cachePart[2].ToString().Contains('/')) {
                _ = DateTime.TryParse(cachePart[2].ToString(), out v);
            }
            else {
                v = CommonProcs.FromFileString(cachePart[2].ToString());
            }

            string? newMessage = $"El empleado {empName} ha registrado {CommonObjs.EvTypes[int.Parse(cachePart[1].ToString() ?? "0")]} a las {v:HH:mm:ss} horas el día {v:d/MM/yyyy}.";

            return newMessage;
        }
    }
}
