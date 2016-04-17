using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    public class Filter
    {
        public List<Check> checks { get; set; } // checks are processed in serial (a && b), inversion gives (!a || !b) logic
        public bool invert { get; set; }

        public Filter(ConfigNode node)
        {
            checks = new List<Check>();
            foreach (ConfigNode subNode in node.GetNodes("CHECK"))
            {
                checks.Add(new Check(subNode));
            }
            checks.RemoveAll(c => c.isEmpty());

            bool tmp;
            bool.TryParse(node.GetValue("invert"), out tmp);
            invert = tmp;
        }

        public Filter(Filter f)
        {
            checks = new List<Check>();
            for (int i = 0; i < f.checks.Count; i++)
            {
                if (!f.checks[i].isEmpty())
                    checks.Add(new Check(f.checks[i]));
            }

            invert = f.invert;
        }

        public Filter(bool invert)
        {
            checks = new List<Check>();
            this.invert = invert;
        }

        public ConfigNode toConfigNode()
        {
            ConfigNode node = new ConfigNode("FILTER");
            node.AddValue("invert", this.invert.ToString());
            foreach (Check c in checks)
                node.AddNode(c.toConfigNode());

            return node;
        }

        internal bool checkFilter(AvailablePart part, int depth = 0)
        {
            for (int i = 0; i < checks.Count; i++)
            {
                if (!checks[i].checkPart(part, depth))
                    return invert ? true : false;
            }
            return invert ? false : true;
        }

        /// <summary>
        /// compare subcategory filter lists, returning true for matches
        /// </summary>
        /// <param name="fLA"></param>
        /// <param name="fLB"></param>
        /// <returns></returns>
        public static bool compareFilterLists(List<Filter> fLA, List<Filter> fLB)
        {
            if (fLA.Count != fLB.Count && fLA.Count != 0)
                return false;

            foreach (Filter fA in fLA)
            {
                if (!fLB.Any(fB => fB.Equals(fA)))
                    return false;
            }
            return true;
        }

        public bool Equals(Filter f2)
        {
            if (f2 == null)
                return false;

            if (this.invert != f2.invert)
                return false;
            else
            {
                foreach (Check c1 in this.checks)
                {
                    if (!f2.checks.Any(c2 => c1.Equals(c2)))
                        return false;
                }
                return true;
            }
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (Check c in this.checks)
            {
                hash *= c.GetHashCode();
            }

            return hash * this.invert.GetHashCode();
        }
    }
}
