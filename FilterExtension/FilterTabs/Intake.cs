using System;
using System.Collections.Generic;
using System.Linq;

namespace PartFilters.FilterTabs
{
    using UnityEngine;
    using PartFilters.Categoriser;

    class Intake
    {
        internal void Filter()
        {
            PartCategorizer.Icon icon = PartCategorizer.Instance.GetIcon("R&D_node_icon_experimentalaerodynamics");

            PartCategorizer.Category filterByFunction = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            PartCategorizer.AddCustomSubcategoryFilter(filterByFunction, "Intakes", icon, p => PartType.isResourceIntake(p));

            RUIToggleButtonTyped button = filterByFunction.button.activeButton;

            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }
}
