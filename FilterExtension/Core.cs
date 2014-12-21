using System;
using System.Collections.Generic;

namespace FilterExtensions
{
    using UnityEngine;
    using FilterExtensions.Categoriser;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Core : MonoBehaviour
    {
        List<subCategory> subCategories = new List<subCategory>();

        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(SubCategories);

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SUBCATEGORY"))
            {
                subCategories.Add(new subCategory(node));
            }
        }

        private void SubCategories()
        {
            foreach (subCategory sC in subCategories)
            {
                sC.initialise();
            }
        }
    }
}
