using System;
using System.Collections.Generic;
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

        #region initialise
        public void Awake()
        {
            Debug.Log("[Filter Creator] Version 1.0");

            subCategoryNodes = GameDatabase.Instance.GetConfigNodes("SUBCATEGORY").ToList();
        }
        #endregion

        public void OnGUI()
        {
            GUI.skin = HighLogic.Skin;

            if (!PartCategorizer.Ready)
                return;

            if (AppLauncherEditor.bDisplayEditor)
                windowRect = GUILayout.Window(579164, windowRect, drawWindow, "");
        }

        private void drawWindow(int id)
        {
            ConfigNode subCategory = new ConfigNode();
            ConfigNode subCategory_selected = new ConfigNode();

            HighLogic.Skin.button.wordWrap = true;

            GUILayout.BeginHorizontal();
            // Categories column
            categoryScroll = GUILayout.BeginScrollView(categoryScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(220));
            GUILayout.BeginVertical();
            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
            {
                if (activeCategory == null)
                    activeCategory = c;

                if (GUILayout.Toggle(activeCategory == c, c.button.categoryName, HighLogic.Skin.button, GUILayout.Width(200)))
                {
                    activeCategory = c;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            // subCategories column
            subCategoryScroll = GUILayout.BeginScrollView(subCategoryScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(230));
            GUILayout.BeginVertical();
            foreach (PartCategorizer.Category sC in activeCategory.subcategories)
            {
                subCategory = subCategoryNodes.FirstOrDefault(n => n.GetValue("title") == sC.button.categoryName);
                if (subCategory != null)
                {
                    if (GUILayout.Toggle(activeSubCategory == sC, "title: " + sC.button.categoryName + "\r\ncategory: "
                        + subCategory.GetValue("category").Split(',').FirstOrDefault(s => s.Trim() == activeCategory.button.categoryName)
                        + "\r\noldTitle: " + subCategory.GetValue("oldTitle")
                        + "\r\nicon: " + subCategory.GetValue("icon"), HighLogic.Skin.button, GUILayout.Width(200)))
                    {
                        activeSubCategory = sC;
                        subCategory_selected = subCategoryNodes.FirstOrDefault(n => n.GetValue("title") == sC.button.categoryName);
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // Filters column
            filterScroll = GUILayout.BeginScrollView(filterScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(220));
            if (subCategory_selected != null && subCategory_selected.GetNodes("FILTER") != null)
            {
                GUILayout.BeginVertical();
                foreach (ConfigNode fil in subCategory_selected.GetNodes("FILTER"))
                {
                    if (GUILayout.Toggle(activeFilter == fil, "invert: " + fil.GetValue("invert"), HighLogic.Skin.button, GUILayout.Width(200)))
                    {
                        activeFilter = fil;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            // Checks column
            checkScroll = GUILayout.BeginScrollView(checkScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(220));
            if (activeFilter != null && activeFilter.GetNodes("CHECK") != null)
            {
                GUILayout.BeginVertical();
                foreach (ConfigNode check in activeFilter.GetNodes("CHECK"))
                {
                    if (GUILayout.Toggle(activeCheck == check, "type: " + check.GetValue("type") + "\r\nvalue: " + check.GetValue("value") + "\r\ninvert: " + check.GetValue("invert"), HighLogic.Skin.button, GUILayout.Width(200)))
                    {
                        activeCheck = check;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            // Parts column
            if (subCategory_selected != null)
            {
                FilterExtensions.customSubCategory sC = new FilterExtensions.customSubCategory(subCategory_selected, "");
                partsScroll = GUILayout.BeginScrollView(partsScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(220));

                GUILayout.BeginVertical();
                foreach (AvailablePart ap in PartLoader.Instance.parts)
                {
                    if (sC.checkFilters(ap))
                    {
                        GUILayout.Label(ap.title, GUILayout.Width(200));
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        internal static void Log(object o)
        {
            Debug.Log("[Filter Creator] " + o);
        }
    }
}
