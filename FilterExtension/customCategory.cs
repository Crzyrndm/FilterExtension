using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    using Categoriser;

    public class customCategory
    {
        internal string categoryTitle;
        internal string iconName;
        internal Color colour;
        internal string type;
        internal string[] value; // mod folder name for mod type categories
        internal bool all;

        private static readonly List<string> categoryNames = new List<string> { "Pods", "Engines", "Fuel Tanks", "Command and Control", "Structural", "Aerodynamics", "Utility", "Science" };

        public customCategory(ConfigNode node)
        {
            categoryTitle = node.GetValue("title");
            iconName = node.GetValue("icon");
            convertToColor(node.GetValue("colour"));
            
            type = node.GetValue("type");
            string temp = node.GetValue("value");
            if (!string.IsNullOrEmpty(temp))
                value = temp.Split(',');

            bool.TryParse(node.GetValue("all"), out all);

            if (type == "mod")
                generateSubCategories();
        }

        public void initialise()
        {
            if (categoryTitle == null)
                return;
            PartCategorizer.Icon icon = Core.getIcon(iconName);
            if (icon == null)
                icon = PartCategorizer.Instance.fallbackIcon;
            PartCategorizer.AddCustomFilter(categoryTitle, icon, colour);
            
            PartCategorizer.Category category = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == categoryTitle);
            category.displayType = EditorPartList.State.PartsList;
            category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;
        }

        private void generateSubCategories()
        {
            string folders = "";
            foreach (string folder in value)
            {
                folders += folder + ",";
            }

            if (all)
            {
                ConfigNode folderCheck = new ConfigNode("CHECK");
                folderCheck.AddValue("type", "folder");
                folderCheck.AddValue("value", folders);

                ConfigNode nodeFilter = new ConfigNode("FILTER");
                nodeFilter.AddValue("invert", "false");
                nodeFilter.AddNode(folderCheck);

                ConfigNode nodeSub = new ConfigNode("SUBCATEGORY");
                nodeSub.AddValue("category", categoryTitle);
                nodeSub.AddValue("title", "All");
                nodeSub.AddValue("icon", "All");
                nodeSub.AddNode(nodeFilter);

                Core.Instance.subCategories.Add(new customSubCategory(nodeSub, categoryTitle));
            }

            foreach (string s in categoryNames)
            {
                ConfigNode folderCheck = new ConfigNode("CHECK");
                folderCheck.AddValue("type", "folder");
                folderCheck.AddValue("value", folders);

                ConfigNode catCheck = new ConfigNode("CHECK");
                catCheck.AddValue("type", "category");
                catCheck.AddValue("value", s);

                ConfigNode nodeFilter = new ConfigNode("FILTER");
                nodeFilter.AddValue("invert", "false");
                nodeFilter.AddNode(folderCheck);
                nodeFilter.AddNode(catCheck);

                ConfigNode nodeSub = new ConfigNode("SUBCATEGORY");
                nodeSub.AddValue("category", categoryTitle);
                nodeSub.AddValue("title", s);
                nodeSub.AddValue("icon", "stock_" + s);
                nodeSub.AddNode(nodeFilter);

                Core.Instance.subCategories.Add(new customSubCategory(nodeSub, categoryTitle));
            }
        }

        private void convertToColor(string hex_ARGB)
        {
            hex_ARGB = hex_ARGB.Replace("#", "");
            hex_ARGB = hex_ARGB.Replace("0x", "");

            if (hex_ARGB.Length == 8)
            {
                Color c = new Color();
                c.a = (float)byte.Parse(hex_ARGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                c.r = (float)byte.Parse(hex_ARGB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                c.g = (float)byte.Parse(hex_ARGB.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                c.b = (float)byte.Parse(hex_ARGB.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                colour = c;
            }
            else if (hex_ARGB.Length == 6)
            {
                Color c = new Color();
                c.a = 1;
                c.r = (float)byte.Parse(hex_ARGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                c.g = (float)byte.Parse(hex_ARGB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                c.b = (float)byte.Parse(hex_ARGB.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                colour = c;
            }
        }
    }
}
