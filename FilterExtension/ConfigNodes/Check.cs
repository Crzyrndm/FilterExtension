using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using Utility;

    public class Check : ICloneable
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

            public CheckParameters(CheckType Type, string TypeStr, bool Contains = false, bool Equality = false)
            {
                typeEnum = Type;
                typeString = TypeStr;
                usesContains = Contains;
                usesEquality = Equality;
            }
        }

        public static readonly Dictionary<string, CheckParameters> checkParams = new Dictionary<string, CheckParameters>(StringComparer.OrdinalIgnoreCase)
            {
                { "name",           new CheckParameters(CheckType.partName, "name") },
                { "title",          new CheckParameters(CheckType.partTitle, "title") },
                { "moduleName",     new CheckParameters(CheckType.moduleName, "moduleName", true) },
                { "moduleTitle",    new CheckParameters(CheckType.moduleTitle, "moduleTitle", true) },
                { "resource",       new CheckParameters(CheckType.resource, "resource", true) },
                { "propellant",     new CheckParameters(CheckType.propellant, "propellant", true) },
                { "tech",           new CheckParameters(CheckType.tech, "tech") },
                { "manufacturer",   new CheckParameters(CheckType.manufacturer, "manufacturer") },
                { "folder",         new CheckParameters(CheckType.folder, "folder") },
                { "path",           new CheckParameters(CheckType.path, "path") },
                { "category",       new CheckParameters(CheckType.category, "category") },
                { "size",           new CheckParameters(CheckType.size, "size", true, true) },
                { "crew",           new CheckParameters(CheckType.crew, "crew", false, true) },
                { "custom",         new CheckParameters(CheckType.custom, "custom") },
                { "mass",           new CheckParameters(CheckType.mass, "mass", false, true) },
                { "cost",           new CheckParameters(CheckType.cost, "cost", false, true) },
                { "crash",          new CheckParameters(CheckType.crashTolerance, "crash", false, true) },
                { "maxTemp",        new CheckParameters(CheckType.maxTemp, "maxTemp", false, true) },
                { "profile",        new CheckParameters(CheckType.profile, "profile", true) },
                { "check",          new CheckParameters(CheckType.check, "check") },
                { "subcategory",    new CheckParameters(CheckType.subcategory, "subcategory") },
                { "tag",            new CheckParameters(CheckType.tag, "tag", true) }
            };

        public CheckParameters type { get; set; }
        public string[] values { get; set; }
        public bool invert { get; set; }
        public bool contains { get; set; }
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
            if (c.values != null)
                values = (string[])c.values.Clone();
            invert = c.invert;
            contains = c.contains;

            if (c.checks != null)
            {
                checks = new List<Check>(c.checks.Count);
                for (int i = 0; i < c.checks.Count; ++i)
                    checks.Add(new Check(c.checks[i]));
            }
        }

        public Check(string Type, string Value, bool Invert = false, bool Contains = true, Equality Compare = Equality.Equals)
        {
            type = getCheckType(Type);
            values = Value.Split(',');
            for (int i = 0; i < values.Length; ++i)
                values[i] = values[i].Trim();

            invert = Invert;
            contains = Contains;
            equality = Compare;
            checks = new List<Check>();
        }

        public bool checkPart(AvailablePart part, int depth = 0)
        {
            bool result = true;
            switch (type.typeEnum)
            {
                case CheckType.moduleTitle: // check by module title
                    result = PartType.checkModuleTitle(part, values, contains);
                    break;
                case CheckType.moduleName:
                    result = PartType.checkModuleName(part, values, contains);
                    break;
                case CheckType.partName: // check by part name (cfg name)
                    result = PartType.checkName(part, values);
                    break;
                case CheckType.partTitle: // check by part title (in game name)
                    result = PartType.checkTitle(part, values);
                    break;
                case CheckType.resource: // check for a resource
                    result = PartType.checkResource(part, values, contains);
                    break;
                case CheckType.propellant: // check for engine propellant
                    result = PartType.checkPropellant(part, values, contains);
                    break;
                case CheckType.tech: // check by tech
                    result = PartType.checkTech(part, values);
                    break;
                case CheckType.manufacturer: // check by manufacturer
                    result = PartType.checkManufacturer(part, values);
                    break;
                case CheckType.folder: // check by mod root folder
                    result = PartType.checkFolder(part, values);
                    break;
                case CheckType.path: // check by part folder location
                    result = PartType.checkPath(part, values);
                    break;
                case CheckType.category:
                    result = PartType.checkCategory(part, values);
                    break;
                case CheckType.size: // check by largest stack node size
                    result = PartType.checkPartSize(part, values, contains, equality);
                    break;
                case CheckType.crew:
                    result = PartType.checkCrewCapacity(part, values, equality);
                    break;
                case CheckType.custom: // for when things get tricky
                    result = PartType.checkCustom(part, values);
                    break;
                case CheckType.mass:
                    result = PartType.checkMass(part, values, equality);
                    break;
                case CheckType.cost:
                    result = PartType.checkCost(part, values, equality);
                    break;
                case CheckType.crashTolerance:
                    result = PartType.checkCrashTolerance(part, values, equality);
                    break;
                case CheckType.maxTemp:
                    result = PartType.checkTemperature(part, values, equality);
                    break;
                case CheckType.profile:
                    result = PartType.checkBulkHeadProfiles(part, values, contains);
                    break;
                case CheckType.check:
                    for (int i = 0; i < checks.Count; i++ )
                    {
                        if (!checks[i].checkPart(part))
                        {
                            result = false;
                            break;
                        }
                    }
                    break;
                case CheckType.subcategory:
                    result = PartType.checkSubcategory(part, values, depth);
                    break;
                case CheckType.tag:
                    result = PartType.checkTags(part, values, contains);
                    break;
                default:
                    Core.Log("invalid Check type specified");
                    result = false;
                    break;
            }
            
            if (invert)
                return !result;
            return result;
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
            if (this.type == c2.type && this.values == c2.values && this.invert == c2.invert && this.contains == c2.contains && this.checks == c2.checks && this.equality == c2.equality)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            int checks = this.checks.Any() ? this.checks.GetHashCode() : 1;
            return this.type.GetHashCode() * this.values.GetHashCode() * this.invert.GetHashCode() * this.contains.GetHashCode() * this.equality.GetHashCode() * checks;
        }
    }
}
