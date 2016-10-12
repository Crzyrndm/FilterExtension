using KSP.UI.Screens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // only used once
using UnityEngine;

namespace FilterExtensions
{
    using ConfigNodes;
    using Utility;

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class Editor : MonoBehaviour
    {
        public static Editor instance;
        public static bool subcategoriesChecked;
        public bool ready = false;

        public void Start()
        {
            instance = this;
            StartCoroutine(editorInit());
        }

        /// <summary>
        /// names of all parts that shouldn't be visible to the player
        /// </summary>
        public static HashSet<string> blackListedParts;

        private IEnumerator editorInit()
        {
            ready = false;

            while (PartCategorizer.Instance == null)
                yield return null;
            yield return null;

            if (Settings.debug)
                Core.Log("Starting on Stock Filters", Core.LogLevel.Log);

            // stock filters
            // If I edit them later everything breaks
            // custom categories can't be created at this point
            // The event which most mods will be hooking into fires after this, so they still get their subCategories even though FE may clear the category
            foreach (PartCategorizer.Category C in PartCategorizer.Instance.filters)
            {
                customCategory cat;
                if (Core.Instance.Categories.TryGetValue(c => c.categoryName == C.button.categoryName, out cat) && cat.type == customCategory.categoryType.Stock)
                    cat.initialise();
            }

            // custom categories
            // wait until the part menu is initialised
            while (!PartCategorizer.Ready)
                yield return null;

            // frames after the flag is set to wait before initialising. Minimum of two for things to work consistently
            for (int i = 0; i < 4; i++)
                yield return null;
            if (Settings.debug)
                Core.Log("Starting on general categories", Core.LogLevel.Log);

            // all FE categories
            foreach (customCategory c in Core.Instance.Categories)
            {
                if (c.type == customCategory.categoryType.New)
                    c.initialise();
            }

            // wait again so icon edits don't occur immediately and cause breakages
            for (int i = 0; i < 4; i++)
                yield return null;
            if (Settings.debug)
                Core.Log("Starting on late categories", Core.LogLevel.Log);

            // generate the set of parts to block
            if (blackListedParts == null)
            {
                findPartsToBlock();
                // since this wasn't created until now, the already created categories may be completely empty
                // remove them now
                //PartCategorizer.Category cat;
                //for (int i = PartCategorizer.Instance.categories.Count - 1; i >= 0; --i)
                //{
                //    cat = PartCategorizer.Instance.categories[i];
                //    for (int j = cat.subcategories.Count - 1; j >= 0; --j)
                //    {
                //        bool hasParts = false;
                //        foreach (AvailablePart part in PartLoader.Instance.parts)
                //        {
                //            if (cat.subcategories[j].exclusionFilter.FilterCriteria.Invoke(part))
                //                hasParts = true;
                //        }
                //        if (hasParts)
                //            continue;
                //        cat.subcategories[j].DeleteSubcategory();
                //    }
                //    if (cat.subcategories.Count > 0)
                //        continue;
                //    cat.DeleteCategory();
                //}
            }

            // this is to be used for altering subcategories in a category added by another mod
            foreach (customCategory c in Core.Instance.Categories)
            {
                if (c.type == customCategory.categoryType.Mod)
                    c.initialise();
            }

            //
            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
                namesAndIcons(c);

            // Remove any category with no subCategories (causes major breakages if selected).
            for (int i = 0; i < 4; i++)
                yield return null;
            if (Settings.debug)
                Core.Log("Starting on removing categories", Core.LogLevel.Log);
            List<PartCategorizer.Category> catsToDelete = PartCategorizer.Instance.filters.FindAll(c => c.subcategories.Count == 0);
            foreach (PartCategorizer.Category cat in catsToDelete)
            {
                PartCategorizer.Instance.scrollListMain.RemoveItem(cat.button.container, true);
                PartCategorizer.Instance.filters.Remove(cat);
            }

            // make the categories visible
            if (Settings.setAdvanced)
                PartCategorizer.Instance.SetAdvancedMode();

            for (int i = 0; i < 4; i++)
                yield return null;
            if (Settings.debug)
                Core.Log("Refreshing parts list", Core.LogLevel.Log);
            setSelectedCategory();

            subcategoriesChecked = ready = true;
        }

        /// <summary>
        /// In the editor, checks all subcategories of a category and edits their names/icons if required
        /// </summary>
        public void namesAndIcons(PartCategorizer.Category category)
        {
            HashSet<string> toRemove = new HashSet<string>();
            foreach (PartCategorizer.Category c in category.subcategories)
            {
                if (Core.Instance.removeSubCategory.Contains(c.button.categoryName))
                    toRemove.Add(c.button.categoryName);
                else
                {
                    string tmp;
                    if (Core.Instance.Rename.TryGetValue(c.button.categoryName, out tmp)) // update the name first
                        c.button.categoryName = tmp;

                    RUI.Icons.Selectable.Icon icon;
                    if (Core.tryGetIcon(tmp, out icon) || Core.tryGetIcon(c.button.categoryName, out icon)) // if there is an explicit setIcon for the subcategory or if the name matches an icon
                        c.button.SetIcon(icon); // change the icon
                }
            }
            category.subcategories.RemoveAll(c => toRemove.Contains(c.button.categoryName));
        }

        /// <summary>
        /// refresh the visible subcategories to ensure all changes are visible
        /// </summary>
        public static void setSelectedCategory()
        {
            try
            {
                PartCategorizer.Category cat;
                if (Settings.categoryDefault != string.Empty)
                {
                    cat = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == Settings.categoryDefault);
                    if (cat != null)
                        cat.button.activeButton.SetState(KSP.UI.UIRadioButton.State.True, KSP.UI.UIRadioButton.CallType.APPLICATION, null, true);
                }

                if (Settings.subCategoryDefault != string.Empty)
                {
                    // set the subcategory button
                    cat = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.activeButton.Value);
                    if (cat != null)
                    {
                        cat = cat.subcategories.FirstOrDefault(sC => sC.button.categoryName == Settings.subCategoryDefault);
                        if (cat != null)
                            cat.button.activeButton.SetState(KSP.UI.UIRadioButton.State.True, KSP.UI.UIRadioButton.CallType.APPLICATION, null, true);
                    }
                }
            }
            catch (Exception e)
            {
                Core.Log($"Category refresh failed\r\n{e.InnerException}\r\n{e.StackTrace}", Core.LogLevel.Error);
            }
        }

        private void findPartsToBlock()
        {
            blackListedParts = new HashSet<string>();

            // Only checking the category which should be Filter by Function (should I find FbF explicitly?)
            PartCategorizer.Category mainCat = PartCategorizer.Instance.filters[0];

            AvailablePart part;
            for (int i = 0; i < PartLoader.Instance.loadedParts.Count; ++i)
            {
                part = PartLoader.Instance.loadedParts[i];
                if (part.category == PartCategories.none && !checkPartVisible(part, mainCat))
                    blackListedParts.Add(part.name);
            }
        }

        private bool checkPartVisible(AvailablePart part, PartCategorizer.Category category)
        {
            for (int i = 0; i < category.subcategories.Count; ++i)
            {
                if (category.subcategories[i].exclusionFilter.FilterCriteria.Invoke(part))
                    return true;
            }
            return false;
        }
    }
}