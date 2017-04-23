using System;

namespace FilterExtensions.ConfigNodes.CheckNodes
{
    public enum CompareType
    {
        Equals, // default
        LessThan,
        GreaterThan,
        String
    }

    public abstract class CheckNode : IEquatable<CheckNode>
    {
        public abstract string CheckID { get; }
        protected string[] Values { get; }
        protected bool Invert { get; }

        public CheckNode(ConfigNode node)
        {
            Values = LoadValues(node);
            Invert = LoadInvert(node);
        }

        protected static string[] LoadValues(ConfigNode node)
        {
            string tmpStr = string.Empty;
            if (node.TryGetValue("value", ref tmpStr) && !string.IsNullOrEmpty(tmpStr))
            {
                string[] Values = tmpStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Values.Length; ++i)
                    Values[i] = Values[i].Trim();
                return Values;
            }
            return null;
        }

        protected static bool LoadInvert(ConfigNode node)
        {
            bool tmpBool = false;
            if (node.TryGetValue("invert", ref tmpBool))
                return tmpBool;
            return false;
        }

        protected static bool LoadContains(ConfigNode node)
        {
            bool tmpBool = false;
            if (node.TryGetValue("contains", ref tmpBool))
                return tmpBool;
            return true;
        }

        protected static bool LoadExact(ConfigNode node)
        {
            bool tmpBool = false;
            if (node.TryGetValue("exact", ref tmpBool))
                return tmpBool;
            return false;
        }

        protected static CompareType LoadEquality(ConfigNode node)
        {
            string tmpStr = string.Empty;
            if (node.TryGetValue("equality", ref tmpStr))
            {
                try
                {
                    return (CompareType)Enum.Parse(typeof(CompareType), tmpStr, true);
                }
                catch { }
            }
            return CompareType.Equals;
        }

        public abstract bool CheckResult(AvailablePart part, int depth = 0);

        public virtual ConfigNode ToConfigNode()
        {
            ConfigNode node = new ConfigNode("CHECK");
            node.AddValue("type", CheckID);
            if (Values != null)
            {
                node.AddValue("value", string.Join(",", Values));
            }
            node.AddValue("invert", Invert.ToString());
            return node;
        }

        public override bool Equals(object obj)
        {
            if (obj is CheckNode)
                return Equals((CheckNode)obj);
            return false;
        }

        public virtual bool Equals(CheckNode c2)
        {
            if (c2 == null)
                return false;
            return CheckID == c2.CheckID && Values == c2.Values && Invert == c2.Invert;
        }

        public override int GetHashCode()
        {
            int hash = CheckID.GetHashCode();
            hash = hash * 17 + Values?.GetHashCode() ?? 0;
            hash = hash * 17 + Invert.GetHashCode();
            return hash;
        }
    }
}