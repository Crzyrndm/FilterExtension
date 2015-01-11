using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using Utility;

    public class Check
    {
        internal string type = ""; // type of check to perform (module, title/name, resource,...)
        internal string value = "";
        internal bool invert;

        internal Check(ConfigNode node)
        {
            type = node.GetValue("type");
            value = node.GetValue("value");
            bool.TryParse(node.GetValue("invert"), out invert);
        }

        internal bool checkPart(AvailablePart partToCheck)
        {
            bool result;
            switch (type)
            {
                case "moduleTitle": // check by module title
                    result = PartType.checkModuleTitle(partToCheck, value);
                    break;
                case "moduleName":
                    result = PartType.checkModuleName(partToCheck, value);
                    break;
                case "name": // check by part name (cfg name)
                    result = PartType.checkName(partToCheck, value);
                    break;
                case "title": // check by part title (in game name)
                    result = PartType.checkTitle(partToCheck, value);
                    break;
                case "resource": // check for a resource
                    result = PartType.checkResource(partToCheck, value);
                    break;
                case "propellant": // check for engine propellant
                    result = PartType.checkPropellant(partToCheck, value);
                    break;
                case "tech": // check by tech
                    result = PartType.checkTech(partToCheck, value);
                    break;
                case "manufacturer": // check by manufacturer
                    result = PartType.checkManufacturer(partToCheck, value);
                    break;
                case "folder": // check by mod root folder
                    result = PartType.checkFolder(partToCheck, value);
                    break;
                case "category":
                    result = PartType.checkCategory(partToCheck, value);
                    break;
                case "size": // check by largest stack node size
                    result = PartType.checkPartSize(partToCheck, value);
                    break;
                case "crew":
                    result = PartType.checkCrewCapacity(partToCheck, value);
                    break;
                case "custom": // for when things get tricky
                    result = PartType.checkCustom(partToCheck, value);
                    break;
                default:
                    result = false;
                    break;
            }

            if (invert)
                result = !result;
            return result;
        }

        public bool Equals(Check c2)
        {
            if (c2 == null)
                return false;
            if (this.type == c2.type && this.value == c2.value && this.invert == c2.invert)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.type.GetHashCode() * this.value.GetHashCode() * this.invert.GetHashCode();
        }
    }
}
