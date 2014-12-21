using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    class subCategory
    {
        internal string category = ""; // parent category
        internal string subCategoryTitle = ""; // title of this subcategory
        internal string defaultTitle = ""; // title generated for the auto extending categories to search by
        internal string iconName = ""; // icon to use
        internal List<Filter> filters = new List<Filter>(); // Filters are OR'd together (pass if it meets this filter, or this filter)
        internal bool filter;

        public subCategory(ConfigNode node)
        {
            category = node.GetValue("category");
            try
            {
                subCategoryTitle = node.GetValue("title");
            }
            catch { }
            try
            {
                iconName = node.GetValue("icon");
            }
            catch { }
            try
            {
                defaultTitle = node.GetValue("oldTitle");
            }
            catch { }

            foreach (ConfigNode subNode in node.GetNodes("FILTER"))
            {
                filters.Add(new Filter(subNode));
                filter = true;
            }
        }

        internal bool checkFilters(AvailablePart part)
        {
            foreach (Filter f in filters)
            {
                bool val = f.checkFilter(part);
                if (val)
                    return true;
            }
            return false; // part passed no filter, not compatible with this subcategory
        }

        internal void initialise()
        {
            PartCategorizer.Icon icon = PartCategorizer.Instance.GetIcon(iconName);

            

            if (filter)
            {
                PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
                PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, icon, p => checkFilters(p));

                RUIToggleButtonTyped button = Filter.button.activeButton;
                button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
                button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
            }
            else
            {
                List<PartCategorizer.Category> modules = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category).subcategories;
                if (subCategoryTitle == "")
                {
                    Debug.Log("Removing item");
                    modules.Remove(modules.Find(m => m.button.categoryName == defaultTitle));
                }
                else
                {
                    List<PartCategorizerButton> b = modules.Select(m => m.button).ToList();
                    PartCategorizerButton but = b.Find(c => c.categoryName == defaultTitle);
                    if (but != null)
                    {
                        but.categoryName = subCategoryTitle;
                        but.SetIcon(icon);
                    }
                }
            }
        }
    }
}
