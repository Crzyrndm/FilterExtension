using System;

namespace FilterExtensions.ConfigNodes
{
    public class SubCategoryItem : IEquatable<SubCategoryItem>
    {
        public string SubcategoryName { get; set; }
        public bool ApplyTemplate { get; set; }

        public SubCategoryItem()
        {
            ApplyTemplate = true;
        }

        public SubCategoryItem(string name, bool useTemplate = true)
        {
            SubcategoryName = name;
            ApplyTemplate = useTemplate;
        }

        public bool Equals(SubCategoryItem sub)
        {
            if (ReferenceEquals(null, sub))
            {
                return false;
            }
            if (ReferenceEquals(this, sub))
            {
                return true;
            }

            return SubcategoryName.Equals(sub.SubcategoryName);
        }

        public override int GetHashCode()
        {
            return SubcategoryName.GetHashCode();
        }

        public override string ToString()
        {
            return SubcategoryName;
        }
    }
}