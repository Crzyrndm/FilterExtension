using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using KSP.UI.Screens;
    using Utility;

    public class customCategory : IEquatable<customCategory>, ICloneable
    {
        public enum categoryType
        {
            New = 1, // new category
            Stock = 2, // modification to stock category
            Mod = 4, // modification to a mod cateogry
        }

        public enum categoryBehaviour
        {
            Add = 1, // only add to existing categories
            Replace = 2, // wipe existing categories
            Engines = 4 // generate unique engine types
        }

        public string categoryName { get; set; }
        public string iconName { get; set; }
        public Color colour { get; set; }
        public categoryType type { get; set; }
        public categoryBehaviour behaviour { get; set; }
        public bool all { get; set; } // has an all parts subCategory
        public List<subCategoryItem> subCategories { get; set; } // array of subcategories
        public List<Filter> templates { get; set; } // Checks to add to every Filter in a category with the template tag

        public customCategory(ConfigNode node)
        {
            bool tmpBool;
            string tmpStr = string.Empty;

            categoryName = node.GetValue("name");
            iconName = node.GetValue("icon");
            colour = GUIUtils.convertToColor(node.GetValue("colour"));

            makeTemplate(node);

            bool.TryParse(node.GetValue("all"), out tmpBool);
            this.all = tmpBool;
            
            ConfigNode[] subcategoryList = node.GetNodes("SUBCATEGORIES");
            subCategories = new List<subCategoryItem>();
            if (subcategoryList != null)
            {
                List<subCategoryItem> unorderedSubCats = new List<subCategoryItem>();
                List<string> stringList = new List<string>();
                for (int i = 0; i < subcategoryList.Length; i++)
                    stringList.AddRange(subcategoryList[i].GetValues());
                
                subCategoryItem[] subs = new subCategoryItem[1000];
                for (int i = 0; i < stringList.Count; i++)
                {
                    string[] indexAndValue = stringList[i].Split(',').Select(s => s.Trim()).ToArray();

                    subCategoryItem newSubItem = new subCategoryItem();
                    int index;
                    if (int.TryParse(indexAndValue[0], out index)) // has position index
                    {
                        if (indexAndValue.Length >= 2)
                            newSubItem.subcategoryName = indexAndValue[1];
                        if (string.IsNullOrEmpty(newSubItem.subcategoryName))
                            continue;

                        if (indexAndValue.Length >= 3 && string.Equals(indexAndValue[2], "dont template", StringComparison.CurrentCultureIgnoreCase))
                            newSubItem.applyTemplate = false;
                        subs[index] = newSubItem;
                    }
                    else // no valid position index
                    {
                        newSubItem.subcategoryName = indexAndValue[0];
                        if (string.IsNullOrEmpty(newSubItem.subcategoryName))
                            continue;

                        if (indexAndValue.Length >= 2 && string.Equals(indexAndValue[1], "dont template", StringComparison.CurrentCultureIgnoreCase))
                            newSubItem.applyTemplate = false;
                        unorderedSubCats.Add(newSubItem);
                    }
                }
                subCategories = subs.Distinct().ToList(); // no duplicates and no gaps in a single line. Yay
                subCategories.AddUniqueRange(unorderedSubCats); // tack unordered subcats on to the end
                subCategories.RemoveAll(s => s == null);
            }

            if (node.TryGetValue("type", ref tmpStr))
                tmpStr = tmpStr.ToLower();
            switch (tmpStr)
            {
                case "stock":
                    type = categoryType.Stock;
                    break;
                case "mod":
                    type = categoryType.Mod;
                    break;
                case "new":
                default:
                    type = categoryType.New;
                    break;
            }
            if (node.TryGetValue("value", ref tmpStr))
            {
                if (string.Equals(tmpStr, "replace", StringComparison.OrdinalIgnoreCase))
                    behaviour = categoryBehaviour.Replace;
                else if (string.Equals(tmpStr, "engine", StringComparison.OrdinalIgnoreCase))
                {
                    behaviour = categoryBehaviour.Engines;
                    foreach (List<string> combo in Core.Instance.propellantCombos)
                    {
                        string dummy = string.Empty, subcatName = string.Join(",", combo.ToArray());
                        Core.Instance.SetNameAndIcon(ref subcatName, ref dummy);
                        subCategories.AddUnique(new subCategoryItem(subcatName));
                    }
                }
                else
                    behaviour = categoryBehaviour.Add;
            }
        }

        public customCategory(customCategory c)
        {
            categoryName = c.categoryName;
            iconName = c.iconName;
            colour = c.colour;
            type = c.type;
            behaviour = c.behaviour;
            all = c.all;

            subCategories = new List<subCategoryItem>();
            for (int i = 0; i < c.subCategories.Count; ++i)
                subCategories.Add(new subCategoryItem(c.subCategories[i].subcategoryName, c.subCategories[i].applyTemplate));

            templates = new List<Filter>();
            for (int i = 0; i < c.templates.Count; ++i)
                templates.Add(new Filter(c.templates[i]));
        }

        public object Clone()
        {
            return new customCategory(this);
        }

        public void initialise()
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                Core.Log("Category name is null or empty", Core.LogLevel.Warn);
                return;
            }
            if (!hasSubCategories())
            {
                Core.Log(categoryName + " has no subcategories", Core.LogLevel.Warn);
                return;
            }
            PartCategorizer.Category category;
            if (type == categoryType.New)
            {
                RUI.Icons.Selectable.Icon icon = Core.getIcon(iconName);
                PartCategorizer.AddCustomFilter(categoryName, icon, colour);

                category = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == categoryName);
                category.displayType = EditorPartList.State.PartsList;
                category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;
            }
            else 
            {
                if (!PartCategorizer.Instance.filters.TryGetValue(c => c.button.categoryName == categoryName, out category))
                {
                    Core.Log("No category of this name was found to manipulate: " + categoryName, Core.LogLevel.Warn);
                    return;
                }
                else if (behaviour == categoryBehaviour.Replace)
                {
                    for (int i = category.subcategories.Count - 1; i >= 0; --i)
                    {
                        category.subcategories[i].DeleteSubcategory();
                    }
                }
            }

            for (int i = 0; i < subCategories.Count; i++)
                initSubcategory(i, subCategories[i], category);


        }

        public void initSubcategory(int index, subCategoryItem toInit, PartCategorizer.Category category)
        {
            if (toInit == null || string.IsNullOrEmpty(toInit.ToString()))
                return;

            List<string> conflictsList;
            if (Core.Instance.conflictsDict.TryGetValue(toInit.ToString(), out conflictsList)) // if we have a conflict with some other subcategory
            {
                for (int j = 0; j < index; ++j) // iterate over the subcategories we've already added to see if it's one of them
                {
                    if (conflictsList.Contains(subCategories[j].subcategoryName))
                    {
                        // if so, we skip this subcategory
                        Core.Log($"Filters duplicated in category {categoryName} between subCategories: {toInit.subcategoryName} and {subCategories[j].subcategoryName}", Core.LogLevel.Warn);
                        return;
                    }
                }
            }
            customSubCategory subcategory = null;
            if (!Core.Instance.subCategoriesDict.TryGetValue(toInit.ToString(), out subcategory))
            {
                Core.Log($"subcategory {toInit.subcategoryName} not found in subcategories Dictionary", Core.LogLevel.Warn);
                return;
            }

            customSubCategory sC = new customSubCategory(subcategory);
            if (toInit.applyTemplate)
                sC.template = templates;

            try
            {
                if (sC.checkSubCategoryHasParts(categoryName))
                    sC.initialise(category);
            }
            catch (Exception ex)
            {
                // extended logging for errors
                Core.Log($"{subCategories[index]} failed to initialise\r\nCategory: {categoryName}, Subcategory: {sC.subCategoryTitle}, filter?: {sC.hasFilters}, filter count: {sC.filters.Count}, Icon: {Core.getIcon(sC.iconName)}\r\n{ex.Message}\r\n{ex.StackTrace}",
                            Core.LogLevel.Error);
            }
        }

        private void makeTemplate(ConfigNode node)
        {
            ConfigNode[] filtNodes = node.GetNodes("FILTER");
            if (filtNodes == null)
                return;
            templates = new List<Filter>();
            foreach (ConfigNode n in filtNodes)
                templates.Add(new Filter(n));
        }

        public bool hasSubCategories()
        {
            return (subCategories?.Count ?? 0) > 0;
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
    }

    public class subCategoryItem : IEquatable<subCategoryItem>
    {
        public string subcategoryName { get; set; }
        public bool applyTemplate { get; set; }

        public subCategoryItem()
        {
            applyTemplate = true;
        }
        public subCategoryItem(string name, bool useTemplate = true)
        {
            subcategoryName = name;
            applyTemplate = useTemplate;
        }

        public bool Equals(subCategoryItem sub)
        {
            if (ReferenceEquals(null, sub))
                return false;
            if (ReferenceEquals(this, sub))
                return true;

            return subcategoryName.Equals(sub.subcategoryName);
        }

        public override int GetHashCode()
        {
            return subcategoryName.GetHashCode();
        }

        public override string ToString()
        {
            return subcategoryName;
        }
    }
}
