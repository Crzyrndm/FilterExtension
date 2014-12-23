using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    class Category
    {
        internal string categoryTitle;
        internal string iconName;
        internal Color colour;

        internal static AvailablePart roid = PartLoader.Instance.parts.First(ap => ap.name == "PotatoRoid");

        public Category(ConfigNode node)
        {
            categoryTitle = node.GetValue("title");
            iconName = node.GetValue("icon");
            convertToColor(node.GetValue("colour"));
        }

        internal void initialise()
        {
            if (categoryTitle == null)
                return;

            PartCategorizer.AddCustomFilter(categoryTitle, Core.getIcon(iconName), colour);

            PartCategorizer.Category category = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == categoryTitle);
            category.displayType = EditorPartList.State.PartsList;
            category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;

            //else
            //{

            //    // buttonType Filter => add subcategories
            //    // buttonType Category => auto adds filter by function to it

            //    PartCategorizer.Category cat = new PartCategorizer.Category(
            //        PartCategorizer.ButtonType.FILTER, EditorPartList.State.PartsList, categoryTitle, Core.getIcon(iconName),
            //        colour, colour, PartCategorizer.Instance.filters[0].exclusionFilter);

            //    PartCategorizer.Instance.filters.Add(cat);
            //}
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
