using System;
using System.Collections.Generic;

namespace FilterExtensions.ConfigNodes
{
    public class Filter : IEquatable<Filter>, ICloneable
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

        public object Clone()
        {
            return new Filter(this);
        }

        public bool filterResult(AvailablePart part, int depth = 0)
        {
            for(int i = 0; i < checks.Count; ++i )
            {
                if(!checks[i].checkResult(part, depth))
                    return invert;
            }
            return !invert;
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
            for (int i = fLA.Count - 1; i >= 0; --i)
            {
                if (!fLB.Contains(fLA[i]))
                    return false;
            }
            return true;
        }

        public bool Equals(Filter f2)
        {
            if (f2 == null)
                return false;

            if (invert != f2.invert)
                return false;
            for (int i = checks.Count -1; i >= 0; --i)
            {
                if (!f2.checks.Contains(checks[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (int i = checks.Count - 1; i >= 0; --i)
            {
                hash *= checks[i].GetHashCode();
            }

            return hash ^ invert.GetHashCode();
        }
    }
}
