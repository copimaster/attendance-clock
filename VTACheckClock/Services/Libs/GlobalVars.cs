using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VTACheckClock.Models;

namespace VTACheckClock.Services.Libs
{
    class GlobalVars
    {
        #region Constantes
        public const int FMDFormat = 16842753;
        public const string FMDVersion = "1.0.0";
        public const int ClockSetPriv = 2;
        public const int ClockStartPriv = 1;
        public const int ClockSyncPriv = 3;
        public const int AdminSetPriv = 9;
        public static readonly OfficeData? managmntoff = new() { Offid = 0, Offname = "Management Module", Offdesc = "VTAttendance Management Module Application." };
        #endregion
        public static readonly char[] SeparAtor = { '§' };
        public static string AppWorkPath = AppDomain.CurrentDomain.BaseDirectory;
        public static string? SysTempRoot = Environment.GetEnvironmentVariable("APPDATA") ?? "";
        public static string? SysAppRoot = Environment.GetEnvironmentVariable("PROGRAMFILES") ?? "";
        public static string? TempFolder = Environment.GetEnvironmentVariable("TEMP") ?? "";
        public static string TempFTPPath = @"" + TempFolder + @"\VTAManage\tempftp";
        public static string DefWorkPath = @"" + SysTempRoot + @"\VTSoft\VTAttendance";
        public static string DefRegKey = @"VTSoft\VTAttendance\";
        public static int VTAttModule = 0;
        public static int[]? UserPrivileges;
        public static bool StartingUp = true;
        public static bool IsRestart = false;
        public static bool ForceExit = false;
        public static bool BeOffline = false;
        public static bool NoFPReader = false;
        public static bool SyncOnly = false;
        public static bool OfflineInvoked = false;
        public static bool DoReinstall = false;
        public static bool SyncRetryPending = false;
        public static DateTime CachedTime = DateTime.MinValue;
        public static DateTime StartTime;
        public static ClockSettings? clockSettings;
        public static MainSettings? mainSettings;
        public static SessionData? mySession;
        public static SessionData? clockSession;
        public static OfficeData? this_office;
        public static List<ParamData>? sysParams;
        public static CacheMan? AppCache;
        public static Stopwatch RunningTime = new();
        public static string? TimeZone;
    }
}
