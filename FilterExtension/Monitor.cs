//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace FilterExtensions
//{
//    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
//    class Monitor : MonoBehaviour
//    {
//        public void Update()
//        {
//            if (PartCategorizer.Instance != null)
//            {
//                int index = 0;
//                foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
//                {
//                    if (c.button.activeButton.State == RUIToggleButtonTyped.ButtonState.TRUE)
//                    {
//                        print(c.button.categoryName);
//                        foreach (PartCategorizer.Category sC in c.subcategories)
//                        {
//                            if (sC.button.activeButton.State == RUIToggleButtonTyped.ButtonState.TRUE)
//                            {
//                                print(sC.button.categoryName);
//                                print(index = c.subcategories.FindIndex(sC2 => sC2.button.categoryName == sC.button.categoryName));
//                            }
//                        }
//                        if (Input.GetKeyDown(KeyCode.DownArrow))
//                        {
//                            //c.subcategories[index].button.activeButton.SetFalse(c.subcategories[index + 1].button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
//                            c.subcategories[index + 1].button.activeButton.SetTrue(c.subcategories[index + 1].button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
//                            c.button.activeButton.SetTrue(c.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
//                        }
//                        else if (Input.GetKeyDown(KeyCode.UpArrow))
//                        {
//                            //c.subcategories[index].button.activeButton.SetFalse(c.subcategories[index + 1].button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
//                            c.subcategories[index - 1].button.activeButton.SetTrue(c.subcategories[index + 1].button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
//                            c.button.activeButton.SetTrue(c.button.activeButton, RUIToggleButtonTyped.ClickType.FORCED);
//                        }
//                        break;
//                    }
//                }
//            }
//        }
//    }
//}
