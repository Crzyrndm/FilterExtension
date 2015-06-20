using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    using ConfigNodes;

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class Editor : MonoBehaviour
    {
        public static Editor instance;
        void Start()
        {
            instance = this;
            StartCoroutine(editorInit());
        }

        public static HashSet<string> blackListedParts;

        IEnumerator editorInit()
        {
            while (PartCategorizer.Instance == null)
                yield return null;
            if (Core.Instance.debug)
                Core.Log("Starting on Stock Filters");
            // stock filters
            // If I edit them later everything breaks
            // custom categories can't be created at this point
            // The event which most mods will be hooking into fires after this, so they still get their subCategories even though I clear the category
            //PartCategorizer a = PartCategorizer.Instance;
            //Editor thing = instance;
            //Core refCore = Core.Instance;
            //HashSet<string> parts = blackListedParts;
            foreach (PartCategorizer.Category C in PartCategorizer.Instance.filters)
            {
                customCategory cat = Core.Instance.Categories.FirstOrDefault(c => c.categoryName == C.button.categoryName);
                if (cat != null && cat.hasSubCategories() && cat.stockCategory)
                {
                    if (cat.behaviour == categoryTypeAndBehaviour.StockReplace)
                        C.subcategories.Clear();
                    cat.initialise();
                }
            }
            // custom categories
            // wait until the part menu is initialised
            while (!PartCategorizer.Ready)
                yield return null;

            // frames after the flag is set to wait before initialising. Minimum of two for things to work consistently
            for (int i = 0; i < 4; i++)
                yield return null;
            if (Core.Instance.debug)
                Core.Log("Starting on other filters");
            // run everything
            foreach (customCategory c in Core.Instance.Categories)
                if (!c.stockCategory)
                    c.initialise();

            // wait again so icon edits don't occur immediately and cause breakages
            for (int i = 0; i < 4; i++)
                yield return null;
            // edit names and icons of all subcategories
            if (Core.Instance.debug)
                Core.Log("Starting on setting names and icons");
            if (blackListedParts == null)
                findPartsToBlock();
            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
                Core.Instance.namesAndIcons(c);

            // Remove any category with no subCategories (causes major breakages if selected).
            for (int i = 0; i < 4; i++)
                yield return null;
            if (Core.Instance.debug)
                Core.Log("Starting on removing categories");
            List<PartCategorizer.Category> catsToDelete = PartCategorizer.Instance.filters.FindAll(c => c.subcategories.Count == 0);
            foreach (PartCategorizer.Category cat in catsToDelete)
            {
                PartCategorizer.Instance.scrollListMain.scrollList.RemoveItem(cat.button.container, true);
                PartCategorizer.Instance.filters.Remove(cat);
            }

            if (Core.Instance.setAdvanced)
                PartCategorizer.Instance.SetAdvancedMode();

            for (int i = 0; i < 4; i++)
                yield return null;
            if (Core.Instance.debug)
                Core.Log("Refreshing parts list");
            Core.setSelectedCategory();

            //while (true)
            //    yield return null;
        }

        /// <summary>
        /// checks all subcats not created by FE for visibility of parts set to "category = none"
        /// </summary>
        void findPartsToBlock()
        {
            // all parts that may not be visible
            List<AvailablePart> partsToCheck = PartLoader.Instance.parts.FindAll(ap => ap.category == PartCategories.none);
            // Only checking the category which should be Filter by Function (should I find FbF explcitly?)
            PartCategorizer.Category mainCat = PartCategorizer.Instance.filters[0];
            // has a reference to all the subcats that FE added to the category
            customCategory customMainCat = Core.Instance.Categories.Find(C => C.categoryName == mainCat.button.categoryName);
            // loop through the subcategories. Mark FE ones as seen incase of duplication and check the shortlisted parts against other mods categories for visibility
            HashSet<string> subCatsSeen = new HashSet<string>();
            for (int i = 0; i < mainCat.subcategories.Count; i++)
            {
                PartCategorizer.Category subCat = mainCat.subcategories[i];
                // if the name is an FE subcat and the category should have that FE subcat and it's not the duplicate of one already seen created by another mod, mark it seen and move on
                if (Core.Instance.subCategoriesDict.ContainsKey(subCat.button.categoryName) && customMainCat.subCategories.Contains(subCat.button.categoryName) && !subCatsSeen.Contains(subCat.button.categoryName))
                    subCatsSeen.Add(subCat.button.categoryName);
                else // subcat created by another mod
                {
                    // can't remove parts from a collection being looped over, need to remember the visible parts
                    List<AvailablePart> visibleParts = new List<AvailablePart>();
                    for (int j = 0; j < partsToCheck.Count; j++)
                    {
                        AvailablePart AP = partsToCheck[j];
                        if (subCat.exclusionFilter.FilterCriteria.Invoke(AP)) // if visible
                            visibleParts.Add(AP);
                    }
                    // remove all visible parts from the list to block
                    foreach (AvailablePart ap in visibleParts)
                        partsToCheck.Remove(ap);
                }
            }
            // add the blocked parts to a hashset for later lookup
            blackListedParts = new HashSet<string>();
            foreach (AvailablePart ap in partsToCheck)
                blackListedParts.Add(ap.name);
        }
    }
}
