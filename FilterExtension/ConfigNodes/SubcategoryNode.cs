using System;
using System.Collections.Generic;

namespace FilterExtensions.ConfigNodes
{
    using FilterExtensions.ConfigNodes.CheckNodes;

    public class SubcategoryNode : IEquatable<SubcategoryNode>
    {
        public string SubCategoryTitle { get; } // title of this subcategory
        public string IconName { get; } // default icon to use
        public List<FilterNode> Filters { get; } // Filters are OR'd together (pass if it meets this filter, or this filter)
        public bool UnPurchasedOverride { get; } // allow unpurchased parts to be visible even if the global setting hides them
        CategoryNode Category { get; } = null;

        public bool HasFilters { get => (Filters?.Count ?? 0) > 0; }

        public SubcategoryNode(ConfigNode node)
        {
            SubCategoryTitle = node.GetValue("name");
            if (SubCategoryTitle == string.Empty)
            {
                SubCategoryTitle = node.GetValue("categoryName"); // for playing nice with stock generated subcats
            }
            IconName = node.GetValue("icon");

            bool.TryParse(node.GetValue("showUnpurchased"), out bool tmp);
            UnPurchasedOverride = tmp;

            Filters = new List<FilterNode>();
            foreach (ConfigNode subNode in node.GetNodes("FILTER"))
            {
                Filters.Add(new FilterNode(subNode));
            }
            foreach (ConfigNode subNode in node.GetNodes("PARTS"))
            {
                var filtNode = new ConfigNode("FILTER");
                filtNode.AddNode(CheckNodeFactory.MakeCheckNode(CheckName.ID, string.Join(",", subNode.GetValues("part"))));
                Filters.Add(new FilterNode(filtNode));
            }
        }

        public SubcategoryNode(SubcategoryNode cloneFrom, CategoryNode category)
        {
            SubCategoryTitle = cloneFrom.SubCategoryTitle;
            IconName = cloneFrom.IconName;
            Filters = cloneFrom.Filters;
            UnPurchasedOverride = cloneFrom.UnPurchasedOverride;
            Category = category;
        }

        /// <summary>
        /// used mostly for purpose of creating a deep copy
        /// </summary>
        /// <returns></returns>
        public ConfigNode ToConfigNode()
        {
            var node = new ConfigNode("SUBCATEGORY");

            node.AddValue("name", SubCategoryTitle);
            node.AddValue("icon", IconName);
            node.AddValue("showUnpurchased", UnPurchasedOverride);
            foreach (FilterNode f in Filters)
            {
                node.AddNode(f.ToConfigNode());
            }
            return node;
        }

        /// <summary>
        /// make confignode from params...
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static ConfigNode MakeSubcategoryNode(string name, string icon, bool unpurchasedVisible, List<ConfigNode> filters)
        {
            var node = new ConfigNode("SUBCATEGORY");
            node.AddValue("name", name);
            node.AddValue("icon", icon);
            node.AddValue("showUnpurchased", unpurchasedVisible);
            foreach (ConfigNode f in filters)
            {
                node.AddNode(f);
            }
            return node;
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
                {
                    return false;
                }
            }

            ModuleFilter pmf = part.partPrefab.Modules.GetModule<ModuleFilter>();
            if (pmf != null)
            {
                if (pmf.CheckForForceAdd(SubCategoryTitle))
                {
                    return true;
                }
                if (pmf.CheckForForceBlock(SubCategoryTitle))
                {
                    return false;
                }
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
            {
                return true;
            }
            if (!CheckCategoryFilter(ap, depth))
            {
                return false;
            }
            foreach (FilterNode f in Filters)
            {
                if (f.FilterResult(ap, depth))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckCategoryFilter(AvailablePart ap, int depth = 0)
        {
            if (Category == null)
            {
                return true;
            }
            foreach (FilterNode f in Filters)
            {
                if (f.FilterResult(ap, depth))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Equals(SubcategoryNode sC2)
        {
            if (sC2 == null)
            {
                return false;
            }
            if (SubCategoryTitle == sC2.SubCategoryTitle)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return SubCategoryTitle.GetHashCode();
        }
    }
}