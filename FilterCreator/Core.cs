using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterCreator
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class Core : MonoBehaviour
    {
        // mod folder for each part by internal name
        internal static Dictionary<string, string> partFolderDict = new Dictionary<string, string>();
        // Dictionary of icons created on entering the main menu
        internal static Dictionary<string, PartCategorizer.Icon> iconDict = new Dictionary<string, PartCategorizer.Icon>();
        // GUI Rectangle
        Rect windowRect = new Rect(400, 100, 0, 0);

        Vector2 categoryScroll = new Vector2(0, 0);
        Vector2 subCategoryScroll = new Vector2(0, 0);
        Vector2 filterScroll = new Vector2(0, 0);
        Vector2 checkScroll = new Vector2(0, 0);

        PartCategorizer.Category activeCategory;
        PartCategorizer.Category activeSubCategory;
        ConfigNode activeFilter;
        ConfigNode activeCheck;

        List<ConfigNode> subCategoryNodes = new List<ConfigNode>();

        #region initialise
        public void Awake()
        {
            Debug.Log("[Filter Creator] Version 1.0");

            assignModsToParts();
            loadIcons();
        }

        private void assignModsToParts()
        {
            // Build list of mod folder names and Dict associating parts with mods
            List<string> modNames = new List<string>();
            foreach (AvailablePart p in PartLoader.Instance.parts)
            {
                // don't want dummy parts
                if (p.category == PartCategories.none)
                    continue;

                if (string.IsNullOrEmpty(p.partUrl))
                    RepairAvailablePartUrl(p);

                // if the url is still borked, can't assign a mod to it
                if (string.IsNullOrEmpty(p.partUrl))
                    continue;

                string name = p.partUrl.Split(new char[] { '/', '\\' })[0]; // mod folder name (\\ is escaping the \, read as  '\')

                // if we haven't seen any from this mod before
                if (!modNames.Contains(name))
                    modNames.Add(name);

                // associate the mod to the part
                if (!partFolderDict.ContainsKey(p.name))
                    partFolderDict.Add(p.name, name);
                else
                    Log(p.name + " duplicated part key in part-mod dictionary");
            }
        }

        private void loadIcons()
        {
            List<GameDatabase.TextureInfo> texList = GameDatabase.Instance.databaseTexture.Where(t => t.texture != null
                                                                                                && t.texture.height <= 40 && t.texture.width <= 40
                                                                                                && t.texture.width >= 25 && t.texture.height >= 25
                                                                                                ).ToList();

            Dictionary<string, GameDatabase.TextureInfo> texDict = new Dictionary<string, GameDatabase.TextureInfo>();
            // using a dictionary for looking up _selected textures. Else the list has to be iterated over for every texture
            foreach (GameDatabase.TextureInfo t in texList)
            {
                if (!texDict.ContainsKey(t.name))
                    texDict.Add(t.name, t);
            }

            foreach (GameDatabase.TextureInfo t in texList)
            {
                Texture2D selectedTex = null;

                if (texDict.ContainsKey(t.name + "_selected"))
                    selectedTex = texDict[t.name + "_selected"].texture;
                else
                    selectedTex = t.texture;

                string[] name = t.name.Split(new char[] { '/', '\\' });
                PartCategorizer.Icon icon = new PartCategorizer.Icon(name[name.Length - 1], t.texture, selectedTex, false);

                if (!iconDict.ContainsKey(icon.name))
                    iconDict.Add(icon.name, icon);
            }
        }

        internal static PartCategorizer.Icon getIcon(string name)
        {
            if (iconDict.ContainsKey(name))
            {
                return iconDict[name];
            }
            else if (PartCategorizer.Instance.iconDictionary.ContainsKey(name))
            {
                return PartCategorizer.Instance.iconDictionary[name];
            }
            else if (name.StartsWith("stock_"))
            {
                PartCategorizer.Category fbf = PartCategorizer.Instance.filters.Find(c => c.button.categoryName == "Filter by Function");
                name = name.Substring(6);
                return fbf.subcategories.FirstOrDefault(sC => sC.button.categoryName == name).button.icon;
            }
            return null;
        }

        // credit to EvilReeperx for this lifesaving function
        private void RepairAvailablePartUrl(AvailablePart ap)
        {
            var url = GameDatabase.Instance.GetConfigs("PART").FirstOrDefault(u => u.name.Replace('_', '.') == ap.name);

            if (url == null)
                return;

            ap.partUrl = url.url;
        }
        #endregion

        public void OnGUI()
        {
            GUI.skin = HighLogic.Skin;

            if (!PartCategorizer.Ready)
                return;

            if (AppLauncherEditor.bDisplayEditor)
                windowRect = GUILayout.Window(579164, windowRect, drawWindow, "");

            subCategoryNodes = GameDatabase.Instance.GetConfigNodes("SUBCATEGORY").ToList();
        }

        private void drawWindow(int id)
        {
            ConfigNode subCategory = new ConfigNode();

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
            subCategoryScroll = GUILayout.BeginScrollView(subCategoryScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(220));
            GUILayout.BeginVertical();
            foreach (PartCategorizer.Category sC in activeCategory.subcategories)
            {
                if (GUILayout.Toggle(activeSubCategory == sC, sC.button.categoryName, HighLogic.Skin.button, GUILayout.Width(200)))
                {
                    activeSubCategory = sC;
                    subCategory = subCategoryNodes.FirstOrDefault(n => n.GetValue("title") == sC.button.categoryName);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginVertical();

            if (GUILayout.Button("Add New Filter"))
            {

            }

            filterScroll = GUILayout.BeginScrollView(filterScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(220));
            foreach (ConfigNode node in subCategory.GetNodes("FILTER"))
            {
                if (GUILayout.Toggle(activeFilter == node, node.GetValue("name"), HighLogic.Skin.button, GUILayout.Width(200)))
                {
                    activeFilter = node;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            //GUILayout.BeginVertical();

            //if (GUILayout.Button("Add New Check"))
            //{

            //}

            //checkScroll = GUILayout.BeginScrollView(checkScroll, GUILayout.Height((float)(Screen.height * 0.7)), GUILayout.Width(220));
            //foreach (ConfigNode node in activeFilter.GetNodes("CHECK"))
            //{
            //    if (GUILayout.Toggle(activeCheck == node, node.GetValue("type"), HighLogic.Skin.button, GUILayout.Width(200)))
            //    {
            //        activeFilter = node;
            //    }
            //}
            //GUILayout.EndScrollView();
            //GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        internal static void Log(object o)
        {
            Debug.Log("[Filter Creator] " + o);
        }
    }
}
