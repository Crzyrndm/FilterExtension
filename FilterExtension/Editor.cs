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

        IEnumerator editorInit()
        {
            while (PartCategorizer.Instance == null)
                yield return null;

            // stock filters
            // If I edit them later everything breaks
            // custom categories can't be created at this point
            // The event which most mods will be hooking into fires after this, so they still get their subCategories even though I clear the category
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

            // run everything
            foreach (customCategory c in Core.Instance.Categories)
                if (!c.stockCategory)
                    c.initialise();

            // wait again so icon edits don't occur immediately and cause breakages
            for (int i = 0; i < 4; i++)
                yield return null;
            // edit names and icons of all subcategories
            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
                Core.Instance.namesAndIcons(c);

            Core.Instance.setSelectedCategory();
            // Remove any category with no subCategories (causes major breakages).
            for (int i = 0; i < 4; i++)
                yield return null;
            List<PartCategorizer.Category> catsToDelete = PartCategorizer.Instance.filters.FindAll(c => c.subcategories.Count == 0);
            foreach (PartCategorizer.Category cat in catsToDelete)
            {
                //Core.Log("removing Category " + cat.button.categoryName);
                PartCategorizer.Instance.scrollListMain.scrollList.RemoveItem(cat.button.container, true);
                PartCategorizer.Instance.filters.Remove(cat);
            }

            // reveal categories because why not
            PartCategorizer.Instance.SetAdvancedMode();
        }
    }
}
