using System;
using UnityEngine;

namespace FilterExtensions
{
    internal static class Logger
    {
        public static readonly Version version = new Version(3, 0, 1);
        public static readonly string versionString = $"[Filter Extensions {version}]:";

        internal enum LogLevel
        {
            Debug,
            Warn,
            Error
        }


        /// <summary>
        /// format the string to be logged and prefix with mod id + version
        /// </summary>
        /// <param name="format">string format to be logged</param>
        /// <param name="o">params for the format</param>
        /// <returns></returns>
        static string LogString(string format, params object[] o)
        {
            return $"{versionString} {string.Format(format, o)}";
        }

        /// <summary>
        /// Debug messages only compiled in debug build. Also much easier to search for once debugging/development complete...
        /// </summary>
        /// <param name="o"></param>
        /// <param name="level"></param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Dev(object o, LogLevel level = LogLevel.Debug)
        {
            Log(o, level);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Dev(string format, LogLevel level = LogLevel.Debug, params object[] o)
        {
            Log(format, level, o);
        }

        /// <summary>
        /// Debug.Log with FE id/version inserted
        /// </summary>
        /// <param name="o"></param>
        internal static void Log(object o, LogLevel level = LogLevel.Debug)
        {
            Log(o.ToString(), level);
        }

        internal static void Log(string format, LogLevel level = LogLevel.Debug, params object[] o)
        {
            if (level == LogLevel.Debug)
            {
                Debug.Log(LogString(format, o));
            }
            else if (level == LogLevel.Warn)
            {
                Debug.LogWarning(LogString(format, o));
            }
            else
            {
                Debug.LogError(LogString(format, o));
            }
        }
    }
}
