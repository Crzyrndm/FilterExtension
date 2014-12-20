using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PartFilters
{
    class Wheel
    {
        internal void WheelFilter()
        {
            PartCategorizer.Icon icon = PartCategorizer.Instance.GetIcon("R&D_node_icon_fieldscience");

            PartCategorizer.Category filterByFunction = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            PartCategorizer.AddCustomSubcategoryFilter(filterByFunction, "Wheels", icon, p => p.moduleInfos.Any(m => m.moduleName == "Wheel" || m.moduleName == "Landing Gear"));

            RUIToggleButtonTyped button = filterByFunction.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }
}
