using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    using KSP.UI.Screens;
    public class customSubCategory : IEquatable<customSubCategory>, ICloneable
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

        public customSubCategory(customSubCategory subCat)
        {
            subCategoryTitle = subCat.subCategoryTitle;
            iconName = subCat.iconName;
            filters = new List<Filter>(subCat.filters.Count);
            subCat.filters.ForEach(f => filters.Add(new Filter(f)));

            template = new List<Filter>(subCat.template.Count);
            subCat.template.ForEach(f => template.Add(new Filter(f)));

            unPurchasedOverride = subCat.unPurchasedOverride;
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
            PartCategorizer.AddCustomSubcategoryFilter(cat, this.subCategoryTitle, icon, p => checkPartFilters(p));
        }

        /// <summary>
        /// used mostly for purpose of creating a deep copy
        /// </summary>
        /// <returns></returns>
        public ConfigNode toConfigNode()
        {
            ConfigNode node = new ConfigNode("SUBCATEGORY");

            node.AddValue("name", subCategoryTitle);
            node.AddValue("icon", iconName);
            node.AddValue("showUnpurchased", unPurchasedOverride);

            foreach (Filter f in this.filters)
                node.AddNode(f.toConfigNode());

            return node;
        }

        public object Clone()
        {
            return new customSubCategory(this);
        }

        /// <summary>
        /// called by subcategory check type, has depth limit protection
        /// </summary>
        /// <param name="part"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public bool checkPartFilters(AvailablePart part, int depth = 0)
        {
            if (Editor.blackListedParts != null)
            {
                if (part.category == PartCategories.none && Editor.blackListedParts.Contains(part.name))
                    return false;
            }
            if (!unPurchasedOverride && Settings.hideUnpurchased && !ResearchAndDevelopment.PartModelPurchased(part) && !ResearchAndDevelopment.IsExperimentalPart(part))
                return false;

            PartModuleFilter pmf;
            if (Core.Instance.filterModules.TryGetValue(part.name, out pmf))
            {
                if (pmf.CheckForForceAdd(subCategoryTitle))
                    return true;
                if (pmf.CheckForForceBlock(subCategoryTitle))
                    return false;
            }

            return checkTemplate(part, depth);
        }

        /// <summary>
        /// Go through the category template filters. If the template list is empty or the part matches one of the template filters, go on to check against the filters of this subcategory
        /// </summary>
        /// <param name="ap"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private bool checkTemplate(AvailablePart ap, int depth = 0)
        {
            if (template.Count == 0)
                return checkFilters(ap, depth);
            else
            {
                Filter t;
                for (int i = 0; i < template.Count; ++i)
                {
                    t = template[i];
                    if (t.filterResult(ap, depth))
                    {
                        return checkFilters(ap, depth);
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Go through the filters of this subcategory. If the filter list is empty or the part matches one of the filters we can accept that part into this subcategory
        /// Templates have already been checked at this point
        /// if there is no template or filter, hasFilters property will be false and this subcategory will be removed prior to this point
        /// </summary>
        /// <param name="ap"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private bool checkFilters(AvailablePart ap, int depth = 0)
        {
            if (filters.Count == 0)
                return true;
            else
            {
                Filter f;
                for (int i = 0; i < filters.Count; ++i)
                {
                    f = filters[i];
                    if (f.filterResult(ap, depth))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// if a subcategory doesn't have any parts, it shouldn't be used. Doesn't account for the blackListed parts the first time the editor is entered
        /// </summary>
        /// <param name="sC">the subcat to check</param>
        /// <param name="category">the category for logging purposes</param>
        /// <returns>true if the subcategory contains any parts</returns>
        public bool checkSubCategoryHasParts(string category)
        {
            PartModuleFilter pmf;
            AvailablePart p;
            for (int i = 0; i < PartLoader.Instance.parts.Count; i++)
            {
                pmf = null;
                p = PartLoader.Instance.parts[i];
                if (Core.Instance.filterModules.TryGetValue(p.name, out pmf))
                {
                    if (pmf.CheckForForceAdd(subCategoryTitle))
                        return true;
                    if (pmf.CheckForForceBlock(subCategoryTitle))
                        return false;
                }
                if (checkPartFilters(PartLoader.Instance.parts[i]))
                    return true;
            }

            if (Settings.debug)
            {
                if (!string.IsNullOrEmpty(category))
                    Core.Log(subCategoryTitle + " in category " + category + " has no valid parts and was not initialised");
                else
                    Core.Log(subCategoryTitle + " has no valid parts and was not initialised");
            }
            return false;
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
        public static bool checkForCheckMatch(customSubCategory subcategory, Check.CheckType type, string value, bool invert = false, bool contains = true, Check.Equality equality = Check.Equality.Equals)
        {
            for (int j = 0; j < subcategory.filters.Count; j++)
            {
                Filter f = subcategory.filters[j];
                for (int k = 0; k < f.checks.Count; k++)
                {
                    Check c = f.checks[k];
                    if (c.type.typeEnum == type && c.values.Contains(value) && c.values.Length == 1 && c.invert == invert && c.contains == contains && c.equality == equality)
                        return true;
                }
            }
            return false;
        }

        public bool Equals(customSubCategory sC2)
        {
            if (sC2 == null)
                return false;

            if (subCategoryTitle == sC2.subCategoryTitle)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return subCategoryTitle.GetHashCode();
        }
    }
}
