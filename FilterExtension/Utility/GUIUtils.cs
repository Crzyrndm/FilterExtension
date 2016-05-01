using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FilterExtensions.Utility
{
    public static class GUIUtils
    {
        /// <summary>
        /// Draws a label and text box on the same row
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        public static void DrawLabelPlusBox(string label, ref string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            text = GUILayout.TextField(text);
            GUILayout.EndHorizontal();
        }

        public static Color convertToColor(string hex_ARGB)
        {
            if (string.IsNullOrEmpty(hex_ARGB))
                return Color.clear;
            hex_ARGB = hex_ARGB.Replace("#", "").Replace("0x", ""); // remove any hexadecimal identifiers

            byte a = 255;
            if (hex_ARGB.Length >= 8)
            {
                a = byte.Parse(hex_ARGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                hex_ARGB = hex_ARGB.Substring(2);
            }
            byte r = byte.Parse(hex_ARGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex_ARGB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex_ARGB.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, a);
        }
    }
}
