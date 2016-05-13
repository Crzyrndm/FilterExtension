using System;
using System.Collections.Generic;
using System.Linq;


namespace FilterExtensions
{
    using ConfigNodes;
    using UnityEngine;
    using Utility;

    using KSP.UI.Screens;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Core : MonoBehaviour
    {
        public static readonly Version version = new Version(2, 5, 3, 0);
        
        private static Core instance;
        public static Core Instance
        {
            get
            {
                return instance;
            }
        }

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
        // Dictionary of all filtering part modules by part name
        public Dictionary<string, PartModuleFilter> filterModules = new Dictionary<string, PartModuleFilter>();


        // Config has options to disable the FbM replacement, and the default Category/SC and sort method


        const string fallbackIcon = "stockIcon_fallback";

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
            Log(string.Empty, LogLevel.Warn);

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
            Settings.LoadSettings();

            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("FilterRename");
            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode node = nodes[i];
                string[] names = node.GetValues("name");
                for (int j = 0; j < names.Length; j++)
                {
                    string[] s = names[j].Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Trim()).ToArray();
                    if (s.Length >= 2 && !Rename.ContainsKey(s[0]))
                            Rename.Add(s[0], s[1]);
                }
            }

            nodes = GameDatabase.Instance.GetConfigNodes("FilterSetIcon");
            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode node = nodes[i];
                string[] icons = node.GetValues("icon");
                for (int j = 0; j < icons.Length; j++)
                {
                    string[] s = icons[j].Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Trim()).ToArray();
                    if (s.Length >= 2 && !setIcon.ContainsKey(s[0]))
                        setIcon.Add(s[0], s[1]);
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
                if (!string.IsNullOrEmpty(p.partUrl))
                {
                    // list of GameData folders
                    modNames.AddUnique(p.partUrl.Split(new char[] { '/', '\\' })[0]);

                    // associate the path to the part
                    if (!partPathDict.ContainsKey(p.name))
                        partPathDict.Add(p.name, p.partUrl);
                    else
                        Log(p.name + " duplicated part key in part path dictionary", LogLevel.Warn);

                    if (PartType.isEngine(p))
                        processEnginePropellants(p);

                    if (p.partPrefab.Resources != null)
                    {
                        foreach (PartResource r in p.partPrefab.Resources)
                            resources.AddUnique(r.resourceName);
                    }
                }
                if (p.partPrefab.Modules.Contains<PartModuleFilter>())
                    filterModules.Add(p.name, p.partPrefab.Modules.GetModule<PartModuleFilter>());
            }
            generateEngineTypes();

            if (Settings.replaceFbM)
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
                        if (customSubCategory.checkForCheckMatch(subcategory, Check.CheckType.resource, s))
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
            List<ModuleEngines> engines = p.partPrefab.Modules.GetModules<ModuleEngines>();
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
        /// create the subcategories for each unique propellant combination found
        /// </summary>
        private void generateEngineTypes()
        {
            List<subCategoryItem> engines = new List<subCategoryItem>();
            for (int i = 0; i < propellantCombos.Count; i++)
            {
                string[] ls = propellantCombos[i].ToArray();
                string propList = string.Join(",", ls);

                List<Check> checks = new List<Check>();
                checks.Add(new Check("propellant", propList, Exact:true)); //, true, false)); // exact match to propellant list. Nothing extra, nothing less

                string name = propList;
                string icon = propList;
                SetNameAndIcon(ref name, ref icon);

                if (!string.IsNullOrEmpty(name) && !subCategoriesDict.ContainsKey(name))
                {
                    customSubCategory sC = new customSubCategory(name, icon);

                    Filter f = new Filter(false);
                    f.checks = checks;
                    sC.filters.Add(f);
                    subCategoriesDict.Add(name, sC);
                }
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
        /// mark all subcategories that have identical filtering
        /// </summary>
        private void checkAndMarkConflicts()
        {
            // Can't guarantee iteration order of dict will be the same each time so need a set of elements that have been processed
            // to ensure conflicts are only checked against elements that are already checked
            // by only checking against processed elements we know we're only adding checking for collisions between each pair once
            List<string> processedElements = new List<string>();
            foreach (KeyValuePair<string, customSubCategory> kvpOuter in subCategoriesDict)
            {
                foreach (string subcatName in processedElements)
                {
                    customSubCategory processedSubcat = subCategoriesDict[subcatName];
                    if (Filter.compareFilterLists(processedSubcat.filters, kvpOuter.Value.filters))
                    {
                        // add conflict entry for the already entered subCategory
                        List<string> conflicts;
                        if (conflictsDict.TryGetValue(subcatName, out conflicts))
                            conflicts.Add(kvpOuter.Key);
                        else
                            conflictsDict.Add(subcatName, new List<string>() { kvpOuter.Key });

                        // add a conflict entry for the new subcategory
                        if (conflictsDict.TryGetValue(kvpOuter.Key, out conflicts))
                            conflicts.Add(subcatName);
                        else
                            conflictsDict.Add(kvpOuter.Key, new List<string>() { subcatName });
                    }
                }
                processedElements.Add(kvpOuter.Key);
            }
        }

        /// <summary>
        /// loads all textures that are 32x32px into a dictionary using the filename as a key
        /// </summary>
        private static void loadIcons()
        {
            GameDatabase.TextureInfo[] texArray = GameDatabase.Instance.databaseTexture.Where(t => t.texture != null && t.texture.height == 32 && t.texture.width == 32).ToArray();
            Dictionary<string, GameDatabase.TextureInfo> texDict = texArray.ToDictionary(k => k.name);

            Texture2D selectedTex = null;
            foreach (GameDatabase.TextureInfo t in texArray)
            {
                GameDatabase.TextureInfo texInfo;
                if (texDict.TryGetValue(t.name + "_selected", out texInfo))
                    selectedTex = texInfo.texture;
                else
                    selectedTex = t.texture;

                string name = t.name.Split(new char[] { '/', '\\' }).Last();
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

        public enum LogLevel
        {
            Log,
            Warn,
            Error
        }

        /// <summary>
        /// Debug.Log with FE id/version inserted
        /// </summary>
        /// <param name="o"></param>
        internal static void Log(object o, LogLevel level = LogLevel.Log)
        {
            if (level == LogLevel.Log)
                Debug.LogFormat("[Filter Extensions {0}]: {1}", version, o);
            else if (level == LogLevel.Warn)
                Debug.LogWarningFormat("[Filter Extensions {0}]: {1}", version, o);
            else
                Debug.LogErrorFormat("[Filter Extensions {0}]: {1}", version, o);
        }

        internal static void Log(string format, LogLevel level = LogLevel.Log, params object[] o)
        {
            if (level == LogLevel.Log)
                Debug.LogFormat("[Filter Extensions {0}]: {1}", version, string.Format(format, o));
            else if (level == LogLevel.Warn)
                Debug.LogWarningFormat("[Filter Extensions {0}]: {1}", version, string.Format(format, o));
            else
                Debug.LogErrorFormat("[Filter Extensions {0}]: {1}", version, string.Format(format, o));
        }
    }
}