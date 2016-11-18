using FilterExtensions.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FilterExtensions
{
    internal class FESettings : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomParameterUI("Hide unpurchased parts"
            , gameMode = GameParameters.GameMode.CAREER
            , toolTip = "Hide any parts that have been researched but not yet purchased")]
        public bool hideUnpurchased = true;

        [GameParameters.CustomParameterUI("Enable debug logging"
            , toolTip = "If you encounter a bug, please attempt to reproduce with this setting enabled")]
        public bool debug = false;

        [GameParameters.CustomParameterUI("Default to advanced display"
            , toolTip = "Enable to display both levels of the part categories on entering the editor")]
        public bool setAdvanced = true;

        [GameParameters.CustomParameterUI("Locate parts by mod"
            , toolTip = "Enable to split parts by mod folder in the \"Filter by Manufacturer\" category")]
        public bool replaceFbM = true;

        [GameParameters.CustomStringParameterUI("Default category"
            , toolTip = "The category to open on entering the editor"
            , lines = 1)]
        public string categoryDefault = string.Empty;

        [GameParameters.CustomStringParameterUI("Default subcategory"
            , toolTip = "The subcategory to open on entering the editor. Will only be populated once a default category is chosen and the window is reopened"
            , lines = 1)]
        public string subCategoryDefault = string.Empty;

        public override string Title
        {
            get
            {
                return "Filter Extensions";
            }
        }

        public override string Section
        {
            get
            {
                return "Editor Settings";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 10;
            }
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            hideUnpurchased = true;
            debug = false;
            setAdvanced = true;
            replaceFbM = true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            if (member.Name == "categoryDefault")
            {
                List<string> categories = new List<string>() { string.Empty };
                foreach (ConfigNodes.customCategory C in Core.Instance.Categories)
                {
                    categories.Add(C.categoryName);
                }
                return categories;
            }
            if (member.Name == "subCategoryDefault")
            {
                List<string> subcategories = new List<string>() { string.Empty };
                ConfigNodes.customCategory cat;
                if (Core.Instance.Categories.TryGetValue(C => C.categoryName == categoryDefault, out cat))
                {
                    foreach (ConfigNodes.subCategoryItem sc in cat.subCategories)
                    {
                        subcategories.Add(sc.subcategoryName);
                    }
                }
                return subcategories;
            }
            return null;
        }
    }
}