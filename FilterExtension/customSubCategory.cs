using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    public class customSubCategory
    {
        internal string category; // parent category
        internal string subCategoryTitle; // title of this subcategory
        internal string oldTitle; // title generated for the auto extending categories to search by
        internal string iconName; // default icon to use
        internal List<Filter> filters = new List<Filter>(); // Filters are OR'd together (pass if it meets this filter, or this filter)
        internal bool filter = false;

        public customSubCategory(ConfigNode node, string category)
        {
            this.category = category;
            subCategoryTitle = node.GetValue("title");
            iconName = node.GetValue("icon");
            oldTitle = node.GetValue("oldTitle");

            foreach (ConfigNode subNode in node.GetNodes("FILTER"))
            {
                filters.Add(new Filter(subNode));
            }
            filter = filters.Count > 0;
        }

        public bool checkFilters(AvailablePart part)
        {
            foreach (Filter f in filters)
            {
                bool val = f.checkFilter(part);
                if (val)
                    return true;
            }
            return false; // part passed no filter(s), not compatible with this subcategory
        }

        public void initialise()
        {
            PartCategorizer.Icon icon;
            if (string.IsNullOrEmpty(iconName))
            {
                Core.Log(this.subCategoryTitle + " missing icon reference");
                icon = PartCategorizer.Instance.fallbackIcon;
            }
            else
            {
                icon = Core.getIcon(iconName);
                if (icon == null)
                {
                    Core.Log(this.subCategoryTitle + " no icon found");
                    icon = PartCategorizer.Instance.fallbackIcon;
                }
            }

            if (filter)
            {
                PartCategorizer.Category category = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == this.category);
                if (category == null)
                {
                    return;
                }

                PartCategorizer.AddCustomSubcategoryFilter(category, subCategoryTitle, icon, p => checkFilters(p));
            }
            else if (!string.IsNullOrEmpty(oldTitle))
            {
                Edit_Delete(oldTitle, string.IsNullOrEmpty(subCategoryTitle), icon);
            }
            else
            {
                Edit_Delete(subCategoryTitle, false, icon);
            }
        }

        private void Edit_Delete(string title, bool delete, PartCategorizer.Icon icon)
        {
            List<PartCategorizer.Category> subCategories = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category).subcategories;
            if (delete)
                subCategories.Remove(subCategories.Find(m => m.button.categoryName == title));
            else
            {
                PartCategorizerButton but = subCategories.FirstOrDefault(sC => sC.button.categoryName == title).button;
                if (but != null)
                {
                    but.categoryName = subCategoryTitle;
                    if (icon != PartCategorizer.Instance.fallbackIcon)
                        but.SetIcon(icon);
                }
            }
        }

        public bool Equals(customSubCategory sC2)
        {
            if (sC2 == null)
                return false;

            if (this.category != sC2.category || this.filter != sC2.filter || this.iconName != sC2.iconName
                || this.oldTitle != sC2.oldTitle || this.subCategoryTitle != sC2.subCategoryTitle)
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
            return hash * this.category.GetHashCode() * this.filter.GetHashCode() * this.iconName.GetHashCode()
                * this.oldTitle.GetHashCode() * this.subCategoryTitle.GetHashCode();
        }
    }
}
