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
            NEW = 0, // new category
            STOCK, // modification to stock category
            MOD // modification to a mod cateogry
        }

        public enum CategoryBehaviour
        {
            Add = 0, // only add to existing categories
            Replace, // wipe existing categories
            Engines // generate unique engine types
        }

        public string CategoryName { get; }
        public string IconName { get; }
        public Color Colour { get; }
        public CategoryType Type { get; } = CategoryType.NEW;
        public CategoryBehaviour Behaviour { get; } = CategoryBehaviour.Add;
        public bool All { get; } = false; // has an all parts subCategory
        public List<SubCategoryItem> SubCategories { get; } = new List<SubCategoryItem>(); // array of subcategories
        public List<FilterNode> Templates { get; } // Checks to add to every Filter in a category with the template tag

        public CategoryNode(ConfigNode node, LoadAndProcess data)
        {
            CategoryName = node.GetValue("name");
            IconName = node.GetValue("icon");
            Colour = GUIUtils.ConvertToColor(node.GetValue("colour"));

            ConfigNode[] filtNodes = node.GetNodes("FILTER");
            if (filtNodes != null)
            {
                Templates = new List<FilterNode>();
                foreach (ConfigNode n in filtNodes)
                {
                    Templates.Add(new FilterNode(n));
                }
            }
            if (bool.TryParse(node.GetValue("all"), out bool tmpBool))
            {
                All = tmpBool;
            }
            LoadSubcategoryItems(node.GetNodes("SUBCATEGORIES"), SubCategories);
            string tmpStr = string.Empty;
            if (node.TryGetValue("type", ref tmpStr))
            {
                try
                {
                    Type = (CategoryType)Enum.Parse(typeof(CategoryType), tmpStr.ToUpperInvariant());
                }
                catch {} // leave as default
            }
            if (node.TryGetValue("value", ref tmpStr))
            {
                if (string.Equals(tmpStr, "replace", StringComparison.OrdinalIgnoreCase))
                {
                    Behaviour = CategoryBehaviour.Replace;
                }
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
            }
        }

        public bool HasSubCategories()
        {
            return (SubCategories?.Count ?? 0) > 0;
        }

        public static void LoadSubcategoryItems(ConfigNode[] nodes, List<SubCategoryItem> loadTo)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return;
            }
            var stringList = new List<string>();
            foreach (ConfigNode node in nodes)
            {
                stringList.AddUniqueRange(node.GetValues());
            }
            SubCategoryItem[] subs = new SubCategoryItem[1000];
            var unorderedSubCats = new List<SubCategoryItem>();
            foreach (string s in stringList)
            {
                int splitIndex = s.IndexOf(',');
                if (splitIndex == -1 || splitIndex == s.Length) // just the category name
                {
                    unorderedSubCats.AddUnique(new SubCategoryItem(s));
                }
                else // atleast two items
                {
                    string firstSplit = s.Substring(0, splitIndex);
                    if (!int.TryParse(s.Substring(0, splitIndex), out int index))  // name + "dont template"
                    {
                        unorderedSubCats.AddUnique(new SubCategoryItem(s.Substring(0, splitIndex),
                                !string.Equals(s.Substring(splitIndex, s.Length - splitIndex - 1), "dont template", StringComparison.OrdinalIgnoreCase)));
                    }
                    else // has position index
                    {
                        int lastSplitIndex = s.LastIndexOf(',');
                        if (lastSplitIndex == splitIndex) // only 2 items, index + cat name
                        {
                            subs[index] = new SubCategoryItem(s.Substring(splitIndex + 1));
                        }
                        else // three items, index + name + "dont template"
                        {
                            subs[index] = new SubCategoryItem(s.Substring(splitIndex + 1, lastSplitIndex - splitIndex - 1),
                                !string.Equals(s.Substring(lastSplitIndex + 1), "dont template", StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
            }
            loadTo = subs.Distinct().ToList(); // no duplicates and no gaps in a single line. Yay
            loadTo.AddUniqueRange(unorderedSubCats); // tack unordered subcats on to the end
            loadTo.RemoveAll(s => s == null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((CategoryNode)obj);
        }

        public bool Equals(CategoryNode C)
        {
            if (ReferenceEquals(null, C))
            {
                return false;
            }
            if (ReferenceEquals(this, C))
            {
                return true;
            }
            return CategoryName.Equals(C.CategoryName);
        }

        public override int GetHashCode()
        {
            return CategoryName.GetHashCode();
        }
    }
}