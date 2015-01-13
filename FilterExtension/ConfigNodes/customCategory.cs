using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using Utility;

    public class customCategory
    {
        public string categoryName { get; set; }
        public string iconName { get; set; }
        public Color colour { get; set; }
        public string type { get; set; }
        public string value { get; set; } // mod folder name for mod type categories
        public bool all { get; set; }

        private static readonly List<string> categoryNames = new List<string> { "Pods", "Engines", "Fuel Tanks", "Command and Control", "Structural", "Aerodynamics", "Utility", "Science" };

        public customCategory(ConfigNode node)
        {
            categoryName = node.GetValue("name");
            if (string.IsNullOrEmpty(categoryName))
                categoryName = node.GetValue("title");

            iconName = node.GetValue("icon");
            colour = convertToColor(node.GetValue("colour"));
            
            type = node.GetValue("type");
            value = node.GetValue("value");

            bool tmp;
            bool.TryParse(node.GetValue("all"), out tmp);
            this.all = tmp;

            if (all)
            {
                if (!Core.Instance.categoryAllSub.ContainsKey(categoryName))
                    Core.Instance.categoryAllSub.Add(categoryName, new customSubCategory("All Parts in Category", categoryName, iconName));
            }
            typeSwitch();
        }

        public customCategory(string name, string icon, string colour, string type = "", string value = "", string all = "")
        {
            categoryName = name;
            iconName = icon;
            this.colour = convertToColor(colour);

            this.type = type;
            this.value = value;
            
            bool tmp;
            bool.TryParse(all, out tmp);
            this.all = tmp;

            if (this.all)
            {
                if (!Core.Instance.categoryAllSub.ContainsKey(categoryName))
                    Core.Instance.categoryAllSub.Add(categoryName, new customSubCategory("All Parts in Category", categoryName, iconName));
            }
            typeSwitch();
        }

        public void initialise()
        {
            if (categoryName == null)
                return;
            PartCategorizer.Icon icon = Core.getIcon(iconName);
            if (icon == null)
                icon = PartCategorizer.Instance.fallbackIcon;
            PartCategorizer.AddCustomFilter(categoryName, icon, colour);
            
            PartCategorizer.Category category = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == categoryName);
            category.displayType = EditorPartList.State.PartsList;
            category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;
        }

        private void typeSwitch()
        {
            switch (type)
            {
                case "mod":
                    generateModSubCategories();
                    return;
                case "engine":
                    generateEngineTypes();
                    return;
            }
        }

        private void generateModSubCategories()
        {
            foreach (string s in categoryNames)
            {
                Check ch1 = new Check("folder", value);
                Check ch2 = new Check("category", s);
                Filter f = new Filter(false);
                customSubCategory sC = new customSubCategory(s, categoryName, "stock_" + s);

                f.checks.Add(ch1);
                f.checks.Add(ch2);
                sC.filters.Add(f);

                Core.Instance.subCategories.Add(sC);

                if (Core.Instance.categoryAllSub.ContainsKey(categoryName))
                    Core.Instance.categoryAllSub[categoryName].filters.Add(f);
            }
        }

        private void generateEngineTypes()
        {
            foreach (List<string> ls in Core.propellantCombos)
            {
                List<Check> checks = new List<Check>();
                string props = "";
                foreach (string s in ls)
                {
                    checks.Add(new Check("propellant", s));
                    props += s + ",";
                }
                props = props.Substring(0, props.Length - 1);

                customSubCategory sC = new customSubCategory(props, this.categoryName, "stock_Engines");

                Filter f = new Filter(false);
                f.checks = checks;
                sC.filters.Add(f);

                Core.Instance.subCategories.Add(sC);

                if (Core.Instance.categoryAllSub.ContainsKey(categoryName))
                    Core.Instance.categoryAllSub[categoryName].filters.Add(f);
            }
        }

        public static Color convertToColor(string hex_ARGB)
        {
            hex_ARGB = hex_ARGB.Replace("#", "").Replace("0x", ""); // remove any hexadecimal identifiers
            if (System.Text.RegularExpressions.Regex.IsMatch(hex_ARGB, "[0-9a-fA-F]{6,8}"))
            {
                if (hex_ARGB.Length == 8)
                {
                    Color c = new Color();
                    c.a = (float)byte.Parse(hex_ARGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    c.r = (float)byte.Parse(hex_ARGB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    c.g = (float)byte.Parse(hex_ARGB.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    c.b = (float)byte.Parse(hex_ARGB.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    return c;
                }
                else if (hex_ARGB.Length == 6)
                {
                    Color c = new Color();
                    c.a = 1;
                    c.r = (float)byte.Parse(hex_ARGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    c.g = (float)byte.Parse(hex_ARGB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    c.b = (float)byte.Parse(hex_ARGB.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    return c;
                }
            }
            return Color.clear;
        }
    }
}
