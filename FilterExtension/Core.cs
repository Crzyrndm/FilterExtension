using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Majority of Core only runs once.

namespace FilterExtensions
{
    using ConfigNodes;
    using FilterExtensions.ConfigNodes.Checks;
    using KSP.UI.Screens;
    using UnityEngine;
    using Utility;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Core : MonoBehaviour
    {
        public static readonly Version version = new Version(2, 8, 1, 2);

        private static Core instance;
        public static Core Instance
        {
            get
            {
                return instance;
            }
        }

        // storing categories loaded at Main Menu for creation when entering SPH/VAB
        public List<CustomCategory> Categories = new List<CustomCategory>();

        public CustomCategory FilterByManufacturer;

        // storing all subCategory definitions for categories to reference
        public Dictionary<string, CustomSubCategory> subCategoriesDict = new Dictionary<string, CustomSubCategory>();

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

        private const string fallbackIcon = "stockIcon_fallback";

        private IEnumerator Start()
        {
            instance = this;
            DontDestroyOnLoad(this);
            Log(string.Empty, LogLevel.Warn);

            yield return null;
            yield return null;
            yield return null;

            GetConfigs();
            GetPartData();
            ProcessFilterDefinitions();
            LoadIcons();
            CheckAndMarkConflicts();
        }

        /// <summary>
        /// Loads the settings, rename, set icon, and deletion data into an actionable format
        /// </summary>
        private void GetConfigs()
        {
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
        private void GetPartData()
        {
            List<string> modNames = new List<string>();
            Editor.blackListedParts = new HashSet<string>();

            foreach (AvailablePart p in PartLoader.LoadedPartsList)
            {
                if (p == null)
                    continue;
                if (string.Equals(p.TechRequired, "Unresearchable", StringComparison.OrdinalIgnoreCase))
                {
                    Log(p.name);
                    Editor.blackListedParts.Add(p.name);
                    continue;
                }

                if (string.IsNullOrEmpty(p.partUrl))
                    RepairAvailablePartUrl(p);

                // if the url is still borked, can't associate a mod to the part
                if (!string.IsNullOrEmpty(p.partUrl))
                {
                    // list of GameData folders
                    modNames.AddUnique(p.partUrl.Split(new char[] { '/', '\\' })[0]);

                    // associate the path to the part
                    if (!partPathDict.ContainsKey(p.name))
                    {
                        partPathDict.Add(p.name, p.partUrl);
                    }
                    else
                        Log(p.name + " duplicated part key in part path dictionary", LogLevel.Warn);

                    if (PartType.IsEngine(p))
                        ProcessEnginePropellants(p);

                    if (p.partPrefab.Resources != null)
                    {
                        foreach (PartResource r in p.partPrefab.Resources)
                            resources.AddUnique(r.resourceName);
                    }
                }
            }
            GenerateEngineTypes();

            ProcessFilterByManufacturer(modNames);
        }

        /// <summary>
        /// turn the loaded category and subcategory nodes into useable data
        /// </summary>
        private void ProcessFilterDefinitions()
        {
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("CATEGORY");
            foreach (ConfigNode node in nodes)
            {
                CustomCategory C = new CustomCategory(node);
                if (C.SubCategories == null)
                    continue;
                if (!Categories.Any(n => n.CategoryName == C.CategoryName))
                    Categories.Add(C);
            }

            //load all subCategory configs
            nodes = GameDatabase.Instance.GetConfigNodes("SUBCATEGORY");
            foreach (ConfigNode node in nodes)
            {
                CustomSubCategory sC = new CustomSubCategory(node);
                if (!sC.HasFilters || string.IsNullOrEmpty(sC.SubCategoryTitle))
                {
                    Log($"subcategory format error: {sC.SubCategoryTitle}", LogLevel.Error);
                    continue;
                }
                if (subCategoriesDict.TryGetValue(sC.SubCategoryTitle, out CustomSubCategory subcategory)) // if something does have the same title
                {
                    Log($"subcategory name duplicated: {sC.SubCategoryTitle}", LogLevel.Error);
                    continue;
                }
                else // if nothing else has the same title
                {
                    subCategoriesDict.Add(sC.SubCategoryTitle, sC);
                }
            }

            CustomCategory Cat = Categories.Find(C => C.CategoryName == "Filter by Resource");
            if (Cat != null)
            {
                foreach (string s in resources)
                {
                    // add spaces before each capital letter
                    string name = System.Text.RegularExpressions.Regex.Replace(s, @"\B([A-Z])", " $1");

                    if (subCategoriesDict.TryGetValue(name, out CustomSubCategory subcategory))
                    {
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(name))
                    {
                        ConfigNode checkNode = CheckFactory.MakeCheckNode(CheckResource.ID, s);
                        ConfigNode filtNode = Filter.MakeFilterNode(false, new List<ConfigNode>(){ checkNode });
                        ConfigNode subcatNode = CustomSubCategory.MakeSubcategoryNode(name, name, new List<ConfigNode>() { filtNode });
                        subCategoriesDict.Add(name, new CustomSubCategory(subcatNode));
                        Cat.SubCategories.AddUnique(new SubCategoryItem(name));
                    }
                }
            }

            foreach (CustomCategory C in Categories)
            {
                if (C == null || !C.All)
                    continue;

                List<Filter> filterList = new List<Filter>();
                if (C.SubCategories != null)
                {
                    for (int j = 0; j < C.SubCategories.Count; j++)
                    {
                        SubCategoryItem s = C.SubCategories[j];
                        if (s == null)
                            continue;

                        if (subCategoriesDict.TryGetValue(s.SubcategoryName, out CustomSubCategory subcategory))
                            filterList.AddUniqueRange(subcategory.Filters);
                    }
                }
                List<ConfigNode> filternodes = new List<ConfigNode>();
                foreach (var f in filterList)
                {
                    filternodes.Add(f.ToConfigNode());
                }
                CustomSubCategory newSub = new CustomSubCategory(CustomSubCategory.MakeSubcategoryNode("All parts in " + C.CategoryName, C.IconName, filternodes));
                subCategoriesDict.Add(newSub.SubCategoryTitle, newSub);
                C.SubCategories.Insert(0, new SubCategoryItem(newSub.SubCategoryTitle));
            }
        }

        /// <summary>
        /// check for a unique propellant combination and add to the list if one is found
        /// </summary>
        /// <param name="p"></param>
        private void ProcessEnginePropellants(AvailablePart p)
        {
            List<ModuleEngines> engines = p.partPrefab.Modules.GetModules<ModuleEngines>();
            foreach (ModuleEngines e in engines)
            {
                List<string> propellants = new List<string>();
                for (int j = 0; j < e.propellants.Count; j++)
                    propellants.Add(e.propellants[j].name);
                propellants.Sort();

                if (!StringListComparer(propellants))
                    propellantCombos.Add(propellants);
            }
        }

        /// <summary>
        /// create the subcategories for each unique propellant combination found
        /// </summary>
        private void GenerateEngineTypes()
        {
            List<SubCategoryItem> engines = new List<SubCategoryItem>();
            foreach (List<string> ls in propellantCombos)
            {
                string propList = string.Join(",", ls.ToArray());
                string name = propList;
                string icon = propList;
                SetNameAndIcon(ref name, ref icon);

                if (!string.IsNullOrEmpty(name) && !subCategoriesDict.ContainsKey(name))
                {
                    var checks = new List<ConfigNode>() { CheckFactory.MakeCheckNode(CheckPropellant.ID, propList, exact: true) };
                    var filters = new List<ConfigNode>() { Filter.MakeFilterNode(false, checks) };
                    CustomSubCategory sC = new CustomSubCategory(CustomSubCategory.MakeSubcategoryNode(name, icon, filters));
                    subCategoriesDict.Add(name, sC);
                }
            }
        }

        /// <summary>
        /// create the subcategories for filter by manufacturer by discovered GameData folder
        /// </summary>
        /// <param name="modNames"></param>
        private void ProcessFilterByManufacturer(List<string> modNames)
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
                    var checks = new List<ConfigNode>() { CheckFactory.MakeCheckNode(CheckFolder.ID, modNames[i]) };
                    var filters = new List<ConfigNode>() { Filter.MakeFilterNode(false, checks) };
                    CustomSubCategory sC = new CustomSubCategory(CustomSubCategory.MakeSubcategoryNode(name, icon, filters));
                    subCategoriesDict.Add(name, sC);
                }
            }

            ConfigNode manufacturerSubs = new ConfigNode("SUBCATEGORIES");
            for (int i = 0; i < subCatNames.Count; i++)
                manufacturerSubs.AddValue("list", i.ToString() + "," + subCatNames[i]);

            ConfigNode filterByManufacturer = new ConfigNode("CATEGORY");
            filterByManufacturer.AddValue("name", "Filter by Manufacturer");
            filterByManufacturer.AddValue("type", "stock");
            filterByManufacturer.AddValue("value", "replace");
            filterByManufacturer.AddNode(manufacturerSubs);
            FilterByManufacturer = new CustomCategory(filterByManufacturer);
        }

        /// <summary>
        /// returns true if the list passed exactly matches an entry already in propellantCombos
        /// </summary>
        /// <param name="propellants"></param>
        /// <returns></returns>
        private bool StringListComparer(List<string> propellants)
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
        private void CheckAndMarkConflicts()
        {
            // Can't guarantee iteration order of dict will be the same each time so need a set of elements that have been processed
            // to ensure conflicts are only checked against elements that are already checked
            // by only checking against processed elements we know we're only adding checking for collisions between each pair once
            List<string> processedElements = new List<string>();
            foreach (KeyValuePair<string, CustomSubCategory> kvpOuter in subCategoriesDict)
            {
                foreach (string subcatName in processedElements)
                {
                    CustomSubCategory processedSubcat = subCategoriesDict[subcatName];
                    if (Filter.CompareFilterLists(processedSubcat.Filters, kvpOuter.Value.Filters))
                    {
                        // add conflict entry for the already entered subCategory
                        if (conflictsDict.TryGetValue(subcatName, out List<string> conflicts))
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
        private static void LoadIcons()
        {
            GameDatabase.TextureInfo texInfo = null;
            Texture2D selectedTex = null;
            Dictionary<string, GameDatabase.TextureInfo> texDict = new Dictionary<string, GameDatabase.TextureInfo>();
            for (int i = GameDatabase.Instance.databaseTexture.Count - 1; i >= 0; --i)
            {
                texInfo = GameDatabase.Instance.databaseTexture[i];
                if (texInfo.texture != null && texInfo.texture.width == 32 && texInfo.texture.height == 32)
                {
                    texDict.TryAdd(texInfo.name, texInfo);
                }
            }

            foreach (KeyValuePair<string, GameDatabase.TextureInfo> kvp in texDict)
            {
                if (texDict.TryGetValue(kvp.Value.name + "_selected", out texInfo))
                    selectedTex = texInfo.texture;
                else
                    selectedTex = kvp.Value.texture;

                string name = kvp.Value.name.Split(new char[] { '/', '\\' }).Last();
                RUI.Icons.Selectable.Icon icon = new RUI.Icons.Selectable.Icon(name, kvp.Value.texture, selectedTex, false);
                Instance.iconDict.TryAdd(icon.name, icon);
            }
        }

        /// <summary>
        /// get the icon that matches a name
        /// </summary>
        /// <param name="name">the icon name</param>
        /// <returns>the icon if it is found, or the fallback icon if it is not</returns>
        public static RUI.Icons.Selectable.Icon GetIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
                return PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];

            if (Instance.iconDict.TryGetValue(name, out RUI.Icons.Selectable.Icon icon) || PartCategorizer.Instance.iconLoader.iconDictionary.TryGetValue(name, out icon))
                return icon;
            return PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];
        }

        /// <summary>
        /// get icon following the TryGet* syntax
        /// </summary>
        /// <param name="name">the icon name</param>
        /// <param name="icon">the icon that matches the name, or the fallback if no matches were found</param>
        /// <returns>true if a matching icon was found, false if fallback was required</returns>
        public static bool TryGetIcon(string name, out RUI.Icons.Selectable.Icon icon)
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
            if (Rename.TryGetValue(name, out string tmp))
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
                Debug.LogFormat($"[Filter Extensions {version}]: {o}");
            else if (level == LogLevel.Warn)
                Debug.LogWarningFormat($"[Filter Extensions {version}]: {o}");
            else
                Debug.LogErrorFormat($"[Filter Extensions {version}]: {o}");
        }

        internal static void Log(string format, LogLevel level = LogLevel.Log, params object[] o)
        {
            if (level == LogLevel.Log)
                Debug.LogFormat($"[Filter Extensions {version}]: {string.Format(format, o)}");
            else if (level == LogLevel.Warn)
                Debug.LogWarningFormat($"[Filter Extensions {version}]: {string.Format(format, o)}");
            else
                Debug.LogErrorFormat($"[Filter Extensions {version}]: {string.Format(format, o)}");
        }
    }
}