using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    using Categoriser;

    internal class Check
    {
        internal string type = ""; // type of check to perform (module, title/name, resource,...)
        internal string value = "";
        internal bool pass = true;

        internal Check(ConfigNode node)
        {
            type = node.GetValue("type");
            value = node.GetValue("value");
            pass = bool.Parse(node.GetValue("pass"));
        }

        internal bool checkPart(AvailablePart partToCheck)
        {
            switch (type)
            {
                case "moduleTitle": // check by module title
                    return checkModule(partToCheck);
                case "name": // check by part name (cfg name)
                    return checkName(partToCheck);
                case "title": // check by part title (in game name)
                    return checkTitle(partToCheck);
                case "resource": // check for a resource
                    return checkResource(partToCheck);
                case "tech": // check by tech
                    return checkTech(partToCheck);
                case "manufacturer": // check by manufacturer
                    return checkManufacturer(partToCheck);
                case "custom": // filters using PartType class
                    return checkCustom(partToCheck);
                default:
                    return false;
            }
        }

        private bool checkModule(AvailablePart part)
        {
            bool moduleCheck = part.moduleInfos.Any(m => m.moduleName == value);

            return (moduleCheck && pass) || !(moduleCheck || pass);
        }

        private bool checkName(AvailablePart part)
        {
            bool nameCheck = part.name == value;

            return (nameCheck && pass) || !(nameCheck || pass);
        }

        private bool checkTitle(AvailablePart part)
        {
            bool titleCheck = part.title.Contains(value);

            return (titleCheck && pass) || !(titleCheck || pass);
        }

        private bool checkResource(AvailablePart part)
        {
            bool resourceCheck = part.resourceInfos.Any(r => r.resourceName == value);

            return (resourceCheck && pass) || !(resourceCheck || pass);
        }

        private bool checkTech(AvailablePart part)
        {
            bool techCheck = part.TechRequired == value;

            return (techCheck && pass) || !(techCheck || pass);
        }

        private bool checkManufacturer(AvailablePart part)
        {
            bool manuCheck = part.manufacturer == value;

            return (manuCheck && pass) || !(manuCheck || pass);
        }

        private bool checkCustom(AvailablePart part)
        {
            bool val;
            switch (value)
            {
                case "isEngine":
                    val = PartType.isEngine(part);
                    break;
                case "isCommand":
                    val = PartType.isCommand(part);
                    break;
                case "LFLOx Engine":
                    val = PartType.isLFLOxEngine(part);
                    break;
                case "LF Engine":
                    val = PartType.isLFEngine(part);
                    break;
                case "adapter":
                    val = PartType.isAdapter(part);
                    break;
                case "Xenon Engine":
                    val = PartType.isIonEngine(part);
                    break;
                default:
                    val = false;
                    break;
            }
            return (val && pass) || (!val && !pass);
        }
    }

    internal class CheckEqualityComparer : IEqualityComparer<Check>
    {
        public bool Equals(Check c1, Check c2)
        {
            if (c1.type == c2.type && c1.value == c2.value && c1.pass == c2.pass)
                return true;
            else
                return false;
        }

        public int GetHashCode(Check c)
        {
            return c.type.GetHashCode() + c.value.GetHashCode() + c.pass.GetHashCode();
        }
    }
}
