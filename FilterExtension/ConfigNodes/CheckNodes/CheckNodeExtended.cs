using FilterExtensions.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExtensions.ConfigNodes.CheckNodes
{
    /// <summary>
    /// adds contains and invert variables to the mix
    /// </summary>
    public abstract class CheckNodeExtended : CheckNode
    {
        protected bool Contains { get; }
        protected bool Exact { get; }
        protected CheckNodeExtended(ConfigNode node) : base(node)
        {
            Contains = LoadContains(node);
            Exact = LoadExact(node);
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
    /// part module name
    /// </summary>
    public class CheckModuleName : CheckNodeExtended
    {
        public const string ID = "moduleName";
        public override string CheckID { get => ID; }
        public CheckModuleName(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckModuleName(part, Values, Contains, Exact);
        }
    }

    /// <summary>
    /// part module title
    /// </summary>
    public class CheckModuleTitle : CheckNodeExtended
    {
        public const string ID = "moduleTitle";
        public override string CheckID { get => ID; }
        public CheckModuleTitle(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckModuleTitle(part, Values, Contains, Exact);
        }
    }

    /// <summary>
    /// part profile check
    /// </summary>
    public class CheckProfile : CheckNodeExtended
    {
        public const string ID = "profile";
        public override string CheckID { get => ID; }
        public CheckProfile(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckBulkHeadProfiles(part, Values, Contains, Exact);
        }
    }

    /// <summary>
    /// part profile check
    /// </summary>
    public class CheckTag : CheckNodeExtended
    {
        public const string ID = "tag";
        public override string CheckID { get => ID; }
        public CheckTag(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckTags(part, Values, Contains, Exact);
        }
    }

    /// <summary>
    /// checks part resources
    /// </summary>
    public class CheckResource : CheckNodeExtended
    {
        public const string ID = "resource";
        public override string CheckID { get => ID; }
        public CheckResource(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckResource(part, Values, Contains, Exact);
        }
    }

    /// <summary>
    /// checks part engine propellants
    /// </summary>
    public class CheckPropellant : CheckNodeExtended
    {
        public const string ID = "propellant";
        public override string CheckID { get => ID; }
        public CheckPropellant(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckPropellant(part, Values, Contains, Exact);
        }
    }

    public class CheckGroup : CheckNode
    {
        public const string ID = "check";
        public override string CheckID { get => ID; }
        List<CheckNode> Group { get; }

        public CheckGroup(ConfigNode node) : base(node)
        {
            ConfigNode[] nodes = node.GetNodes("CHECK");
            var checks = new List<CheckNode>();
            foreach (var n in nodes)
            {
                CheckNode c = CheckNodeFactory.MakeCheck(n);
                if (c != null)
                {
                    checks.Add(c);
                }
            }
            Group = checks;
        }

        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ Group.All(c => c.CheckResult(part));
        }

        public override ConfigNode ToConfigNode()
        {
            var node = base.ToConfigNode();
            foreach (var n in Group)
            {
                node.AddNode(n.ToConfigNode());
            }
            return node;
        }

        public override bool Equals(CheckNode c2)
        {
            if (!(c2 is CheckGroup))
                return false;
            CheckGroup comp = (CheckGroup)c2;
            return Invert == comp.Invert && Group.Count == comp.Group.Count && !Group.Except(comp.Group).Any();
        }
    }
}
