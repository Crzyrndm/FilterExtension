using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FilterExtensions.ConfigNodes
{
    public static class SubcategoryNodeModifier
    {
        static readonly string[] splitter = new string[] { "=>" };
        public static Dictionary<string, string> MakeRenamers(ConfigNode node)
        {
            var renames = new Dictionary<string, string>();
            foreach (string s in node.GetValues("name"))
            {
                string[] split = s.Split(splitter, StringSplitOptions.RemoveEmptyEntries)
                    .Select(str => str.Trim()).ToArray();
                if (split.Length != 2)
                {
                    Logger.Log($"bad length in rename string {s}", Logger.LogLevel.Error);
                    continue;
                }
                if (!renames.ContainsKey(split[0]))
                {
                    renames.Add(split[0], split[1]);
                }
            }
            return renames;
        }

        public static Dictionary<string, string> MakeIconChangers(ConfigNode node)
        {
            var icons = new Dictionary<string, string>();
            foreach (string s in node.GetValues("icon"))
            {
                string[] split = s.Split(splitter, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Trim()).ToArray();
                if (split.Length != 2)
                {
                    Logger.Log($"bad length in set icon string {s}", Logger.LogLevel.Error);
                    continue;
                }
                if (!icons.ContainsKey(split[0]))
                {
                    icons.Add(split[0], split[1]);
                }
            }
            return icons;
        }

        public static HashSet<string> MakeDeleters(ConfigNode node)
        {
            HashSet<string> deleters = new HashSet<string>();
            foreach (string s in node.GetValues("remove"))
            {
                string str = s.Trim();
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }
                deleters.Add(str); // hashset doesn't need duplicate check
            }
            return deleters;
        }
    }
}
