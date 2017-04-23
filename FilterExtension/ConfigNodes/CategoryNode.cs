using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using Utility;

    public class CategoryNode : IEquatable<CategoryNode>
    {
        public enum CategoryType
        {
            New = 0, // new category
            Stock, // modification to stock category
            Mod // modification to a mod cateogry
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
        public List<FilterNode> Templates { get; } // Checks to add to every Filter in a category with the template tag

        public CategoryNode(ConfigNode node, LoadAndProcess data)
        {
            string tmpStr = string.Empty;

            CategoryName = node.GetValue("name");
            IconName = node.GetValue("icon");
            Colour = GUIUtils.convertToColor(node.GetValue("colour"));

            ConfigNode[] filtNodes = node.GetNodes("FILTER");
            if (filtNodes == null)
                return;
            Templates = new List<FilterNode>();
            foreach (ConfigNode n in filtNodes)
                Templates.Add(new FilterNode(n));

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
                    foreach (List<string> combo in data.propellantCombos)
                    {
                        string dummy = string.Empty, subcatName = string.Join(",", combo.ToArray());
                        data.SetNameAndIcon(ref subcatName, ref dummy);
                        SubCategories.AddUnique(new SubCategoryItem(subcatName));
                    }
                }
                else
                    Behaviour = CategoryBehaviour.Add;
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
            return Equals((CategoryNode)obj);
        }

        public bool Equals(CategoryNode C)
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
}