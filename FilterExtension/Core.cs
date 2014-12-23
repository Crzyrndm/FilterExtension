using System;
using System.Linq;
using System.Collections.Generic;


namespace FilterExtensions
{
    using UnityEngine;
    using FilterExtensions.Categoriser;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Core : MonoBehaviour
    {
        List<Category> Categories = new List<Category>();
        List<subCategory> subCategories = new List<subCategory>();
        internal static Dictionary<string, GameDatabase.TextureInfo> texDict = new Dictionary<string, GameDatabase.TextureInfo>(); // all the icons inside folders named filterIcon

        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(SubCategories);

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CATEGORY"))
            {
                Category C = new Category(node);
                if (!Categories.Contains(C))
                    Categories.Add(C);
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SUBCATEGORY"))
            {
                subCategory sC = new subCategory(node);
                if (checkForConflicts(sC))
                    subCategories.Add(sC);
            }
        }

        private void SubCategories()
        {
            loadIcons();

            foreach (Category c in Categories)
            {
                PartCategorizer.AddCustomFilter(c.categoryTitle, getIcon(c.iconName), c.colour);
            }

            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
            {
                checkIcons(c);
            }

            foreach (subCategory sC in subCategories)
            {
                try
                {
                    sC.initialise();
                }
                catch {
                    Debug.Log("[Filter Extensions] " + sC.subCategoryTitle + " failed to initialise");
                }
            }
            refreshList();
        }

        private void refreshList()
        {
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            RUIToggleButtonTyped button = Filter.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }

        private bool checkForConflicts(subCategory sCToCheck)
        {
            foreach (subCategory sC in subCategories) // iterate through the already added sC's
            {
                if (sCToCheck.category == sC.category)
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

        private bool compareFilterLists(List<Filter> filterListA, List<Filter> filterListB) //can't just compare directly because order could be different
        {
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
            foreach(PartCategorizer.Category c in category.subcategories)
            {
                if (PartCategorizer.Instance.iconDictionary.ContainsKey(c.button.categoryName))
                {
                    c.button.SetIcon(PartCategorizer.Instance.iconDictionary[c.button.categoryName]);
                }
            }
        }

        private void loadIcons()
        {
            List<GameDatabase.TextureInfo> texList = GameDatabase.Instance.GetAllTexturesInFolder("");
            foreach (GameDatabase.TextureInfo t in texList) // remove all textures from the list that have a dimension > 40 pixels
            {
                if (t.texture.height > 40 || t.texture.width > 40)
                {
                    texList.Remove(t);
                }
            }
            // use a dictionary for looking up _selected textures. Else the list has to be iterated over for every texture
            texDict = texList.ToDictionary(k => k.name);
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

                string[] name = t.name.Split('/');
                PartCategorizer.Icon icon = new PartCategorizer.Icon(name[name.Length - 1], t.texture, selectedTex, simple);
                PartCategorizer.Instance.iconDictionary.Add(icon.name, icon);
            }
        }

        internal static PartCategorizer.Icon getIcon(string name)
        {
            PartCategorizer.Icon icon = PartCategorizer.Instance.GetIcon(name);
            if (icon.name == PartCategorizer.Instance.fallbackIcon.name)
            {
                if (PartCategorizer.Instance.iconDictionary.ContainsKey(name))
                    return PartCategorizer.Instance.iconDictionary[name];
                else
                    return icon;
            }
            return icon;
        }
    }
}
