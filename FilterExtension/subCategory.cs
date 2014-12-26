using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    class subCategory
    {
        internal string[] categories; // parent category
        internal string subCategoryTitle; // title of this subcategory
        internal string oldTitle; // title generated for the auto extending categories to search by
        internal string iconName; // default icon to use
        internal List<Filter> filters = new List<Filter>(); // Filters are OR'd together (pass if it meets this filter, or this filter)
        internal bool filter;

        public subCategory(ConfigNode node)
        {
            categories = node.GetValue("category").Split(',');
            subCategoryTitle = node.GetValue("title");
            iconName = node.GetValue("icon");
            oldTitle = node.GetValue("oldTitle");

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
            return false; // part passed no filter(s), not compatible with this subcategory
        }

        internal void initialise()
        {
            foreach (string s in categories)
            {
                Debug.Log(s);
                PartCategorizer.Icon icon;
                if (string.IsNullOrEmpty(iconName))
                {
                    Debug.Log("[Filter Extensions] " + this.subCategoryTitle + " missing icon reference");
                    icon = PartCategorizer.Instance.fallbackIcon;
                }
                else
                {
                    icon = Core.getIcon(iconName);
                }
                Debug.Log("1");
                if (filter)
                {
                    Debug.Log("2");
                    PartCategorizer.Category Filter = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == s.Trim());
                    Debug.Log("3");
                    PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, icon, p => checkFilters(p));
                }
                else if (!string.IsNullOrEmpty(oldTitle))
                {
                    List<PartCategorizer.Category> subCategories = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == s.Trim()).subcategories;
                    if (string.IsNullOrEmpty(subCategoryTitle))
                        subCategories.Remove(subCategories.Find(m => m.button.categoryName == oldTitle));
                    else
                    {
                        PartCategorizerButton but = subCategories.FirstOrDefault(sC => sC.button.categoryName == oldTitle).button;
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
}
