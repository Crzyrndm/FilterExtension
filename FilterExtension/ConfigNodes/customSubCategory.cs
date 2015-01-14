using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.ConfigNodes
{
    public class customSubCategory
    {
        public string category { get; set; } // parent category
        public string subCategoryTitle { get; set; } // title of this subcategory
        public string oldTitle { get; set; } // title generated for the auto extending categories to search by
        public string iconName { get; set; } // default icon to use
        public List<Filter> filters { get; set; } // Filters are OR'd together (pass if it meets this filter, or this filter)

        public bool hasFilters
        {
            get
            {
                return filters.Count > 0;
            }
        }

        public customSubCategory(ConfigNode node, string Category)
        {
            this.category = Category;
            subCategoryTitle = node.GetValue("name");
            if (string.IsNullOrEmpty(subCategoryTitle))
                subCategoryTitle = node.GetValue("title");

            iconName = node.GetValue("icon");
            oldTitle = node.GetValue("oldTitle");

            filters = new List<Filter>();
            foreach (ConfigNode subNode in node.GetNodes("FILTER"))
            {
                filters.Add(new Filter(subNode));

                // if there's an "All parts" subcategory, add the filters to it
                if (Core.Instance.categoryAllSub.ContainsKey(category))
                    Core.Instance.categoryAllSub[category].filters.Add(new Filter(subNode));
            }
        }

        public customSubCategory(string name, string category, string icon)
        {
            filters = new List<Filter>();
            this.category = category;
            this.subCategoryTitle = name;
            this.iconName = icon;
            this.oldTitle = "";
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

            if (hasFilters)
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
            PartCategorizer.Category category = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == this.category);
            List<PartCategorizer.Category> subCategories = category.subcategories;
            if (delete)
                subCategories.Remove(subCategories.Find(m => m.button.categoryName == title));
            else
            {
                PartCategorizerButton but = subCategories.FirstOrDefault(sC => sC.button.categoryName == title).button;
                if (but != null)
                {
                    but.categoryName = subCategoryTitle;
                    if (icon != PartCategorizer.Instance.fallbackIcon)
                    {
                        but.SetIcon(icon);

                        category.button.activeButton.SetTrue(category.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
                        // need to individually refresh after an icon edit (why did this break, wasn't required earlier?)
                        but.activeButton.SetFalse(but.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
                        but.activeButton.SetTrue(but.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
                    }
                }
            }
        }

        public bool Equals(customSubCategory sC2)
        {
            if (sC2 == null)
                return false;

            if (this.category != sC2.category || this.hasFilters != sC2.hasFilters || this.iconName != sC2.iconName
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
            return hash * this.category.GetHashCode() * this.hasFilters.GetHashCode() * this.iconName.GetHashCode()
                * this.oldTitle.GetHashCode() * this.subCategoryTitle.GetHashCode();
        }
    }
}
