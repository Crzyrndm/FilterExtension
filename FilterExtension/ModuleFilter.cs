using System.Collections.Generic;
using System.Linq;

namespace FilterExtensions
{
    public class ModuleFilter : PartModule
    {
        /// <summary>
        /// list names of FE categories to push this part into even if it doesn't otherwise match the filters
        /// </summary>
        [KSPField]
        public string filterAdd;
        public HashSet<string> subcategoriesToAdd;

        /// <summary>
        /// list names of FE categories to never let this part be added to
        /// </summary>
        [KSPField]
        public string filterBlock;
        public HashSet<string> subcategoriesToBlock;

        public bool CheckForForceAdd(string subcategory)
        {
            AddStringToSet();
            return subcategoriesToAdd.Contains(subcategory);
        }

        public bool CheckForForceBlock(string subcategory)
        {
            BlockStringToSet();
            return subcategoriesToBlock.Contains(subcategory);
        }

        void AddStringToSet()
        {
            if (subcategoriesToAdd == null)
            {
                subcategoriesToAdd = new HashSet<string>();
                string[] temp = filterAdd.Split(',');
                for (int i = 0; i < temp.Length; ++i)
                {
                    subcategoriesToAdd.Add(temp[i].Trim());
                }
            }
        }

        void BlockStringToSet()
        {
            if (subcategoriesToBlock == null)
            {
                subcategoriesToBlock = new HashSet<string>();
                string[] temp = filterBlock.Split(',');
                for (int i = 0; i < temp.Length; ++i)
                {
                    subcategoriesToBlock.Add(temp[i].Trim());
                }
            }
        }

        public bool HasForceAdd()
        {
            AddStringToSet();
            return subcategoriesToAdd.Any();
        }

        public bool HasForceBlock()
        {
            BlockStringToSet();
            return subcategoriesToBlock.Any();
        }
    }
}