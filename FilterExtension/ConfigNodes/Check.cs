using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using Utility;

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
        subcategory
    }

    public class Check
    {
        public enum Equality
        {
            Equals, // default
            LessThan,
            GreaterThan
        }
        public CheckType type { get; set; }
        public string[] value { get; set; }
        public bool invert { get; set; }
        public bool contains { get; set; }
        public Equality equality { get; set; }
        public List<Check> checks { get; set; } 

        public Check(ConfigNode node)
        {
            type = getType(node.GetValue("type"));

            string tmpVal = node.GetValue("value");
            if (tmpVal != null)
            {
                value = tmpVal.Split(',');
                for (int i = 0; i < this.value.Length; ++i)
                    value[i] = value[i].Trim();
            }

            bool tmp;
            bool.TryParse(node.GetValue("invert"), out tmp);
            invert = tmp;

            if (bool.TryParse(node.GetValue("contains"), out tmp))
                contains = tmp;
            else
                contains = true;

            checks = new List<Check>();
            if (type == CheckType.check)
            {
                foreach (ConfigNode subNode in node.GetNodes("CHECK"))
                {
                    checks.Add(new Check(subNode));
                }
            }

            switch (node.GetValue("equality"))
            {
                case "LessThan":
                    equality = Equality.LessThan;
                    break;
                case "GreaterThan":
                    equality = Equality.GreaterThan;
                    break;
                default:
                    equality = Equality.Equals;
                    break;
            }
        }

        public Check(Check c)
        {
            type = c.type;
            value = (string[])c.value.Clone();
            invert = c.invert;
            contains = c.contains;

            checks = new List<Check>();
            for (int i = 0; i < c.checks.Count; i++)
                checks.Add(new Check(c.checks[i]));
        }

        public Check(string type, string value, bool invert = false, bool contains = true, Equality compare = Equality.Equals)
        {
            this.type = getType(type);
            this.value = value.Split(',');
            for (int i = 0; i < this.value.Length; ++i)
                this.value[i] = this.value[i].Trim();

            this.invert = invert;
            this.contains = contains;
            equality = compare;
            this.checks = new List<Check>();
        }

        public ConfigNode toConfigNode()
        {
            ConfigNode node = new ConfigNode("CHECK");
            node.AddValue("type", getTypeString(type));

            if (value != null)
                node.AddValue("value", string.Join(",", value));
            if (invert)
                node.AddValue("invert", this.invert.ToString());
            if (!contains && checkUsesContains())
                node.AddValue("contains", this.contains.ToString());
            if (equality != Equality.Equals && checkUsesEquality())
                node.AddValue("equality", this.equality.ToString());

            foreach (Check c in this.checks)
                node.AddNode(c.toConfigNode());

            return node;
        }

        public bool checkPart(AvailablePart part, int depth = 0)
        {
            bool result = true;
            switch (type)
            {
                case CheckType.moduleTitle: // check by module title
                    result = PartType.checkModuleTitle(part, value, contains);
                    break;
                case CheckType.moduleName:
                    result = PartType.checkModuleName(part, value, contains);
                    break;
                case CheckType.partName: // check by part name (cfg name)
                    result = PartType.checkName(part, value);
                    break;
                case CheckType.partTitle: // check by part title (in game name)
                    result = PartType.checkTitle(part, value);
                    break;
                case CheckType.resource: // check for a resource
                    result = PartType.checkResource(part, value, contains);
                    break;
                case CheckType.propellant: // check for engine propellant
                    result = PartType.checkPropellant(part, value, contains);
                    break;
                case CheckType.tech: // check by tech
                    result = PartType.checkTech(part, value);
                    break;
                case CheckType.manufacturer: // check by manufacturer
                    result = PartType.checkManufacturer(part, value);
                    break;
                case CheckType.folder: // check by mod root folder
                    result = PartType.checkFolder(part, value);
                    break;
                case CheckType.path: // check by part folder location
                    result = PartType.checkPath(part, value);
                    break;
                case CheckType.category:
                    result = PartType.checkCategory(part, value);
                    break;
                case CheckType.size: // check by largest stack node size
                    result = PartType.checkPartSize(part, value, contains, equality);
                    break;
                case CheckType.crew:
                    result = PartType.checkCrewCapacity(part, value, equality);
                    break;
                case CheckType.custom: // for when things get tricky
                    result = Editor.instance.ready ? PartType.checkCustom(part, value) : !invert;
                    break;
                case CheckType.mass:
                    result = PartType.checkMass(part, value, equality);
                    break;
                case CheckType.cost:
                    result = PartType.checkCost(part, value, equality);
                    break;
                case CheckType.crashTolerance:
                    result = PartType.checkCrashTolerance(part, value, equality);
                    break;
                case CheckType.maxTemp:
                    result = PartType.checkTemperature(part, value, equality);
                    break;
                case CheckType.profile:
                    result = PartType.checkBulkHeadProfiles(part, value, contains);
                    break;
                case CheckType.check:
                    for (int i = 0; i < checks.Count; i++ )
                    {
                        if (!checks[i].checkPart(part))
                            result = false;
                    }
                    break;
                case CheckType.subcategory:
                    result = PartType.checkSubcategory(part, value, depth);
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
        public static CheckType getType(string type)
        {
            switch(type)
            {
                case "name":
                    return CheckType.partName;
                case "title":
                    return CheckType.partTitle;
                case "moduleName":
                    return CheckType.moduleName;
                case "moduleTitle":
                    return CheckType.moduleTitle;
                case "resource":
                    return CheckType.resource;
                case "propellant":
                    return CheckType.propellant;
                case "tech":
                    return CheckType.tech;
                case "manufacturer":
                    return CheckType.manufacturer;
                case "folder":
                    return CheckType.folder;
                case "path":
                    return CheckType.path;
                case "category":
                    return CheckType.category;
                case "size":
                    return CheckType.size;
                case "crew":
                    return CheckType.crew;
                case "custom":
                    return CheckType.custom;
                case "mass":
                    return CheckType.mass;
                case "cost":
                    return CheckType.cost;
                case "crash":
                    return CheckType.crashTolerance;
                case "maxTemp":
                    return CheckType.maxTemp;
                case "profile":
                    return CheckType.profile;
                case "check":
                    return CheckType.check;
                case "subcategory":
                    return CheckType.subcategory;
                default:
                    return CheckType.category;
            }
        }

        /// <summary>
        /// set type string from type enum
        /// NOTE: Needs the string => enum conversion added to function
        /// </summary>
        /// <param name="type">type enum</param>
        /// <returns>type string</returns>
        public static string getTypeString(CheckType type)
        {
            switch (type)
            {
                case CheckType.partName:
                    return "name";
                case CheckType.partTitle:
                    return "title";
                case CheckType.moduleName:
                    return "moduleName";
                case CheckType.moduleTitle:
                    return "moduleTitle";
                case CheckType.resource:
                    return "resource";
                case CheckType.propellant:
                    return "propellant";
                case CheckType.tech:
                    return "tech";
                case CheckType.manufacturer:
                    return "manufacturer";
                case CheckType.folder:
                    return "folder";
                case CheckType.path:
                    return "path";
                case CheckType.category:
                    return "category";
                case CheckType.size:
                    return "size";
                case CheckType.crew:
                    return "crew";
                case CheckType.custom:
                    return "custom";
                case CheckType.mass:
                    return "mass";
                case CheckType.cost:
                    return "cost";
                case CheckType.crashTolerance:
                    return "crash";
                case CheckType.maxTemp:
                    return "maxTemp";
                case CheckType.profile:
                    return "profile";
                case CheckType.check:
                    return "check";
                case CheckType.subcategory:
                    return "subcategory";
                default:
                    return "category";
            }
        }

        public bool checkUsesContains()
        {
            switch (type)
            {
                case CheckType.moduleTitle:
                case CheckType.moduleName:
                case CheckType.resource:
                case CheckType.propellant:
                case CheckType.size:
                case CheckType.profile:
                    return true;
                default:
                    return false;
            }
        }

        public bool checkUsesEquality()
        {
            switch (type)
            {
                case CheckType.size:
                case CheckType.crew:
                case CheckType.mass:
                case CheckType.cost:
                case CheckType.crashTolerance:
                case CheckType.maxTemp:
                    return true;
                default:
                    return false;
            }
        }

        public bool Equals(Check c2)
        {
            if (c2 == null)
                return false;
            if (this.type == c2.type && this.value == c2.value && this.invert == c2.invert && this.contains == c2.contains && this.checks == c2.checks && this.equality == c2.equality)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            int checks = this.checks.Any() ? this.checks.GetHashCode() : 1;
            return this.type.GetHashCode() * this.value.GetHashCode() * this.invert.GetHashCode() * this.contains.GetHashCode() * this.equality.GetHashCode() * checks;
        }
    }
}
