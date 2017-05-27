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
        public static Dictionary<string, string> partPathDict = new Dictionary<string, string>(); // set null after processing completed

        // storing all subCategory definitions for categories to reference during compilation to instances
        public static Dictionary<string, SubcategoryNode> subCategoriesDict = new Dictionary<string, SubcategoryNode>(); // set null after processing completed
        /// <summary>
        /// provides a typed check for stock modules which then allows for inheritance checking to work using isAssignableFrom
        /// </summary>
        private static Dictionary<string, Type> loaded_modules; // set null after processing completed
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


        private IEnumerator Start()
        {
            Logger.Log(string.Empty, Logger.LogLevel.Warn); // print version
            yield return null;
            yield return null;

            GetConfigs();
            GetPartData();
            ProcessFilterDefinitions();
            IconLib.Load();
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
            var modNames = new List<string>();
            Editor.blackListedParts = new HashSet<string>();
            char[] splitter = new char[] { '/', '\\' };
            foreach (AvailablePart p in PartLoader.LoadedPartsList)
            {
                if (p == null)
                {
                    continue;
                }
                else if (string.Equals(p.TechRequired, "Unresearchable", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log($"part {p.name} is noted as unreasearchable and will not be visible", Logger.LogLevel.Debug);
                    Editor.blackListedParts.Add(p.name);
                    continue;
                }

                if (string.IsNullOrEmpty(p.partUrl))
                {
                    RepairAvailablePartUrl(p);
                }
                // if the url is still borked, can't associate a mod to the part
                if (!string.IsNullOrEmpty(p.partUrl))
                {
                    // list of GameData folders
                    modNames.AddUnique(p.partUrl.Split(splitter)[0]);
                    // associate the path to the part
                    if (!partPathDict.ContainsKey(p.name))
                    {
                        partPathDict.Add(p.name, p.partUrl);
                    }
                    else
                    {
                        Logger.Log(p.name + " duplicated part key in part path dictionary", Logger.LogLevel.Debug);
                    }
                }
                if (PartType.IsEngine(p))
                {
                    ProcessEnginePropellants(p);
                }
                if (p.partPrefab.Resources != null)
                {
                    foreach (PartResource r in p.partPrefab.Resources)
                    {
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
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CATEGORY"))
            {
                var C = new CategoryNode(node, this);
                if (C.SubCategories == null)
                {
                    Logger.Log($"no subcategories present in {C.CategoryName}", Logger.LogLevel.Error);
                    continue;
                }
                CategoryNodes.AddUnique(C);
            }
            //load all subCategory configs
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SUBCATEGORY"))
            {
                var sC = new SubcategoryNode(node);
                if (!sC.HasFilters || string.IsNullOrEmpty(sC.SubCategoryTitle))
                {
                    Logger.Log($"subcategory format error: {sC.SubCategoryTitle}", Logger.LogLevel.Error);
                    continue;
                }
                else if (subCategoriesDict.ContainsKey(sC.SubCategoryTitle)) // if something does have the same title
                {
                    Logger.Log($"subcategory name duplicated: {sC.SubCategoryTitle}", Logger.LogLevel.Error);
                    continue;
                }
                else // if nothing else has the same title
                {
                    subCategoriesDict.Add(sC.SubCategoryTitle, sC);
                }
            }

            CategoryNode Cat = CategoryNodes.Find(C => C.CategoryName == "Filter by Resource");
            if (Cat != null && Cat.Type == CategoryNode.CategoryType.STOCK)
            {
                foreach (string s in resources)
                {
                    // add spaces before each capital letter
                    string name = System.Text.RegularExpressions.Regex.Replace(s, @"\B([A-Z])", " $1");
                    if (subCategoriesDict.ContainsKey(name))
                    {
                        Logger.Log($"resource name already exists, abandoning generation for {name}", Logger.LogLevel.Debug);
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
                {
                    continue;
                }
                var filterList = new List<FilterNode>();
                if (C.SubCategories != null)
                {
                    foreach (SubCategoryItem s in C.SubCategories)
                    {
                        if (subCategoriesDict.TryGetValue(s.SubcategoryName, out SubcategoryNode subcategory))
                        {
                            filterList.AddUniqueRange(subcategory.Filters);
                        }
                    }
                }
                var filternodes = new List<ConfigNode>();
                foreach (FilterNode f in filterList)
                {
                    filternodes.Add(f.ToConfigNode());
                }
                var newSub = new SubcategoryNode(SubcategoryNode.MakeSubcategoryNode("All parts in " + C.CategoryName, C.IconName, false, filternodes));
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
                var propellants = new List<string>();
                for (int j = 0; j < e.propellants.Count; j++)
                {
                    propellants.Add(e.propellants[j].name);
                }
                propellants.Sort();
                if (!StringListComparer(propellants))
                {
                    propellantCombos.Add(propellants);
                }
            }
        }

        /// <summary>
        /// create the subcategories for each unique propellant combination found
        /// </summary>
        private void GenerateEngineTypes()
        {
            var engines = new List<SubCategoryItem>();
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
                    var sC = new SubcategoryNode(SubcategoryNode.MakeSubcategoryNode(name, icon, false, filters));
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
            var subCatNames = new List<string>();
            foreach (string s in modNames)
            {
                string name = s;
                if (subCategoriesDict.ContainsKey(name))
                {
                    name = "mod_" + name;
                }
                string icon = name;
                SetNameAndIcon(ref name, ref icon);

                if (!subCategoriesDict.ContainsKey(name))
                {
                    subCatNames.Add(name);
                    var checks = new List<ConfigNode>() { CheckNodeFactory.MakeCheckNode(CheckFolder.ID, name) };
                    var filters = new List<ConfigNode>() { FilterNode.MakeFilterNode(false, checks) };
                    var sC = new SubcategoryNode(SubcategoryNode.MakeSubcategoryNode(name, icon, false, filters));
                    subCategoriesDict.Add(name, sC);
                }
            }

            var manufacturerSubs = new ConfigNode("SUBCATEGORIES");
            for (int i = 0; i < subCatNames.Count; i++)
            {
                manufacturerSubs.AddValue("list", i.ToString() + "," + subCatNames[i]);
            }

            var filterByManufacturer = new ConfigNode("CATEGORY");
            filterByManufacturer.AddValue("name", "Filter by Manufacturer");
            filterByManufacturer.AddValue("type", "stock");
            filterByManufacturer.AddValue("value", "replace");
            filterByManufacturer.AddNode(manufacturerSubs);
            FilterByManufacturer = new CategoryNode(filterByManufacturer, this);
            CategoryNodes.Add(FilterByManufacturer);
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
                {
                    return true;
                }
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
            var processedElements = new List<string>();
            foreach (KeyValuePair<string, SubcategoryNode> kvpOuter in subCategoriesDict)
            {
                foreach (string subcatName in processedElements)
                {
                    SubcategoryNode processedSubcat = subCategoriesDict[subcatName];
                    if (FilterNode.CompareFilterLists(processedSubcat.Filters, kvpOuter.Value.Filters))
                    {
                        // add conflict entry for the already entered subCategory
                        if (conflictsDict.TryGetValue(subcatName, out List<string> conflicts))
                        {
                            conflicts.Add(kvpOuter.Key);
                        }
                        else
                        {
                            conflictsDict.Add(subcatName, new List<string>() { kvpOuter.Key });
                        }

                        // add a conflict entry for the new subcategory
                        if (conflictsDict.TryGetValue(kvpOuter.Key, out conflicts))
                        {
                            conflicts.Add(subcatName);
                        }
                        else
                        {
                            conflictsDict.Add(kvpOuter.Key, new List<string>() { subcatName });
                        }
                    }
                }
                processedElements.Add(kvpOuter.Key);
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
                    Logger.Log(ex.Message, Logger.LogLevel.Warn);
                }
            }
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
            {
                ap.partUrl = url.url;
            }
        }

        /// <summary>
        /// check the name and icon against the sets for renaming and setting a different icon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        public void SetNameAndIcon(ref string name, ref string icon)
        {
            if (Rename.TryGetValue(name, out string tmp))
            {
                name = tmp;
            }
            if (setIcon.TryGetValue(name, out tmp))
            {
                icon = tmp;
            }
        }
    }
}