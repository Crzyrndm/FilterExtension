using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using KSP.UI.Screens;
    using Utility;

    public class CustomCategory : IEquatable<CustomCategory>, ICloneable
    {
        public enum CategoryType
        {
            New = 1, // new category
            Stock = 2, // modification to stock category
            Mod = 4, // modification to a mod cateogry
        }

        public enum CategoryBehaviour
        {
            Add = 1, // only add to existing categories
            Replace = 2, // wipe existing categories
            Engines = 4 // generate unique engine types
        }

        public string CategoryName { get; }
        public string IconName { get; }
        public Color Colour { get; }
        public CategoryType Type { get; }
        public CategoryBehaviour Behaviour { get; }
        public bool All { get; } // has an all parts subCategory
        public List<SubCategoryItem> SubCategories { get; } // array of subcategories
        public List<Filter> Templates { get; } // Checks to add to every Filter in a category with the template tag

        public CustomCategory(ConfigNode node)
        {
            string tmpStr = string.Empty;

            CategoryName = node.GetValue("name");
            IconName = node.GetValue("icon");
            Colour = GUIUtils.convertToColor(node.GetValue("colour"));

            ConfigNode[] filtNodes = node.GetNodes("FILTER");
            if (filtNodes == null)
                return;
            Templates = new List<Filter>();
            foreach (ConfigNode n in filtNodes)
                Templates.Add(new Filter(n));

            if (bool.TryParse(node.GetValue("all"), out bool tmpBool))
            {
                this.All = tmpBool;
            }

            ConfigNode[] subcategoryList = node.GetNodes("SUBCATEGORIES");
            SubCategories = new List<SubCategoryItem>();
            if (subcategoryList != null)
            {
                List<SubCategoryItem> unorderedSubCats = new List<SubCategoryItem>();
                List<string> stringList = new List<string>();
                for (int i = 0; i < subcategoryList.Length; i++)
                    stringList.AddRange(subcategoryList[i].GetValues());

                SubCategoryItem[] subs = new SubCategoryItem[1000];
                for (int i = 0; i < stringList.Count; i++)
                {
                    string[] indexAndValue = stringList[i].Split(',').Select(s => s.Trim()).ToArray();

                    SubCategoryItem newSubItem = new SubCategoryItem();
                    if (int.TryParse(indexAndValue[0], out int index)) // has position index
                    {
                        if (indexAndValue.Length >= 2)
                            newSubItem.SubcategoryName = indexAndValue[1];
                        if (string.IsNullOrEmpty(newSubItem.SubcategoryName))
                            continue;

                        if (indexAndValue.Length >= 3 && string.Equals(indexAndValue[2], "dont template", StringComparison.CurrentCultureIgnoreCase))
                            newSubItem.ApplyTemplate = false;
                        subs[index] = newSubItem;
                    }
                    else // no valid position index
                    {
                        newSubItem.SubcategoryName = indexAndValue[0];
                        if (string.IsNullOrEmpty(newSubItem.SubcategoryName))
                            continue;

                        if (indexAndValue.Length >= 2 && string.Equals(indexAndValue[1], "dont template", StringComparison.CurrentCultureIgnoreCase))
                            newSubItem.ApplyTemplate = false;
                        unorderedSubCats.Add(newSubItem);
                    }
                }
                SubCategories = subs.Distinct().ToList(); // no duplicates and no gaps in a single line. Yay
                SubCategories.AddUniqueRange(unorderedSubCats); // tack unordered subcats on to the end
                SubCategories.RemoveAll(s => s == null);
            }

            if (node.TryGetValue("type", ref tmpStr))
                tmpStr = tmpStr.ToLower();
            switch (tmpStr)
            {
                case "stock":
                    Type = CategoryType.Stock;
                    break;

                case "mod":
                    Type = CategoryType.Mod;
                    break;

                case "new":
                default:
                    Type = CategoryType.New;
                    break;
            }
            if (node.TryGetValue("value", ref tmpStr))
            {
                if (string.Equals(tmpStr, "replace", StringComparison.OrdinalIgnoreCase))
                    Behaviour = CategoryBehaviour.Replace;
                else if (string.Equals(tmpStr, "engine", StringComparison.OrdinalIgnoreCase))
                {
                    Behaviour = CategoryBehaviour.Engines;
                    foreach (List<string> combo in Core.Instance.propellantCombos)
                    {
                        string dummy = string.Empty, subcatName = string.Join(",", combo.ToArray());
                        Core.Instance.SetNameAndIcon(ref subcatName, ref dummy);
                        SubCategories.AddUnique(new SubCategoryItem(subcatName));
                    }
                }
                else
                    Behaviour = CategoryBehaviour.Add;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Initialise()
        {
            if (string.IsNullOrEmpty(CategoryName))
            {
                Core.Log("Category name is null or empty", Core.LogLevel.Warn);
                return;
            }
            if (!HasSubCategories())
            {
                Core.Log(CategoryName + " has no subcategories", Core.LogLevel.Warn);
                return;
            }
            PartCategorizer.Category category;
            if (Type == CategoryType.New)
            {
                RUI.Icons.Selectable.Icon icon = Core.GetIcon(IconName);
                PartCategorizer.AddCustomFilter(CategoryName, icon, Colour);

                category = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == CategoryName);
                category.displayType = EditorPartList.State.PartsList;
                category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;
            }
            else
            {
                if (!PartCategorizer.Instance.filters.TryGetValue(c => c.button.categoryName == CategoryName, out category))
                {
                    Core.Log("No category of this name was found to manipulate: " + CategoryName, Core.LogLevel.Warn);
                    return;
                }
                else if (Behaviour == CategoryBehaviour.Replace)
                {
                    if (category.button.activeButton.CurrentState == KSP.UI.UIRadioButton.State.True)
                    {
                        var subcat = category.subcategories.Find(c => c.button.activeButton.CurrentState == KSP.UI.UIRadioButton.State.True);
                        if (subcat != null)
                        {
                            subcat.OnFalseSUB(subcat);
                        }
                        PartCategorizer.Instance.scrollListSub.Clear(false);
                    }
                    category.subcategories.Clear();
                }
            }

            for (int i = 0; i < SubCategories.Count; i++)
                InitSubcategory(i, SubCategories[i], category);
        }

        public void InitSubcategory(int index, SubCategoryItem toInit, PartCategorizer.Category category)
        {
            if (toInit == null || string.IsNullOrEmpty(toInit.ToString()))
                return;

            if (Core.Instance.conflictsDict.TryGetValue(toInit.ToString(), out List<string> conflictsList)) // if we have a conflict with some other subcategory
            {
                for (int j = 0; j < index; ++j) // iterate over the subcategories we've already added to see if it's one of them
                {
                    if (conflictsList.Contains(SubCategories[j].SubcategoryName))
                    {
                        // if so, we skip this subcategory
                        Core.Log($"Filters duplicated in category {CategoryName} between subCategories: {toInit.SubcategoryName} and {SubCategories[j].SubcategoryName}", Core.LogLevel.Warn);
                        return;
                    }
                }
            }
            if (!Core.Instance.subCategoriesDict.TryGetValue(toInit.ToString(), out CustomSubCategory subcategory))
            {
                Core.Log($"subcategory {toInit.SubcategoryName} not found in subcategories Dictionary", Core.LogLevel.Warn);
                return;
            }

            CustomSubCategory sC = new CustomSubCategory(subcategory, toInit.ApplyTemplate ? this : null);

            try
            {
                if (sC.CheckSubCategoryHasParts(CategoryName))
                    sC.Initialise(category);
            }
            catch (Exception ex)
            {
                // extended logging for errors
                Core.Log($"{SubCategories[index]} failed to initialise"
                    + $"\r\nCategory: {CategoryName}"
                    + $"\r\nSubcategory: {sC.SubCategoryTitle}"
                    + $"\r\nFilter?: {sC.HasFilters}"
                    + $"\r\nFilter count: {sC.Filters.Count}"
                    + $"\r\nIcon: {Core.GetIcon(sC.IconName)}"
                    + $"\r\n{ex.Message}"
                    + $"\r\n{ex.StackTrace}",
                    Core.LogLevel.Error);
            }
        }

        public bool HasSubCategories()
        {
            return (SubCategories?.Count ?? 0) > 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((CustomCategory)obj);
        }

        public bool Equals(CustomCategory C)
        {
            if (ReferenceEquals(null, C))
                return false;
            if (ReferenceEquals(this, C))
                return true;

            return CategoryName.Equals(C.CategoryName);
        }

        public override int GetHashCode()
        {
            return CategoryName.GetHashCode();
        }
    }

    public class SubCategoryItem : IEquatable<SubCategoryItem>
    {
        public string SubcategoryName { get; set; }
        public bool ApplyTemplate { get; set; }

        public SubCategoryItem()
        {
            ApplyTemplate = true;
        }

        public SubCategoryItem(string name, bool useTemplate = true)
        {
            SubcategoryName = name;
            ApplyTemplate = useTemplate;
        }

        public bool Equals(SubCategoryItem sub)
        {
            if (ReferenceEquals(null, sub))
                return false;
            if (ReferenceEquals(this, sub))
                return true;

            return SubcategoryName.Equals(sub.SubcategoryName);
        }

        public override int GetHashCode()
        {
            return SubcategoryName.GetHashCode();
        }

        public override string ToString()
        {
            return SubcategoryName;
        }
    }
}