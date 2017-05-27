using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using UnityEngine;

namespace FilterExtensions
{
    internal static class Logger
    {
        public static readonly Version version = new Version(3, 0, 0, 0);

        internal enum LogLevel
        {
            Debug,
            Warn,
            Error
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
                Debug.LogFormat($"[Filter Extensions {version}]: {string.Format(format, o)}");
            }
            else if (level == LogLevel.Warn)
            {
                Debug.LogWarningFormat($"[Filter Extensions {version}]: {string.Format(format, o)}");
            }
            else
            {
                Debug.LogErrorFormat($"[Filter Extensions {version}]: {string.Format(format, o)}");
            }
        }
    }
}
