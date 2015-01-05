using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FilterCreator
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class Core : MonoBehaviour
    {
        // GUI Rectangle
        Rect windowRect = new Rect(400, 100, 0, 0);

        Vector2 categoryScroll = new Vector2(0, 0);
        Vector2 subCategoryScroll = new Vector2(0, 0);
        Vector2 filterScroll = new Vector2(0, 0);
        Vector2 checkScroll = new Vector2(0, 0);
        Vector2 partsScroll = new Vector2(0, 0);

        PartCategorizer.Category activeCategory;
        PartCategorizer.Category activeSubCategory;
        ConfigNode activeFilter = new ConfigNode();
        ConfigNode activeCheck;

        List<ConfigNode> subCategoryNodes = new List<ConfigNode>();
        List<ConfigNode> categoryNodes = new List<ConfigNode>();

        bool showNewWindow = false;

        [Flags]
        enum newWindowStyle
        {
            Category,
            subCategory,
            Filter,
            Check
        }

        newWindowStyle style = newWindowStyle.Category;

        #region initialise
        public void Awake()
        {
            Debug.Log("[Filter Creator] Version 1.0");

            subCategoryNodes = GameDatabase.Instance.GetConfigNodes("SUBCATEGORY").ToList();
            categoryNodes = GameDatabase.Instance.GetConfigNodes("CATEGORY").ToList();
        }
        #endregion

        public void OnGUI()
        {
            GUI.skin = HighLogic.Skin;

            if (!PartCategorizer.Ready)
                return;

            if (AppLauncherEditor.bDisplayEditor)
            {
                windowRect = GUILayout.Window(579164, windowRect, drawWindow, "");
                if (showNewWindow)
                    GUILayout.Window(597165, new Rect(windowRect.x, windowRect.y, 0,0), newCategoryWindow, "");
            }
        }

        private void drawWindow(int id)
        {
            ConfigNode category = new ConfigNode();
            ConfigNode subCategory = new ConfigNode();
            ConfigNode active_subCategory_node = new ConfigNode();

            GUILayout.BeginHorizontal();
            // Categories column
            GUILayout.BeginVertical();
            if (GUILayout.Button("Create Category"))
            {
                addCategory("blah", "blah", "#000000");
            }

            categoryScroll = GUILayout.BeginScrollView(categoryScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(240));
            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
            {                
                category = categoryNodes.FirstOrDefault(n => n.GetValue("title") == c.button.categoryName);
                string label = string.Format("title: {0}\r\nicon: {1}\r\ncolour: {2}", c.button.categoryName, category == null ? "" : category.GetValue("icon"), category == null ? "" : category.GetValue("colour"));
                if (GUILayout.Toggle(activeCategory == c, label, HighLogic.Skin.button, GUILayout.Width(200)))
                {
                    activeCategory = c;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            
            // subCategories column
            GUILayout.BeginVertical();
            if (GUILayout.Button("Create Sub-Category") && activeCategory != null)
            {
                addSubCategory("blah", activeCategory.button.categoryName, "testIcon");
            }

            subCategoryScroll = GUILayout.BeginScrollView(subCategoryScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(240));
            if (activeCategory != null)
            {
                foreach (PartCategorizer.Category sC in activeCategory.subcategories)
                {
                    subCategory = subCategoryNodes.FirstOrDefault(n => n.GetValue("title") == sC.button.categoryName && n.GetValue("category").Split(',').Any(s => s.Trim() == activeCategory.button.categoryName));
                    if (subCategory != null)
                    {
                        if (GUILayout.Toggle(activeSubCategory == sC, "title: " + sC.button.categoryName + "\r\ncategory: "
                            + subCategory.GetValue("category").Split(',').FirstOrDefault(s => s.Trim() == activeCategory.button.categoryName)
                            + "\r\noldTitle: " + subCategory.GetValue("oldTitle")
                            + "\r\nicon: " + subCategory.GetValue("icon"), HighLogic.Skin.button, GUILayout.Width(200)))
                        {
                            activeSubCategory = sC;
                            active_subCategory_node = subCategoryNodes.FirstOrDefault(n => n.GetValue("title") == sC.button.categoryName);
                        }
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Filters column
            GUILayout.BeginVertical();
            if (GUILayout.Button("Create Filter") && active_subCategory_node != null)
            {
                addFilter(active_subCategory_node);
            }

            int index = 0, sel = 0;
            filterScroll = GUILayout.BeginScrollView(filterScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(240));
            if (active_subCategory_node != null && active_subCategory_node.GetNodes("FILTER") != null)
            {
                foreach (ConfigNode fil in active_subCategory_node.GetNodes("FILTER"))
                {
                    index++;
                    if (GUILayout.Toggle(activeFilter == fil, "invert: " + fil.GetValue("invert"), HighLogic.Skin.button, GUILayout.Width(200)))
                    {
                        sel = index;
                        activeFilter = fil;
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Checks column
            GUILayout.BeginVertical();
            if (GUILayout.Button("Create Check") && activeFilter != null)
            {
                addCheck(active_subCategory_node, activeFilter, "folder", "Squad", sel);
            }

            checkScroll = GUILayout.BeginScrollView(checkScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(240));
            if (activeFilter != null && activeFilter.GetNodes("CHECK") != null)
            {
                foreach (ConfigNode check in activeFilter.GetNodes("CHECK"))
                {
                    if (GUILayout.Toggle(activeCheck == check, "type: " + check.GetValue("type") + "\r\nvalue: " + check.GetValue("value") + "\r\ninvert: " + check.GetValue("invert"), HighLogic.Skin.button, GUILayout.Width(200)))
                    {
                        activeCheck = check;
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Parts column
            if (active_subCategory_node != null)
            {
                GUILayout.BeginVertical();

                FilterExtensions.customSubCategory sC = new FilterExtensions.customSubCategory(active_subCategory_node, "");
                partsScroll = GUILayout.BeginScrollView(partsScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(240));

                foreach (AvailablePart ap in PartLoader.Instance.parts)
                {
                    if (sC.checkFilters(ap))
                    {
                        GUILayout.Label(ap.title, GUILayout.Width(200));
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        internal static void Log(object o)
        {
            Debug.Log("[Filter Creator] " + o);
        }

        private void addCategory(string title, string icon, string colour)
        {
            ConfigNode category = new ConfigNode("CATEGORY");
            category.AddValue("title", title);
            category.AddValue("icon", icon);
            category.AddValue("colour", colour);

            FilterExtensions.customCategory c = new FilterExtensions.customCategory(category);
            c.initialise();

            ConfigNode sC = new ConfigNode("SUBCATEGORY");
            sC.AddValue("category", title);
            sC.AddValue("title", "all");

            ConfigNode filter = new ConfigNode("FILTER");
            sC.AddNode(filter);

            FilterExtensions.customSubCategory dummySC = new FilterExtensions.customSubCategory(sC, title);
            dummySC.initialise();
        }

        private void addSubCategory(string title, string category, string icon)
        {
            ConfigNode sC = new ConfigNode("SUBCATEGORY");
            sC.AddValue("category", category);
            sC.AddValue("title", title);
            sC.AddValue("icon", icon);

            ConfigNode filter = new ConfigNode("FILTER");
            filter.AddValue("invert", "");
            sC.AddNode(filter);

            subCategoryNodes.Add(sC);
            FilterExtensions.customSubCategory newSC = new FilterExtensions.customSubCategory(sC, category);
            newSC.initialise();

            FilterExtensions.Core.Instance.refreshList();
        }

        private void addFilter(ConfigNode sC, bool invert = false)
        {
            ConfigNode filter = new ConfigNode("FILTER");
            filter.AddValue("invert", invert.ToString());
            sC.AddNode(filter);
        }

        private void addCheck(ConfigNode sC, ConfigNode filter, string type, string value, int filterIndex, bool invert = false)
        {
            sC.RemoveNode("FILTER");
            ConfigNode check = new ConfigNode("CHECK");
            check.AddValue("type", type);
            check.AddValue("value", value);
            check.AddValue("invert", invert);
            
            filter.AddNode(check);
            
            sC.AddNode(filter);
        }

        private void newCategoryWindow(int id)
        {
            
        }
    }
}
