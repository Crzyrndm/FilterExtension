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
            Logger.Log("Starting on general categories", Logger.LogLevel.Debug);
            foreach (CategoryInstance c in LoadAndProcess.Categories) // all non mod specific FE categories
            {
                if ((c.Type == CategoryNode.CategoryType.NEW || c.Type == CategoryNode.CategoryType.STOCK)
                    && (settings.replaceFbM || !string.Equals(c.Name, "Filter by Manufacturer", StringComparison.OrdinalIgnoreCase)))
                {
                    c.Initialise();
                }
            }

            yield return null;
            Logger.Log("Starting on late categories", Logger.LogLevel.Debug);
            // this is to be used for altering subcategories in a category added by another mod
            foreach (CategoryInstance c in LoadAndProcess.Categories)
            {
                if (c.Type == CategoryNode.CategoryType.MOD)
                {
                    c.Initialise();
                }
            }

            yield return null;
            Logger.Log("Starting on removing categories", Logger.LogLevel.Debug);
            // Remove any category with no subCategories (causes major breakages if selected).
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
            Logger.Log("Refreshing parts list", Logger.LogLevel.Debug);
            SetSelectedCategory();
        }


        /// <summary>
        /// refresh the visible subcategories to ensure all changes are visible
        /// </summary>
        public static void SetSelectedCategory()
        {
            try
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<Settings>().categoryDefault != string.Empty)
                {
                    PartCategorizer.Category cat = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.categoryName == HighLogic.CurrentGame.Parameters.CustomParams<Settings>().categoryDefault);
                    if (cat != null)
                    {
                        cat.button.activeButton.SetState(KSP.UI.UIRadioButton.State.True, KSP.UI.UIRadioButton.CallType.APPLICATION, null, true);
                    }
                }

                if (HighLogic.CurrentGame.Parameters.CustomParams<Settings>().subCategoryDefault != string.Empty)
                {
                    // set the subcategory button
                    KSP.UI.UIRadioButton but = PartCategorizer.Instance.filters.FirstOrDefault(f => f.button.activeButton.Value)?.subcategories.FirstOrDefault(sC => {
                        return sC.button.categoryName == HighLogic.CurrentGame.Parameters.CustomParams<Settings>().subCategoryDefault;
                    })?.button.activeButton;
                    but.SetState(KSP.UI.UIRadioButton.State.True, KSP.UI.UIRadioButton.CallType.APPLICATION, null, true);
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Category refresh failed\r\n{e.InnerException}\r\n{e.StackTrace}", Logger.LogLevel.Error);
            }
        }

        private bool CheckPartVisible(AvailablePart part, PartCategorizer.Category category)
        {
            foreach (PartCategorizer.Category subcat in category.subcategories)
            {
                if (subcat.exclusionFilter.FilterCriteria(part))
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