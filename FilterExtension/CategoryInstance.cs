using FilterExtensions.ConfigNodes;
using FilterExtensions.Utility;
using KSP.Localization;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExtensions
{
    public class CategoryInstance
    {
        public string Name { get; }
        public string Icon { get; }
        public UnityEngine.Color Colour { get; }
        public CategoryNode.CategoryType Type { get; }
        public CategoryNode.CategoryBehaviour Behaviour { get; }

        public List<SubCategoryInstance> Subcategories { get; }


        public CategoryInstance(CategoryNode protoC, Dictionary<string, SubcategoryNode> allSubCats)
        {
            Name = Localizer.Format(protoC.CategoryName);
            if (string.IsNullOrEmpty(Name))
            {
                Logger.Log("Category name is blank", Logger.LogLevel.Warn);
                return;
            }
            Icon = protoC.IconName;
            Colour = protoC.Colour;
            Type = protoC.Type;
            Behaviour = protoC.Behaviour;
            Subcategories = new List<SubCategoryInstance>();
            foreach (SubCategoryItem sci in protoC.SubCategories)
            {
                if (allSubCats.TryGetValue(sci.SubcategoryName, out SubcategoryNode protoSC) && protoSC != null)
                {
                    var node = new SubcategoryNode(protoSC, sci.ApplyTemplate ? protoC : null);
                    var instance = new SubCategoryInstance(node, PartLoader.LoadedPartsList);
                    if (instance.Valid)
                    {
                        Subcategories.Add(instance);
                    }
                }
            }
            if (!Subcategories.Any())
            {
                throw new ArgumentException($"No subcategories valid, abandon instantiation of {Name}");
            }
        }

        public void Initialise()
        {            
            if (Type == CategoryNode.CategoryType.NEW)
            {
                RUI.Icons.Selectable.Icon icon = IconLib.GetIcon(this);
                PartCategorizer.Category category = PartCategorizer.AddCustomFilter(Name, Localizer.Format(Name), icon, Colour);
                category.displayType = EditorPartList.State.PartsList;
                category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;
                InstanceSubcategories(category);
            }
            else
            {
                if (!PartCategorizer.Instance.filters.TryGetValue(c => string.Equals(Localizer.Format(c.button.categoryName), Name, StringComparison.OrdinalIgnoreCase),
                    out PartCategorizer.Category category))
                {
                    Logger.Log($"No category of this name was found to manipulate: {Name}", Logger.LogLevel.Warn);
                    return;
                }
                else if (Behaviour == CategoryNode.CategoryBehaviour.Replace)
                {
                    if (category.button.activeButton.CurrentState == KSP.UI.UIRadioButton.State.True)
                    {
                        PartCategorizer.Category subcat = category.subcategories.Find(c => c.button.activeButton.CurrentState == KSP.UI.UIRadioButton.State.True);
                        if (subcat != null)
                        {
                            subcat.OnFalseSUB(subcat);
                        }
                        PartCategorizer.Instance.scrollListSub.Clear(false);
                    }
                    category.subcategories.Clear();
                }
                InstanceSubcategories(category);
            }
        }

        void InstanceSubcategories(PartCategorizer.Category category)
        {
            foreach (SubCategoryInstance sc in Subcategories)
            {
                try
                {
                    sc.Initialise(category);
                }
                catch (Exception ex)
                {
                    Logger.Log($"{sc} failed to initialise"
                        + $"\r\nCategory: {Name}"
                        + $"\r\nSubcategory Valid: {sc.Valid}"
                        + $"\r\n{ex.Message}"
                        + $"\r\n{ex.StackTrace}",
                        Logger.LogLevel.Error);
                }
            }
        }
    }
}
