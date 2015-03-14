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
        public string type { get; set; } // procedural categories
        public string value { get; set; } // mod folder name for mod type categories
        public bool all { get; set; } // has an all parts subCategory
        public string[] subCategories { get; set; } // array of subcategories
        public bool stockCategory { get; set; } // editing a stock category or creating a completely new one

        private static readonly List<string> categoryNames = new List<string> { "Pods", "Engines", "Fuel Tanks", "Command and Control", "Structural", "Aerodynamics", "Utility", "Science" };

        public customCategory(ConfigNode node)
        {
            categoryName = node.GetValue("name");
            iconName = node.GetValue("icon");
            colour = convertToColor(node.GetValue("colour"));

            type = node.GetValue("type");
            value = node.GetValue("value");

            bool tmp;
            bool.TryParse(node.GetValue("all"), out tmp);
            this.all = tmp;
            
            bool.TryParse(node.GetValue("stock"), out tmp);
            this.stockCategory = tmp;
            
            ConfigNode subcategoryList = node.GetNode("SUBCATEGORIES", 0);
            if (subcategoryList != null)
            {
                string[] stringList = subcategoryList.GetValues();
                subCategories = new string[1000];
                for (int i = 0; i < stringList.Length; i++)
                {
                    string[] indexAndValue = stringList[i].Split(',');
                    if (indexAndValue.Length >= 2)
                    {
                        int index;
                        if (int.TryParse(indexAndValue[0], out index))
                            subCategories[index] = indexAndValue[1].Trim();
                    }
                }
                subCategories = subCategories.Distinct().ToArray(); // no duplicates and no gaps in a single function...
            }
        }

        public void initialise()
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                Core.Log("Category name is null or empty");
                return;
            }
            if (subCategories == null || subCategories.Length == 0)
            {
                Core.Log(categoryName + " has no subcategories");
                return;
            }

            PartCategorizer.Category category;
            if (!stockCategory)
            {
                PartCategorizer.Icon icon = Core.getIcon(iconName);
                if (icon == null)
                    icon = PartCategorizer.Instance.fallbackIcon;
                PartCategorizer.AddCustomFilter(categoryName, icon, colour);

                category = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == categoryName);
                category.displayType = EditorPartList.State.PartsList;
                category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;
            }
            else
            {
                category = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == categoryName);
            }
            
            for (int i = 0; i < subCategories.Length; i++)
            {
                if (!string.IsNullOrEmpty(subCategories[i]) && Core.Instance.subCategoriesDict.ContainsKey(subCategories[i]))
                {
                    customSubCategory sC = Core.Instance.subCategoriesDict[subCategories[i]];
                    try
                    {
                        sC.initialise(category);
                    }
                    catch (Exception ex)
                    {
                        // extended logging for errors
                        Core.Log(subCategories[i] + " failed to initialise");
                        Core.Log("Category:" + categoryName + ", filter:" + sC.hasFilters + ", Count:" + sC.filters.Count + ", Icon:" + Core.getIcon(sC.iconName));
                        Core.Log(ex.StackTrace);
                    }
                }
            }
        }

        private void typeSwitch()
        {
            switch (type)
            {
                case "engine":
                    generateEngineTypes();
                    return;
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
                    if (props != "")
                        props += ",";

                    checks.Add(new Check("propellant", s));
                    props += s;
                }
                checks.Add(new Check("propellant", props, true, false)); // exact match to propellant list. Nothing extra, nothing less

                customSubCategory sC = new customSubCategory(props, "stock_Engines");

                Filter f = new Filter(false);
                f.checks = checks;
                sC.filters.Add(f);

                if (Core.Instance.categoryAllSub.ContainsKey(categoryName))
                    Core.Instance.categoryAllSub[categoryName].filters.Add(f);
            }
        }

        public static Color convertToColor(string hex_ARGB)
        {
            if (string.IsNullOrEmpty(hex_ARGB))
                return Color.clear;

            hex_ARGB = hex_ARGB.Replace("#", "").Replace("0x", ""); // remove any hexadecimal identifiers
            if (System.Text.RegularExpressions.Regex.IsMatch(hex_ARGB, "[0-9a-fA-F]{6,8}")) // check it is valid hex
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

        public bool hasSubCategories()
        {
            return (this.subCategories != null && this.subCategories.Length > 0);
        }
    }
}
