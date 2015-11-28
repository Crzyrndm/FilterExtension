using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;


namespace FilterExtensions
{
    using UnityEngine;
    using Utility;
    using ConfigNodes;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Core : MonoBehaviour
    {
        private static Core instance;

        // storing categories loaded at Main Menu for creation when entering SPH/VAB
        public List<customCategory> Categories = new List<customCategory>();
        // storing all subCategory definitions for categories to reference
        public Dictionary<string, customSubCategory> subCategoriesDict = new Dictionary<string, customSubCategory>();
        // all subcategories with duplicated filters
        public Dictionary<string, List<string>> conflictsDict = new Dictionary<string, List<string>>();
        // renaming categories
        public Dictionary<string, string> Rename = new Dictionary<string, string>();
        // icons for categories
        public Dictionary<string, string> setIcon = new Dictionary<string, string>();
        // removing categories
        public HashSet<string> removeSubCategory = new HashSet<string>();
        // url for each part by internal name
        public Dictionary<string, string> partPathDict = new Dictionary<string, string>();
        // entry for each unique combination of propellants
        public List<List<string>> propellantCombos = new List<List<string>>();
        // entry for each unique resource
        public List<string> resources = new List<string>();

        // Dictionary of icons created on entering the main menu
        public Dictionary<string, RUI.Icons.Selectable.Icon> iconDict = new Dictionary<string, RUI.Icons.Selectable.Icon>();

        // Config has options to disable the FbM replacement, and the default Category/SC and sort method
        public bool hideUnpurchased = true;
        public bool debug = false;
        public bool setAdvanced = true;
        public bool replaceFbM = true;
        public string categoryDefault = "";
        public string subCategoryDefault = "";

        const string fallbackIcon = "stockIcon_fallback";

        public static Core Instance
        {
            get
            {
                return instance;
            }
        }

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
            Log("Version 2.4.1");

            getConfigs();
            getPartData();
            processFilterDefinitions();
            loadIcons();
            checkAndMarkConflicts();
        }

        /// <summary>
        /// Loads the settings, rename, set icon, and deletion data into an actionable format
        /// </summary>
        private void getConfigs()
        {
            ConfigNode settings = GameDatabase.Instance.GetConfigNodes("FilterSettings").FirstOrDefault();
            if (settings != null)
            {
                bool.TryParse(settings.GetValue("hideUnpurchased"), out hideUnpurchased);
                bool.TryParse(settings.GetValue("debug"), out debug);
                if (!bool.TryParse(settings.GetValue("setAdvanced"), out setAdvanced))
                    setAdvanced = true;
                if (!bool.TryParse(settings.GetValue("replaceFbM"), out replaceFbM))
                    replaceFbM = true;
                categoryDefault = settings.GetValue("categoryDefault");
                subCategoryDefault = settings.GetValue("subCategoryDefault");
            }

            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("FilterRename");
            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode node = nodes[i];
                string[] names = node.GetValues("name");
                for (int j = 0; j < names.Length; j++)
                {
                    string s = names[j];
                    if (s.Split().Length >= 2)
                    {
                        string nameToReplace = s.Split(',')[0].Trim();
                        string newName = s.Split(',')[1].Trim();
                        if (!Rename.ContainsKey(nameToReplace))
                            Rename.Add(nameToReplace, newName);
                    }
                }
            }

            nodes = GameDatabase.Instance.GetConfigNodes("FilterSetIcon");
            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode node = nodes[i];
                string[] icons = node.GetValues("icon");
                for (int j = 0; j < icons.Length; j++)
                {
                    string s = icons[j];
                    if (s.Split().Length >= 2)
                    {
                        string categoryName = s.Split(',')[0].Trim();
                        string icon = s.Split(',')[1].Trim();
                        if (!setIcon.ContainsKey(categoryName))
                            setIcon.Add(categoryName, icon);
                    }
                }
            }

            nodes = GameDatabase.Instance.GetConfigNodes("FilterRemove");
            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode node = nodes[i];
                string[] toRemove = node.GetValues("remove");
                for (int j = 0; j < toRemove.Length; j++)
                {
                    string s = toRemove[j].Trim();
                    if (string.IsNullOrEmpty(s))
                        continue;
                    removeSubCategory.Add(s); // hashset doesn't need duplicate check
                }
            }
        }

        /// <summary>
        /// generate the associations between parts and folders, create all the mod categories, get all propellant combinations,
        /// </summary>
        private void getPartData()
        {
            List<string> modNames = new List<string>();

            for (int i = 0; i < PartLoader.Instance.parts.Count; i++)
            {
                AvailablePart p = PartLoader.Instance.parts[i];
                if (p == null)
                    continue;
                
                if (string.IsNullOrEmpty(p.partUrl))
                    RepairAvailablePartUrl(p);
                
                // if the url is still borked, can't associate a mod to the part
                if (string.IsNullOrEmpty(p.partUrl))
                    continue;
                
                // list of GameData folders
                modNames.AddUnique(p.partUrl.Split(new char[] { '/', '\\' })[0]);

                // associate the path to the part
                if (!partPathDict.ContainsKey(p.name))
                    partPathDict.Add(p.name, p.partUrl);
                else
                    Log(p.name + " duplicated part key in part path dictionary");

                if (PartType.isEngine(p))
                    processEnginePropellants(p);

                if (p.partPrefab.Resources != null)
                {
                    foreach (PartResource r in p.partPrefab.Resources)
                        resources.AddUnique(r.resourceName);
                }
            }
            
            if (replaceFbM)
                processFilterByManufacturer(modNames);
        }

        /// <summary>
        /// turn the loaded category and subcategory nodes into useable data
        /// </summary>
        private void processFilterDefinitions()
        {
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("CATEGORY");
            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode node = nodes[i];
                customCategory C = new customCategory(node);
                if (C.subCategories == null)
                    continue;
                if (!Categories.Any(n => n.categoryName == C.categoryName))
                    Categories.Add(C);
            }
            
            //load all subCategory configs
            nodes = GameDatabase.Instance.GetConfigNodes("SUBCATEGORY");
            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode node = nodes[i];
                customSubCategory sC = new customSubCategory(node);
                if (!sC.hasFilters || string.IsNullOrEmpty(sC.subCategoryTitle))
                    continue;
                
                customSubCategory subcategory;
                if (subCategoriesDict.TryGetValue(sC.subCategoryTitle, out subcategory)) // if something does have the same title
                    subcategory.filters.AddRange(sC.filters);
                else // if nothing else has the same title
                    subCategoriesDict.Add(sC.subCategoryTitle, sC);
            }
            
            customCategory Cat = Categories.Find(C => C.categoryName == "Filter by Resource");
            if (Cat != null)
            {
                for (int i = 0; i < resources.Count; i++)
                {
                    string s = resources[i];
                    // add spaces before each capital letter
                    string name = System.Text.RegularExpressions.Regex.Replace(s, @"\B([A-Z])", " $1");

                    customSubCategory subcategory;
                    if (subCategoriesDict.TryGetValue(name, out subcategory))
                    {
                        // if the collision is already looking for the specified resource
                        if (customSubCategory.checkForCheckMatch(subcategory, CheckType.resource, s))
                            continue;
                        name = "res_" + name;
                    }
                    if (!string.IsNullOrEmpty(name) && !subCategoriesDict.ContainsKey(name))
                    {
                        customSubCategory sC = new customSubCategory(name, name);
                        Check c = new Check("resource", s);
                        Filter f = new Filter(false);
                        f.checks.Add(c);
                        sC.filters.Add(f);
                        subCategoriesDict.Add(name, sC);
                    }
                    if (!string.IsNullOrEmpty(name))
                        Cat.subCategories.AddUnique(new subCategoryItem(name));
                }
            }

            for (int i = 0; i < Categories.Count; i++)
            {
                customCategory C = Categories[i];
                if (C == null || !C.all)
                    continue;

                List<Filter> filterList = new List<Filter>();
                if (C.subCategories != null)
                {
                    for (int j = 0; j < C.subCategories.Count; j++)
                    {
                        subCategoryItem s = C.subCategories[j];
                        if (s == null)
                            continue;

                        customSubCategory subcategory;
                        if (subCategoriesDict.TryGetValue(s.subcategoryName, out subcategory))
                            filterList.AddUniqueRange(subcategory.filters);
                    }
                }
                customSubCategory newSub = new customSubCategory("All parts in " + C.categoryName, C.iconName);
                newSub.filters = filterList;
                subCategoriesDict.Add(newSub.subCategoryTitle, newSub);
                C.subCategories.Insert(0, new subCategoryItem(newSub.subCategoryTitle));
            }
        }

        /// <summary>
        /// check for a unique propellant combination and add to the list if one is found
        /// </summary>
        /// <param name="p"></param>
        private void processEnginePropellants(AvailablePart p)
        {
            List<ModuleEngines> engines = p.partPrefab.GetModules<ModuleEngines>();
            for (int i = 0; i < engines.Count; i++)
            {
                ModuleEngines e = engines[i];
                List<string> propellants = new List<string>();
                for (int j = 0; j < e.propellants.Count; j++)
                    propellants.Add(e.propellants[j].name);
                propellants.Sort();

                if (!stringListComparer(propellants))
                    propellantCombos.Add(propellants);
            }
        }

        /// <summary>
        /// create the subcategories for filter by manufacturer by discovered GameData folder
        /// </summary>
        /// <param name="modNames"></param>
        private void processFilterByManufacturer(List<string> modNames)
        {
            // define the mod subcategories
            List<string> subCatNames = new List<string>();
            for (int i = 0; i < modNames.Count; i++)
            {
                string name = modNames[i];
                if (subCategoriesDict.ContainsKey(modNames[i]))
                    name = "mod_" + name;
                string icon = name;
                SetNameAndIcon(ref name, ref icon);

                if (!subCategoriesDict.ContainsKey(name))
                {
                    subCatNames.Add(name);

                    Check ch = new Check("folder", modNames[i]);
                    Filter f = new Filter(false);
                    customSubCategory sC = new customSubCategory(name, icon);

                    f.checks.Add(ch);
                    sC.filters.Add(f);
                    subCategoriesDict.Add(name, sC);
                }
            }

            customCategory fbm = Categories.FirstOrDefault(C => C.categoryName == "Filter by Manufacturer");
            if (fbm == null)
            {
                ConfigNode manufacturerSubs = new ConfigNode("SUBCATEGORIES");
                for (int i = 0; i < subCatNames.Count; i++)
                    manufacturerSubs.AddValue("list", i.ToString() + "," + subCatNames[i]);

                ConfigNode filterByManufacturer = new ConfigNode("CATEGORY");
                filterByManufacturer.AddValue("name", "Filter by Manufacturer");
                filterByManufacturer.AddValue("type", "stock");
                filterByManufacturer.AddValue("value", "replace");
                filterByManufacturer.AddNode(manufacturerSubs);
                Categories.Add(new customCategory(filterByManufacturer));
            }
            else
            {
                for (int i = 0; i < modNames.Count; i++)
                    fbm.subCategories.AddUnique(new subCategoryItem(modNames[i])); // append the mod names
            }
        }

        /// <summary>
        /// returns true if the list passed exactly matches an entry already in propellantCombos
        /// </summary>
        /// <param name="propellants"></param>
        /// <returns></returns>
        private bool stringListComparer(List<string> propellants)
        {
            for (int i = 0; i < propellantCombos.Count; i++)
            {
                List<string> ls = propellantCombos[i];
                if (propellants.Count == ls.Count && !propellants.Except(ls).Any())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// refresh the visible subcategories to ensure all changes are visible
        /// </summary>
        public static void setSelectedCategory()
        {
            try
            {
                PartCategorizer.Category Filter = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.activeButton.State == RUIToggleButtonTyped.ButtonState.TRUE);
                if (Filter != null)
                    Filter.button.activeButton.SetFalse(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
                
                Filter = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == instance.categoryDefault);
                if (Filter != null)
                    Filter.button.activeButton.SetTrue(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
                else
                {
                    Filter = PartCategorizer.Instance.filters[0];
                    if (Filter != null)
                    {
                        Filter.button.activeButton.SetTrue(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
                    }
                }

                // set the subcategory button
                //Filter = Filter.subcategories.FirstOrDefault(sC => sC.button.categoryName == instance.subCategoryDefault);
                //if (Filter != null && Filter.button.activeButton.State != RUIToggleButtonTyped.ButtonState.TRUE)
                //    Filter.button.activeButton.SetTrue(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
            }
            catch (Exception e)
            {
                Log("Category refresh failed");
                Log(e.InnerException);
                Log(e.StackTrace);
            }
        }

        /// <summary>
        /// mark all subcategories that have identical filtering
        /// </summary>
        private void checkAndMarkConflicts()
        {
            // Can't guarantee iteration order of dict will be the same each time so need a set of elements that have been processed
            // to ensure conflicts are only checked against elements that are already checked
            // by only checking against processed elements we know we're only adding checking for collisions between each pair once
            HashSet<string> processedElements = new HashSet<string>();
            foreach (KeyValuePair<string, customSubCategory> kvpOuter in subCategoriesDict)
            {
                foreach (KeyValuePair<string, customSubCategory> kvp in subCategoriesDict) // iterate through the already added sC's
                {
                    if (kvp.Key == kvpOuter.Key || !processedElements.Contains(kvp.Key))
                        continue;
                    if (Filter.compareFilterLists(kvp.Value.filters, kvpOuter.Value.filters)) // check for duplicated filters
                    {
                        // add conflict entry for the already entered subCategory
                        List<string> conflicts;
                        if (conflictsDict.TryGetValue(kvp.Key, out conflicts))
                            conflicts.Add(kvpOuter.Key);
                        else
                            conflictsDict.Add(kvp.Key, new List<string>() { kvpOuter.Key});

                        // add a conflict entry for the new subcategory
                        if (conflictsDict.TryGetValue(kvpOuter.Key, out conflicts))
                            conflicts.Add(kvp.Key);
                        else
                            conflictsDict.Add(kvpOuter.Key, new List<string>() { kvp.Key });
                    }
                }
                processedElements.Add(kvpOuter.Key);
            }
        }

        /// <summary>
        /// checks all subcategories and edits their names/icons if required
        /// </summary>
        public void namesAndIcons(PartCategorizer.Category category)
        {
            HashSet<string> toRemove = new HashSet<string>();
            foreach (PartCategorizer.Category c in category.subcategories)
            {
                if (removeSubCategory.Contains(c.button.categoryName))
                    toRemove.Add(c.button.categoryName);
                else
                {
                    string tmp;
                    if (Rename.TryGetValue(c.button.categoryName, out tmp)) // update the name first
                        c.button.categoryName = tmp;

                    RUI.Icons.Selectable.Icon icon;
                    if (tryGetIcon(tmp, out icon) || tryGetIcon(c.button.categoryName, out icon)) // if there is an explicit setIcon for the subcategory or if the name matches an icon
                        c.button.SetIcon(icon); // change the icon
                }
            }
            category.subcategories.RemoveAll(c => toRemove.Contains(c.button.categoryName));
        }

        /// <summary>
        /// loads all textures between 25 and 40 px in dimensions into a dictionary using the filename as a key
        /// </summary>
        private static void loadIcons()
        {
            List<GameDatabase.TextureInfo> texList = GameDatabase.Instance.databaseTexture.Where(t => t.texture != null && t.texture.height <= 40 && t.texture.width <= 40 && t.texture.width >= 25 && t.texture.height >= 25).ToList();

            Dictionary<string, GameDatabase.TextureInfo> texDict = new Dictionary<string, GameDatabase.TextureInfo>();
            // using a dictionary for looking up _selected textures. Else the list has to be iterated over for every texture
            foreach(GameDatabase.TextureInfo t in texList)
            {
                if (!texDict.ContainsKey(t.name))
                    texDict.Add(t.name, t);
                else
                {
                    int i = 1;
                    while (texDict.ContainsKey(t.name + i.ToString()) && i < 1000)
                        i++;
                    if (i < 1000)
                    {
                        texDict.Add(t.name + i.ToString(), t);
                        Log(t.name + i.ToString());
                    }
                }
            }

            Texture2D selectedTex = null;
            foreach (GameDatabase.TextureInfo t in texList)
            {
                GameDatabase.TextureInfo texInfo;
                if (texDict.TryGetValue(t.name + "_selected", out texInfo))
                    selectedTex = texInfo.texture;
                else
                    selectedTex = t.texture;

                string name = t.name.Split(new char[] { '/', '\\' }).Last();
                if (Instance.iconDict.ContainsKey(name))
                {
                    int i = 1;
                    while (Instance.iconDict.ContainsKey(name + i.ToString()) && i < 1000)
                        i++;
                    if (i != 1000)
                        name = name + i.ToString();
                    if (instance.debug)
                        Log("Duplicated texture name \"" + t.name.Split(new char[] { '/', '\\' }).Last() + "\" at:\r\n" + t.name + "\r\n New reference is: " + name);
                }

                RUI.Icons.Selectable.Icon icon = new RUI.Icons.Selectable.Icon(name, t.texture, selectedTex, false);
                Instance.iconDict.TryAdd(icon.name, icon);
            }
        }

        /// <summary>
        /// get the icon that matches a name
        /// </summary>
        /// <param name="name">the icon name</param>
        /// <returns>the icon if it is found, or the fallback icon if it is not</returns>
        public static RUI.Icons.Selectable.Icon getIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
                return PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];

            RUI.Icons.Selectable.Icon icon;
            if (Instance.iconDict.TryGetValue(name, out icon) || PartCategorizer.Instance.iconLoader.iconDictionary.TryGetValue(name, out icon))
                return icon;
            return PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];
        }

        /// <summary>
        /// get icon following the TryGet* syntax
        /// </summary>
        /// <param name="name">the icon name</param>
        /// <param name="icon">the icon that matches the name, or the fallback if no matches were found</param>
        /// <returns>true if a matching icon was found, false if fallback was required</returns>
        public static bool tryGetIcon(string name, out RUI.Icons.Selectable.Icon icon)
        {
            if (string.IsNullOrEmpty(name))
            {
                icon = PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];
                return false;
            }
            if (Instance.iconDict.TryGetValue(name, out icon))
                return true;
            if (PartCategorizer.Instance.iconLoader.iconDictionary.TryGetValue(name, out icon))
                return true;
            icon = PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];
            return false;
        }

        // credit to EvilReeperx for this lifesaving function
        /// <summary>
        /// Fills in the part url which KSP strips after loading is complete
        /// </summary>
        /// <param name="ap">the part to add the url back to</param>
        private void RepairAvailablePartUrl(AvailablePart ap)
        {
            UrlDir.UrlConfig url = GameDatabase.Instance.GetConfigs("PART").FirstOrDefault(u => u.name.Replace('_', '.') == ap.name);
            if (url != null)
                ap.partUrl = url.url;
        }

        /// <summary>
        /// if a subcategory doesn't have any parts, it shouldn't be used. Doesn't account for the blackListed parts the first time the editor is entered
        /// </summary>
        /// <param name="sC">the subcat to check</param>
        /// <param name="category">the category for logging purposes</param>
        /// <returns>true if the subcategory contains any parts</returns>
        public static bool checkSubCategoryHasParts(customSubCategory sC, string category)
        {
            for (int i = 0; i < PartLoader.Instance.parts.Count; i++)
            {
                if (sC.checkFilters(PartLoader.Instance.parts[i]))
                    return true;
            }

            if (instance.debug)
            {
                if (!string.IsNullOrEmpty(category))
                    Log(sC.subCategoryTitle + " in category " + category + " has no valid parts and was not initialised");
                else
                    Log(sC.subCategoryTitle + " has no valid parts and was not initialised");
            }
            return false;
        }

        /// <summary>
        /// check the name and icon against the sets for renaming and setting a different icon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        public void SetNameAndIcon(ref string name, ref string icon)
        {
            string tmp;
            if (Rename.TryGetValue(name, out tmp))
                name = tmp;
            if (setIcon.TryGetValue(name, out tmp))
                icon = tmp;
        }

        /// <summary>
        /// Debug.Log with FE id inserted
        /// </summary>
        /// <param name="o"></param>
        internal static void Log(object o)
        {
            Debug.Log("[Filter Extensions] " + o);
        }
    }
}