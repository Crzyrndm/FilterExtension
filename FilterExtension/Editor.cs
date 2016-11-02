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
        public void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(StartEditor);
        }

        public void StartEditor()
        {
            StartCoroutine(editorInit());
        }

        /// <summary>
        /// names of all parts that shouldn't be visible to the player
        /// </summary>
        public static HashSet<string> blackListedParts;

        public IEnumerator editorInit()
        {
            GameEvents.onGUIEditorToolbarReady.Remove(StartEditor);
            FESettings settings = HighLogic.CurrentGame.Parameters.CustomParams<FESettings>();
            if (settings.debug)
                Core.Log("Starting on Stock Filters", Core.LogLevel.Log);

            foreach (PartCategorizer.Category C in PartCategorizer.Instance.filters) // one pass for stock
            {
                customCategory cat;
                if (Core.Instance.Categories.TryGetValue(c => c.categoryName == C.button.categoryName, out cat) && cat.type == customCategory.categoryType.Stock)
                    cat.initialise();
                else if (C.button.categoryName == "Filter by Manufacturer" && settings.replaceFbM)
                {
                    Core.Instance.FilterByManufacturer.initialise();
                }
            }

            if (settings.debug)
                Core.Log("Starting on general categories", Core.LogLevel.Log);

            foreach (customCategory c in Core.Instance.Categories) // all non stock FE categories
            {
                if (c.type == customCategory.categoryType.New)
                    c.initialise();
            }

            yield return null;
            if (settings.debug)
                Core.Log("Starting on late categories", Core.LogLevel.Log);

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
            yield return null;
            if (settings.debug)
                Core.Log("Starting on removing categories", Core.LogLevel.Log);
            for (int i = PartCategorizer.Instance.filters.Count - 1; i >= 0; --i)
            {
                if (PartCategorizer.Instance.filters[i].subcategories.Count == 0)
                {
                    PartCategorizer.Instance.filters[i].DeleteCategory();
                }
            }
            // make the categories visible
            if (settings.setAdvanced)
                PartCategorizer.Instance.SetAdvancedMode();

            yield return null;
            if (HighLogic.CurrentGame.Parameters.CustomParams<FESettings>().debug)
                Core.Log("Refreshing parts list", Core.LogLevel.Log);
            setSelectedCategory();
        }

        /// <summary>
        /// In the editor, checks all subcategories of a category and edits their names/icons if required
        /// </summary>
        public static void namesAndIcons(PartCategorizer.Category category)
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
                if (HighLogic.CurrentGame.Parameters.CustomParams<FESettings>().categoryDefault != string.Empty)
                {
                    cat = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == HighLogic.CurrentGame.Parameters.CustomParams<FESettings>().categoryDefault);
                    if (cat != null)
                        cat.button.activeButton.SetState(KSP.UI.UIRadioButton.State.True, KSP.UI.UIRadioButton.CallType.APPLICATION, null, true);
                }

                if (HighLogic.CurrentGame.Parameters.CustomParams<FESettings>().subCategoryDefault != string.Empty)
                {
                    // set the subcategory button
                    cat = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.activeButton.Value);
                    if (cat != null)
                    {
                        cat = cat.subcategories.FirstOrDefault(sC => sC.button.categoryName == HighLogic.CurrentGame.Parameters.CustomParams<FESettings>().subCategoryDefault);
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

        private bool checkPartVisible(AvailablePart part, PartCategorizer.Category category)
        {
            for (int i = 0; i < category.subcategories.Count; ++i)
            {
                if (category.subcategories[i].exclusionFilter.FilterCriteria.Invoke(part))
                    return true;
            }
            return false;
        }

        private bool checkIsEmptyCategory(PartCategorizer.Category category)
        {
            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
                if (checkPartVisible(part, category))
                    return false;
            }
            return true;
        }
    }
}