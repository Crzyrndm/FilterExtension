using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.Utility
{
    public static class PartType
    {        
        private static bool categoryCheck(AvailablePart part)
        {
            if (part.category != PartCategories.none)
                return false;
            else
                return true;
        }

        internal static bool checkCustom(AvailablePart part, string value)
        {
            bool val;
            switch (value)
            {
                case "adapter":
                    val = isAdapter(part);
                    break;
                case "multicoupler":
                    val = isMultiCoupler(part);
                    break;
                default:
                    Core.Log("incorrect value in custom type check");
                    val = false;
                    break;
            }
            return val;
        }

        internal static bool checkCategory(AvailablePart part, string value)
        {
            foreach (string s in value.Split(','))
            {
                switch (s.Trim())
                {
                    case "Pods":
                        if (part.category == PartCategories.Pods)
                            return true;
                        break;
                    case "Engines":
                        if (part.category == PartCategories.Engine)
                            return true;
                        else if (part.category == PartCategories.Propulsion && PartType.isEngine(part))
                            return true;
                        break;
                    case "Fuel Tanks":
                        if (part.category == PartCategories.FuelTank)
                            return true;
                        else if (part.category == PartCategories.Propulsion && !PartType.isEngine(part))
                            return true;
                        break;
                    case "Command and Control":
                        if (part.category == PartCategories.Control)
                            return true;
                        break;
                    case "Structural":
                        if (part.category == PartCategories.Structural)
                            return true;
                        break;
                    case "Aerodynamics":
                        if (part.category == PartCategories.Aero)
                            return true;
                        break;
                    case "Utility":
                        if (part.category == PartCategories.Utility)
                            return true;
                        break;
                    case "Science":
                        if (part.category == PartCategories.Science)
                            return true;
                        break;
                    case "None":
                        if (part.category == PartCategories.none)
                            return true;
                        break;
                }
            }

            return false;
        }

        internal static bool checkModuleTitle(AvailablePart part, string value, bool contains = true)
        {
            if (part.moduleInfos == null)
                return false;
            if (contains)
            {
                foreach (string s in value.Split(','))
                {
                    if (part.moduleInfos.Any(m => m.moduleName == s.Trim()))
                        return true;
                }
            }
            else
            {
                foreach (AvailablePart.ModuleInfo i in part.moduleInfos)
                {
                    if (!value.Split(',').Contains(i.moduleName))
                        return true;
                }
            }
            return false;
        }

        internal static bool checkModuleName(AvailablePart part, string value, bool contains = true)
        {
            if (part.partPrefab == null || part.partPrefab.Modules == null)
                return false;
            if (contains)
                return value.Split(',').Any(s => part.partPrefab.Modules.Contains(s.Trim()));
            else
            {
                foreach (PartModule module in part.partPrefab.Modules)
                    if (!value.Split(',').Contains(module.ClassName))
                        return true;
                return false;
            }
        }

        internal static bool checkName(AvailablePart part, string value)
        {
            return value.Split(',').Any(s => s.Trim().ToLower() == part.name.ToLower());
        }

        internal static bool checkTitle(AvailablePart part, string value)
        {
            return value.Split(',').Any(s => part.title.ToLower().Contains(s.Trim().ToLower()));
        }

        internal static bool checkResource(AvailablePart part, string value, bool contains = true)
        {
            if (part.partPrefab == null || part.partPrefab.Resources == null)
                return false;

            if (contains)
                return value.Split(',').Any(s => part.partPrefab.Resources.Contains(s.Trim()));
            else
            {
                foreach (PartResource r in part.partPrefab.Resources)
                    if (!value.Split(',').Contains(r.resourceName))
                        return true;
                return false;
            }
        }

        internal static bool checkPropellant(AvailablePart part, string value, bool contains = true)
        {
            List<List<Propellant>> propellants = new List<List<Propellant>>();
            foreach (ModuleEngines e in part.partPrefab.GetModuleEngines())
                propellants.Add(e.propellants);

            if (contains)
            {
                foreach (List<Propellant> Lp in propellants)
                    foreach (Propellant p in Lp)
                        if (value.Split(',').Any(s => s == p.name))
                            return true;
            }
            else
            {
                bool result = true;
                foreach (List<Propellant> Lp in propellants)
                {
                    bool tmp = false;
                    foreach (Propellant p in Lp)
                        tmp |= !value.Split(',').Contains(p.name); // tmp is true if any propellant is not listed
                    result &= tmp;
                }
                return result;
            }
            return false;
        }

        internal static bool checkTech(AvailablePart part, string value)
        {
            return value.Split(',').Any(s => part.TechRequired == s.Trim());
        }

        internal static bool checkManufacturer(AvailablePart part, string value)
        {
            return value.Split(',').Any(s => part.manufacturer == s.Trim());
        }

        internal static bool checkFolder(AvailablePart part, string value)
        {
            string[] values = value.Split(',');
            return checkFolder(part, values);
        }

        internal static bool checkFolder(AvailablePart part, string[] values)
        {
            if (Core.Instance.partPathDict.ContainsKey(part.name))
            {
                string folder = Core.Instance.partPathDict[part.name].Split(new char[] { '\\', '/' })[0];
                return values.Any(s => s.Trim() == folder);
            }
            return false;
        }

        internal static bool checkPath(AvailablePart part, string value)
        {
            string[] values = value.Replace('\\', '/').Split(',');
            return checkPath(part, values);
        }

        internal static bool checkPath(AvailablePart part, string[] values)
        {
            if (Core.Instance.partPathDict.ContainsKey(part.name))
                return values.Any(s => Core.Instance.partPathDict[part.name].StartsWith(s.Trim(), StringComparison.InvariantCultureIgnoreCase));

            return false;
        }

        public static bool checkPartSize(AvailablePart part, string value, bool contains, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null || part.partPrefab.attachNodes == null)
                return false;

            string[] values = value.Split(',');
            if (equality == ConfigNodes.Check.Equality.Equals)
            {
                foreach (AttachNode node in part.partPrefab.attachNodes)
                {
                    if (contains)
                        if (values.Contains(node.size.ToString()))
                            return true;
                        else
                            if (!values.Contains(node.size.ToString()))
                                return true;
                }
            }
            else if (equality == ConfigNodes.Check.Equality.GreaterThan)
            {
                int i;
                if (int.TryParse(values[0], out i))
                {
                    foreach (AttachNode node in part.partPrefab.attachNodes)
                    {
                        if (node.size > i)
                            return true;
                    }
                }
            }
            else
            {
                int i;
                if (int.TryParse(values[0], out i))
                {
                    foreach (AttachNode node in part.partPrefab.attachNodes)
                    {
                        if (node.size < i)
                            return true;
                    }
                }
            }
            return false;
        }

        public static bool checkCrewCapacity(AvailablePart part, string value, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null)
                return false;

            if (equality == ConfigNodes.Check.Equality.Equals)
            {
                foreach (string s in value.Split(','))
                {
                    int i;
                    if (int.TryParse(s.Trim(), out i))
                    {
                        if (Math.Max(i, 0) == part.partPrefab.CrewCapacity)
                            return true;
                    }
                }
            }
            else if (equality == ConfigNodes.Check.Equality.GreaterThan)
            {
                int i;
                if (int.TryParse(value, out i))
                {
                    if (part.partPrefab.CrewCapacity > i)
                        return true;
                }
            }
            else
            {
                int i;
                if (int.TryParse(value, out i))
                {
                    if (part.partPrefab.CrewCapacity < i)
                        return true;
                }
            }
            return false;
        }

        public static bool checkMass(AvailablePart part, string value, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null)
                return false;

            int i;
            if (int.TryParse(value, out i))
            {
                if (equality == ConfigNodes.Check.Equality.Equals && part.partPrefab.mass == i)
                    return true;
                if (equality == ConfigNodes.Check.Equality.GreaterThan && part.partPrefab.mass > i)
                    return true;
                if (equality == ConfigNodes.Check.Equality.LessThan && part.partPrefab.mass < i)
                    return true;
            }
            return false;
        }

        public static bool checkCost(AvailablePart part, string value, ConfigNodes.Check.Equality equality)
        {
            int i;
            if (int.TryParse(value, out i))
            {
                if (equality == ConfigNodes.Check.Equality.Equals && part.cost == i)
                    return true;
                if (equality == ConfigNodes.Check.Equality.GreaterThan && part.cost > i)
                    return true;
                if (equality == ConfigNodes.Check.Equality.LessThan && part.cost < i)
                    return true;
            }
            return false;
        }

        public static bool checkCrashTolerance(AvailablePart part, string value, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null)
                return false;

            int i;
            if (int.TryParse(value, out i))
            {
                if (equality == ConfigNodes.Check.Equality.Equals && part.partPrefab.crashTolerance == i)
                    return true;
                if (equality == ConfigNodes.Check.Equality.GreaterThan && part.partPrefab.crashTolerance > i)
                    return true;
                if (equality == ConfigNodes.Check.Equality.LessThan && part.partPrefab.crashTolerance < i)
                    return true;
            }
            return false;
        }

        public static bool checkTemperature(AvailablePart part, string value, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null)
                return false;

            int i;
            if (int.TryParse(value, out i))
            {
                if (equality == ConfigNodes.Check.Equality.Equals && part.partPrefab.maxTemp == i)
                    return true;
                if (equality == ConfigNodes.Check.Equality.GreaterThan && part.partPrefab.maxTemp > i)
                    return true;
                if (equality == ConfigNodes.Check.Equality.LessThan && part.partPrefab.maxTemp < i)
                    return true;
            }
            return false;
        }

        public static bool checkBulkHeadProfiles(AvailablePart part, string value, bool contains)
        {
            if (part.bulkheadProfiles == null)
            {
                if (value.Trim() == "srf")
                    return true;
                return false;
            }

            string[] values = value.Split(',');
            foreach (string s in part.bulkheadProfiles.Split(','))
            {
                if (values.Any(v => v.Trim() == s.Trim()))
                    return true;
            }
            return false;
        }

        public static bool isCommand(AvailablePart part)
        {
            if (isMannedPod(part))
                return true;
            if (isDrone(part))
                return true;
            if (part.partPrefab.Modules.OfType<KerbalSeat>().Any())
                return true;
            return false;
        }
        
        public static bool isEngine(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleEngines>().Any())
                return true;
            if (part.partPrefab.Modules.OfType<ModuleEnginesFX>().Any())
                return true;
            if (part.partPrefab.Modules.OfType<MultiModeEngine>().Any())
                return true;
            return false;
        }

        public static bool isMannedPod(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleCommand>().Any() && part.partPrefab.CrewCapacity > 0)
                return true;
            return false;
        }

        public static bool isDrone(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleCommand>().Any() && part.partPrefab.CrewCapacity == 0)
                return true;
            return false;
        }

        public static bool isCabin(AvailablePart part)
        {
            if (!part.partPrefab.Modules.OfType<ModuleCommand>().Any() && part.partPrefab.CrewCapacity > 0)
                return true;
            return false;
        }

        public static bool isMultiCoupler(AvailablePart part)
        {
            if (part.partPrefab == null || part.partPrefab.attachNodes == null)
                return false;

            if (part.partPrefab.attachNodes.Count <= 2 || part.title.Contains("Cargo Bay"))
                return false;
            float pos = part.partPrefab.attachNodes.Last().position.y;
            if (part.partPrefab.attachNodes.FindAll(n => n.position.y == pos).Count > 1 && part.partPrefab.attachNodes.FindAll(n => n.position.y == pos).Count < part.partPrefab.attachNodes.Count)
                return true;
            
            return false;
        }

        public static bool isAdapter(AvailablePart part)
        {
            if (part.partPrefab == null || part.partPrefab.attachNodes == null)
                return false;

            if (isCommand(part))
                return false;
            if (part.partPrefab.attachNodes.Count != 2)
                return false;
            if (part.partPrefab.attachNodes[0].size != part.partPrefab.attachNodes[1].size)
                return true;
            return false;
        }

        public static bool isWing(AvailablePart part)
        {
            if (part.partPrefab is Winglet)
                return true;
            if (part.partPrefab.Modules.Contains("FARWingAerodynamicModel"))
                return true;
            
            return false;
        }

        public static List<T> GetModules<T>(this Part part) where T : PartModule
        {
            return part.Modules.OfType<T>().ToList();
        }

        public static T GetModule<T>(this Part part) where T : PartModule
        {
            return part.Modules.OfType<T>().FirstOrDefault();
        }

        public static List<ModuleEngines> GetModuleEngines(this Part part)
        {
            return part.GetModules<ModuleEngines>();
        }

        [Obsolete("FX now inherits from moduleEngines, use GetModuleEngines for all engine types")]
        public static List<ModuleEnginesFX> GetModuleEnginesFx(this Part part)
        {
            return part.GetModules<ModuleEnginesFX>();
        }
    }
}
