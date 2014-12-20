using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PartFilters
{
    class ControlSurface
    {
        internal void SurfaceFilter()
        {
            PartCategorizer.Icon icon = PartCategorizer.Instance.GetIcon("R&D_node_icon_stability");

            PartCategorizer.Category filterByFunction = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            PartCategorizer.AddCustomSubcategoryFilter(filterByFunction, "Control Surfaces", icon, p => p.moduleInfos.Any(m => m.moduleName == "Control Surface" || m.moduleName == "FAR Controllable"));

            RUIToggleButtonTyped button = filterByFunction.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }
}
