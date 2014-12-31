using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;


namespace FilterExtensions
{
    using UnityEngine;
    using FilterExtensions.Categoriser;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Core : MonoBehaviour
    {
        // storing categories/subCategories loaded at Main Menu for creation when entering SPH/VAB
        internal static List<customCategory> Categories = new List<customCategory>();
        internal static List<customSubCategory> subCategories = new List<customSubCategory>();

        // mod folder for each part by internal name
        internal static Dictionary<string, string> partFolderDict = new Dictionary<string, string>();

        // Dictionary of icons created on entering the main menu
        internal static Dictionary<string, PartCategorizer.Icon> iconDict = new Dictionary<string, PartCategorizer.Icon>();

        void Awake()
        {
            // Add event for when the Editor GUI becomes active. This is never removed because we need it to fire every time
            GameEvents.onGUIEditorToolbarReady.Add(editor);

            // generate the associations between parts and folders, and create all the mod categories
            assignModsToParts();

            // mod categories key: title, value: folder
            // used for adding the folder check to subCategories
            Dictionary<string, string> folderToCategoryDict = new Dictionary<string, string>();
            // load all category configs
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CATEGORY"))
            {
                customCategory C = new customCategory(node);
                if (Categories.Find(n => n.categoryTitle == C.categoryTitle) == null)
                {
                    Categories.Add(C);
                    if (C.value != null)
                    {
                        foreach (string s in C.value)
                        {
                            if (!folderToCategoryDict.ContainsKey(C.categoryTitle))
                                folderToCategoryDict.Add(C.categoryTitle, s);
                        }
                    }
                }
            }

            // load all subCategory configs
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SUBCATEGORY"))
            {
                // if multiple categories are specified, create multiple subCategories
                string[] categories = node.GetValue("category").Split(',');
                foreach (string s in categories)
                {
                    customSubCategory sC = new customSubCategory(node, s.Trim());
                    if (sC.filter && folderToCategoryDict.ContainsKey(sC.category))
                    {
                        foreach(Filter f in sC.filters)
                        {
                            ConfigNode nodeCheck = new ConfigNode("CHECK");
                            nodeCheck.AddValue("type", "folder");
                            nodeCheck.AddValue("value", folderToCategoryDict[sC.category]);

                            f.checks.Add(new Check(nodeCheck));
                        }
                    }

                    if (checkForConflicts(sC))
                        subCategories.Add(sC);
                }
            }
            StartCoroutine(checkForEmptySubCategories());
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
                    Debug.Log("[Filter Extensions] " + p.name + " duplicated part key in part-mod dictionary");
            }
            // Create subcategories for Manufacturer category
            foreach (string s in modNames)
            {
                ConfigNode nodeCheck = new ConfigNode("CHECK");
                nodeCheck.AddValue("type", "folder");
                nodeCheck.AddValue("value", s);

                ConfigNode nodeFilter = new ConfigNode("FILTER");
                nodeFilter.AddValue("invert", "false");
                nodeFilter.AddNode(nodeCheck);

                ConfigNode nodeSub = new ConfigNode("SUBCATEGORY");
                nodeSub.AddValue("category", "Filter by Manufacturer");
                nodeSub.AddValue("title", s);
                nodeSub.AddValue("icon", s);
                nodeSub.AddNode(nodeFilter);

                subCategories.Add(new customSubCategory(nodeSub, nodeSub.GetValue("category")));
            }
        }

        private void editor()
        {
            // clear manufacturers from Filter by Manufacturer
            // Don't rename incase other mods depend on finding it (and the name isn't half bad either...)
            PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Manufacturer").subcategories.Clear();

            // Add all the categories
            foreach (customCategory c in Categories)
            {
                c.initialise();
            }

            // icon autoloader pass
            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
            {
                checkIcons(c);
            }

            // create all the new subCategories
            foreach (customSubCategory sC in subCategories)
            {
                try
                {
                    sC.initialise();
                }
                catch {
                    Debug.Log("[Filter Extensions]" + sC.subCategoryTitle + " failed to initialise");
                }
            }

            // update icons
            refreshList();

            // Remove any category with no subCategories (causes major breakages)
            PartCategorizer.Instance.filters.RemoveAll(c => c.subcategories.Count == 0);
            // refresh icons
            PartCategorizer.Instance.UpdateCategoryNameLabel();

            // reveal categories
            PartCategorizer.Instance.SetAdvancedMode();
        }

        private void refreshList()
        {
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            RUIToggleButtonTyped button = Filter.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }

        private bool checkForConflicts(customSubCategory sCToCheck)
        {
            foreach (customSubCategory sC in subCategories) // iterate through the already added sC's
            {
                // collision only possible within a category
                if (sC.category == sCToCheck.category)
                {
                    if (compareFilterLists(sC.filters, sCToCheck.filters)) // check for duplicated filters
                    {
                        Debug.Log("[Filter Extensions] " + sC.subCategoryTitle + " has duplicated the filters of " + sCToCheck.subCategoryTitle);
                        return false; // ignore this subCategory, only the first processed sC in a conflict will get through
                    }
                    else if (sC.subCategoryTitle == sCToCheck.subCategoryTitle) // if they have the same name, just add the new filters on (OR'd together)
                    {
                        Debug.Log("[Filter Extensions] " + sC.subCategoryTitle + " has multiple entries. Filters are being combined");
                        sCToCheck.filters.AddRange(sC.filters);
                        return false; // all other elements of this list have already been check for this condition. Don't need to continue
                    }
                }
            }
            return true;
        }

        private bool compareFilterLists(List<Filter> filterListA, List<Filter> filterListB) 
        {//can't just compare directly because order could be different, hence this ugly mess
            if (filterListA.Count == 0 || filterListB.Count == 0)
                return false;

            if (filterListA.Count != filterListB.Count)
                return false;

            foreach(Filter fA in filterListA)
            {
                bool match = false;
                foreach (Filter fB in filterListB)
                {
                    match = compareCheckLists(fA.checks, fB.checks, new CheckEqualityComparer());
                    if (match)
                        break;
                }

                if (!match)
                    return false;
            }
            return true;
        }

        private bool compareCheckLists<T>(List<T> listA, List<T> listB, IEqualityComparer<T> comparer)
        {
            if (listA.Count != listB.Count)
                return false;

            Dictionary<T, int> cntDict = new Dictionary<T, int>(comparer);
            foreach (T t in listA)
            {
                if (cntDict.ContainsKey(t))
                    cntDict[t]++;
                else
                    cntDict.Add(t, 1);
            }
            foreach (T s in listB)
            {
                if (cntDict.ContainsKey(s))
                    cntDict[s]--;
                else
                    return false;
            }
            return cntDict.Values.All(c => c == 0);
        }

        private void checkIcons(PartCategorizer.Category category)
        {
            foreach (PartCategorizer.Category c in category.subcategories)
            {
                // if any of the names of the loaded icons match the subCategory name, then replace their current icon with the match
                if (iconDict.ContainsKey(c.button.categoryName))
                    c.button.SetIcon(getIcon(c.button.categoryName));
            }
        }

        private void loadIcons()
        {
            List<GameDatabase.TextureInfo> texList = GameDatabase.Instance.databaseTexture.Where(t => t.texture != null).ToList();
            Dictionary<string, GameDatabase.TextureInfo> texDict = new Dictionary<string, GameDatabase.TextureInfo>();

            texList.RemoveAll(t => t.texture.width > 40 || t.texture.width < 25 || t.texture.height > 40 || t.texture.height < 25);
            
            // using a dictionary for looking up _selected textures. Else the list has to be iterated over for every texture
            foreach(GameDatabase.TextureInfo t in texList)
            {
                if (!texDict.ContainsKey(t.name))
                    texDict.Add(t.name, t);
            }

            foreach (GameDatabase.TextureInfo t in texList)
            {
                bool simple = false;
                Texture2D selectedTex = null;

                if (texDict.ContainsKey(t.name + "_selected"))
                    selectedTex = texDict[t.name + "_selected"].texture;
                else
                {
                    selectedTex = t.texture;
                    simple = true;
                }

                string[] name = t.name.Split(new char[] { '/', '\\' });
                PartCategorizer.Icon icon = new PartCategorizer.Icon(name[name.Length - 1], t.texture, selectedTex, simple);
                
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

        // check for empty subCategories. Only does 1k checks per frame to avoid any crazy overhead for users with lots of parts and categories
        IEnumerator checkForEmptySubCategories()
        {
            int i = 0;
            List<customSubCategory> notEmpty = new List<customSubCategory>();

            foreach (customSubCategory sC in subCategories)
            {
                foreach (AvailablePart p in PartLoader.Instance.parts)
                {
                    i++;
                    if (i > 1000)
                    {
                        i = 0;
                        yield return null;
                    }

                    if (sC.checkFilters(p))
                    {
                        notEmpty.Add(sC);
                        break;
                    }
                }
            }
            subCategories = notEmpty;
        }
    }
}
