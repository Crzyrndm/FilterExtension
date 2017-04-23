using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Majority of Core only runs once.

namespace FilterExtensions
{
    using ConfigNodes;
    using ConfigNodes.CheckNodes;
    using KSP.UI.Screens;
    using UnityEngine;
    using Utility;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class LoadAndProcess : MonoBehaviour
    {
        public static readonly Version version = new Version(2, 9, 0, 0);

        // storing categories loaded at Main Menu for creation when entering SPH/VAB
        public List<CategoryNode> CategoryNodes = new List<CategoryNode>();

        public CategoryNode FilterByManufacturer;

        // all subcategories with duplicated filters
        public Dictionary<string, List<string>> conflictsDict = new Dictionary<string, List<string>>();

        // renaming categories
        public Dictionary<string, string> Rename = new Dictionary<string, string>();

        // icons for categories
        public Dictionary<string, string> setIcon = new Dictionary<string, string>();

        // removing categories
        public HashSet<string> removeSubCategory = new HashSet<string>();

        // entry for each unique combination of propellants
        public List<List<string>> propellantCombos = new List<List<string>>();

        // entry for each unique resource
        public List<string> resources = new List<string>();

        // url for each part by internal name
        public static Dictionary<string, string> partPathDict = new Dictionary<string, string>();

        // Dictionary of icons created on entering the main menu
        public static Dictionary<string, RUI.Icons.Selectable.Icon> IconDict = new Dictionary<string, RUI.Icons.Selectable.Icon>();
        // storing all subCategory definitions for categories to reference during compilation to instances
        public static Dictionary<string, SubcategoryNode> subCategoriesDict = new Dictionary<string, SubcategoryNode>();
        /// <summary>
        /// provides a typed check for stock modules which then allows for inheritance checking to work using isAssignableFrom
        /// </summary>
        private static Dictionary<string, Type> loaded_modules;

        public static Dictionary<string, Type> Loaded_Modules
        {
            // dont pay for what you dont use...
            get
            {
                if (loaded_modules == null)
                {
                    loaded_modules = new Dictionary<string, Type>();
                    foreach (AvailablePart ap in PartLoader.LoadedPartsList)
                    {
                        foreach (PartModule pm in ap.partPrefab.Modules)
                        {
                            if (!string.IsNullOrEmpty(pm.moduleName) && !Loaded_Modules.ContainsKey(pm.moduleName))
                            {
                                loaded_modules.Add(pm.moduleName, pm.GetType());
                            }
                        }
                    }
                }
                return loaded_modules;
            }
        }

        // static list of compiled categories to instantiate in the editor
        public static List<CategoryInstance> Categories = new List<CategoryInstance>();

        private const string fallbackIcon = "stockIcon_fallback";

        private IEnumerator Start()
        {
            Log(string.Empty, LogLevel.Warn);

            yield return null;
            yield return null;
            yield return null;

            GetConfigs();
            GetPartData();
            ProcessFilterDefinitions();
            LoadIcons();
            CheckAndMarkConflicts();

            CompileCategories();

            partPathDict = null;
            subCategoriesDict = null;
            loaded_modules = null;
        }

        /// <summary>
        /// Loads the settings, rename, set icon, and deletion data into an actionable format
        /// </summary>
        private void GetConfigs()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("FilterRename"))
            {
                SubcategoryNodeModifier.MakeRenamers(node, Rename);
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("FilterSetIcon"))
            {
                SubcategoryNodeModifier.MakeRenamers(node, setIcon);
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("FilterRemove"))
            {
                SubcategoryNodeModifier.MakeDeleters(node, removeSubCategory);
            }
        }

        /// <summary>
        /// generate the associations between parts and folders, create all the mod categories, get all propellant combinations,
        /// </summary>
        private void GetPartData()
        {
            List<string> modNames = new List<string>();
            Editor.blackListedParts = new HashSet<string>();
            var splitter = new char[] { '/', '\\' };
            foreach (AvailablePart p in PartLoader.LoadedPartsList)
            {
                if (p == null)
                    continue;
                else if (string.Equals(p.TechRequired, "Unresearchable", StringComparison.OrdinalIgnoreCase))
                {
                    Log($"part {p.name} is noted as unreasearchable and will not be visible", LogLevel.Warn);
                    Editor.blackListedParts.Add(p.name);
                    continue;
                }

                if (string.IsNullOrEmpty(p.partUrl))
                    RepairAvailablePartUrl(p);
                // if the url is still borked, can't associate a mod to the part
                if (!string.IsNullOrEmpty(p.partUrl))
                {
                    // list of GameData folders
                    modNames.AddUnique(p.partUrl.Split(splitter)[0]);
                    // associate the path to the part
                    if (!partPathDict.ContainsKey(p.name))
                        partPathDict.Add(p.name, p.partUrl);
                    else
                        Log(p.name + " duplicated part key in part path dictionary", LogLevel.Warn);
                }
                if (PartType.IsEngine(p))
                    ProcessEnginePropellants(p);
                if (p.partPrefab.Resources != null)
                {
                    foreach (PartResource r in p.partPrefab.Resources)
                        resources.AddUnique(r.resourceName);
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
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CATEGORY"))
            {
                CategoryNode C = new CategoryNode(node, this);
                if (C.SubCategories == null)
                {
                    Log($"no subcategories present in {C.CategoryName}", LogLevel.Error);
                    continue;
                }
                CategoryNodes.AddUnique(C);
            }
            //load all subCategory configs
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SUBCATEGORY"))
            {
                SubcategoryNode sC = new SubcategoryNode(node);
                if (!sC.HasFilters || string.IsNullOrEmpty(sC.SubCategoryTitle))
                {
                    Log($"subcategory format error: {sC.SubCategoryTitle}", LogLevel.Error);
                    continue;
                }
                else if (subCategoriesDict.ContainsKey(sC.SubCategoryTitle)) // if something does have the same title
                {
                    Log($"subcategory name duplicated: {sC.SubCategoryTitle}", LogLevel.Error);
                    continue;
                }
                else // if nothing else has the same title
                {
                    subCategoriesDict.Add(sC.SubCategoryTitle, sC);
                }
            }

            CategoryNode Cat = CategoryNodes.Find(C => C.CategoryName == "Filter by Resource");
            if (Cat != null && Cat.Type == CategoryNode.CategoryType.Stock)
            {
                foreach (string s in resources)
                {
                    // add spaces before each capital letter
                    string name = System.Text.RegularExpressions.Regex.Replace(s, @"\B([A-Z])", " $1");
                    if (subCategoriesDict.ContainsKey(name))
                    {
                        Log($"resource name already exists, abandoning generation for {name}", LogLevel.Warn);
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(name))
                    {
                        ConfigNode checkNode = CheckNodeFactory.MakeCheckNode(CheckResource.ID, s);
                        ConfigNode filtNode = FilterNode.MakeFilterNode(false, new List<ConfigNode>(){ checkNode });
                        ConfigNode subcatNode = SubcategoryNode.MakeSubcategoryNode(name, name, false, new List<ConfigNode>() { filtNode });
                        subCategoriesDict.Add(name, new SubcategoryNode(subcatNode));
                        Cat.SubCategories.AddUnique(new SubCategoryItem(name));
                    }
                }
            }

            foreach (CategoryNode C in CategoryNodes)
            {
                if (!C.All)
                    continue;

                List<FilterNode> filterList = new List<FilterNode>();
                if (C.SubCategories != null)
                {
                    foreach (var s in C.SubCategories)
                    {
                        if (subCategoriesDict.TryGetValue(s.SubcategoryName, out SubcategoryNode subcategory))
                            filterList.AddUniqueRange(subcategory.Filters);
                    }
                }
                List<ConfigNode> filternodes = new List<ConfigNode>();
                foreach (var f in filterList)
                {
                    filternodes.Add(f.ToConfigNode());
                }
                SubcategoryNode newSub = new SubcategoryNode(SubcategoryNode.MakeSubcategoryNode("All parts in " + C.CategoryName, C.IconName, false, filternodes));
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
                    var checks = new List<ConfigNode>() { CheckNodeFactory.MakeCheckNode(CheckPropellant.ID, propList, exact: true) };
                    var filters = new List<ConfigNode>() { FilterNode.MakeFilterNode(false, checks) };
                    SubcategoryNode sC = new SubcategoryNode(SubcategoryNode.MakeSubcategoryNode(name, icon, false, filters));
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
            foreach (string s in modNames)
            {
                string name = s;
                if (subCategoriesDict.ContainsKey(name))
                    name = "mod_" + name;
                string icon = name;
                SetNameAndIcon(ref name, ref icon);

                if (!subCategoriesDict.ContainsKey(name))
                {
                    subCatNames.Add(name);
                    var checks = new List<ConfigNode>() { CheckNodeFactory.MakeCheckNode(CheckFolder.ID, name) };
                    var filters = new List<ConfigNode>() { FilterNode.MakeFilterNode(false, checks) };
                    SubcategoryNode sC = new SubcategoryNode(SubcategoryNode.MakeSubcategoryNode(name, icon, false, filters));
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
            FilterByManufacturer = new CategoryNode(filterByManufacturer, this);
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
            foreach (KeyValuePair<string, SubcategoryNode> kvpOuter in subCategoriesDict)
            {
                foreach (string subcatName in processedElements)
                {
                    SubcategoryNode processedSubcat = subCategoriesDict[subcatName];
                    if (FilterNode.CompareFilterLists(processedSubcat.Filters, kvpOuter.Value.Filters))
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
                IconDict.TryAdd(icon.name, icon);
            }
        }

        public void CompileCategories()
        {
            foreach (CategoryNode cn in CategoryNodes)
            {
                try
                {
                    Categories.Add(new CategoryInstance(cn, subCategoriesDict));
                }
                catch (Exception ex)
                {
                    Log(ex.Message, LogLevel.Warn);
                }
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

            if (IconDict.TryGetValue(name, out RUI.Icons.Selectable.Icon icon) || PartCategorizer.Instance.iconLoader.iconDictionary.TryGetValue(name, out icon))
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
            if (IconDict.TryGetValue(name, out icon))
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