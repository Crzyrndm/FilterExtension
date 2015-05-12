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
        // renaming categories not defined by FE
        public Dictionary<string, string> Rename = new Dictionary<string, string>();
        // icons for categories not defined by FE
        public Dictionary<string, string> setIcon = new Dictionary<string, string>();
        // removing cateogries not defined by FE
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
        public bool replaceFbM = true;
        public string categoryDefault;
        public string subCategoryDefault;

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
            DontDestroyOnLoad(this);
            Log("Version 2.1.1");

            // settings, rename, iconset, and subCat removals
            getConfigs();

            // generate the associations between parts and folders, create all the mod categories, get all propellant combinations,
            getPartData();
            
            // load all category configs
            processFilterDefinitions();

            loadIcons();
            checkAndMarkConflicts();
        }

        private void getConfigs()
        {
            ConfigNode settings = GameDatabase.Instance.GetConfigNode("FilterSettings");
            if (settings != null)
            {
                if (!bool.TryParse(settings.GetValue("replaceFbM"), out replaceFbM))
                    replaceFbM = true;
                categoryDefault = settings.GetValue("categoryDefault");
                subCategoryDefault = settings.GetValue("subCategoryDefault");
            }


            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("FilterRename"))
            {
                string[] names = node.GetValues("name");
                foreach (string s in names)
                {
                    if (s.Split().Length >= 2)
                    {
                        string nameToReplace = s.Split(',')[0].Trim();
                        string newName = s.Split(',')[1].Trim();
                        if (!Rename.ContainsKey(nameToReplace))
                            Rename.Add(nameToReplace, newName);
                    }
                }
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("FilterSetIcon"))
            {
                string[] icons = node.GetValues("icon");
                foreach (string s in icons)
                {
                    if (s.Split().Length >= 2)
                    {
                        string categoryName = s.Split(',')[0].Trim();
                        string icon = s.Split(',')[1].Trim();
                        if (!setIcon.ContainsKey(categoryName))
                            setIcon.Add(categoryName, icon);
                    }
                }
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("FilterRemove"))
            {
                string[] toRemove = node.GetValues("remove");
                foreach (string s in toRemove)
                {
                    if (string.IsNullOrEmpty(s.Trim()))
                        continue;
                    removeSubCategory.Add(s.Trim()); // hashset apparently doesn't need duplicate check
                }
            }
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
            
            if (replaceFbM)
                processFilterByManufacturer(modNames);
        }

        private void processFilterDefinitions()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CATEGORY"))
            {
                customCategory C = new customCategory(node);
                if (Categories.Find(n => n.categoryName == C.categoryName) == null && C.subCategories != null)
                {
                    Categories.Add(C);
                }
            }

            //load all subCategory configs
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SUBCATEGORY"))
            {
                customSubCategory sC = new customSubCategory(node);
                if (sC.hasFilters)
                {
                    if (sC.subCategoryTitle != null && checkSubCategoryHasParts(sC))
                    {
                        if (!subCategoriesDict.ContainsKey(sC.subCategoryTitle)) // if nothing else has the same title
                            subCategoriesDict.Add(sC.subCategoryTitle, sC);
                        else if (subCategoriesDict.ContainsKey(sC.subCategoryTitle)) // if something does have the same title
                            subCategoriesDict[sC.subCategoryTitle].filters.AddRange(sC.filters);
                    }
                }
            }

            customCategory Cat = Categories.Find(C => C.categoryName == "Filter by Resource");
            if (Cat != null)
            {
                foreach (string s in resources)
                {
                    // add spaces before each capital letter
                    string name = System.Text.RegularExpressions.Regex.Replace(s, @"\B([A-Z])", " $1");
                    if (subCategoriesDict.ContainsKey(name))
                    {
                        if (subCategoriesDict[name].filters.Count == 1 && subCategoriesDict[name].filters[0].checks.Count > 0)
                        {
                            if (subCategoriesDict[name].filters[0].checks[0].type == "resource" && subCategoriesDict[name].filters[0].checks[0].value == s)
                                continue;
                        }
                        name = "res_" + name;
                    }
                    string icon = name;
                    //proceduralNameandIcon(ref name, ref icon);
                    if (name != null && !subCategoriesDict.ContainsKey(name))
                    {
                        customSubCategory sC = new customSubCategory(name, icon);
                        Check c = new Check("resource", s);
                        Filter f = new Filter(false);
                        f.checks.Add(c);
                        sC.filters.Add(f);
                        subCategoriesDict.Add(name, sC);
                    }
                    Cat.subCategories.AddUnique(name);
                }
            }

            foreach (customCategory C in Categories)
            {// generating the "all parts in ..." subcategories
                if (!C.all)
                    continue;
                List<Filter> filterList = new List<Filter>();
                if (C.subCategories != null)
                {
                    foreach (string s in C.subCategories)
                    {
                        if (s != null && subCategoriesDict.ContainsKey(s))
                            filterList.AddUniqueRange(subCategoriesDict[s].filters);
                    }
                }
                customSubCategory newSub = new customSubCategory("All parts in " + C.categoryName, C.iconName);
                newSub.filters = filterList;
                subCategoriesDict.Add(newSub.subCategoryTitle, newSub);
                C.subCategories.Insert(0, newSub.subCategoryTitle);
            }
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
            List<string> subCatNames = new List<string>();
            for (int i = 0; i < modNames.Count; i++)
            {
                string name = modNames[i];
                if (subCategoriesDict.ContainsKey(modNames[i]))
                    name = "mod_" + name;
                string icon = name;
                proceduralNameandIcon(ref name, ref icon);

                Check ch = new Check("folder", modNames[i]);
                Filter f = new Filter(false);
                customSubCategory sC = new customSubCategory(name, icon);
                subCatNames.Add(name);

                f.checks.Add(ch);
                sC.filters.Add(f);
                if (!subCategoriesDict.ContainsKey(name))
                    subCategoriesDict.Add(name, sC);
            }

            // if there's nothing defined for fbm, create the category
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
                fbm.subCategories.AddUniqueRange(modNames); // append the mod names
        }

        private bool stringListComparer(List<string> propellants)
        {
            foreach (List<string> ls in propellantCombos)
                if (propellants.Count == ls.Count && !propellants.Except(ls).Any())
                    return true;
            return false;
        }

        public void setSelectedCategory()
        {
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.activeButton.State == RUIToggleButtonTyped.ButtonState.TRUE);
            if (Filter != null)
                Filter.button.activeButton.SetFalse(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);

            Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == categoryDefault);
            if (Filter != null)
                Filter.button.activeButton.SetTrue(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
            else
            {
                Filter = PartCategorizer.Instance.filters[0];
                Filter.button.activeButton.SetTrue(Filter.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
            }

            Filter = Filter.subcategories.Find(sC => sC.button.categoryName == subCategoryDefault);
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
            if (fLA.Count != fLB.Count && fLA.Count != 0)
                return false;

            foreach (Filter fA in fLA)
            {
                if (!fLB.Any(fB => fB.Equals(fA)))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// checks all subcategories and edits their names/icons if required
        /// </summary>
        public void namesAndIcons(PartCategorizer.Category category)
        {
            List<string> toRemove = new List<string>();
            foreach (PartCategorizer.Category c in category.subcategories)
            {
                if (removeSubCategory.Contains(c.button.categoryName))
                    toRemove.Add(c.button.categoryName);
                else
                {
                    if (Rename.ContainsKey(c.button.categoryName)) // update the name first
                        c.button.categoryName = Rename[c.button.categoryName];

                    if (setIcon.ContainsKey(c.button.categoryName)) // update the icon
                    {
                        if (iconDict.ContainsKey(setIcon[c.button.categoryName])) // if the icon dict contains a matching name
                            c.button.SetIcon(getIcon(setIcon[c.button.categoryName]));
                        else if (iconDict.ContainsKey(c.button.categoryName)) // if it doesn't
                            c.button.SetIcon(getIcon(c.button.categoryName));
                    }
                    else if (iconDict.ContainsKey(c.button.categoryName))
                        c.button.SetIcon(getIcon(c.button.categoryName));
                }
            }
            category.subcategories.RemoveAll(c => toRemove.Contains(c.button.categoryName));
        }

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
                if (Instance.iconDict.ContainsKey(name))
                {
                    int i = 1;
                    while (Instance.iconDict.ContainsKey(name + i.ToString()) && i < 1000)
                        i++;
                    if (i != 1000)
                        name = name + i.ToString();
                    Log("Duplicated texture name \"" + t.name.Split(new char[] { '/', '\\' }).Last() + "\" at:\r\n" + t.name + "\r\n New reference is: " + name);
                }

                if (!Instance.iconDict.ContainsKey(name))
                {
                    RUI.Icons.Selectable.Icon icon = new RUI.Icons.Selectable.Icon(name, t.texture, selectedTex, false);
                    Instance.iconDict.Add(icon.name, icon);
                }
            }
        }

        public static RUI.Icons.Selectable.Icon getIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            if (Instance.iconDict.ContainsKey(name))
                return Instance.iconDict[name];
            if (PartCategorizer.Instance.iconLoader.iconDictionary.ContainsKey(name))
                return PartCategorizer.Instance.iconLoader.iconDictionary[name];
            
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

        public static bool checkSubCategoryHasParts(customSubCategory sC, string category = "")
        {
            foreach (AvailablePart p in PartLoader.Instance.parts)
                if (sC.checkFilters(p))
                    return true;

            if (!string.IsNullOrEmpty(category))
                Log(sC.subCategoryTitle + " in category " + category + " has no valid parts and was not initialised");
            else
                Log(sC.subCategoryTitle + " has no valid parts and was not initialised");
            return false;
        }

        public void proceduralNameandIcon(ref string name, ref string icon)
        {
            if (Rename.ContainsKey(name))
                name = Rename[name];
            if (setIcon.ContainsKey(name))
                icon = setIcon[name];
        }

        internal static void Log(object o)
        {
            Debug.Log("[Filter Extensions] " + o);
        }
    }
}