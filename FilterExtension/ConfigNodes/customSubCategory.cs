using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    public class customSubCategory
    {
        public string subCategoryTitle { get; set; } // title of this subcategory
        public string iconName { get; set; } // default icon to use
        public List<Filter> filters { get; set; } // Filters are OR'd together (pass if it meets this filter, or this filter)
        public List<Filter> template { get; set; } // from the category, checked seperately
        public bool unPurchasedOverride { get; set; } // allow unpurchased parts to be visible even if the global setting hides them

        public bool hasFilters
        {
            get
            {
                return filters.Any() || template.Any();
            }
        }

        public customSubCategory(ConfigNode node)
        {
            subCategoryTitle = node.GetValue("name");
            iconName = node.GetValue("icon");

            bool tmp;
            bool.TryParse(node.GetValue("showUnpurchased"), out tmp);
            unPurchasedOverride = tmp;

            filters = new List<Filter>();
            foreach (ConfigNode subNode in node.GetNodes("FILTER"))
            {
                filters.Add(new Filter(subNode));
            }
            template = new List<Filter>();
        }

        public customSubCategory(string name, string icon)
        {
            filters = new List<Filter>();
            template = new List<Filter>();
            this.subCategoryTitle = name;
            this.iconName = icon;
        }

        /// <summary>
        /// called in the editor when creating the subcategory
        /// </summary>
        /// <param name="cat">The category to add this subcategory to</param>
        public void initialise(PartCategorizer.Category cat)
        {
            if (cat == null)
                return;
            RUI.Icons.Selectable.Icon icon = Core.getIcon(iconName);
            PartCategorizer.AddCustomSubcategoryFilter(cat, this.subCategoryTitle, icon, p => checkFilters(p));
        }

        /// <summary>
        /// used mostly for purpose of creating a deep copy
        /// </summary>
        /// <returns></returns>
        public ConfigNode toConfigNode()
        {
            ConfigNode node = new ConfigNode("SUBCATEGORY");

            node.AddValue("name", this.subCategoryTitle);
            node.AddValue("icon", this.iconName);
            node.AddValue("showUnpurchased", this.unPurchasedOverride);

            foreach (Filter f in this.filters)
                node.AddNode(f.toConfigNode());

            return node;
        }

        /// <summary>
        /// called by subcategory check type, has depth limit protection
        /// </summary>
        /// <param name="part"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public bool checkFilters(AvailablePart part, int depth = 0)
        {
            if (Editor.blackListedParts != null)
            {
                if (part.category == PartCategories.none && Editor.blackListedParts.Contains(part.name))
                    return false;
            }
            if (!unPurchasedOverride && Core.Instance.hideUnpurchased && !ResearchAndDevelopment.PartModelPurchased(part) && !ResearchAndDevelopment.IsExperimentalPart(part))
                return false;
            return ((!template.Any() || template.Any(t => t.checkFilter(part, depth))) && filters.Any(f => f.checkFilter(part, depth))); // part passed a template if present, and a subcategory filter
        }

        /// <summary>
        /// check to see if any checks in a subcategory match a given check
        /// </summary>
        /// <param name="subcategory"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="contains"></param>
        /// <param name="equality"></param>
        /// <param name="invert"></param>
        /// <returns>true if there is a matching check in the category</returns>
        public static bool checkForCheckMatch(customSubCategory subcategory, CheckType type, string value, bool invert = false, bool contains = true, Check.Equality equality = Check.Equality.Equals)
        {
            for (int j = 0; j < subcategory.filters.Count; j++)
            {
                Filter f = subcategory.filters[j];
                for (int k = 0; k < f.checks.Count; k++)
                {
                    Check c = f.checks[k];
                    if (c.type == type && c.value.Contains(value) && c.value.Length == 1 && c.invert == invert && c.contains == contains && c.equality == equality)
                        return true;
                }
            }
            return false;
        }

        public bool Equals(customSubCategory sC2)
        {
            if (sC2 == null)
                return false;

            if (this.subCategoryTitle == sC2.subCategoryTitle)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.subCategoryTitle.GetHashCode();
        }
    }
}
