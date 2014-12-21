using System;
using System.Collections.Generic;
using System.Linq;

namespace PartFilters.FilterTabs
{
    using UnityEngine;
    using PartFilters.Categoriser;

    class ControlSurface
    {
        internal void Filter()
        {
            PartCategorizer.Icon icon = PartCategorizer.Instance.GetIcon("R&D_node_icon_stability");

            PartCategorizer.Category filterByFunction = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            PartCategorizer.AddCustomSubcategoryFilter(filterByFunction, "Control Surfaces", icon, p => PartType.isControlSurface(p));

            RUIToggleButtonTyped button = filterByFunction.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }
}
