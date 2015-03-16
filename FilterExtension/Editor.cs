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
        List<PartSort> sorters = new List<PartSort>();

        void Start()
        {
            StartCoroutine(editorInit());   
        }

        IEnumerator editorInit()
        {
            while (PartCategorizer.Instance == null)
                yield return null;

            // stock filters
            // If I edit them later everything breaks
            // custom categories can't be created at this point
            // The event which most mods will be hooking into fires after this, so they still get their subCategories if I clear the category
            foreach (PartCategorizer.Category C in PartCategorizer.Instance.filters)
            {
                customCategory cat = Core.Instance.Categories.FirstOrDefault(c => c.categoryName == C.button.categoryName);
                if (cat != null && cat.hasSubCategories() && cat.stockCategory)
                {
                    C.subcategories.Clear();
                    cat.initialise();
                }
            }

            // custom categories
            // wait until the part menu is initialised
            while (!PartCategorizer.Ready)
                yield return null;
            // frames after the flag is set to wait before initialising. Minimum of two for things to actually work
            for (int i = 0; i < 4; i++)
                yield return null;
            // run everything
            Core.Instance.editor();

            //for (int i = 0; i < EditorPartList.Instance.sortingGroup.sortingButtons.Count; i++)
            //{
            //    UIStateToggleBtn but = EditorPartList.Instance.sortingGroup.sortingButtons[i];
            //    Core.Log(but.spriteText.text); // Name, Mass, Cost, Size
            //    Core.Log(but.StateName); // ASC, DESC

            //    //PartSort sorter = new PartSort(but);
            //    //but.SetInputDelegate(sorter.sortDelegate);
            //    //sorters.Add(sorter);
            //}
        }

        void Update()
        {
            
        }
    }
}
