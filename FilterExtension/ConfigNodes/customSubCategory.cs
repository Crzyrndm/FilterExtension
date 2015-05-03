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

        public customSubCategory(string name, string icon)
        {
            filters = new List<Filter>();
            this.subCategoryTitle = name;
            this.iconName = icon;
        }

        public ConfigNode toConfigNode()
        {
            ConfigNode node = new ConfigNode("SUBCATEGORY");

            node.AddValue("name", this.subCategoryTitle);
            node.AddValue("icon", this.iconName);

            foreach (Filter f in this.filters)
                node.AddNode(f.toConfigNode());

            return node;
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

        public void initialise(PartCategorizer.Category cat)
        {
            RUI.Icons.Selectable.Icon icon = Core.getIcon(iconName);
            if (icon == null)
            {
                Core.Log(this.subCategoryTitle + " no icon found");
                icon = PartCategorizer.Instance.iconLoader.iconDictionary.First().Value;
            }

            if (hasFilters)
            {
                if (cat == null)
                    return;
                PartCategorizer.AddCustomSubcategoryFilter(cat, this.subCategoryTitle, icon, p => checkFilters(p));
            }
            else
                Core.Log("Invalid subCategory definition");
        }

        private void Edit(string title, RUI.Icons.Selectable.Icon icon)
        {
            PartCategorizer.Category category = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == "");
            List<PartCategorizer.Category> subCategories = category.subcategories;

            PartCategorizerButton but = subCategories.FirstOrDefault(sC => sC.button.categoryName == title).button;
            if (but != null)
            {
                but.categoryName = subCategoryTitle;
                if (icon != PartCategorizer.Instance.iconLoader.iconDictionary["number1"])
                {
                    but.SetIcon(icon);
                }
            }
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
