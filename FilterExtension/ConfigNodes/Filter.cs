using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterExtensions.ConfigNodes
{
    using Checks;
    public class Filter : IEquatable<Filter>, ICloneable
    {
        List<Check> Checks { get; } // checks are processed in serial (a && b), inversion gives (!a || !b) logic
        bool Invert { get; }

        public Filter(ConfigNode node)
        {
            Checks = new List<Check>();
            foreach (ConfigNode subNode in node.GetNodes("CHECK"))
            {
                var c = CheckFactory.MakeCheck(subNode);
                if (c != null)
                {
                    Checks.Add(c);
                }
            }
            bool.TryParse(node.GetValue("invert"), out bool tmp);
            Invert = tmp;
        }

        public ConfigNode ToConfigNode()
        {
            ConfigNode node = new ConfigNode("FILTER");
            node.AddValue("invert", this.Invert.ToString());
            foreach (Check c in Checks)
                node.AddNode(c.ToConfigNode());
            return node;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool FilterResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ Checks.All(c => c.CheckResult(part, depth));
        }

        /// <summary>
        /// compare subcategory filter lists, returning true for matches
        /// </summary>
        /// <param name="fLA"></param>
        /// <param name="fLB"></param>
        /// <returns></returns>
        public static bool CompareFilterLists(List<Filter> fLA, List<Filter> fLB)
        {
            if (fLA.Count != fLB.Count && fLA.Count != 0)
                return false;
            return fLA.Intersect(fLB).Count() == fLA.Count;
        }

        public override bool Equals(object obj)
        {
            if (obj is Filter)
                return Equals((Filter)obj);
            return false;
        }

        public bool Equals(Filter f2)
        {
            if (f2 == null)
                return false;
            return Invert == f2.Invert && Checks.Count == f2.Checks.Count && !Checks.Except(f2.Checks).Any();
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var c in Checks)
            {
                hash *= c.GetHashCode();
            }
            hash *= 17;

            return hash + Invert.GetHashCode();
        }

        public static ConfigNode MakeFilterNode(bool invert, List<ConfigNode> checkNodess)
        {
            ConfigNode node = new ConfigNode("FILTER");
            node.AddValue("invert", invert);
            foreach (var c in checkNodess)
            {
                node.AddNode(c);
            }
            return node;
        }
    }
}
