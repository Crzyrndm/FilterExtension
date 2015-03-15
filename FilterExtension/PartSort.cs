using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExtensions
{
    class PartSort
    {
        public EZInputDelegate sortDelegate;
        UIStateToggleBtn button;

        public PartSort(UIStateToggleBtn but)
        {
            sortDelegate = new EZInputDelegate(OnInput);
            button = but;
        }

        public void OnInput(ref POINTER_INFO ptr)
        {
            if (ptr.evt == POINTER_INFO.INPUT_EVENT.PRESS)
                Sort();
        }

        void Sort()
        {
            Core.Log("sorting " + button.spriteText.text + " in " + ((button.StateName == "ASC") ? "ascending order" : "descending order"));
        }
    }
}
