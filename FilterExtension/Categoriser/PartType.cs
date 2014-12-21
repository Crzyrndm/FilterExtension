using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PartFilters.Categoriser
{
    static class PartType
    {
        public static bool isCompatible(AvailablePart part, List<string> filters)
        {
            bool compatible = false;
            foreach (string str in filters)
            {
                switch (str)
                {
                    case "isEngine":
                        compatible = isEngine(part);
                        break;
                    case "isResourceContainer":
                        compatible = isResourceContainer(part);
                        break;
                    case "isResourceIntake":
                        compatible = isResourceIntake(part);
                        break;
                    case "isMannedPod":
                        compatible = isMannedPod(part);
                        break;
                    case "isProbeCore":
                        compatible = isDrone(part);
                        break;
                    case "isRCSThruster":
                        compatible = isRCSThruster(part);
                        break;
                    case "isReactionWheel":
                        compatible = isReactionWheel(part);
                        break;
                    case "isFuselage":
                        compatible = isFuselage(part);
                        break;
                    case "isMulticoupler":
                        compatible = isMultiCoupler(part);
                        break;
                    case "isDecoupler":
                        compatible = isDecoupler(part);
                        break;
                    case "isAdapter":
                        compatible = isAdapter(part);
                        break;
                    case "isCargoBay":
                        compatible = isCargoBay(part);
                        break;
                    case "isNosecone":
                        compatible = isNoseCone(part);
                        break;
                    case "isControlSurface":
                        compatible = isControlSurface(part);
                        break;
                    case "isResourceGenerator":
                        compatible = isResourceGenerator(part);
                        break;
                    case "isRoverWheel":
                        compatible = isRoverWheel(part);
                        break;
                    case "isLanding":
                        compatible = isLanding(part);
                        break;
                    case "isLandingLeg":
                        compatible = isLandingLeg(part);
                        break;
                    case "isLandingGear":
                        compatible = isLandingGear(part);
                        break;
                    case "isParachute":
                        compatible = isParachute(part);
                        break;
                    case "isDockingPort":
                        compatible = isDockingPort(part);
                        break;
                    case "isExperiment":
                        compatible = hasExperiment(part);
                        break;
                    case "isScienceLab":
                        compatible = hasLab(part);
                        break;
                    case "isAntenna":
                        compatible = hasAntenna(part);
                        break;
                    case "isLF+LOx":
                        compatible = isLFLOxTank(part);
                        break;
                    case "isLF+LOxEngine":
                        compatible = isLFLOxEngine(part);
                        break;
                    case "isJetFuel":
                        compatible = isJetTank(part);
                        break;
                    case "isJetEngine":
                        compatible = isLFEngine(part);
                        break;
                    case "isSRB":
                        compatible = isSRB(part);
                        break;
                    case "isIonEngine":
                        compatible = isIonEngine(part);
                        break;
                    case "isMonopropellant":
                        compatible = isMonoPropTank(part);
                        break;
                    case "isXenon":
                        compatible = isXenonTank(part);
                        break;
                    case "isBattery":
                        compatible = isBattery(part);
                        break;
                    case "isAero":
                        compatible = isAero(part);
                        break;
                    case "isLight":
                        compatible = isLight(part);
                        break;
                    case "isLadder":
                        compatible = isLadder(part);
                        break;
                    case "isScience":
                        compatible = isScience(part);
                        break;
                    case "isControl":
                        compatible = isControl(part);
                        break;
                    case "isCabin":
                        compatible = isCabin(part);
                        break;
                    case "isWing":
                        compatible = isWing(part);
                        break;
                    case "TabCommand":
                        compatible = CommandTab(part);
                        break;
                    case "TabPropulsion":
                        compatible = PropulsionTab(part);
                        break;
                    case "TabControl":
                        compatible = ControlTab(part);
                        break;
                    case "TabUtility":
                        compatible = UtilityTab(part);
                        break;
                    case "TabAero":
                        compatible = AeroTab(part);
                        break;
                    case "TabScience":
                        compatible = ScienceTab(part);
                        break;
                    case "TabStructural":
                        compatible = StructuralTab(part);
                        break;
                    // Near future module checks begin
                    case "NFECapacitor":
                        compatible = isNFECapacitor(part);
                        break;
                    case "NFEFuelDrum":
                        compatible = isNFEFuelDrum(part);
                        break;
                    case "NFEFuelReprocessor":
                        compatible = isNFEFuelReprocessor(part);
                        break;
                    case "NFERadiator":
                        compatible = isNFERadiator(part);
                        break;
                    case "NFEReactor":
                        compatible = isNFERadiator(part);
                        break;
                    case "NFEArgonTank":
                        compatible = hasNFEResArgon(part);
                        break;
                    case "NFEHydrogenTank":
                        compatible = hasNFEResHydrogen(part);
                        break;
                    case "NFEArgonEngine":
                        compatible = isNFEArgonEngine(part);
                        break;
                    case "NFEVariableThrust":
                        compatible = isNFEVariableEngine(part);
                        break;
                    case "NFEVariableISP":
                        compatible = isNFEVasmirEngine(part);
                        break;
                    case "NFECurvedPanel":
                        compatible = isNFECurvedPanel(part);
                        break;
                    // Near future module checks end
                }
                if (compatible)
                    return true;
            }

            return false;
        }

        public static bool PropulsionTab(AvailablePart part)
        {
            return (part.category == PartCategories.Propulsion);
        }

        public static bool CommandTab(AvailablePart part)
        {
            return (part.category == PartCategories.Pods);
        }

        public static bool ControlTab(AvailablePart part)
        {
            return (part.category == PartCategories.Control);
        }

        public static bool StructuralTab(AvailablePart part)
        {
            return (part.category == PartCategories.Structural);
        }

        public static bool UtilityTab(AvailablePart part)
        {
            return (part.category == PartCategories.Utility);
        }

        public static bool AeroTab(AvailablePart part)
        {
            return (part.category == PartCategories.Aero);
        }

        public static bool ScienceTab(AvailablePart part)
        {
            return (part.category == PartCategories.Science);
        }

        public static string isSize(AvailablePart part)
        {
            int size = -1;
            foreach (AttachNode node in part.partPrefab.attachNodes)
            {
                if (size < node.size)
                    size = node.size;
            }

            if (size == 0)
                return "0.625m";
            else if (size > 0)
                return string.Format("{0:0.00}m", size * 1.25);
            else
                return "Radial";
        }

        public static int partSize(AvailablePart part)
        {
            int size = -1;
            foreach (AttachNode node in part.partPrefab.attachNodes)
            {
                if (size < node.size)
                    size = node.size;
            }
            return size;
        }

        public static bool isPropulsion(AvailablePart part)
        {
            if (isEngine(part))
                return true;
            if (isResourceContainer(part))
                return true;
            if (isResourceIntake(part))
                return true;
            return false;
        }
        
        public static bool isControl(AvailablePart part)
        {
            if (isCommand(part))
                return true;
            if (isReactionWheel(part))
                return true;
            if (isRCSThruster(part))
                return true;
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

        public static bool isStructural(AvailablePart part)
        {
            if (part.category.ToString() == "Aero")
                return false;
            if (isFuselage(part))
                return true;
            if (isAdapter(part))
                return true;
            if (isDecoupler(part))
                return true;
            if (isCargoBay(part))
                return true;
            if (isMultiCoupler(part))
                return true;
            if (isDecoupler(part))
                return true;
            return false;
        }

        public static bool isUtility(AvailablePart part)
        {
            if (isResourceGenerator(part))
                return true;
            if (isLanding(part))
                return true;
            if (isParachute(part))
                return true;
            if (isRoverWheel(part))
                return true;
            if (isDockingPort(part))
                return true;
            if (isLight(part))
                return true;
            if (isLadder(part))
                return true;
            return false;
        }

        public static bool isAero(AvailablePart part)
        {
            if (isControlSurface(part))
                return true;
            if (part.category.ToString() == "Aero" && !isResourceIntake(part))
                return true;
            return false;
        }

        public static bool isScience(AvailablePart part)
        {
            if (hasExperiment(part))
                return true;
            if (hasLab(part))
                return true;
            if (hasAntenna(part))
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

        public static bool isResourceContainer(AvailablePart part)
        {
            if (isCommand(part))
                return false;
            if (part.partPrefab.Resources.Count > 1)
                return true;
            if ((part.partPrefab.Resources.Count == 1 && !(part.partPrefab.Resources[0].resourceName == "IntakeAir" || part.partPrefab.Resources[0].resourceName == "SolidFuel" || (part.partPrefab.Resources[0].resourceName == "ElectricCharge" && part.partPrefab.Modules.OfType<ModuleAlternator>().Any()))))
                return true;
            return false;
        }

        public static bool isResourceIntake(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleResourceIntake>().Any())
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

        public static bool isRCSThruster(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleRCS>().Any())
                return true;
            return false;
        }

        public static bool isReactionWheel(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleReactionWheel>().Any() && !part.partPrefab.Modules.OfType<ModuleCommand>().Any())
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

        public static bool isDecoupler(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleDecouple>().Any())
                return true;
            if (part.partPrefab.Modules.OfType<ModuleAnchoredDecoupler>().Any())
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

        public static bool isCargoBay(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleCargoBay>().Any())
                return true;
            if (part.name.Contains("Cargo Bay") || part.name.Contains("Cargobay"))
                return true;
            return false;
        }

        public static bool isNoseCone(AvailablePart part)
        {
            if (part.partPrefab.Modules.Count == 0 && part.partPrefab.attachRules.allowStack && part.partPrefab.attachNodes.Count == 1)
                return true;
            return false;
        }

        public static bool isControlSurface(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleControlSurface>().Any())
                return true;
            if (part.partPrefab.Modules.Contains("FARControllableSurface"))
                return true;
            return false;
        }

        public static bool isResourceGenerator(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleGenerator>().Any() && !part.partPrefab.Modules.OfType<LaunchClamp>().Any())
                return true;
            if (part.partPrefab.Modules.OfType<ModuleDeployableSolarPanel>().Any())
                return true;
            return false;
        }

        public static bool isRoverWheel(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleWheel>().Any())
                return true;
            return false;
        }

        public static bool isLanding(AvailablePart part)
        {
            if (isLandingGear(part))
                return true;
            if (isLandingLeg(part))
                return true;
            return false;
        }

        public static bool isLandingLeg(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleLandingLeg>().Any())
                return true;
            return false;
        }

        public static bool isLandingGear(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleWheel>().Any())
                return true;
            return false;
        }

        public static bool isParachute(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleParachute>().Any())
                return true;
            if (part.partPrefab.Modules.Contains("RealChuteModule"))
                return true;
            return false;
        }

        public static bool isDockingPort(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleDockingNode>().Any())
                return true;
            return false;
        }

        public static bool isCabin(AvailablePart part)
        {
            if (!part.partPrefab.Modules.OfType<ModuleCommand>().Any() && part.partPrefab.CrewCapacity > 0)
                return true;
            return false;
        }

        public static bool hasExperiment(AvailablePart part)
        {
            if (part.partPrefab.CrewCapacity > 0)
                return false;
            if (part.partPrefab.Modules.OfType<ModuleScienceExperiment>().Any())
                return true;
            return false;
        }

        public static bool hasLab(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleScienceLab>().Any())
                return true;
            return false;
        }

        public static bool hasAntenna(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleDataTransmitter>().Any())
                return true;
            return false;
        }

        public static bool isLight(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<ModuleLight>().Any() && part.partPrefab.Modules.Count == 1)
                return true;
            return false;
        }

        public static bool isLadder(AvailablePart part)
        {
            if (part.partPrefab.Modules.OfType<RetractableLadder>().Any() && part.partPrefab.Modules.Count == 1)
                return true;
            if (part.name == "ladder1") // blasted radial ladder thing.
                return true;
            return false;
        }

        public static bool isLFLOxTank(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            if (hasLF(part) && hasLOx(part))
                return true;
            return false;
        }

        public static bool isLFLOxEngine(AvailablePart part)
        {
            bool LF = false, LOx = false;
            List<Propellant> propellants = new List<Propellant>();

            if (part.partPrefab.GetModuleEngines() != null)
            {
                propellants = part.partPrefab.GetModuleEngines().propellants;
            }
            else if (part.partPrefab.GetModuleEnginesFX() != null)
            {
                propellants = part.partPrefab.GetModuleEnginesFX().propellants;
            }

            foreach (Propellant p in propellants)
            {
                if (p.name == "LiquidFuel")
                    LF = true;
                else if (p.name == "Oxidizer")
                    LOx = true;
            }
            return LF && LOx;
        }

        public static bool isLFEngine(AvailablePart part)
        {
            bool LF = false, LOx = false;
            List<Propellant> propellants = new List<Propellant>();

            if (part.partPrefab.GetModuleEngines() != null)
            {
                propellants = part.partPrefab.GetModuleEngines().propellants;
            }
            else if (part.partPrefab.GetModuleEnginesFX() != null)
            {
                propellants = part.partPrefab.GetModuleEnginesFX().propellants;
            }

            foreach (Propellant p in propellants)
            {
                if (p.name == "LiquidFuel")
                    LF = true;
                else if (p.name == "Oxidizer")
                    LOx = true;
            }
            return LF && !LOx;
        }

        public static bool isSRB(AvailablePart part)
        {
            bool Solid = false;
            List<Propellant> propellants = new List<Propellant>();
            
            if (part.partPrefab.GetModuleEngines() != null)
            {
                propellants = part.partPrefab.GetModuleEngines().propellants;
            }
            else if (part.partPrefab.GetModuleEnginesFX() != null)
            {
                propellants = part.partPrefab.GetModuleEnginesFX().propellants;
            }

            foreach (Propellant p in propellants)
            {
                if (p.name == "SolidFuel")
                    Solid = true;
            }
            return Solid;
        }

        public static bool isIonEngine(AvailablePart part)
        {
            bool Ec = false, Xe = false;
            List<Propellant> propellants = new List<Propellant>();
            //Engine modules
            if (part.partPrefab.GetModuleEngines() != null)
            {
                propellants = part.partPrefab.GetModuleEngines().propellants;
            }
            else if (part.partPrefab.GetModuleEnginesFX() != null)
            {
                propellants = part.partPrefab.GetModuleEnginesFX().propellants;
            }
            // get type
            foreach (Propellant p in propellants)
            {
                if (p.name == "XenonGas")
                    Xe = true;
                else if (p.name == "ElectricCharge")
                    Ec = true;
            }
            return Xe && Ec;
        }

        public static bool isJetTank(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            if (hasLF(part) && !hasLOx(part))
                return true;
            return false;
        }

        public static bool isMonoPropTank(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            if (hasMonoProp(part))
                return true;
            return false;
        }

        public static bool isXenonTank(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            if (hasXenon(part))
                return true;
            return false;
        }

        public static bool isBattery(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            if (hasEc(part))
                return true;
            return false;
        }

        public static bool hasLF(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            foreach (PartResource resource in part.partPrefab.Resources)
            {
                if (resource.resourceName == "LiquidFuel")
                    return true;
            }
            return false;
        }

        public static bool hasLOx(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            foreach (PartResource resource in part.partPrefab.Resources)
            {
                if (resource.resourceName == "Oxidizer")
                    return true;
            }
            return false;
        }

        public static bool hasSolidFuel(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            foreach (PartResource resource in part.partPrefab.Resources)
            {
                if (resource.resourceName == "SolidFuel")
                    return true;
            }
            return false;
        }

        public static bool hasMonoProp(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            foreach (PartResource resource in part.partPrefab.Resources)
            {
                if (resource.resourceName == "MonoPropellant")
                    return true;
            }
            return false;
        }

        public static bool hasXenon(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            foreach (PartResource resource in part.partPrefab.Resources)
            {
                if (resource.resourceName == "XenonGas")
                    return true;
            }
            return false;
        }

        public static bool hasEc(AvailablePart part)
        {
            if (isCommand(part))
                return false;

            foreach (PartResource resource in part.partPrefab.Resources)
            {
                if (resource.resourceName == "ElectricCharge")
                    return true;
            }
            return false;
        }

        public static bool isNFECapacitor(AvailablePart part) // Near future capacitor
        {
            if (part.partPrefab.Modules.Contains("DischargeCapacitor") && part.partPrefab.Resources.Contains("StoredCharge"))
                return true;
            return false;
        }

        public static bool isNFEFuelDrum(AvailablePart part) // Near future nuclear fuel container
        {
            if (part.partPrefab.Modules.Contains("FissionContainer") && part.partPrefab.Resources.Contains("EnrichedUranium"))
                return true;
            return false;
        }

        public static bool isNFEFuelReprocessor(AvailablePart part) // Near future nuclear fuel reprocessor
        {
            if (part.partPrefab.Modules.Contains("FissionReprocessor"))
                return true;
            return false;
        }

        public static bool isNFERadiator(AvailablePart part) // Near future radiator
        {
            if (part.partPrefab.Modules.Contains("FissionRadiator"))
                return true;
            return false;
        }

        public static bool isNFEReactor(AvailablePart part) // Near future reactor
        {
            if (part.partPrefab.Modules.Contains("FissionGenerator") && part.partPrefab.Resources.Contains("EnrichedUranium") && hasEc(part))
                return true;
            return false;
        }

        public static bool hasNFEResArgon(AvailablePart part) // Near future resource Argon
        {
            if (part.partPrefab.Resources.Contains("ArgonGas"))
                return true;
            return false;
        }

        public static bool hasNFEResHydrogen(AvailablePart part) // Near future resource Hydrogen
        {
            if (part.partPrefab.Resources.Contains("LiquidHydrogen"))
                return true;
            return false;
        }

        public static bool isNFEArgonEngine(AvailablePart part) // Near future argon EC engine
        {
            if (part.partPrefab.Modules.Contains("VariablePowerEngine"))
                return false;

            bool Ec = false, Arg = false;
            List<Propellant> propellants = new List<Propellant>();
            //Engine modules
            if (part.partPrefab.GetModuleEngines() != null)
            {
                propellants = part.partPrefab.GetModuleEngines().propellants;
            }
            else if (part.partPrefab.GetModuleEnginesFX() != null)
            {
                propellants = part.partPrefab.GetModuleEnginesFX().propellants;
            }
            // get type
            foreach (Propellant p in propellants)
            {
                if (p.name == "ArgonGas")
                    Arg = true;
                else if (p.name == "ElectricCharge")
                    Ec = true;
            }

            return Arg && Ec;
        }

        public static bool isNFEVariableEngine(AvailablePart part) // Near future variable thrust argon EC engine
        {
            if (!part.partPrefab.Modules.Contains("VariablePowerEngine"))
                return false;
            
            bool Ec = false, Arg = false;
            List<Propellant> propellants = new List<Propellant>();
            //Engine modules
            if (part.partPrefab.GetModuleEngines() != null)
            {
                propellants = part.partPrefab.GetModuleEngines().propellants;
            }
            else if (part.partPrefab.GetModuleEnginesFX() != null)
            {
                propellants = part.partPrefab.GetModuleEnginesFX().propellants;
            }
            // get type
            foreach (Propellant p in propellants)
            {
                if (p.name == "ArgonGas")
                    Arg = true;
                else if (p.name == "ElectricCharge")
                    Ec = true;
            }

            return Arg && Ec;
        }

        public static bool isNFEVasmirEngine(AvailablePart part) // Near future vasmir engine. Runs off hydrogen or argon (variable ISP module seems to be unique)
        {
            if (part.partPrefab.Modules.Contains("VariableISPEngine"))
                return true;
            return false;
        }

        public static bool isNFECurvedPanel(AvailablePart part) // Near future curved solar panel
        {
            if (part.partPrefab.Modules.Contains("ModuleCurvedSolarPanel"))
                return true;
            return false;
        }
    }
}
