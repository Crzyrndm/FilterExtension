using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterExtensions.ConfigNodes
{
    using FilterExtensions.ConfigNodes.Checks;
    using KSP.UI.Screens;

    public class CustomSubCategory : IEquatable<CustomSubCategory>, ICloneable
    {
        public string SubCategoryTitle { get; } // title of this subcategory
        public string IconName { get; } // default icon to use
        public List<Filter> Filters { get; } // Filters are OR'd together (pass if it meets this filter, or this filter)
        bool UnPurchasedOverride { get; } // allow unpurchased parts to be visible even if the global setting hides them
        CustomCategory Category { get; } = null;

        public bool HasFilters { get => (Filters?.Count ?? 0) > 0; }

        public CustomSubCategory(ConfigNode node)
        {
            SubCategoryTitle = node.GetValue("name");
            if (SubCategoryTitle == string.Empty)
            {
                SubCategoryTitle = node.GetValue("categoryName"); // for playing nice with stock generated subcats
            }
            IconName = node.GetValue("icon");

            bool.TryParse(node.GetValue("showUnpurchased"), out bool tmp);
            UnPurchasedOverride = tmp;

            Filters = new List<Filter>();
            foreach (ConfigNode subNode in node.GetNodes("FILTER"))
            {
                Filters.Add(new Filter(subNode));
            }
            foreach (ConfigNode subNode in node.GetNodes("PARTS"))
            {
                ConfigNode filtNode = new ConfigNode("FILTER");
                filtNode.AddNode(CheckFactory.MakeCheckNode(CheckName.ID, string.Join(",", subNode.GetValues("part"))));
                Filters.Add(new Filter(filtNode));
            }
        }

        public CustomSubCategory(CustomSubCategory cloneFrom, CustomCategory category)
        {
            SubCategoryTitle = cloneFrom.SubCategoryTitle;
            IconName = cloneFrom.IconName;
            Filters = cloneFrom.Filters;
            UnPurchasedOverride = cloneFrom.UnPurchasedOverride;
            Category = category;
        }

        /// <summary>
        /// called in the editor when creating the subcategory
        /// </summary>
        /// <param name="cat">The category to add this subcategory to</param>
        public void Initialise(PartCategorizer.Category cat)
        {
            if (cat == null)
                return;
            RUI.Icons.Selectable.Icon icon = Core.GetIcon(IconName);
            PartCategorizer.AddCustomSubcategoryFilter(cat, this.SubCategoryTitle, icon, p => CheckPartFilters(p));
        }

        /// <summary>
        /// used mostly for purpose of creating a deep copy
        /// </summary>
        /// <returns></returns>
        public ConfigNode ToConfigNode()
        {
            ConfigNode node = new ConfigNode("SUBCATEGORY");

            node.AddValue("name", SubCategoryTitle);
            node.AddValue("icon", IconName);
            node.AddValue("showUnpurchased", UnPurchasedOverride);
            foreach (Filter f in Filters)
                node.AddNode(f.ToConfigNode());
            return node;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// called by subcategory check type, has depth limit protection
        /// </summary>
        /// <param name="part"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public bool CheckPartFilters(AvailablePart part, int depth = 0)
        {
            if (Editor.blackListedParts != null)
            {
                if (part.category == PartCategories.none && Editor.blackListedParts.Contains(part.name))
                    return false;
            }
            if (!UnPurchasedOverride && HighLogic.CurrentGame.Parameters.CustomParams<FESettings>().hideUnpurchased && !(ResearchAndDevelopment.PartModelPurchased(part) || ResearchAndDevelopment.IsExperimentalPart(part)))
                return false;

            PartModuleFilter pmf = part.partPrefab.Modules.GetModule<PartModuleFilter>();
            if (pmf != null)
            {
                if (pmf.CheckForForceAdd(SubCategoryTitle))
                    return true;
                if (pmf.CheckForForceBlock(SubCategoryTitle))
                    return false;
            }

            return CheckFilters(part, depth);
        }

        /// <summary>
        /// Go through the filters of this subcategory. If the filter list is empty or the part matches one of the filters we can accept that part into this subcategory
        /// Templates have already been checked at this point
        /// if there is no template or filter, hasFilters property will be false and this subcategory will be removed prior to this point
        /// </summary>
        /// <param name="ap"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private bool CheckFilters(AvailablePart ap, int depth = 0)
        {
            if (Filters.Count == 0 && (Category == null || Category.Templates.Count == 0))
                return true;
            if (!CheckCategoryFilter(ap, depth))
                return false;
            foreach (var f in Filters)
            {
                if (f.FilterResult(ap, depth))
                    return true;
            }
            return false;
        }

        private bool CheckCategoryFilter(AvailablePart ap, int depth = 0)
        {
            if (Category == null)
                return true;
            foreach (var f in Filters)
            {
                if (f.FilterResult(ap, depth))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// if a subcategory doesn't have any parts, it shouldn't be used. Doesn't account for the blackListed parts the first time the editor is entered
        /// </summary>
        /// <param name="sC">the subcat to check</param>
        /// <param name="category">the category for logging purposes</param>
        /// <returns>true if the subcategory contains any parts</returns>
        public bool CheckSubCategoryHasParts(string category)
        {
            PartModuleFilter pmf;
            foreach (AvailablePart p in PartLoader.LoadedPartsList)
            {
                pmf = p.partPrefab.Modules.GetModule<PartModuleFilter>();
                if (pmf != null)
                {
                    if (pmf.CheckForForceAdd(SubCategoryTitle))
                        return true;
                    if (pmf.CheckForForceBlock(SubCategoryTitle))
                        return false;
                }
                if (CheckPartFilters(p))
                    return true;
            }

            if (HighLogic.CurrentGame.Parameters.CustomParams<FESettings>().debug)
            {
                if (!string.IsNullOrEmpty(category))
                    Core.Log(SubCategoryTitle + " in category " + category + " has no valid parts and was not initialised", Core.LogLevel.Warn);
                else
                    Core.Log(SubCategoryTitle + " has no valid parts and was not initialised", Core.LogLevel.Warn);
            }
            return false;
        }

        public bool Equals(CustomSubCategory sC2)
        {
            if (sC2 == null)
                return false;

            if (SubCategoryTitle == sC2.SubCategoryTitle)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return SubCategoryTitle.GetHashCode();
        }

        public static ConfigNode MakeSubcategoryNode(string name, string icon, List<ConfigNode> filters)
        {
            ConfigNode node = new ConfigNode("SUBCATEGORY");
            node.AddValue("name", name);
            node.AddValue("icon", icon);
            foreach (var f in filters)
            {
                node.AddNode(f);
            }
            return node;
        }
    }
}