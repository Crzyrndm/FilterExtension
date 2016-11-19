using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterExtensions.Utility
{
    using ModuleWheels;

    public static class PartType
    {
        /// <summary>
        /// check the part against another subcategory. Hard limited to a depth of 10
        /// </summary>
        public static bool checkSubcategory(AvailablePart part, string[] value, int depth)
        {
            if (depth > 10)
            {
                Core.Log("subcategory check depth limit (10) exceeded. Check terminated on suspicion of circular subcategory checking!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", Core.LogLevel.Error);
                return false;
            }
            foreach (string s in value)
            {
                ConfigNodes.customSubCategory subcategory;
                if (Core.Instance.subCategoriesDict.TryGetValue(s, out subcategory) && subcategory.checkPartFilters(part, ++depth))
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
                        testVal = ResearchAndDevelopment.PartModelPurchased(part);
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
        public static bool checkCategory(AvailablePart part, string[] value)
        {
            switch (part.category)
            {
                case PartCategories.Pods:
                    return value.Contains("Pods", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Propulsion:
                    if (isEngine(part))
                        return value.Contains("Engines", StringComparer.OrdinalIgnoreCase) || value.Contains("Engine", StringComparer.OrdinalIgnoreCase);
                    else
                        return value.Contains("Fuel Tanks", StringComparer.OrdinalIgnoreCase) || value.Contains("FuelTank", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Engine:
                    return value.Contains("Engines", StringComparer.OrdinalIgnoreCase) || value.Contains("Engine", StringComparer.OrdinalIgnoreCase);

                case PartCategories.FuelTank:
                    return value.Contains("Fuel Tanks", StringComparer.OrdinalIgnoreCase) || value.Contains("FuelTank", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Control:
                    return value.Contains("Control", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Structural:
                    return value.Contains("Structural", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Aero:
                    return value.Contains("Aerodynamics", StringComparer.OrdinalIgnoreCase) || value.Contains("Aero", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Utility:
                    return value.Contains("Utility", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Science:
                    return value.Contains("Science", StringComparer.OrdinalIgnoreCase);

                case PartCategories.none:
                    return value.Contains("None", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Communication:
                    return value.Contains("Communications", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Ground:
                    return value.Contains("Ground", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Thermal:
                    return value.Contains("Thermal", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Electrical:
                    return value.Contains("Electrical", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Coupling:
                    return value.Contains("Coupling", StringComparer.OrdinalIgnoreCase);

                case PartCategories.Payload:
                    return value.Contains("Payload", StringComparer.OrdinalIgnoreCase);

                default:
                    return false;
            }
        }

        /// <summary>
        /// check the user visible names of each part module against a string list
        /// </summary>
        public static bool checkModuleTitle(AvailablePart part, string[] values, bool contains = true, bool exact = false)
        {
            if (part.moduleInfos == null)
                return false;

            return Contains(values, part.moduleInfos, m => m.moduleName, contains, exact);
        }

        /// <summary>
        /// check the part module type against a string list
        /// </summary>
        public static bool checkModuleName(AvailablePart part, string[] values, bool contains = true, bool exact = false)
        {
            if (!exact)
                return contains == values.Any(s => checkModuleNameType(part, s));
            else
                return part.partPrefab.Modules.Count == values.Length && values.All(s => checkModuleNameType(part, s));
        }

        /// <summary>
        /// provides a typed check for stock modules which then allows for inheritance to work
        /// </summary>
        private static Dictionary<string, Type> loaded_modules;

        private static Dictionary<string, Type> Loaded_Modules
        {
            get
            {
                if (loaded_modules == null)
                {
                    loaded_modules = new Dictionary<string, Type>();
                    foreach (AvailablePart ap in PartLoader.LoadedPartsList)
                    {
                        foreach (PartModule pm in ap.partPrefab.Modules)
                        {
                            if (!string.IsNullOrEmpty(pm.moduleName) && !Loaded_Modules.ContainsKey(pm.moduleName))
                            {
                                loaded_modules.Add(pm.moduleName, pm.GetType());
                            }
                        }
                    }
                }
                return loaded_modules;
            }
        }

        public static bool checkModuleNameType(AvailablePart part, string value)
        {
            switch (value)
            {
                case "ModuleAblator":
                    return part.partPrefab.Modules.Contains<ModuleAblator>();

                case "ModuleActiveRadiator":
                    return part.partPrefab.Modules.Contains<ModuleActiveRadiator>();

                case "ModuleAeroSurface":
                    return part.partPrefab.Modules.Contains<ModuleAeroSurface>();

                case "ModuleAlternator":
                    return part.partPrefab.Modules.Contains<ModuleAlternator>();

                case "ModuleAnalysisResource":
                    return part.partPrefab.Modules.Contains<ModuleAnalysisResource>();

                case "ModuleAnchoredDecoupler":
                    return part.partPrefab.Modules.Contains<ModuleAnchoredDecoupler>();

                case "ModuleAnimateGeneric":
                    return part.partPrefab.Modules.Contains<ModuleAnimateGeneric>();

                case "ModuleAnimateHeat":
                    return part.partPrefab.Modules.Contains<ModuleAnimateHeat>();

                case "ModuleAnimationGroup":
                    return part.partPrefab.Modules.Contains<ModuleAnimationGroup>();

                case "ModuleAnimatorLandingGear":
                    return part.partPrefab.Modules.Contains<ModuleAnimatorLandingGear>();

                case "ModuleAsteroid":
                    return part.partPrefab.Modules.Contains<ModuleAsteroid>();

                case "ModuleAsteroidAnalysis":
                    return part.partPrefab.Modules.Contains<ModuleAsteroidAnalysis>();

                case "ModuleAsteroidDrill":
                    return part.partPrefab.Modules.Contains<ModuleAsteroidDrill>();

                case "ModuleAsteroidInfo":
                    return part.partPrefab.Modules.Contains<ModuleAsteroidInfo>();

                case "ModuleAsteroidResource":
                    return part.partPrefab.Modules.Contains<ModuleAsteroidResource>();

                case "ModuleBiomeScanner":
                    return part.partPrefab.Modules.Contains<ModuleBiomeScanner>();

                case "ModuleCargoBay":
                    return part.partPrefab.Modules.Contains<ModuleCargoBay>();

                case "ModuleCommand":
                    return part.partPrefab.Modules.Contains<ModuleCommand>();

                case "ModuleConductionMultiplier":
                    return part.partPrefab.Modules.Contains<ModuleConductionMultiplier>();

                case "ModuleControlSurface":
                    return part.partPrefab.Modules.Contains<ModuleControlSurface>();

                case "ModuleCoreHeat":
                    return part.partPrefab.Modules.Contains<ModuleCoreHeat>();

                case "ModuleDataTransmitter":
                    return part.partPrefab.Modules.Contains<ModuleDataTransmitter>();

                case "ModuleDecouple":
                    return part.partPrefab.Modules.Contains<ModuleDecouple>();

                case "ModuleDeployableRadiator":
                    return part.partPrefab.Modules.Contains<ModuleDeployableRadiator>();

                case "ModuleDeployableSolarPanel":
                    return part.partPrefab.Modules.Contains<ModuleDeployableSolarPanel>();

                case "ModuleDisplaceTweak":
                    return part.partPrefab.Modules.Contains<ModuleDisplaceTweak>();

                case "ModuleDockingNode":
                    return part.partPrefab.Modules.Contains<ModuleDockingNode>();

                case "ModuleDragModifier":
                    return part.partPrefab.Modules.Contains<ModuleDragModifier>();

                case "ModuleEffectTest":
                    return part.partPrefab.Modules.Contains<ModuleEffectTest>();

                case "ModuleEngines":
                    return part.partPrefab.Modules.Contains<ModuleEngines>();

                case "ModuleEnginesFX":
                    return part.partPrefab.Modules.Contains<ModuleEnginesFX>();

                case "ModuleEnviroSensor":
                    return part.partPrefab.Modules.Contains<ModuleEnviroSensor>();

                case "ModuleFuelJettison":
                    return part.partPrefab.Modules.Contains<ModuleFuelJettison>();

                case "ModuleGenerator":
                    return part.partPrefab.Modules.Contains<ModuleGenerator>();

                case "ModuleGimbal":
                    return part.partPrefab.Modules.Contains<ModuleGimbal>();

                case "ModuleGPS":
                    return part.partPrefab.Modules.Contains<ModuleGPS>();

                case "ModuleGrappleNode":
                    return part.partPrefab.Modules.Contains<ModuleGrappleNode>();

                case "ModuleJettison":
                    return part.partPrefab.Modules.Contains<ModuleJettison>();

                case "ModuleJointMotor":
                    return part.partPrefab.Modules.Contains<ModuleJointMotor>();

                case "ModuleJointMotorTest":
                    return part.partPrefab.Modules.Contains<ModuleJointMotorTest>();

                case "ModuleJointPivot":
                    return part.partPrefab.Modules.Contains<ModuleJointPivot>();

                case "ModuleLiftingSurface":
                    return part.partPrefab.Modules.Contains<ModuleLiftingSurface>();

                case "ModuleLight":
                    return part.partPrefab.Modules.Contains<ModuleLight>();

                case "ModuleOrbitalScanner":
                    return part.partPrefab.Modules.Contains<ModuleOrbitalScanner>();

                case "ModuleOrbitalSurveyor":
                    return part.partPrefab.Modules.Contains<ModuleOrbitalSurveyor>();

                case "ModuleOverheatDisplay":
                    return part.partPrefab.Modules.Contains<ModuleOverheatDisplay>();

                case "ModuleParachute":
                    return part.partPrefab.Modules.Contains<ModuleParachute>();

                case "ModulePhysicMaterial":
                    return part.partPrefab.Modules.Contains<ModulePhysicMaterial>();

                case "ModuleProceduralFairing":
                    return part.partPrefab.Modules.Contains<ModuleProceduralFairing>();

                case "ModuleRCS":
                    return part.partPrefab.Modules.Contains<ModuleRCS>();

                case "ModuleReactionWheel":
                    return part.partPrefab.Modules.Contains<ModuleReactionWheel>();

                case "ModuleRemoteController":
                    return part.partPrefab.Modules.Contains<ModuleRemoteController>();

                case "ModuleResourceConverter":
                    return part.partPrefab.Modules.Contains<ModuleResourceConverter>();

                case "ModuleResourceHarvester":
                    return part.partPrefab.Modules.Contains<ModuleResourceHarvester>();

                case "ModuleResourceIntake":
                    return part.partPrefab.Modules.Contains<ModuleResourceIntake>();

                case "ModuleResourceScanner":
                    return part.partPrefab.Modules.Contains<ModuleResourceScanner>();

                case "ModuleRotatingJoint":
                    return part.partPrefab.Modules.Contains<ModuleRotatingJoint>();

                case "ModuleSampleCollector":
                    return part.partPrefab.Modules.Contains<ModuleSampleCollector>();

                case "ModuleSampleContainer":
                    return part.partPrefab.Modules.Contains<ModuleSampleContainer>();

                case "ModuleSAS":
                    return part.partPrefab.Modules.Contains<ModuleSAS>();

                case "ModuleScienceContainer":
                    return part.partPrefab.Modules.Contains<ModuleScienceContainer>();

                case "ModuleScienceConverter":
                    return part.partPrefab.Modules.Contains<ModuleScienceConverter>();

                case "ModuleScienceExperiment":
                    return part.partPrefab.Modules.Contains<ModuleScienceExperiment>();

                case "ModuleScienceLab":
                    return part.partPrefab.Modules.Contains<ModuleScienceLab>();

                case "ModuleSeeThroughObject":
                    return part.partPrefab.Modules.Contains<ModuleSeeThroughObject>();

                case "ModuleStatusLight":
                    return part.partPrefab.Modules.Contains<ModuleStatusLight>();

                case "ModuleSurfaceFX":
                    return part.partPrefab.Modules.Contains<ModuleSurfaceFX>();

                case "ModuleTestSubject":
                    return part.partPrefab.Modules.Contains<ModuleTestSubject>();

                case "ModuleToggleCrossfeed":
                    return part.partPrefab.Modules.Contains<ModuleToggleCrossfeed>();

                case "ModuleTripLogger":
                    return part.partPrefab.Modules.Contains<ModuleTripLogger>();

                case "ModuleWheelBase":
                    return part.partPrefab.Modules.Contains<ModuleWheelBase>();

                case "FXModuleAnimateThrottle":
                    return part.partPrefab.Modules.Contains<FXModuleAnimateThrottle>();

                case "FXModuleConstrainPosition":
                    return part.partPrefab.Modules.Contains<FXModuleConstrainPosition>();

                case "FXModuleLookAtConstraint":
                    return part.partPrefab.Modules.Contains<FXModuleLookAtConstraint>();

                case "ModuleWheelBogey":
                    return part.partPrefab.Modules.Contains<ModuleWheelBogey>();

                case "ModuleWheelBrakes":
                    return part.partPrefab.Modules.Contains<ModuleWheelBrakes>();

                case "ModuleWheelDamage":
                    return part.partPrefab.Modules.Contains<ModuleWheelDamage>();

                case "ModuleWheelDeployment":
                    return part.partPrefab.Modules.Contains<ModuleWheelDeployment>();

                case "ModuleWheelLock":
                    return part.partPrefab.Modules.Contains<ModuleWheelLock>();

                case "ModuleWheelMotor":
                    return part.partPrefab.Modules.Contains<ModuleWheelMotor>();

                case "ModuleWheelMotorSteering":
                    return part.partPrefab.Modules.Contains<ModuleWheelMotorSteering>();

                case "ModuleWheelSteering":
                    return part.partPrefab.Modules.Contains<ModuleWheelSteering>();

                case "ModuleWheelSubmodule":
                    return part.partPrefab.Modules.Contains<ModuleWheelSubmodule>();

                case "ModuleWheelSuspension":
                    return part.partPrefab.Modules.Contains<ModuleWheelSuspension>();

                default: // use specialisation where I can to avoid the "slow" type checking this entails
                    if (Loaded_Modules.ContainsKey(value))
                    {
                        Type string_type = Loaded_Modules[value];
                        foreach (PartModule pm in part.partPrefab.Modules)
                        {
                            if (value == pm.moduleName || string_type.IsAssignableFrom(Loaded_Modules[pm.moduleName]))
                                return true;
                        }
                    }
                    return false;
            }
        }

        /// <summary>
        /// check the part name/id exactly matches one in the list
        /// </summary>
        public static bool checkName(AvailablePart part, string[] value)
        {
            return value.Contains(part.name.Replace('.', '_'), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// check the user viewable part title contains any of the listed values for a partial match
        /// </summary>
        public static bool checkTitle(AvailablePart part, string[] value)
        {
            return value.Any(s => part.title.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1);
        }

        /// <summary>
        /// check the resources the part holds
        /// </summary>
        public static bool checkResource(AvailablePart part, string[] values, bool contains = true, bool exact = false)
        {
            if (part.partPrefab.Resources == null)
                return false;
            return Contains(values, part.partPrefab.Resources, r => r.resourceName, contains, exact);
        }

        /// <summary>
        /// check the propellants this engine uses
        /// </summary>
        public static bool checkPropellant(AvailablePart part, string[] values, bool contains = true, bool exact = false)
        {
            ModuleEngines e;
            for (int i = 0; i < part.partPrefab.Modules.Count; ++i)
            {
                e = part.partPrefab.Modules[i] as ModuleEngines;
                if (e != null && Contains(values, e.propellants, p => p.name, contains, exact))
                    return true;
            }
            return false;
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
                return value.Contains(path.Substring(0, path.IndexOfAny(new char[] { '\\', '/' })));
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
                return value.Any(s => path.StartsWith(s, StringComparison.OrdinalIgnoreCase));

            return false;
        }

        /// <summary>
        /// checks against the attach node sizes on the part
        /// </summary>
        public static bool checkPartSize(AvailablePart part, string[] values, bool contains = true, ConfigNodes.Check.Equality equality = ConfigNodes.Check.Equality.Equals, bool exact = false)
        {
            if (part.partPrefab.attachNodes == null)
                return false;

            if (equality == ConfigNodes.Check.Equality.Equals)
                return Contains(values, part.partPrefab.attachNodes, n => n.size.ToString(), contains, exact);
            else // only compare against the first value here
            {
                if (values.Length > 1)
                    Core.Log("Size comparisons against multiple values when not using Equals only use the first value. Value list is: {0}", Core.LogLevel.Warn, string.Join(", ", values));

                int i;
                if (int.TryParse(values[0], out i))
                {
                    if (equality == ConfigNodes.Check.Equality.GreaterThan)
                    {
                        part.partPrefab.attachNodes.Any(n => n.size > i);
                        return true;
                    }
                    else if (equality == ConfigNodes.Check.Equality.LessThan)
                    {
                        part.partPrefab.attachNodes.Any(n => n.size < i);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// check against the number of crew this part can hold
        /// </summary>
        public static bool checkCrewCapacity(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality = ConfigNodes.Check.Equality.Equals)
        {
            if (part.partPrefab == null)
                return false;

            if (equality == ConfigNodes.Check.Equality.Equals)
                return value.Contains(part.partPrefab.CrewCapacity.ToString(), StringComparer.OrdinalIgnoreCase);
            else // only compare against the first value here
            {
                if (value.Length > 1)
                    Core.Log("Crew comparisons against multiple values when not using Equals only use the first value. Value list is: {0}", Core.LogLevel.Warn, string.Join(", ", value));

                double d;
                if (double.TryParse(value[0], out d))
                {
                    if (equality == ConfigNodes.Check.Equality.GreaterThan && part.partPrefab.CrewCapacity > d)
                        return true;
                    else if (equality == ConfigNodes.Check.Equality.LessThan && part.partPrefab.CrewCapacity < d)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// check the part mass against a list of values
        /// </summary>
        public static bool checkMass(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality = ConfigNodes.Check.Equality.Equals)
        {
            if (part.partPrefab == null)
                return false;

            if (equality == ConfigNodes.Check.Equality.Equals)
                return value.Contains(part.partPrefab.mass.ToString(), StringComparer.OrdinalIgnoreCase);
            else
            {
                if (value.Length > 1)
                    Core.Log("Mass comparisons against multiple values when not using Equals only use the first value. Value list is: {0}", Core.LogLevel.Warn, string.Join(", ", value));

                double d;
                if (double.TryParse(value[0], out d))
                {
                    if (equality == ConfigNodes.Check.Equality.GreaterThan && part.partPrefab.mass > d)
                        return true;
                    else if (equality == ConfigNodes.Check.Equality.LessThan && part.partPrefab.mass < d)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// check the part cost against a string list
        /// </summary>
        public static bool checkCost(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality = ConfigNodes.Check.Equality.Equals)
        {
            if (equality == ConfigNodes.Check.Equality.Equals)
                return value.Contains(part.cost.ToString(), StringComparer.OrdinalIgnoreCase);
            else
            {
                if (value.Length > 1)
                    Core.Log("Cost comparisons against multiple values when not using Equals only use the first value. Value list is: {0}", Core.LogLevel.Warn, string.Join(", ", value));

                double d;
                if (double.TryParse(value[0], out d))
                {
                    if (equality == ConfigNodes.Check.Equality.GreaterThan && part.cost > d)
                        return true;
                    else if (equality == ConfigNodes.Check.Equality.LessThan && part.cost < d)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// check the impact speed at which the part will explode
        /// </summary>
        public static bool checkCrashTolerance(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality = ConfigNodes.Check.Equality.Equals)
        {
            if (part.partPrefab == null)
                return false;

            if (equality == ConfigNodes.Check.Equality.Equals)
                return value.Contains(part.partPrefab.crashTolerance.ToString());
            else
            {
                if (value.Length > 1)
                    Core.Log("Crash tolerance comparisons against multiple values when not using Equals only use the first value. Value list is: {0}", Core.LogLevel.Warn, string.Join(", ", value));

                float f;
                if (float.TryParse(value[0], out f))
                {
                    if (equality == ConfigNodes.Check.Equality.GreaterThan && part.partPrefab.crashTolerance > f)
                        return true;
                    else if (equality == ConfigNodes.Check.Equality.LessThan && part.partPrefab.crashTolerance < f)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// compares against the part max temp
        /// </summary>
        public static bool checkTemperature(AvailablePart part, string[] value, ConfigNodes.Check.Equality equality = ConfigNodes.Check.Equality.Equals)
        {
            if (part.partPrefab == null)
                return false;

            if (equality == ConfigNodes.Check.Equality.Equals)
                return value.Contains(part.partPrefab.maxTemp.ToString(), StringComparer.OrdinalIgnoreCase);
            else
            {
                if (value.Length > 1)
                    Core.Log("Temperature comparisons against multiple values when not using Equals only use the first value. Value list is: {0}", Core.LogLevel.Warn, string.Join(", ", value));
                double d;
                if (double.TryParse(value[0], out d))
                {
                    if (equality == ConfigNodes.Check.Equality.GreaterThan && part.partPrefab.maxTemp > d)
                        return true;
                    else if (equality == ConfigNodes.Check.Equality.LessThan && part.partPrefab.maxTemp < d)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// bulkhead profiles used to id part shapes for stock editor. parts with no profiles get dumped in srf
        /// </summary>
        public static bool checkBulkHeadProfiles(AvailablePart part, string[] values, bool contains = true, bool exact = false)
        {
            if (part.bulkheadProfiles == null)
                return values.Contains("srf");

            return Contains(values, part.bulkheadProfiles.Split(','), contains, exact);
        }

        public static bool checkTags(AvailablePart part, string[] values, bool contains = true, bool exact = false)
        {
            if (string.IsNullOrEmpty(part.tags))
                return false;
            return Contains(values, part.tags.Split(new char[4] { ' ', ',', '|', ';' }, StringSplitOptions.RemoveEmptyEntries), contains, exact);
        }

        /// <summary>
        /// checks if the part can be used to control a vessel
        /// </summary>
        public static bool isCommand(AvailablePart part)
        {
            return isMannedPod(part) || isDrone(part) || part.partPrefab.Modules.Contains<KerbalSeat>();
        }

        /// <summary>
        /// checks if the part is an engine
        /// </summary>
        public static bool isEngine(AvailablePart part)
        {
            return part.partPrefab.Modules.Contains<ModuleEngines>();
        }

        /// <summary>
        /// checks if the part can be used to control a vessel and holds crew
        /// </summary>
        public static bool isMannedPod(AvailablePart part)
        {
            return part.partPrefab.Modules.Contains<ModuleCommand>() && part.partPrefab.CrewCapacity > 0;
        }

        /// <summary>
        /// checks if the part can be used to control a vessel and doesn't hold crew
        /// </summary>
        public static bool isDrone(AvailablePart part)
        {
            return part.partPrefab.Modules.Contains<ModuleCommand>() && part.partPrefab.CrewCapacity == 0;
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
            if (part.partPrefab == null || part.partPrefab.attachNodes == null || part.partPrefab.attachNodes.Count != 2 || isCommand(part))
                return false;
            return part.partPrefab.attachNodes[0].size != part.partPrefab.attachNodes[1].size;
        }

        public static bool Contains(string[] CheckParams, IEnumerable<string> partParams, bool contains = true, bool exact = false)
        {
            if (!exact)
            {
                foreach (string s in partParams)
                {
                    if (contains == CheckParams.Contains(s.Trim(), StringComparer.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            else
            {
                int i = 0;
                foreach (string s in partParams)
                {
                    if (!CheckParams.Contains(s.Trim(), StringComparer.OrdinalIgnoreCase))
                        return false;
                    ++i;
                }
                return i == CheckParams.Length;
            }
        }

        public static bool Contains<T>(string[] CheckParams, IEnumerable<T> partParams, Func<T, string> ToStringFunc, bool contains = true, bool exact = false)
        {
            if (!exact)
            {
                foreach (T t in partParams)
                {
                    if (contains == CheckParams.Contains(ToStringFunc(t).Trim(), StringComparer.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            else
            {
                int i = 0;
                foreach (T t in partParams)
                {
                    if (!CheckParams.Contains(ToStringFunc(t).Trim(), StringComparer.OrdinalIgnoreCase))
                        return false;
                    ++i;
                }
                return i == CheckParams.Length;
            }
        }

        public static bool Contains<T>(string[] CheckParams, IEnumerable<T> partParams, Func<T, string> ToStringFunc, Func<T, bool> selectorFunc, bool contains = true, bool exact = false)
        {
            if (!exact)
            {
                foreach (T t in partParams)
                {
                    if (selectorFunc(t) && contains == CheckParams.Contains(ToStringFunc(t).Trim(), StringComparer.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            else
            {
                int i = 0;
                foreach (T t in partParams)
                {
                    if (selectorFunc(t) && !CheckParams.Contains(ToStringFunc(t).Trim(), StringComparer.OrdinalIgnoreCase))
                        return false;
                    ++i;
                }
                return i == CheckParams.Length;
            }
        }

        public static char[] splitChars = new char[] { ',', ' ', '.' };

        public static bool NodeCheck(AvailablePart part, string[] parameters, ConfigNodes.Check.Equality equality = ConfigNodes.Check.Equality.Equals)
        {
            try
            {
                Type baseType;
                if (parameters.Length < 3
                    || !Loaded_Modules.TryGetValue(parameters[0], out baseType))
                    return false;
                foreach (PartModule pm in part.partPrefab.Modules)
                {
                    if (baseType.IsAssignableFrom(Loaded_Modules[pm.moduleName]))
                    {
                        BaseField f = pm.Fields[parameters[1]];
                        if (f == null)
                        {
                            return false;
                        }
                        double res, org;
                        if (f.originalValue == null)
                        {
                            return parameters[2].Equals("null", StringComparison.OrdinalIgnoreCase);
                        }
                        else if (!double.TryParse(parameters[2], out res) || !double.TryParse(f.originalValue.ToString(), out org))
                        {
                            return string.Equals(parameters[2], f.originalValue.ToString(), StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            if (equality == ConfigNodes.Check.Equality.Equals)
                            {
                                return org == res;
                            }
                            else if (equality == ConfigNodes.Check.Equality.GreaterThan)
                            {
                                return org > res;
                            }
                            else if (equality == ConfigNodes.Check.Equality.LessThan)
                            {
                                return org < res;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            return false;
        }
    }
}