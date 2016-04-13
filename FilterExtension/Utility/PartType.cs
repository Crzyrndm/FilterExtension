using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.Utility
{
    public static class PartType
    {
        /// <summary>
        /// check the part against another subcategory. Hard limited to a depth of 10
        /// </summary>
        /// <param name="part"></param>
        /// <param name="value"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static bool checkSubcategory(AvailablePart part, string[] value, int depth)
        {
            if (depth > 10)
                return false;
            foreach (string s in value)
            {
                FilterExtensions.ConfigNodes.customSubCategory subcategory;
                if (Core.Instance.subCategoriesDict.TryGetValue(s, out subcategory) && subcategory.checkFilters(part, depth + 1))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// steamlined/combined checks on parts, or checks that don't need extra options
        /// </summary>
        public static bool checkCustom(AvailablePart part, string[] value)
        {
            bool testVal = false;
            foreach (string s in value)
            {
                switch (s)
                {
                    case "adapter":
                        testVal = isAdapter(part);
                        break;
                    case "multicoupler":
                        testVal = isMultiCoupler(part);
                        break;
                    case "purchased":
                        testVal = !Editor.instance.ready || ResearchAndDevelopment.PartModelPurchased(part);
                        break;
                }
                if (testVal)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// checks the stock part category
        /// </summary>
        #warning function needs a rewrite. checking against the part multiple times is not intuitive
        public static bool checkCategory(AvailablePart part, string[] value)
        {
            foreach (string s in value)
            {
                switch (s)
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
        
        /// <summary>
        /// check the user visible names of each part module against a string list
        /// </summary>
        public static bool checkModuleTitle(AvailablePart part, string[] value, bool contains = true)
        {
            if (part.moduleInfos == null)
                return false;
            if (contains)
                return part.moduleInfos.Any(m => value.Contains(m.moduleName));
            else
                return part.moduleInfos.Any(m => !value.Contains(m.moduleName));
        }
        
        /// <summary>
        /// check the part module type against a string list
        /// </summary>
        public static bool checkModuleName(AvailablePart part, string[] value, bool contains = true)
        {
            if (part.partPrefab == null || part.partPrefab.Modules == null)
                return false;
            if (contains)
                return value.Any(s => checkModuleNameType(part, s) || part.partPrefab.Modules.Contains(s));
            else
                return value.Any(s => !checkModuleNameType(part, s) && !part.partPrefab.Modules.Contains(s));
        }
        
        /// <summary>
        /// provides a typed check for stock modules which then allows for inheritance to work
        /// </summary>
        public static bool checkModuleNameType(AvailablePart part, string value)
        {
            switch (value)
            {
                case "ModuleAblator":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAblator>() != null;
                case "ModuleActiveRadiator":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleActiveRadiator>() != null;
                case "ModuleAdvancedLandingGear":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAdvancedLandingGear>() != null;
                case "ModuleAerodynamicLift":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAerodynamicLift>() != null;
                case "ModuleAeroSurface":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAeroSurface>() != null;
                case "ModuleAlternator":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAlternator>() != null;
                case "ModuleAnalysisResource":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAnalysisResource>() != null;
                case "ModuleAnchoredDecoupler":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAnchoredDecoupler>() != null;
                case "ModuleAnimateGeneric":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAnimateGeneric>() != null;
                case "ModuleAnimateHeat":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAnimateHeat>() != null;
                case "ModuleAnimationGroup":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAnimationGroup>() != null;
                case "ModuleAnimatorLandingGear":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAnimatorLandingGear>() != null;
                case "ModuleAsteroid":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAsteroid>() != null;
                case "ModuleAsteroidAnalysis":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAsteroidAnalysis>() != null;
                case "ModuleAsteroidDrill":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAsteroidDrill>() != null;
                case "ModuleAsteroidInfo":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAsteroidInfo>() != null;
                case "ModuleAsteroidResource":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleAsteroidResource>() != null;
                case "ModuleBiomeScanner":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleBiomeScanner>() != null;
                case "ModuleCargoBay":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleCargoBay>() != null;
                case "ModuleCommand":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleCommand>() != null;
                case "ModuleConductionMultiplier":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleConductionMultiplier>() != null;
                case "ModuleControlSurface":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleControlSurface>() != null;
                case "ModuleDataTransmitter":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleDataTransmitter>() != null;
                case "ModuleDecouple":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleDecouple>() != null;
                case "ModuleDeployableRadiator":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleDeployableRadiator>() != null;
                case "ModuleDeployableSolarPanel":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleDeployableSolarPanel>() != null;
                case "ModuleDisplaceTweak":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleDisplaceTweak>() != null;
                case "ModuleDockingNode":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleDockingNode>() != null;
                case "ModuleDragModifier":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleDragModifier>() != null;
                case "ModuleEffectTest":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleEffectTest>() != null;
                case "ModuleEngines":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleEngines>() != null;
                case "ModuleEnginesFX":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleEnginesFX>() != null;
                case "ModuleEnviroSensor":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleEnviroSensor>() != null;
                case "ModuleFuelJettison":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleFuelJettison>() != null;
                case "ModuleGenerator":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleGenerator>() != null;
                case "ModuleGimbal":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleGimbal>() != null;
                case "ModuleGPS":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleGPS>() != null;
                case "ModuleGrappleNode":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleGrappleNode>() != null;
                case "ModuleHighDefCamera":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleHighDefCamera>() != null;
                case "ModuleJettison":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleJettison>() != null;
                case "ModuleJointMotor":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleJointMotor>() != null;
                case "ModuleJointMotorTest":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleJointMotorTest>() != null;
                case "ModuleJointPivot":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleJointPivot>() != null;
                case "ModuleLandingGear":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleLandingGear>() != null;
                case "ModuleLandingGearFixed":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleLandingGearFixed>() != null;
                case "ModuleLandingLeg":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleLandingLeg>() != null;
                case "ModuleLiftingSurface":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleLiftingSurface>() != null;
                case "ModuleLight":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleLight>() != null;
                case "ModuleOrbitalScanner":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleOrbitalScanner>() != null;
                case "ModuleOrbitalSurveyor":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleOrbitalSurveyor>() != null;
                case "ModuleOverheatDisplay":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleOverheatDisplay>() != null;
                case "ModuleParachute":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleParachute>() != null;
                case "ModulePhysicMaterial":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModulePhysicMaterial>() != null;
                case "ModuleProceduralFairing":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleProceduralFairing>() != null;
                case "ModuleRCS":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleRCS>() != null;
                case "ModuleReactionWheel":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleReactionWheel>() != null;
                case "ModuleRemoteController":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleRemoteController>() != null;
                case "ModuleResource":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleResource>() != null;
                case "ModuleResourceConverter":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleResourceConverter>() != null;
                case "ModuleResourceHarvester":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleResourceHarvester>() != null;
                case "ModuleResourceIntake":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleResourceIntake>() != null;
                case "ModuleResourceScanner":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleResourceScanner>() != null;
                case "ModuleRotatingJoint":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleRotatingJoint>() != null;
                case "ModuleSampleCollector":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleSampleCollector>() != null;
                case "ModuleSampleContainer":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleSampleContainer>() != null;
                case "ModuleSAS":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleSAS>() != null;
                case "ModuleScienceContainer":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleScienceContainer>() != null;
                case "ModuleScienceConverter":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleScienceConverter>() != null;
                case "ModuleScienceExperiment":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleScienceExperiment>() != null;
                case "ModuleScienceLab":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleScienceLab>() != null;
                case "ModuleSeeThroughObject":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleSeeThroughObject>() != null;
                case "ModuleSteering":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleSteering>() != null;
                case "ModuleSurfaceFX":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleSurfaceFX>() != null;
                case "ModuleTestSubject":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleTestSubject>() != null;
                case "ModuleTripLogger":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleTripLogger>() != null;
                case "ModuleWheel":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleWheel>() != null;
                case "FXModuleAnimateThrottle":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<FXModuleAnimateThrottle>() != null;
                case "FXModuleConstrainPosition":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<FXModuleConstrainPosition>() != null;
                case "FXModuleLookAtConstraint":
                    return part.partPrefab.Modules.FirstOfTypeOrDefault<FXModuleLookAtConstraint>() != null;
                default:
                    return false;
            }
        }

        /// <summary>
        /// check the part name/id exactly matches one in the list
        /// </summary>
        public static bool checkName(AvailablePart part, string[] value)
        {
            return value.Contains(part.name.Replace('.', '_'), new CaseInsensitiveComparer());
        }
        
        /// <summary>
        /// check the user viewable part title contains any of the listed values for a partial match
        /// </summary>
        public static bool checkTitle(AvailablePart part, string[] value)
        {
            return value.Any(s => part.title.ToLower().Contains(s.ToLower()));
        }

        /// <summary>
        /// check the resources the part holds
        /// </summary>
        public static bool checkResource(AvailablePart part, string[] value, bool contains = true)
        {
            if (part.partPrefab == null || part.partPrefab.Resources == null)
                return false;

            if (contains)
                return value.Any(s => part.partPrefab.Resources.Contains(s) && part.partPrefab.Resources[s].maxAmount > 0);
            else
            {
                foreach (PartResource r in part.partPrefab.Resources)
                    if (!value.Contains(r.resourceName))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// check the propellants this engine uses
        /// </summary>
        public static bool checkPropellant(AvailablePart part, string[] value, bool contains = true)
        {
            if (contains)
            {
                foreach (ModuleEngines e in part.partPrefab.GetModules<ModuleEngines>())
                {
                    if (e.propellants.Any(p => value.Contains(p.name)))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                // return false only if every propellant across all combos is in the list
                foreach (ModuleEngines e in part.partPrefab.GetModules<ModuleEngines>())
                {
                    if (e.propellants.Any(p => !value.Contains(p.name)))
                    {
                        Core.Log(part.name);
                        Core.Log("values: " + string.Join(",", value));
                        e.propellants.ForEach(p => Core.Log(p.name));
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// check the tech required to unlock the part outside sandbox
        /// </summary>
        public static bool checkTech(AvailablePart part, string[] value)
        {
            return value.Contains(part.TechRequired);
        }

        /// <summary>
        /// check against the manufacturer of the part
        /// </summary>
        public static bool checkManufacturer(AvailablePart part, string[] value)
        {
            return value.Contains(part.manufacturer);
        }

        /// <summary>
        /// checks against the root GameData folder name for a part.
        /// </summary>
        public static bool checkFolder(AvailablePart part, string[] value)
        {
            string path;
            if (Core.Instance.partPathDict.TryGetValue(part.name, out path))
            {
                string folder = path.Split(new char[] { '\\', '/' }, 2)[0];
                return value.Contains(folder);
            }
            return false;
        }

        /// <summary>
        /// check against the full path from GameData to the part. eg Squad/Parts
        /// </summary>
        public static bool checkPath(AvailablePart part, string[] value)
        {
            string path;
            if (Core.Instance.partPathDict.TryGetValue(part.name, out path))
                return value.Any(s => path.StartsWith(s, StringComparison.InvariantCultureIgnoreCase));

            return false;
        }

        /// <summary>
        /// checks against the attach node sizes on the part
        /// </summary>
        public static bool checkPartSize(AvailablePart part, string[] value, bool contains, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null || part.partPrefab.attachNodes == null)
                return false;

            if (equality == ConfigNodes.Check.Equality.Equals)
            {
                foreach (AttachNode node in part.partPrefab.attachNodes)
                {
                    if (contains)
                        if (value.Contains(node.size.ToString()))
                            return true;
                        else
                            if (!value.Contains(node.size.ToString()))
                                return true;
                }
            }
            else if (equality == ConfigNodes.Check.Equality.GreaterThan)
            {
                int i;
                if (int.TryParse(value[0], out i))
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
                if (int.TryParse(value[0], out i))
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

        /// <summary>
        /// check against the number of crew this part can hold
        /// </summary>
        public static bool checkCrewCapacity(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null)
                return false;

            if (equality == ConfigNodes.Check.Equality.Equals)
            {
                foreach (string s in value)
                {
                    int i;
                    if (int.TryParse(s, out i))
                    {
                        if (Math.Max(i, 0) == part.partPrefab.CrewCapacity)
                            return true;
                    }
                }
            }
            else if (equality == ConfigNodes.Check.Equality.GreaterThan)
            {
                int i;
                if (int.TryParse(value[0], out i))
                {
                    if (part.partPrefab.CrewCapacity > i)
                        return true;
                }
            }
            else
            {
                int i;
                if (int.TryParse(value[0], out i))
                {
                    if (part.partPrefab.CrewCapacity < i)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// check the part mass against a list of values
        /// </summary>
        #warning takes a value array, but only checks the first one
        #warning why the heck is this an integer conversion?
        public static bool checkMass(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null)
                return false;

            int i;
            if (int.TryParse(value[0], out i))
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

        /// <summary>
        /// check the part cost against a string list
        /// </summary>
        #warning takes a value array, but only checks the first one
        #warning cost doesn't have to be an integer...
        public static bool checkCost(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality)
        {
            int i;
            if (int.TryParse(value[0], out i))
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

        /// <summary>
        /// check the impact speed at which the part will explode
        /// </summary>
        #warning takes a value array, but only checks the first one
        #warning why all the int conversions...?
        public static bool checkCrashTolerance(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null)
                return false;

            int i;
            if (int.TryParse(value[0], out i))
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

        /// <summary>
        /// compares against the part max temp
        /// </summary>
        #warning takes a value array, but only checks the first one
        #warning int conversion may actually be valid here, but needs verification
        public static bool checkTemperature(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality)
        {
            if (part.partPrefab == null)
                return false;

            int i;
            if (int.TryParse(value[0], out i))
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

        /// <summary>
        /// bulkhead profiles used to id part shapes for stock editor. parts with no profiles get dumped in srf
        /// </summary>
        public static bool checkBulkHeadProfiles(AvailablePart part, string[] value, bool contains)
        {
            if (part.bulkheadProfiles == null)
            {
                if (value.Contains("srf"))
                    return true;
                return false;
            }

            foreach (string s in part.bulkheadProfiles.Split(','))
            {
                if (value.Any(v => v == s.Trim()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// checks if the part can be used to control a vessel
        /// </summary>
        public static bool isCommand(AvailablePart part)
        {
            if (isMannedPod(part))
                return true;
            if (isDrone(part))
                return true;
            if (part.partPrefab.Modules.FirstOfTypeOrDefault<KerbalSeat>() != null)
                return true;
            return false;
        }
        
        /// <summary>
        /// checks if the part is an engine
        /// </summary>
        public static bool isEngine(AvailablePart part)
        {
            if (part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleEngines>() != null)
                return true;
            return false;
        }

        /// <summary>
        /// checks if the part can be used to control a vessel and holds crew
        /// </summary>
        public static bool isMannedPod(AvailablePart part)
        {
            if (part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleCommand>() != null && part.partPrefab.CrewCapacity > 0)
                return true;
            return false;
        }

        /// <summary>
        /// checks if the part can be used to control a vessel and doesn't hold crew
        /// </summary>
        public static bool isDrone(AvailablePart part)
        {
            if (part.partPrefab.Modules.FirstOfTypeOrDefault<ModuleCommand>() != null && part.partPrefab.CrewCapacity == 0)
                return true;
            return false;
        }

        /// <summary>
        /// checks if the part has multiple bottom attach nodes
        /// </summary>
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

        /// <summary>
        /// checks if the part has two attach nodes and they are different sizes
        /// </summary>
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

        /// <summary>
        /// get a list of partmodules of type T
        /// </summary>
        public static List<T> GetModules<T>(this Part part) where T : PartModule
        {
            List<T> moduleList = new List<T>();
            for (int i = 0; i < part.Modules.Count; i++)
            {
                T module = part.Modules[i] as T;
                if (module != null)
                    moduleList.Add(module);
            }
            return moduleList;
        }
        
        /// <summary>
        /// get the first module of type T
        /// </summary>
        public static T GetModule<T>(this Part part) where T : PartModule
        {
            for (int i = 0; i < part.Modules.Count; i++ )
            {
                T module = part.Modules[i] as T;
                if (module != null)
                    return module;
            }
            return null;
        }

        /// <summary>
        /// used for string contains where we don't care about the string case
        /// </summary>
        #warning gethashcode can be written better (without a toLower...)
        class CaseInsensitiveComparer : IEqualityComparer<string>
        {
            public bool Equals(string s1, string s2)
            {
                return string.Equals(s1, s2, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(string s)
            {
                return s.ToLower().GetHashCode();
            }
        }
    }
}
