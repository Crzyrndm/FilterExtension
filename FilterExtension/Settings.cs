using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class Settings : MonoBehaviour
    {
        Rect settingsRect = new Rect(Screen.width / 2, Screen.height / 2, 400, 0);
        static bool showWindow;
        private static ApplicationLauncherButton btnLauncher;

        public void Start()
        {
            showWindow = false;
            if (btnLauncher != null)
                return;

            btnLauncher = ApplicationLauncher.Instance.AddModApplication(() => showWindow = !showWindow, () => showWindow = !showWindow,
                                                                        null, null, null, null,
                                                                        ApplicationLauncher.AppScenes.SPACECENTER,
                                                                        GameDatabase.Instance.GetTexture("000_FilterExtensions/Icons/FilterCreator", false));
        }

        public void OnDestroy()
        {
            ConfigNode settingsNode = new ConfigNode("FilterSettings");
            settingsNode.AddValue("hideUnpurchased", Core.Instance.hideUnpurchased);
            settingsNode.AddValue("debug", Core.Instance.debug);
            settingsNode.AddValue("setAdvanced", Core.Instance.setAdvanced);
            settingsNode.AddValue("replaceFbM", Core.Instance.replaceFbM);
            settingsNode.AddValue("categoryDefault", Core.Instance.categoryDefault);
            settingsNode.AddValue("subCategoryDefault", Core.Instance.subCategoryDefault);

            ConfigNode nodeToWrite = new ConfigNode();
            nodeToWrite.AddNode(settingsNode);

            nodeToWrite.Save(KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/000_FilterExtensions/Settings.cfg");
        }

        public void OnGUI()
        {
            if (!showWindow)
                return;
            GUI.skin = HighLogic.Skin;
            settingsRect = GUILayout.Window(6548792, settingsRect, drawWindow, "Filter Extensions Settings");
        }

        private void drawWindow(int id)
        {
            Core.Instance.debug = GUILayout.Toggle(Core.Instance.debug, "Enable logging");
            Core.Instance.setAdvanced = GUILayout.Toggle(Core.Instance.setAdvanced, "Default to Advanced mode");
            Core.Instance.hideUnpurchased = GUILayout.Toggle(Core.Instance.hideUnpurchased, "Hide unpurchased parts");
            Core.Instance.replaceFbM = GUILayout.Toggle(Core.Instance.replaceFbM, "Sort parts by folder in manufacturer tab (requires restart)");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Default Category");
            Core.Instance.categoryDefault = GUILayout.TextField(Core.Instance.categoryDefault);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Default Sub-category");
            Core.Instance.subCategoryDefault = GUILayout.TextField(Core.Instance.subCategoryDefault);
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }
    }
}
