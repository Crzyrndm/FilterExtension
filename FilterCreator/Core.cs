using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FilterCreator
{
    using FilterExtensions;
    using FilterExtensions.ConfigNodes;
    using FilterExtensions.Utility;

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class Core : MonoBehaviour
    {
        // GUI Rectangle
        Rect windowRect = new Rect(400, 150, 0, 0);
        Rect catWindowRect = new Rect();

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

        bool showCatWindow = false;

        #region initialise
        public void Awake()
        {
            Debug.Log("[Filter Creator] Version 1.1");

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
                if (showCatWindow)
                    catWindowRect = GUILayout.Window(597165, new Rect(windowRect.x, windowRect.y-150, 0,0), CategoryWindow, "Category Editor", GUILayout.Width(0), GUILayout.Height(0));
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
                showCatWindow = !showCatWindow;
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

                customSubCategory sC = new customSubCategory(active_subCategory_node, "");
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
            customCategory c = new customCategory(title, icon, colour);
            c.initialise();

            customSubCategory dummySC = new customSubCategory("all", title, "");
            dummySC.initialise();
        }

        private void addSubCategory(string title, string category, string icon)
        {
            subCategoryNodes.Add(cfgConstructors.newSubCategoryNode(title, category, icon));

            customSubCategory newSC = new customSubCategory(title, category, icon);
            newSC.initialise();

            FilterExtensions.Core.Instance.refreshList();
        }

        private void addFilter(ConfigNode sC, bool invert = false)
        {
            sC.AddNode(cfgConstructors.newFilterNode(invert));
        }

        private void addFilter(customSubCategory sC, bool invert = false)
        {
            sC.filters.Add(new Filter(invert));
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


        private string catTitle = "";
        private string catColour = "";
        private string catIcon = "";
        private void CategoryWindow(int id)
        {
            GUIStyle textColour = new GUIStyle(HighLogic.Skin.textField);
            Color c = customCategory.convertToColor(catColour);
            if (c != Color.clear)
                textColour.onActive.textColor = textColour.onFocused.textColor = textColour.onHover.textColor = textColour.onNormal.textColor
                    = textColour.active.textColor = textColour.focused.textColor = textColour.hover.textColor = textColour.normal.textColor = c;

            GUIStyle iconFound = new GUIStyle(HighLogic.Skin.textField);
            iconFound.onActive.textColor = iconFound.onFocused.textColor = iconFound.onHover.textColor = iconFound.onNormal.textColor
                    = iconFound.active.textColor = iconFound.focused.textColor = iconFound.hover.textColor = iconFound.normal.textColor = Color.green;
            GUIStyle iconNotFound = new GUIStyle(HighLogic.Skin.textField);
            iconNotFound.onActive.textColor = iconNotFound.onFocused.textColor = iconNotFound.onHover.textColor = iconNotFound.onNormal.textColor
                    = iconNotFound.active.textColor = iconNotFound.focused.textColor = iconNotFound.hover.textColor = iconNotFound.normal.textColor = Color.red;

            // category title
            GUILayout.BeginHorizontal();
            GUILayout.Label("Title:", GUILayout.Width(100));
            catTitle = GUILayout.TextField(catTitle, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            // category colour
            GUILayout.BeginHorizontal();
            GUILayout.Label("Colour:", GUILayout.Width(100));
            catColour = GUILayout.TextField(catColour, textColour, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            // icon name
            GUILayout.BeginHorizontal();
            GUILayout.Label("Icon", GUILayout.Width(100));
            bool validIcon = FilterExtensions.Core.iconDict.ContainsKey(catIcon);
            catIcon = GUILayout.TextField(catIcon, validIcon ? iconFound : iconNotFound, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Log New Category"))
            {
                Log("\r\n" + cfgConstructors.newCategoryNode(catTitle, catIcon, catColour));
            }
        }
    }
}
