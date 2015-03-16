using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterCreator
{
    public static class cfgConstructors
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

        public static ConfigNode newSubCategoryNode(string name, string category, string icon)
        {
            ConfigNode sC = new ConfigNode("SUBCATEGORY");

            sC.AddValue("name", name);
            sC.AddValue("category", category);
            sC.AddValue("icon", icon);

            return sC;
        }

        public static ConfigNode newSubCategoryNode(string name, string category, string icon, List<ConfigNode> filters)
        {
            ConfigNode sC = new ConfigNode("SUBCATEGORY");

            sC.AddValue("name", name);
            sC.AddValue("category", category);
            sC.AddValue("icon", icon);

            if (filters != null)
            {
                foreach (ConfigNode node in filters)
                    sC.AddNode(node);
            }

            return sC;
        }

        public static ConfigNode newFilterNode(bool invert, List<ConfigNode> checks)
        {
            ConfigNode f = new ConfigNode("FILTER");

            f.AddValue("invert", invert.ToString());

            if (checks != null)
            {
                foreach (ConfigNode node in checks)
                    f.AddNode(node);
            }

            return f;
        }

        public static ConfigNode newFilterNode(bool invert)
        {
            ConfigNode f = new ConfigNode("FILTER");

            f.AddValue("invert", invert.ToString());

            return f;
        }

        public static ConfigNode newCheckNode(string type, string value, bool invert = false)
        {
            ConfigNode c = new ConfigNode("CHECK");

            c.AddValue("type", type);
            c.AddValue("value", value);
            c.AddValue("invert", invert.ToString());

            return c;
        }
    }
}
