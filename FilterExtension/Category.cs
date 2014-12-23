using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    class Category : IEquatable<Category>
    {
        internal string categoryTitle;
        internal string iconName;
        internal Color colour;

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

            PartCategorizer.Icon icon = Core.getIcon(iconName);
            PartCategorizer.AddCustomFilter(categoryTitle, icon, colour);
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

        public bool Equals(Category other)
        {
            if (other == null)
                return false;

            if (this.categoryTitle == other.categoryTitle)
                return true;
            else
                return false;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;

            Category CObj = obj as Category;
            if (CObj == null)
                return false;
            else
                return Equals(CObj);
        } 

        public override int GetHashCode()
        {
            return categoryTitle.GetHashCode();
        }

        public static bool operator ==(Category c1, Category c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Category c1, Category c2)
        {
            return !c1.Equals(c2);
        }
    }
}
