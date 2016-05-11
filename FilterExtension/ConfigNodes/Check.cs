using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using Utility;

    public class Check : IEquatable<Check>, ICloneable
    {
        public enum CheckType
        {
            moduleTitle,
            moduleName,
            partName,
            partTitle,
            resource,
            propellant,
            tech,
            manufacturer,
            folder,
            path,
            category,
            size,
            crew,
            custom,
            mass,
            cost,
            crashTolerance,
            maxTemp,
            profile,
            check,
            subcategory,
            tag
        }

        public enum Equality
        {
            Equals, // default
            LessThan,
            GreaterThan
        }

        public class CheckParameters
        {
            public CheckType typeEnum { get; private set; }
            public string typeString { get; private set; }
            public bool usesContains { get; private set; }
            public bool usesEquality { get; private set; }
            public bool usesExact { get; private set; }

            public CheckParameters(CheckType Type, string TypeStr, bool Contains = false, bool Equality = false, bool Exact = false)
            {
                typeEnum = Type;
                typeString = TypeStr;
                usesContains = Contains;
                usesEquality = Equality;
                usesExact = Exact;
            }
        }

        public static readonly Dictionary<string, CheckParameters> checkParams = new Dictionary<string, CheckParameters>(StringComparer.OrdinalIgnoreCase)
            {
                { "name",           new CheckParameters(CheckType.partName, "name") },
                { "title",          new CheckParameters(CheckType.partTitle, "title") },
                { "moduleName",     new CheckParameters(CheckType.moduleName, "moduleName", Contains:true, Exact:true) },
                { "moduleTitle",    new CheckParameters(CheckType.moduleTitle, "moduleTitle", Contains:true, Exact:true) },
                { "resource",       new CheckParameters(CheckType.resource, "resource", Contains:true, Exact:true) },
                { "propellant",     new CheckParameters(CheckType.propellant, "propellant", Contains:true, Exact:true) },
                { "tech",           new CheckParameters(CheckType.tech, "tech") },
                { "manufacturer",   new CheckParameters(CheckType.manufacturer, "manufacturer") },
                { "folder",         new CheckParameters(CheckType.folder, "folder") },
                { "path",           new CheckParameters(CheckType.path, "path") },
                { "category",       new CheckParameters(CheckType.category, "category") },
                { "size",           new CheckParameters(CheckType.size, "size", Contains:true, Exact:true, Equality:true) },
                { "crew",           new CheckParameters(CheckType.crew, "crew", Equality:true) },
                { "custom",         new CheckParameters(CheckType.custom, "custom") },
                { "mass",           new CheckParameters(CheckType.mass, "mass", Equality:true) },
                { "cost",           new CheckParameters(CheckType.cost, "cost", Equality:true) },
                { "crash",          new CheckParameters(CheckType.crashTolerance, "crash", Equality:true) },
                { "maxTemp",        new CheckParameters(CheckType.maxTemp, "maxTemp", Equality:true) },
                { "profile",        new CheckParameters(CheckType.profile, "profile", Contains:true, Exact:true) },
                { "check",          new CheckParameters(CheckType.check, "check") },
                { "subcategory",    new CheckParameters(CheckType.subcategory, "subcategory") },
                { "tag",            new CheckParameters(CheckType.tag, "tag", Contains:true, Exact:true) }
            };

        public CheckParameters type { get; set; }
        public string[] values { get; set; }
        public bool invert { get; set; }
        public bool contains { get; set; }
        public bool exact { get; set; }
        public Equality equality { get; set; }
        public List<Check> checks { get; set; } 

        public Check(ConfigNode node)
        {
            string tmpStr = string.Empty;
            bool tmpBool = false;
            checks = new List<Check>();

            if (node.TryGetValue("value", ref tmpStr) && !string.IsNullOrEmpty(tmpStr))
            {
                values = tmpStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < values.Length; ++i)
                    values[i] = values[i].Trim();
            }

            if (node.TryGetValue("invert", ref tmpBool))
                invert = tmpBool;
            else
                invert = false;

            type = getCheckType(node.GetValue("type"));
            if (type.typeEnum == CheckType.check)
            {
                foreach (ConfigNode subNode in node.GetNodes("CHECK"))
                    checks.Add(new Check(subNode));
            }

            if (type.usesContains && node.TryGetValue("contains", ref tmpBool))
                contains = tmpBool;
            else
                contains = true;

            if (type.usesExact && node.TryGetValue("exact", ref tmpBool))
                exact = tmpBool;
            else
                exact = false;

            if (type.usesEquality && node.TryGetValue("equality", ref tmpStr))
            {
                try
                {
                    equality = (Equality)Enum.Parse(typeof(Equality), tmpStr, true);
                }
                catch
                {
                    equality = Equality.Equals;
                }
            }
            else
                equality = Equality.Equals;
        }

        public Check(Check c)
        {
            type = c.type;
            invert = c.invert;
            contains = c.contains;
            equality = c.equality;
            exact = c.exact;

            if (c.values != null)
                values = (string[])c.values.Clone();
            checks = new List<Check>();
            if (c.checks != null)
            {
                checks = new List<Check>();
                for (int i = 0; i < c.checks.Count; ++i)
                    checks.Add(new Check(c.checks[i]));
            }
        }

        public Check(string Type, string Value, bool Invert = false, bool Contains = true, Equality Compare = Equality.Equals, bool Exact = false)
        {
            type = getCheckType(Type);
            values = Value.Split(',');
            for (int i = 0; i < values.Length; ++i)
                values[i] = values[i].Trim();

            invert = Invert;
            contains = Contains;
            exact = Exact;
            equality = Compare;
            checks = new List<Check>();
        }

        public bool checkResult(AvailablePart part, int depth = 0)
        {
            switch (type.typeEnum)
            {
                case CheckType.moduleTitle: // check by module title
                    return invert ^ PartType.checkModuleTitle(part, values, contains);
                case CheckType.moduleName:
                    return invert ^ PartType.checkModuleName(part, values, contains);
                case CheckType.partName: // check by part name (cfg name)
                    return invert ^ PartType.checkName(part, values);
                case CheckType.partTitle: // check by part title (in game name)
                    return invert ^ PartType.checkTitle(part, values);
                case CheckType.resource: // check for a resource
                    return invert ^ PartType.checkResource(part, values, contains);
                case CheckType.propellant: // check for engine propellant
                    return invert ^ PartType.checkPropellant(part, values, contains, exact);
                case CheckType.tech: // check by tech
                    return invert ^ PartType.checkTech(part, values);
                case CheckType.manufacturer: // check by manufacturer
                    return invert ^ PartType.checkManufacturer(part, values);
                case CheckType.folder: // check by mod root folder
                    return invert ^ PartType.checkFolder(part, values);
                case CheckType.path: // check by part folder location
                    return invert ^ PartType.checkPath(part, values);
                case CheckType.category:
                    return invert ^ PartType.checkCategory(part, values);
                case CheckType.size: // check by largest stack node size
                    return invert ^ PartType.checkPartSize(part, values, contains, equality);
                case CheckType.crew:
                    return invert ^ PartType.checkCrewCapacity(part, values, equality);
                case CheckType.custom: // for when things get tricky
                    return invert ^ PartType.checkCustom(part, values);
                case CheckType.mass:
                    return invert ^ PartType.checkMass(part, values, equality);
                case CheckType.cost:
                    return invert ^ PartType.checkCost(part, values, equality);
                case CheckType.crashTolerance:
                    return invert ^ PartType.checkCrashTolerance(part, values, equality);
                case CheckType.maxTemp:
                    return invert ^ PartType.checkTemperature(part, values, equality);
                case CheckType.profile:
                    return invert ^ PartType.checkBulkHeadProfiles(part, values, contains);
                case CheckType.subcategory:
                    return invert ^ PartType.checkSubcategory(part, values, depth);
                case CheckType.tag:
                    return invert ^ PartType.checkTags(part, values, contains);
                case CheckType.check:
                    for (int i = 0; i < checks.Count; i++)
                    {
                        if (invert == checks[i].checkResult(part))
                            return false;
                    }
                    return true;
                default:
                    Core.Log("invalid Check type specified", Core.LogLevel.Warn);
                    return invert ^ false;
            }
        }

        /// <summary>
        /// set type enum from type string
        /// NOTE: Needs the enum => string conversion added to function as subcategories are created from confignodes
        /// </summary>
        /// <param name="type">type string</param>
        /// <returns>type enum</returns>
        public static CheckParameters getCheckType(string type)
        {
            CheckParameters tmpParams;
            if (!checkParams.TryGetValue(type, out tmpParams))
                tmpParams = checkParams["category"];
            return tmpParams;
        }

        public bool isEmpty()
        {
            return !checks.Any() && (values == null || values.Length == 0);
        }

        public ConfigNode toConfigNode()
        {
            ConfigNode node = new ConfigNode("CHECK");
            node.AddValue("type", type.typeString);

            if (values != null)
                node.AddValue("value", string.Join(",", values));
            node.AddValue("invert", invert.ToString());

            if (type.usesContains)
                node.AddValue("contains", contains.ToString());
            if (type.usesEquality)
                node.AddValue("equality", equality.ToString());

            foreach (Check c in this.checks)
                node.AddNode(c.toConfigNode());

            return node;
        }

        public object Clone()
        {
            return new Check(this);
        }

        public bool Equals(Check c2)
        {
            if (c2 == null)
                return false;
            if (type == c2.type && values == c2.values && invert == c2.invert && contains == c2.contains && checks == c2.checks && equality == c2.equality)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() ^ values.GetHashCode() ^ invert.GetHashCode() ^ contains.GetHashCode() ^ equality.GetHashCode() ^ (int)checks.Average(c => c.GetHashCode());
        }
    }
}
