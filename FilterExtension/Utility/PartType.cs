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
                case "isEngine":
                    val = isEngine(part);
                    break;
                case "isCommand":
                    val = isCommand(part);
                    break;
                case "adapter":
                    val = isAdapter(part);
                    break;
                case "multicoupler":
                    val = isMultiCoupler(part);
                    break;
                case "crewCabin":
                    val = isCabin(part);
                    break;
                default:
                    val = false;
                    break;
            }
            return val;
        }

        internal static bool checkCategory(AvailablePart part, string value)
        {
            foreach (string s in value.Split())
            {
                switch (value.Trim())
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
                }
            }

            return false;
        }

        internal static bool checkModuleTitle(AvailablePart part, string value, bool contains = true)
        {
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
            if (contains)
                return value.Split(',').Any(s => part.partPrefab.Modules.Contains(s.Trim()));
            else
            {
                foreach (PartModule module in part.partPrefab.Modules)
                {
                    if (!value.Split(',').Contains(module.ClassName))
                        return true;
                }
                return false;
            }
        }

        internal static bool checkName(AvailablePart part, string value)
        {
            return value.Split(',').Any(s => s.Trim() == part.name);
        }

        internal static bool checkTitle(AvailablePart part, string value)
        {
            return value.Split(',').Any(s => part.title.Contains(s.Trim()));
        }

        internal static bool checkResource(AvailablePart part, string value, bool contains = true)
        {
            if (contains)
                return value.Split(',').Any(s => part.partPrefab.Resources.Contains(s.Trim()));
            else
            {
                foreach (PartResource r in part.partPrefab.Resources)
                {
                    if (!value.Split(',').Contains(r.resourceName))
                        return true;
                }
                return false;
            }
        }

        internal static bool checkPropellant(AvailablePart part, string value, bool contains = true)
        {
            List<List<Propellant>> propellants = new List<List<Propellant>>();
            
            foreach (ModuleEngines e in part.partPrefab.GetModuleEngines())
                propellants.Add(e.propellants);
            foreach (ModuleEnginesFX ex in part.partPrefab.GetModuleEnginesFx())
                propellants.Add(ex.propellants);

            foreach (List<Propellant> Lp in propellants)
            {
                if (contains)
                {
                    foreach (string s in value.Split(','))
                    {
                        if (Lp.Any(p => p.name == s.Trim()))
                            return true;
                    }
                }
                else
                {
                    foreach (Propellant p in Lp)
                    {
                        Core.Log(p.name);
                        if (!value.Split(',').Contains(p.name))
                            return true;
                    }
                }
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
            if (Core.partFolderDict.ContainsKey(part.name))
            {
                foreach (string s in values)
                {
                    if (Core.partFolderDict[part.name] == s.Trim())
                        return true;
                }
            }

            return false;
        }

        public static bool checkPartSize(AvailablePart part, string value, bool contains = true)
        {
            foreach (AttachNode node in part.partPrefab.attachNodes)
            {
                if (contains)
                {
                    if (value.Split(',').Any(p => p.Trim() == node.size.ToString()))
                        return true;
                }
                else
                {
                    if (!value.Split(',').Contains(node.size.ToString()))
                        return true;
                }
            }
            return false;
        }

        public static bool checkCrewCapacity(AvailablePart part, string value)
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

        public static bool isFuselage(AvailablePart part)
        {
            if (!(part.partPrefab.Modules.Count == 0 && part.partPrefab.Resources.Count == 0 && part.partPrefab.attachNodes.Count == 2 && part.category.ToString() != "Aero"))
                return false;
            if (part.partPrefab.attachNodes[0].size == part.partPrefab.attachNodes[1].size)
                return true;
            return false;
        }

        public static bool isMultiCoupler(AvailablePart part)
        {
            if (part.partPrefab.attachNodes.Count > 2)
                return true;
            return false;
        }

        public static bool isAdapter(AvailablePart part)
        {
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
            if (part.partPrefab.GetComponent<Winglet>() != null)
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

        public static List<ModuleEnginesFX> GetModuleEnginesFx(this Part part)
        {
            return part.GetModules<ModuleEnginesFX>();
        }
    }
}
