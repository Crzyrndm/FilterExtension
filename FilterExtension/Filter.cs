using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    class Filter
    {
        internal List<Check> checks = new List<Check>(); // checks are processed in serial (a && b), inversion gives (!a || !b) logic
        internal bool invert = false;

        public Filter(ConfigNode node)
        {
            foreach (ConfigNode subNode in node.GetNodes("CHECK"))
            {
                checks.Add(new Check(subNode));
            }
            try
            {
                invert = bool.Parse(node.GetValue("invert"));
            }
            catch { }
            if (invert == null)
                invert = false;
        }

        internal bool checkFilter(AvailablePart part)
        {
            foreach (Check c in checks)
            {
                bool val = c.checkPart(part);
                if (!val)
                {
                    if (invert)
                        return true; // part failed a check, result inverted
                    else
                        return false; // part failed a check
                }
            }
            if (invert)
                return false; // part passed all checks, result inverted
            else
                return true; // part passed all checks, thus meets the filter requirements
        }
    }
}
