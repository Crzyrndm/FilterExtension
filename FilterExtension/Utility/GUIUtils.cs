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
    }
}
