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
        internal string iconName = ""; // icon to use
        internal List<Filter> filters = new List<Filter>(); // Filters are OR'd together (pass if it meets this filter, or this filter)

        public subCategory(ConfigNode node)
        {
            category = node.GetValue("category");
            subCategoryTitle = node.GetValue("title");
            iconName = node.GetValue("icon");

            foreach (ConfigNode subNode in node.GetNodes("FILTER"))
            {
                filters.Add(new Filter(subNode));
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

            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, icon, p => checkFilters(p));

            RUIToggleButtonTyped button = Filter.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }
}
