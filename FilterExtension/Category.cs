using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    using Categoriser;

    class customCategory
    {
        internal string categoryTitle;
        internal string iconName;
        internal Color colour;
        internal string type;
        internal string value; // mod folder name for mod type categories

        internal static AvailablePart roid = PartLoader.Instance.parts.First(ap => ap.name == "PotatoRoid");

        public customCategory(ConfigNode node)
        {
            categoryTitle = node.GetValue("title");
            iconName = node.GetValue("icon");
            convertToColor(node.GetValue("colour"));
            type = node.GetValue("type");
            value = node.GetValue("value");
        }

        internal void initialise()
        {
            if (categoryTitle == null)
                return;

            // PartCategorizer.Instance.filters.Add(new PartCategorizer.Category(
            //    PartCategorizer.ButtonType.FILTER, EditorPartList.State.PartsList, categoryTitle, Core.getIcon(iconName), colour, colour, 

            PartCategorizer.AddCustomFilter(categoryTitle, Core.getIcon(iconName), colour);

            PartCategorizer.Category category = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == categoryTitle);
            category.displayType = EditorPartList.State.PartsList;
            category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;

            if (type == "mod")
                generateSubCategories(category);
        }

        private void generateSubCategories(PartCategorizer.Category category)
        {
            PartCategorizer.Category fbf = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == "Filter by Function");
            PartCategorizer.AddCustomSubcategoryFilter(category, "Pods", fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == "Pods").button.icon, p => Filter(p, "Pod"));
            PartCategorizer.AddCustomSubcategoryFilter(category, "Engines", fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == "Engines").button.icon, p => Filter(p, "Engine"));
            PartCategorizer.AddCustomSubcategoryFilter(category, "Fuel Tanks", fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == "Fuel Tanks").button.icon, p => Filter(p, "Tank"));
            PartCategorizer.AddCustomSubcategoryFilter(category, "Command and Control", fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == "Command and Control").button.icon, p => Filter(p, "Command"));
            PartCategorizer.AddCustomSubcategoryFilter(category, "Structural", fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == "Structural").button.icon, p => Filter(p, "Struct"));
            PartCategorizer.AddCustomSubcategoryFilter(category, "Aerodynamics", fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == "Aerodynamics").button.icon, p => Filter(p, "Aero"));
            PartCategorizer.AddCustomSubcategoryFilter(category, "Utility", fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == "Utility").button.icon, p => Filter(p, "Utility"));
            PartCategorizer.AddCustomSubcategoryFilter(category, "Science", fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == "Science").button.icon, p => Filter(p, "Science"));
        }

        private bool Filter(AvailablePart part, string category)
        {
            if (PartType.checkCategory(part, category) && PartType.checkFolder(part, value))
                return true;
            else
                return false;
        }

        private void convertToColor(string hex_ARGB)
        {
            hex_ARGB = hex_ARGB.Replace("#", "");

            if (hex_ARGB.Length == 8)
            {
                Color c = new Color();
                c.a = (int)byte.Parse(hex_ARGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255;
                c.r = (int)byte.Parse(hex_ARGB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255;
                c.g = (int)byte.Parse(hex_ARGB.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255;
                c.b = (int)byte.Parse(hex_ARGB.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255;
                colour = c;
            }
            else if (hex_ARGB.Length == 6)
            {
                Color c = new Color();
                c.a = 1;
                c.r = (int)byte.Parse(hex_ARGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255;
                c.g = (int)byte.Parse(hex_ARGB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255;
                c.b = (int)byte.Parse(hex_ARGB.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) / 255;
                colour = c;
            }
        }
    }
}
