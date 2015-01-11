using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using Utility;

    public class customCategory
    {
        internal string categoryName;
        internal string iconName;
        internal Color colour;
        internal string type;
        internal string value; // mod folder name for mod type categories
        internal bool all;

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

            if (bool.TryParse(node.GetValue("all"), out all))
            {
                if (!Core.Instance.categoryAllSub.ContainsKey(categoryName))
                    Core.Instance.categoryAllSub.Add(categoryName, Constructors.newSubCategory("All Parts in Category", categoryName, iconName));
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
                case "engine": // not hooked up yet
                    return;
            }
        }


        private void generateModSubCategories()
        {
            foreach (string s in categoryNames)
            {
                Check ch1 = Constructors.newCheck("folder", value);
                Check ch2 = Constructors.newCheck("category", s);
                Filter f = Constructors.newFilter(false);
                customSubCategory sC = Constructors.newSubCategory(s, categoryName, "stock_" + s);

                f.checks.Add(ch1);
                f.checks.Add(ch2);
                sC.filters.Add(f);

                Core.Instance.subCategories.Add(sC);
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
