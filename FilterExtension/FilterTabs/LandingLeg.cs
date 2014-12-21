using System;
using System.Collections.Generic;
using System.Linq;

namespace PartFilters.FilterTabs
{
    using UnityEngine;
    using PartFilters.Categoriser;

    class LandingLeg
    {
        internal void Filter()
        {
            PartCategorizer.Icon icon = PartCategorizer.Instance.GetIcon("R&D_node_icon_landing"); // change this

            PartCategorizer.Category filterByFunction = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            PartCategorizer.AddCustomSubcategoryFilter(filterByFunction, "Legs", icon, p => PartType.isLandingLeg(p));

            RUIToggleButtonTyped button = filterByFunction.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }
}
