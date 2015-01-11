using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExtensions.Utility
{
    using ConfigNodes;

    public static class Constructors
    {
        public static ConfigNode newCategoryNode(string name, string icon, string colour, string type = "", string value = "", string all = "")
        {
            ConfigNode c = new ConfigNode("CATEGORY");

            c.AddValue("name", name);
            c.AddValue("icon", icon);
            c.AddValue("colour", colour);

            if (!string.IsNullOrEmpty(type))
            {
                c.AddValue("type", type);
                c.AddValue("value", value);
            }

            if (!string.IsNullOrEmpty(all))
                c.AddValue("all", all);

            return c;
        }

        public static customCategory newCategory(string name, string icon, string colour, string type = "", string value = "", string all = "")
        {
            return new customCategory(newCategoryNode(name, icon, colour, type, value, all));
        }

        public static ConfigNode newSubCategoryNode(string name, string category, string icon, List<ConfigNode> filters = null)
        {
            ConfigNode sC = new ConfigNode("SUBCATEGORY");

            sC.AddValue("name", name);
            sC.AddValue("category", category);
            sC.AddValue("icon", icon);

            foreach (ConfigNode node in filters)
                sC.AddNode(node);

            return sC;
        }

        public static customSubCategory newSubCategory(string name, string category, string icon)
        {
            return new customSubCategory(newSubCategoryNode(name, category, icon), category);
        }

        public static ConfigNode newFilterNode(bool invert, List<ConfigNode> checks = null)
        {
            ConfigNode f = new ConfigNode("FILTER");

            f.AddValue("invert", invert.ToString());

            foreach (ConfigNode node in checks)
                f.AddNode(node);

            return f;
        }

        public static Filter newFilter(bool invert)
        {
            return new Filter(newFilterNode(invert));
        }

        public static ConfigNode newCheckNode(string type, string value, bool invert = false)
        {
            ConfigNode c = new ConfigNode("CHECK");

            c.AddValue("type", type);
            c.AddValue("value", value);
            c.AddValue("invert", invert.ToString());

            return c;
        }

        public static Check newCheck(string type, string value, bool invert = false)
        {
            return new Check(newCheckNode(type, value, invert));
        }
    }
}
