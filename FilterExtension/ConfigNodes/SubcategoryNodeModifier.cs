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
        public static void MakeRenamers(ConfigNode node, Dictionary<string, string> renames)
        {
            Debug.Assert(renames != null, $"{nameof(renames)} dictionary is assumed to never be null");
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
        }

        public static void MakeIconChangers(ConfigNode node, Dictionary<string, string> icons)
        {
            Debug.Assert(icons != null, $"{nameof(icons)} dictionary is assumed to never be null");
            foreach (string s in node.GetValues("icon"))
            {
                string[] split = s.Split(splitter, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Trim()).ToArray();
                if (split.Length != 2)
                {
                    Logger.Log($"bad length in set icon string {s}", Logger.LogLevel.Error);
                    continue;
                }
                if (icons.ContainsKey(split[0]))
                {
                    icons.Add(split[0], split[1]);
                }
            }
        }

        public static void MakeDeleters(ConfigNode node, HashSet<string> deleters)
        {
            Debug.Assert(deleters != null, $"{nameof(deleters)} hashset is assumed to never be null");
            foreach (string s in node.GetValues("remove"))
            {
                string str = s.Trim();
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }
                deleters.Add(str); // hashset doesn't need duplicate check
            }
        }
    }
}
