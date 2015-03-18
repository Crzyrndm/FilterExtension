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
        internal List<customCategory> Categories = new List<customCategory>();
        // storing all subCategory definitions for categories to reference
        internal Dictionary<string, customSubCategory> subCategoriesDict = new Dictionary<string, customSubCategory>();
        // all subcategories with duplicated filters
        public Dictionary<string, List<string>> conflictsDict = new Dictionary<string, List<string>>();

        // url for each part by internal name
        public static Dictionary<string, string> partPathDict = new Dictionary<string, string>();
        // entry for each unique combination of propellants
        public static List<List<string>> propellantCombos = new List<List<string>>();
        // entry for each unique resource
        public static List<string> resources = new List<string>();

        // Dictionary of icons created on entering the main menu
        public static Dictionary<string, PartCategorizer.Icon> iconDict = new Dictionary<string, PartCategorizer.Icon>();

        // Config has options to disable the FbM replacement, and the default Category/SC and sort method
        public KSP.IO.PluginConfiguration config;

        public static Core Instance // Reminder to self, don't be abusing static
        {
            get
            {
                return instance;
            }
        }

        void Awake()
        {
            instance = this;
            Log("Version 2.0 alpha2");

            config = KSP.IO.PluginConfiguration.CreateForType<Core>();
            config.load();

            // generate the associations between parts and folders, create all the mod categories, get all propellant combinations,
            getPartData();
            
            // mod categories key: title, value: folder
            // used for adding the folder check to subCategories
            Dictionary<string, string> folderToCategoryDict = new Dictionary<string, string>();
            // load all category configs
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CATEGORY"))
            {
                customCategory C = new customCategory(node);
                if (Categories.Find(n => n.categoryName == C.categoryName) == null)
                {
                    Categories.Add(C);
                    if (C.type == "mod" && C.value != null)
                    {
                        if (!folderToCategoryDict.ContainsKey(C.categoryName))
                            folderToCategoryDict.Add(C.categoryName, C.value.Trim());
                    }
                }
            }

            //load all subCategory configs
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SUBCATEGORY"))
            {
                customSubCategory sC = new customSubCategory(node);
                if (sC.hasFilters)
                {
                    foreach (Filter f in sC.filters)
                    {
                        if (folderToCategoryDict.ContainsKey(sC.subCategoryTitle))
                            f.checks.Add(new Check("folder", folderToCategoryDict[sC.subCategoryTitle]));
                    }
                    if (sC.subCategoryTitle != null)
                    {
                        if (!subCategoriesDict.ContainsKey(sC.subCategoryTitle)) // if nothing else has the same title
                            subCategoriesDict.Add(sC.subCategoryTitle, sC);
                        else if (subCategoriesDict.ContainsKey(sC.subCategoryTitle)) // if something does have the same title
                            subCategoriesDict[sC.subCategoryTitle].filters.AddRange(sC.filters);
                    }
                }
            }

            foreach (string s in resources)
            {
                // add spaces before each capital letter
                string name = System.Text.RegularExpressions.Regex.Replace(s, @"\B([A-Z])", " $1");
                if (name != null && !subCategoriesDict.ContainsKey(name))
                {
                    customSubCategory sC = new customSubCategory(name, "");
                    Check c = new Check("resource", s);
                    Filter f = new Filter(false);
                    f.checks.Add(c);
                    sC.filters.Add(f);
                    subCategoriesDict.Add(name, sC);
                }
            }

            foreach (customCategory C in Categories)
            {// generating the "all parts in ..." subcategories
                if (!C.all)
                    continue;

                List<Filter> filterList = new List<Filter>();
                foreach (string s in C.subCategories)
                {
                    if (s != null && subCategoriesDict.ContainsKey(s))
                        filterList.AddUniqueRange(subCategoriesDict[s].filters);
                }

                customSubCategory newSub = new customSubCategory("All parts in " + C.categoryName, C.iconName);
                newSub.filters = filterList;
                subCategoriesDict.Add(newSub.subCategoryTitle, newSub);

                List<string> subCategories = new List<string>() { newSub.subCategoryTitle };
                subCategories.AddUniqueRange(C.subCategories);
                C.subCategories = subCategories.ToArray();
            }
            loadIcons();
            checkAndMarkConflicts();
        }

        private void getPartData()
        {
            List<string> modNames = new List<string>();

            foreach (AvailablePart p in PartLoader.Instance.parts)
            {
                // don't want dummy parts, roids, etc. (need to make MM configs for mods that use this category)
                if (p == null || p.category == PartCategories.none)
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
                        Log(p.name + " duplicated part key in part path dictionary");
                }

                if (PartType.isEngine(p))
                    processEnginePropellants(p);

                if (p.partPrefab.Resources != null)
                    foreach (PartResource r in p.partPrefab.Resources)
                        resources.AddUnique(r.resourceName);
            }
            bool FbM = config.GetValue("replaceFbM", true);
            config["replaceFbM"] = FbM;
            if (FbM)
                processFilterByManufacturer(modNames);
        }

        private void processEnginePropellants(AvailablePart p)
        {
            foreach (ModuleEngines e in p.partPrefab.GetModuleEngines())
            {
                List<string> propellants = new List<string>();
                foreach (Propellant prop in e.propellants)
                    propellants.Add(prop.name);
                propellants.Sort();

                if (!stringListComparer(propellants))
                    propellantCombos.Add(propellants);
            }
            foreach (ModuleEnginesFX ex in p.partPrefab.GetModuleEnginesFx())
            {
                List<string> propellants = new List<string>();
                foreach (Propellant prop in ex.propellants)
                    propellants.Add(prop.name);
                propellants.Sort();

                if (!stringListComparer(propellants))
                    propellantCombos.Add(propellants);
            }
        }

        private void processFilterByManufacturer(List<string> modNames)
        {
            customCategory fbm = Categories.FirstOrDefault(C => C.categoryName == "Filter by Manufacturer");
            // define the mod subcategories
            for (int i = 0; i < modNames.Count; i++)
            {
                Check ch = new Check("folder", modNames[i]);
                Filter f = new Filter(false);
                customSubCategory sC = new customSubCategory(modNames[i], modNames[i]);

                f.checks.Add(ch);
                sC.filters.Add(f);
                if (!subCategoriesDict.ContainsKey(modNames[i]))
                    subCategoriesDict.Add(modNames[i], sC);
            }

            // if there's nothing defined for fbm, create the category
            if (fbm == null)
            {
                ConfigNode manufacturerSubs = new ConfigNode("SUBCATEGORIES");
                for (int i = 0; i < modNames.Count; i++)
                    manufacturerSubs.AddValue("list", i.ToString() + "," + modNames[i]);

                ConfigNode filterByManufacturer = new ConfigNode("CATEGORY");
                filterByManufacturer.AddValue("name", "Filter by Manufacturer");
                filterByManufacturer.AddValue("type", "stock");
                filterByManufacturer.AddValue("value", "replace");
                filterByManufacturer.AddNode(manufacturerSubs);
                Categories.Add(new customCategory(filterByManufacturer));
            }
            else
                fbm.subCategories.AddUniqueRange(modNames); // append the mod names
        }

        private bool stringListComparer(List<string> propellants)
        {
            foreach (List<string> ls in propellantCombos)
            {
                if (propellants.Count == ls.Count)
                {
                    List<string> tmp = propellants.Except(ls).ToList();
                    if (!tmp.Any())
                        return true;
                }
            }
            return false;
        }

        internal void editor()
        {
            // Add all the categories
            foreach (customCategory c in Categories)
            {
                if (!c.stockCategory)
                    c.initialise();
            }

            // icon autoloader pass
            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
            {
                checkIcons(c);
            }

            // update icons
            setSelectedCategory();

            // Remove any category with no subCategories (causes major breakages). Removal doesn't actually prevent icon showing (>.<), just breakages
            PartCategorizer.Instance.filters.RemoveAll(c => c.subcategories.Count == 0);

            // reveal categories because why not
            PartCategorizer.Instance.SetAdvancedMode();
        }

        public void setSelectedCategory()
        {
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.activeButton.State == RUIToggleButtonTyped.ButtonState.TRUE);
            if (Filter != null)
                Filter.button.activeButton.SetFalse(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);

            Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == config.GetValue("categoryDefault", "Filter by Function"));
            if (Filter != null)
            {
                Filter.button.activeButton.SetTrue(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
            }
            else
            {
                Filter = PartCategorizer.Instance.filters[0];
                Filter.button.activeButton.SetTrue(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
            }

            Filter = Filter.subcategories.Find(sC => sC.button.categoryName == config.GetValue("subCategoryDefault", "none"));
            if (Filter != null && Filter.button.activeButton.State != RUIToggleButtonTyped.ButtonState.TRUE)
                Filter.button.activeButton.SetTrue(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
        }

        private void checkAndMarkConflicts()
        {
            foreach (KeyValuePair<string, customSubCategory> kvpOuter in subCategoriesDict)
            {
                foreach (KeyValuePair<string, customSubCategory> kvp in subCategoriesDict) // iterate through the already added sC's
                {
                    if (compareFilterLists(kvp.Value.filters, kvpOuter.Value.filters)) // check for duplicated filters
                    {
                        // add conflict entry for the already entered subCategory
                        if (conflictsDict.ContainsKey(kvp.Key))
                            conflictsDict[kvp.Key].Add(kvpOuter.Key);
                        else
                            conflictsDict.Add(kvp.Key, new List<string>() { kvpOuter.Key});

                        // add a conflict entry for the new subcategory
                        if (conflictsDict.ContainsKey(kvpOuter.Key))
                            conflictsDict[kvpOuter.Key].Add(kvp.Key);
                        else
                            conflictsDict.Add(kvpOuter.Key, new List<string>() { kvp.Key });
                    }
                }
            }
        }

        private bool compareFilterLists(List<Filter> fLA, List<Filter> fLB)
        {
            if (fLA.Count == 0 || fLB.Count == 0)
                return false;

            if (fLA.Count != fLB.Count)
                return false;

            foreach (Filter fA in fLA)
            {
                if (!fLB.Any(fB => fB.Equals(fA)))
                    return false;
            }
            return true;
        }

        private void checkIcons(PartCategorizer.Category category)
        {
            foreach (PartCategorizer.Category c in category.subcategories)
            {
                // if any of the names of the loaded icons match the subCategory name and it didn't get a proper icon
                if (iconDict.ContainsKey(c.button.categoryName) && (c.button.icon == PartCategorizer.Instance.fallbackIcon || !subCategoriesDict.ContainsKey(c.button.categoryName)))
                    c.button.SetIcon(getIcon(c.button.categoryName));
            }
        }

        private static void loadIcons()
        {
            List<GameDatabase.TextureInfo> texList = GameDatabase.Instance.databaseTexture.Where(t => t.texture != null 
                                                                                                && t.texture.height <= 40 && t.texture.width <= 40
                                                                                                && t.texture.width >= 25 && t.texture.height >= 25
                                                                                                ).ToList();

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
                        Log(t.name+i.ToString());
                    }
                }
            }

            foreach (GameDatabase.TextureInfo t in texList)
            {
                Texture2D selectedTex = null;

                if (texDict.ContainsKey(t.name + "_selected"))
                    selectedTex = texDict[t.name + "_selected"].texture;
                else
                    selectedTex = t.texture;

                string name = t.name.Split(new char[] { '/', '\\' }).Last();
                if (iconDict.ContainsKey(name))
                {
                    int i = 1;
                    while (iconDict.ContainsKey(name + i.ToString()) && i < 1000)
                        i++;
                    if (i != 1000)
                        name = name + i.ToString();
                    Log("Duplicated texture name \"" + t.name.Split(new char[] { '/', '\\' }).Last() + "\" at:\r\n" + t.name + "\r\n New reference is: " + name);
                }

                PartCategorizer.Icon icon = new PartCategorizer.Icon(name, t.texture, selectedTex, false);
                
                // shouldn't be neccesary to check, but just in case...
                if (!iconDict.ContainsKey(icon.name))
                    iconDict.Add(icon.name, icon);
            }
        }

        public static PartCategorizer.Icon getIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            if (iconDict.ContainsKey(name))
                return iconDict[name];
            if (PartCategorizer.Instance.iconDictionary.ContainsKey(name))
                return PartCategorizer.Instance.iconDictionary[name];
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

        public static bool checkSubCategoryHasParts(customSubCategory sC)
        {
            foreach (AvailablePart p in PartLoader.Instance.parts)
            {
                if (sC.checkFilters(p))
                {
                    return true;
                }
            }
            Log(sC.subCategoryTitle + " has no valid parts and was not initialised");
            return false;
        }

        internal static void Log(object o)
        {
            Debug.Log("[Filter Extensions] " + o);
        }
    }
}
