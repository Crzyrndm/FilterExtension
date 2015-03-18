using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using Utility;

    public enum categoryTypeAndBehaviour
    {
        None,
        Engines,
        StockAdd,
        StockReplace
    }

    public class customCategory : IEquatable<customCategory>
    {
        public string categoryName { get; set; }
        public string iconName { get; set; }
        public Color colour { get; set; }
        public string type { get; set; } // procedural categories
        public string value { get; set; } // mod folder name for mod type categories
        public categoryTypeAndBehaviour behaviour { get; set; }
        public bool all { get; set; } // has an all parts subCategory
        public List<string> subCategories { get; set; } // array of subcategories
        public List<Check> template { get; set; } // Checks to add to every Filter in a category with the template tag

        private static readonly List<string> categoryNames = new List<string> { "Pods", "Engines", "Fuel Tanks", "Command and Control", "Structural", "Aerodynamics", "Utility", "Science" };

        public customCategory(ConfigNode node)
        {
            bool tmp;
            categoryName = node.GetValue("name");
            iconName = node.GetValue("icon");
            colour = convertToColor(node.GetValue("colour"));

            type = node.GetValue("type");
            value = node.GetValue("value");

            makeTemplate(node);

            bool.TryParse(node.GetValue("all"), out tmp);
            this.all = tmp;
            
            ConfigNode subcategoryList = node.GetNode("SUBCATEGORIES", 0);
            if (subcategoryList != null)
            {
                string[] stringList = subcategoryList.GetValues();
                string[] subs = new string[1000];
                for (int i = 0; i < stringList.Length; i++)
                {
                    string[] indexAndValue = stringList[i].Split(',');
                    if (indexAndValue.Length >= 2)
                    {
                        int index;
                        if (int.TryParse(indexAndValue[0], out index))
                            subs[index] = indexAndValue[1].Trim();
                    }
                }
                subCategories = subs.Distinct().ToList(); // no duplicates and no gaps in a single line. Yay
            }

            typeSwitch();
        }

        public void initialise()
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                Core.Log("Category name is null or empty");
                return;
            }
            if (!hasSubCategories())
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
                if (category == null)
                {
                    Core.Log("No Stock category of this name was found: " + categoryName);
                    return;
                }
            }
            
            for (int i = 0; i < subCategories.Count; i++)
            {
                if (!string.IsNullOrEmpty(subCategories[i]) && Core.Instance.subCategoriesDict.ContainsKey(subCategories[i]))
                {
                    if (Core.Instance.conflictsDict.ContainsKey(subCategories[i]))
                    {
                        List<string> conflicts = Core.Instance.conflictsDict[subCategories[i]].Intersect(subCategories).ToList();
                        
                        if (conflicts.Any(c => subCategories.IndexOf(c) < i))
                        {
                            string conflictList = "";
                            foreach (string s in conflicts)
                                conflictList += "\r\n" + s;
                            Core.Log("Filters duplicated in category " + this.categoryName + " between subCategories" + conflictList);
                            continue;
                        }
                    }

                    customSubCategory sC = new customSubCategory(Core.Instance.subCategoriesDict[subCategories[i]].toConfigNode());
                    if (template != null && template.Any())
                    {
                        foreach (Filter f in sC.filters)
                        {
                            f.checks.AddRange(template);
                        }
                    }

                    try
                    {
                        if (Core.checkSubCategoryHasParts(sC))
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
                    behaviour = categoryTypeAndBehaviour.Engines;
                    break;
                case "stock":
                    if (value == "replace")
                        behaviour = categoryTypeAndBehaviour.StockReplace;
                    else
                        behaviour = categoryTypeAndBehaviour.StockAdd;
                    break;
                default:
                    behaviour = categoryTypeAndBehaviour.None;
                    break;
            }
        }

        private void generateEngineTypes()
        {
            List<string> engines = new List<string>();
            foreach (List<string> ls in Core.Instance.propellantCombos)
            {
                List<Check> checks = new List<Check>();
                string props = "";
                foreach (string s in ls)
                {
                    if (props != "")
                        props += ",";
                    props += s;
                }
                checks.Add(new Check("propellant", props));
                checks.Add(new Check("propellant", props, true, false)); // exact match to propellant list. Nothing extra, nothing less

                string name = props.Replace(',', '/');
                string icon = props;
                if (Core.Instance.proceduralNames.ContainsKey(name))
                    name = Core.Instance.proceduralNames[name];
                if (Core.Instance.proceduralIcons.ContainsKey(name))
                    icon = Core.Instance.proceduralIcons[name];

                if (!Core.Instance.subCategoriesDict.ContainsKey(name))
                {
                    customSubCategory sC = new customSubCategory(name, icon);

                    Filter f = new Filter(false);
                    f.checks = checks;
                    sC.filters.Add(f);
                    Core.Instance.subCategoriesDict.Add(name, sC);
                }
                engines.Add(name);
            }
            subCategories.AddUniqueRange(engines);
        }

        private void makeTemplate(ConfigNode node)
        {
            ConfigNode filtNode = node.GetNode("FILTER");
            if (filtNode == null)
                return;

            Filter filter = new Filter(filtNode);
            this.template = filter.checks;
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
            return (this.subCategories != null && this.subCategories.Any());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((customCategory)obj);
        }

        public bool Equals(customCategory C)
        {
            if (ReferenceEquals(null, C))
                return false;
            if (ReferenceEquals(this, C))
                return true;

            return categoryName.Equals(C.categoryName);
        }

        public override int GetHashCode()
        {
            return categoryName.GetHashCode();
        }

        public bool stockCategory
        {
            get
            {
                return this.behaviour == categoryTypeAndBehaviour.StockAdd || this.behaviour == categoryTypeAndBehaviour.StockReplace;
            }
        }
    }
}
