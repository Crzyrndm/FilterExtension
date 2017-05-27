using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterExtensions.Utility;
using KSP.UI.Screens;
using UnityEngine;

namespace FilterExtensions
{
    public static class IconLib
    {
        // Dictionary of icons created on entering the main menu
        public static Dictionary<string, RUI.Icons.Selectable.Icon> IconDict = new Dictionary<string, RUI.Icons.Selectable.Icon>();
        // if the icon isn't present, use this one
        private const string fallbackIcon = "stockIcon_fallback";

        /// <summary>
        /// loads all textures that are 32x32px into a dictionary using the filename as a key
        /// </summary>
        public static void Load()
        {
            GameDatabase.TextureInfo texInfo = null;
            Texture2D selectedTex = null;
            var texDict = new Dictionary<string, GameDatabase.TextureInfo>();
            for (int i = GameDatabase.Instance.databaseTexture.Count - 1; i >= 0; --i)
            {
                texInfo = GameDatabase.Instance.databaseTexture[i];
                if (texInfo.texture != null && texInfo.texture.width == 32 && texInfo.texture.height == 32)
                {
                    texDict.TryAdd(texInfo.name, texInfo);
                }
            }

            foreach (KeyValuePair<string, GameDatabase.TextureInfo> kvp in texDict)
            {
                if (texDict.TryGetValue(kvp.Value.name + "_selected", out texInfo))
                {
                    selectedTex = texInfo.texture;
                }
                else
                {
                    selectedTex = kvp.Value.texture;
                }

                string name = kvp.Value.name.Split(new char[] { '/', '\\' }).Last();
                var icon = new RUI.Icons.Selectable.Icon(name, kvp.Value.texture, selectedTex, false);
                IconDict.TryAdd(icon.name, icon);
            }
        }


        /// <summary>
        /// get the icon that matches a name
        /// </summary>
        /// <param name="name">the icon name</param>
        /// <returns>the icon if it is found, or the fallback icon if it is not</returns>
        public static RUI.Icons.Selectable.Icon GetIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];
            }
            if (IconDict.TryGetValue(name, out RUI.Icons.Selectable.Icon icon) || PartCategorizer.Instance.iconLoader.iconDictionary.TryGetValue(name, out icon))
            {
                return icon;
            }
            return PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];
        }

        /// <summary>
        /// get icon following the TryGet* syntax
        /// </summary>
        /// <param name="name">the icon name</param>
        /// <param name="icon">the icon that matches the name, or the fallback if no matches were found</param>
        /// <returns>true if a matching icon was found, false if fallback was required</returns>
        public static bool TryGetIcon(string name, out RUI.Icons.Selectable.Icon icon)
        {
            if (string.IsNullOrEmpty(name))
            {
                icon = PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];
                return false;
            }
            if (IconDict.TryGetValue(name, out icon))
            {
                return true;
            }
            if (PartCategorizer.Instance.iconLoader.iconDictionary.TryGetValue(name, out icon))
            {
                return true;
            }
            icon = PartCategorizer.Instance.iconLoader.iconDictionary[fallbackIcon];
            return false;
        }
    }
}
