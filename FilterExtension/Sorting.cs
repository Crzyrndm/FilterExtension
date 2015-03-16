using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExtensions
{
    using Utility;
    class Sorting
    {
        public EZInputDelegate sortDelegate;
        UIStateToggleBtn button;
        public string sortType;
        PartComparer comparer = new PartComparer();

        public Sorting(UIStateToggleBtn but, string sort)
        {
            sortDelegate = new EZInputDelegate(OnInput);
            button = but;
            sortType = sort;
        }

        public void OnInput(ref POINTER_INFO ptr)
        {
            if (ptr.evt == POINTER_INFO.INPUT_EVENT.PRESS)
                Sort();
        }

        void Sort()
        {
            Core.Log("sorting " + button.spriteText.text + " in " + ((button.StateName == "ASC") ? "ascending order" : "descending order"));
            EditorPartList.Instance.Refresh(comparer);
            EditorPartList.Instance.Refresh();
        }
    }

    public class PartComparer : IComparer<AvailablePart>
    {
        public int Compare(AvailablePart p1, AvailablePart p2)
        {
            if (p1 == null)
            {
                if (p2 == null)
                    return 0;
                else
                    return -1;
            }
            if (p2 == null)
                return 1;

            if (p1.partPrefab.mass == p2.partPrefab.mass)
                return 0;
            else if (p1.partPrefab.mass > p2.partPrefab.mass)
                return 1;
            else
                return -1;
        }
    }
}
