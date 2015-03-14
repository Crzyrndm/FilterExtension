using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    using ConfigNodes;

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class Editor : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(editorInit());   
        }

        IEnumerator editorInit()
        {
            while (PartCategorizer.Instance == null)
                yield return null;

            foreach (PartCategorizer.Category C in PartCategorizer.Instance.filters)
            {
                customCategory cat = Core.Instance.Categories.FirstOrDefault(c => c.categoryName == C.button.categoryName);
                if (cat != null && cat.hasSubCategories())
                {
                    C.subcategories.Clear();
                    cat.initialise();
                }
            }

            // wait until the part menu is initialised
            while (!PartCategorizer.Ready)
                yield return null;

            // frames after the flag is set to wait before initialising. Minimum of two for things to actually work
            for (int i = 0; i < 10; i++ )
                yield return null;
            
            // run everything
            Core.Instance.editor();
        }
    }
}
