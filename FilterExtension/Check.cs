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
        internal bool pass;

        internal Check(ConfigNode node)
        {
            type = node.GetValue("type");
            value = node.GetValue("value");
            pass = bool.Parse(node.GetValue("pass"));
            if (pass == null)
                pass = true;
        }

        internal bool checkPart(AvailablePart partToCheck)
        {
            bool result;
            switch (type)
            {
                case "moduleTitle": // check by module title
                    result = PartType.checkModule(partToCheck, value);
                    return (result && pass) || !(result || pass);
                case "name": // check by part name (cfg name)
                    result = PartType.checkName(partToCheck, value);
                    return (result && pass) || !(result || pass);
                case "title": // check by part title (in game name)
                    result = PartType.checkTitle(partToCheck, value);
                    return (result && pass) || !(result || pass);
                case "resource": // check for a resource
                    result = PartType.checkResource(partToCheck, value);
                    return (result && pass) || !(result || pass);
                case "tech": // check by tech
                    result = PartType.checkTech(partToCheck, value);
                    return (result && pass) || !(result || pass);
                case "manufacturer": // check by manufacturer
                    result = PartType.checkManufacturer(partToCheck, value);
                    return (result && pass) || !(result || pass);
                case "folder": // check by mod root folder
                    result = PartType.checkFolder(partToCheck, value);
                    return (result && pass) || !(result || pass);
                case "category":
                    result = PartType.checkCategory(partToCheck, value);
                    return (result && pass) || !(result || pass);
                case "custom": // filters using PartType class
                    result = PartType.checkCustom(partToCheck, value);
                    return (result && pass) || !(result || pass);
                default:
                    return false;
            }
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
