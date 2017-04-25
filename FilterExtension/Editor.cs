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
            GameEvents.onGUIEditorToolbarReady.Remove(StartEditor);
            StartCoroutine(EditorInit());
        }

        /// <summary>
        /// names of all parts that shouldn't be visible to the player
        /// </summary>
        public static HashSet<string> blackListedParts;

        public IEnumerator EditorInit()
        {
            Settings settings = HighLogic.CurrentGame.Parameters.CustomParams<Settings>();
            if (settings.debug)
            {
                LoadAndProcess.Log("Starting on Stock Filters", LoadAndProcess.LogLevel.Log);
                LoadAndProcess.Log("Starting on general categories", LoadAndProcess.LogLevel.Log);
            }

            foreach (CategoryInstance c in LoadAndProcess.Categories) // all non mod specific FE categories
            {
                if ((c.Type == CategoryNode.CategoryType.NEW || c.Type == CategoryNode.CategoryType.STOCK)
                    && (settings.replaceFbM || !string.Equals(c.Name, "Filter by Manufacturer", StringComparison.OrdinalIgnoreCase)))
                {
                    c.Initialise();
                }
            }

            yield return null;
            if (settings.debug)
            {
                LoadAndProcess.Log("Starting on late categories", LoadAndProcess.LogLevel.Log);
            }

            // this is to be used for altering subcategories in a category added by another mod
            foreach (CategoryInstance c in LoadAndProcess.Categories)
            {
                if (c.Type == CategoryNode.CategoryType.MOD)
                {
                    c.Initialise();
                }
            }

            // Remove any category with no subCategories (causes major breakages if selected).
            yield return null;
            if (settings.debug)
            {
                LoadAndProcess.Log("Starting on removing categories", LoadAndProcess.LogLevel.Log);
            }
            for (int i = PartCategorizer.Instance.filters.Count - 1; i >= 0; --i)
            {
                if (PartCategorizer.Instance.filters[i].subcategories.Count == 0)
                {
                    PartCategorizer.Instance.categories.Remove(PartCategorizer.Instance.filters[i]);
                    PartCategorizer.Instance.scrollListMain.RemoveItem(PartCategorizer.Instance.filters[i].button.container, true);
                }
            }
            // make the categories visible
            if (settings.setAdvanced)
            {
                PartCategorizer.Instance.SetAdvancedMode();
            }

            yield return null;
            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings>().debug)
            {
                LoadAndProcess.Log("Refreshing parts list", LoadAndProcess.LogLevel.Log);
            }
            SetSelectedCategory();
        }


        /// <summary>
        /// refresh the visible subcategories to ensure all changes are visible
        /// </summary>
        public static void SetSelectedCategory()
        {
            try
            {
                PartCategorizer.Category cat;
                if (HighLogic.CurrentGame.Parameters.CustomParams<Settings>().categoryDefault != string.Empty)
                {
                    cat = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == HighLogic.CurrentGame.Parameters.CustomParams<Settings>().categoryDefault);
                    if (cat != null)
                    {
                        cat.button.activeButton.SetState(KSP.UI.UIRadioButton.State.True, KSP.UI.UIRadioButton.CallType.APPLICATION, null, true);
                    }
                }

                if (HighLogic.CurrentGame.Parameters.CustomParams<Settings>().subCategoryDefault != string.Empty)
                {
                    // set the subcategory button
                    cat = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.activeButton.Value);
                    if (cat != null)
                    {
                        cat = cat.subcategories.FirstOrDefault(sC => sC.button.categoryName == HighLogic.CurrentGame.Parameters.CustomParams<Settings>().subCategoryDefault);
                        if (cat != null)
                        {
                            cat.button.activeButton.SetState(KSP.UI.UIRadioButton.State.True, KSP.UI.UIRadioButton.CallType.APPLICATION, null, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoadAndProcess.Log($"Category refresh failed\r\n{e.InnerException}\r\n{e.StackTrace}", LoadAndProcess.LogLevel.Error);
            }
        }

        private bool CheckPartVisible(AvailablePart part, PartCategorizer.Category category)
        {
            for (int i = 0; i < category.subcategories.Count; ++i)
            {
                if (category.subcategories[i].exclusionFilter.FilterCriteria.Invoke(part))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckIsEmptyCategory(PartCategorizer.Category category)
        {
            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
                if (CheckPartVisible(part, category))
                {
                    return false;
                }
            }
            return true;
        }
    }
}