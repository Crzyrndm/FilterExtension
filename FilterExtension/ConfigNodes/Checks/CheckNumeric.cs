using FilterExtensions.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExtensions.ConfigNodes.Checks
{
    /// <summary>
    /// adds equality to the mix
    /// </summary>
    public abstract class CheckNumeric : Check
    {
        protected CompareType Equality { get; }
        protected CheckNumeric(ConfigNode node) : base(node)
        {
            Equality = LoadEquality(node);
        }

        public override ConfigNode ToConfigNode()
        {
            var node = base.ToConfigNode();
            node.AddValue("equality", Equality.ToString());
            return node;
        }
    }

    /// <summary>
    /// check part attach nodes size
    /// </summary>
    public class CheckSize : CheckNumeric
    {
        public const string ID = "size";
        public override string CheckID { get => ID; }
        bool Contains { get; }
        bool Exact { get; }

        public CheckSize(ConfigNode node) : base(node)
        {
            Contains = LoadContains(node);
            Exact = LoadExact(node);
        }

        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckPartSize(part, Values, Contains, Equality, Exact);
        }

        public override ConfigNode ToConfigNode()
        {
            var node = base.ToConfigNode();
            node.AddValue("contains", Contains.ToString());
            node.AddValue("exact", Exact.ToString());
            return node;
        }
    }

    /// <summary>
    /// check part crew capacity
    /// </summary>
    public class CheckCrew : CheckNumeric
    {
        public const string ID = "crew";
        public override string CheckID { get => ID; }

        public CheckCrew(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckCrewCapacity(part, Values, Equality);
        }
    }

    /// <summary>
    /// check part dry mass
    /// </summary>
    public class CheckMass : CheckNumeric
    {
        public const string ID = "mass";
        public override string CheckID { get => ID; }

        public CheckMass(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckMass(part, Values, Equality);
        }
    }

    /// <summary>
    /// check part dry cost
    /// </summary>
    public class CheckCost : CheckNumeric
    {
        public const string ID = "cost";
        public override string CheckID { get => ID; }

        public CheckCost(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckCost(part, Values, Equality);
        }
    }

    /// <summary>
    /// check part crash tolerance
    /// </summary>
    public class CheckCrash : CheckNumeric
    {
        public const string ID = "crash";
        public override string CheckID { get => ID; }

        public CheckCrash(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckCrashTolerance(part, Values, Equality);
        }
    }

    /// <summary>
    /// check part max temperature
    /// </summary>
    public class CheckMaxTemp : CheckNumeric
    {
        public const string ID = "maxTemp";
        public override string CheckID { get => ID; }

        public CheckMaxTemp(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckTemperature(part, Values, Equality);
        }
    }
}
