using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class Editor : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(checkState());   
        }

        IEnumerator checkState()
        {
            while (PartCategorizer.Instance == null)
                yield return null;

            // to redefine all fbf filters I need to clear them and recreate them. I have to do that before it becomes visible otherwise everything breaks big time
            if (Core.Instance.firstFilterByFunction != null)
            {
                PartCategorizer.Instance.filters.Find(c => c.button.categoryName == "Filter by Function").subcategories.Clear();
                PartCategorizer.AddCustomSubcategoryFilter(PartCategorizer.Instance.filters.Find(c => c.button.categoryName == "Filter by Function"), Core.Instance.firstFilterByFunction.subCategoryTitle, Core.getIcon(Core.Instance.firstFilterByFunction.iconName), Core.Instance.firstFilterByFunction.checkFilters);
            }
            if (Core.Instance.firstFilterByManufacturer != null)
            {
                PartCategorizer.Instance.filters.Find(c => c.button.categoryName == "Filter by Manufacturer").subcategories.Clear();
                PartCategorizer.AddCustomSubcategoryFilter(PartCategorizer.Instance.filters.Find(c => c.button.categoryName == "Filter by Manufacturer"), Core.Instance.firstFilterByManufacturer.subCategoryTitle, Core.getIcon(Core.Instance.firstFilterByManufacturer.iconName), Core.Instance.firstFilterByManufacturer.checkFilters);
            }
            if (Core.Instance.firstFilterByResource != null)
            {
                PartCategorizer.Instance.filters.Find(c => c.button.categoryName == "Filter by Resource").subcategories.Clear();
                PartCategorizer.AddCustomSubcategoryFilter(PartCategorizer.Instance.filters.Find(c => c.button.categoryName == "Filter by Resource"), Core.Instance.firstFilterByResource.subCategoryTitle, Core.getIcon(Core.Instance.firstFilterByResource.iconName), Core.Instance.firstFilterByResource.checkFilters);
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
