using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    class Filter
    {
        internal List<Check> checks = new List<Check>(); // checks are processed in serial (simulating boolean AND operation)

        public Filter(ConfigNode node)
        {
            foreach (ConfigNode subNode in node.GetNodes("CHECK"))
            {
                checks.Add(new Check(subNode));
            }
        }

        internal bool checkFilter(AvailablePart part)
        {
            foreach (Check c in checks)
            {
                bool val = c.checkPart(part);
                if (!val)
                {
                    return false;
                }
            }
            return true; // part passed all checks, thus meets the filter requirements
        }
    }
}
