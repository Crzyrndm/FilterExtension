using FilterExtensions.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExtensions.ConfigNodes.CheckNodes
{
    /// <summary>
    /// part name check
    /// </summary>
    public class CheckName : CheckNode
    {
        public const string ID = "name";
        public override string CheckID { get => ID; }
        public CheckName(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckName(part, Values);
        }
    }

    /// <summary>
    /// Part title check
    /// </summary>
    public class CheckTitle : CheckNode
    {
        public const string ID = "title";
        public override string CheckID { get => ID; }
        public CheckTitle(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckTitle(part, Values);
        }
    }

    /// <summary>
    /// part technology check
    /// </summary>
    public class CheckTech : CheckNode
    {
        public const string ID = "tech";
        public override string CheckID { get => ID; }
        public CheckTech(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckTech(part, Values);
        }
    }

    /// <summary>
    /// part manufacturer check
    /// </summary>
    public class CheckManufacturer : CheckNode
    {
        public const string ID = "manufacturer";
        public override string CheckID { get => ID; }
        public CheckManufacturer(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckManufacturer(part, Values);
        }
    }

    /// <summary>
    /// part folder check
    /// </summary>
    public class CheckFolder : CheckNode
    {
        public const string ID = "folder";
        public override string CheckID { get => ID; }
        public CheckFolder(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckFolder(part, Values);
        }
    }

    /// <summary>
    /// part path check
    /// </summary>
    public class CheckPath : CheckNode
    {
        public const string ID = "path";
        public override string CheckID { get => ID; }
        public CheckPath(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckPath(part, Values);
        }
    }

    /// <summary>
    /// part category check
    /// </summary>
    public class CheckCategory : CheckNode
    {
        public const string ID = "category";
        public override string CheckID { get => ID; }
        public CheckCategory(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckCategory(part, Values);
        }
    }

    /// <summary>
    /// part profile check
    /// </summary>
    public class CheckSubcategory : CheckNode
    {
        public const string ID = "subcategory";
        public override string CheckID { get => ID; }
        public CheckSubcategory(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckSubcategory(part, Values, depth);
        }
    }

    /// <summary>
    /// check part field by reflection
    /// </summary>
    public class CheckField : CheckNode
    {
        public const string ID = "field";
        public override string CheckID { get => ID; }
        public CheckField(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.NodeCheck(part, Values);
        }
    }

    /// <summary>
    /// for checks that dont fit well into the confignode mold
    /// </summary>
    public class CheckCustom : CheckNode
    {
        public const string ID = "custom";
        public override string CheckID { get => ID; }
        public CheckCustom(ConfigNode node) : base(node) { }
        public override bool CheckResult(AvailablePart part, int depth = 0)
        {
            return Invert ^ PartType.CheckCustom(part, Values);
        }
    }
}
