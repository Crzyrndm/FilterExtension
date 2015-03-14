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

        public bool hasFilters
        {
            get
            {
                return filters.Count > 0;
            }
        }

        public customSubCategory(ConfigNode node)
        {
            subCategoryTitle = node.GetValue("name");
            if (string.IsNullOrEmpty(subCategoryTitle))
                subCategoryTitle = node.GetValue("title");

            iconName = node.GetValue("icon");

            filters = new List<Filter>();
            foreach (ConfigNode subNode in node.GetNodes("FILTER"))
            {
                filters.Add(new Filter(subNode));
            }
        }

        public customSubCategory(string name, string category, string icon)
        {
            filters = new List<Filter>();
            this.subCategoryTitle = name;
            this.iconName = icon;
        }

        public bool checkFilters(AvailablePart part)
        {
            foreach (Filter f in filters)
            {
                if (f.checkFilter(part))
                    return true;
            }
            return false; // part passed no filter(s), not compatible with this subcategory
        }

        //public void initialise()
        //{
        //    PartCategorizer.Icon icon = Core.getIcon(iconName);
        //    if (icon == null)
        //    {
        //        Core.Log(this.subCategoryTitle + " no icon found");
        //        icon = PartCategorizer.Instance.fallbackIcon;
        //    }

        //    if (hasFilters)
        //    {
        //        PartCategorizer.Category category = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == this.category);
        //        if (category == null)
        //            return;

        //        PartCategorizer.AddCustomSubcategoryFilter(category, this.subCategoryTitle, icon, p => checkFilters(p));
        //    }
        //    //else if (!string.IsNullOrEmpty(oldTitle) && string.IsNullOrEmpty(subCategoryTitle))
        //    //    Delete(oldTitle); // if there is an old title and no new title we are deleting
        //    else if (!string.IsNullOrEmpty(subCategoryTitle))
        //        Edit(subCategoryTitle, icon);
        //    else
        //        Core.Log("Invalid subCategory definition");
        //}

        public void initialise(PartCategorizer.Category cat)
        {
            PartCategorizer.Icon icon = Core.getIcon(iconName);
            if (icon == null)
            {
                Core.Log(this.subCategoryTitle + " no icon found");
                icon = PartCategorizer.Instance.fallbackIcon;
            }

            if (hasFilters)
            {
                if (cat == null)
                    return;
                PartCategorizer.AddCustomSubcategoryFilter(cat, this.subCategoryTitle, icon, p => checkFilters(p));
            }
            //else if (!string.IsNullOrEmpty(subCategoryTitle))
            //    Edit(subCategoryTitle, icon);
            else
                Core.Log("Invalid subCategory definition");
        }

        private void Edit(string title, PartCategorizer.Icon icon)
        {
            PartCategorizer.Category category = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == "");
            List<PartCategorizer.Category> subCategories = category.subcategories;

            PartCategorizerButton but = subCategories.FirstOrDefault(sC => sC.button.categoryName == title).button;
            if (but != null)
            {
                but.categoryName = subCategoryTitle;
                if (icon != PartCategorizer.Instance.fallbackIcon)
                {
                    but.SetIcon(icon);
                }
            }
        }

        public bool Equals(customSubCategory sC2)
        {
            if (sC2 == null)
                return false;

            if (this.hasFilters != sC2.hasFilters || this.iconName != sC2.iconName || this.subCategoryTitle != sC2.subCategoryTitle)
                return false;

            if (this.filters.Count != sC2.filters.Count)
                return false;

            foreach (Filter f1 in this.filters)
            {
                if (!sC2.filters.Any(f2 => f1.Equals(f2)))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (Filter f in this.filters)
            {
                hash *= f.GetHashCode();
            }
            return hash * this.hasFilters.GetHashCode() * this.iconName.GetHashCode() * this.subCategoryTitle.GetHashCode();
        }
    }
}
