using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace FilterExtensions
{
    using Utility;
    using KSP.UI.Screens;

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class Settings : MonoBehaviour
    {
        Rect settingsRect = new Rect(Screen.width / 2, Screen.height / 2, 400, 0);
        static bool showWindow;
        private static ApplicationLauncherButton btnLauncher;

        public const string RelativeSettingsPath = "GameData/000_FilterExtensions/PluginData/";

        public static bool hideUnpurchased = true;
        public static bool debug = false;
        public static bool setAdvanced = true;
        public static bool replaceFbM = true;
        public static string categoryDefault = "";
        public static string subCategoryDefault = "";

        public void Start()
        {
            showWindow = false;
            if (btnLauncher == null)
                btnLauncher = ApplicationLauncher.Instance.AddModApplication(() => showWindow = !showWindow, () => showWindow = !showWindow,
                                                                        null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER,
                                                                        GameDatabase.Instance.GetTexture("000_FilterExtensions/Icons/FilterCreator", false));

            LoadSettings();
        }
        
        public void OnDestroy()
        {
            SaveSettings();
        }

        public void OnGUI()
        {
            if (!showWindow)
                return;
            GUI.skin = HighLogic.Skin;
            settingsRect = GUILayout.Window(6548792, settingsRect, drawWindow, "Filter Extensions Settings");
        }

        public static void LoadSettings()
        {
            if (File.Exists(KSPUtil.ApplicationRootPath.Replace("\\", "/") + RelativeSettingsPath + "Settings.cfg"))
            {
                ConfigNode settings = ConfigNode.Load(KSPUtil.ApplicationRootPath.Replace("\\", "/") + RelativeSettingsPath + "Settings.cfg");
                if (settings != null)
                {
                    bool.TryParse(settings.GetValue("hideUnpurchased"), out hideUnpurchased);
                    bool.TryParse(settings.GetValue("debug"), out debug);
                    if (!bool.TryParse(settings.GetValue("setAdvanced"), out setAdvanced))
                        setAdvanced = true;
                    if (!bool.TryParse(settings.GetValue("replaceFbM"), out replaceFbM))
                        replaceFbM = true;
                    categoryDefault = settings.GetValue("categoryDefault");
                    if (categoryDefault == null)
                        categoryDefault = string.Empty;
                    subCategoryDefault = settings.GetValue("subCategoryDefault");
                    if (subCategoryDefault == null)
                        subCategoryDefault = string.Empty;
                }
            }
        }

        public static void SaveSettings()
        {
            ConfigNode settingsNode = new ConfigNode("FilterSettings");
            settingsNode.AddValue("hideUnpurchased", hideUnpurchased);
            settingsNode.AddValue("debug", debug);
            settingsNode.AddValue("setAdvanced", setAdvanced);
            settingsNode.AddValue("replaceFbM", replaceFbM);
            settingsNode.AddValue("categoryDefault", categoryDefault);
            settingsNode.AddValue("subCategoryDefault", subCategoryDefault);

            if (!Directory.Exists(KSPUtil.ApplicationRootPath.Replace("\\", "/") + RelativeSettingsPath))
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath.Replace("\\", "/") + RelativeSettingsPath);
            settingsNode.Save(KSPUtil.ApplicationRootPath.Replace("\\", "/") + RelativeSettingsPath + "Settings.cfg");
        }

        private void drawWindow(int id)
        {
            debug = GUILayout.Toggle(debug, "Enable logging");
            setAdvanced = GUILayout.Toggle(setAdvanced, "Default to Advanced mode");
            hideUnpurchased = GUILayout.Toggle(hideUnpurchased, "Hide unpurchased parts");
            replaceFbM = GUILayout.Toggle(replaceFbM, "Sort parts by folder in manufacturer tab (requires restart)");

            GUIUtils.DrawLabelPlusBox("Default Category", ref categoryDefault);
            GUIUtils.DrawLabelPlusBox("Default Sub-category", ref subCategoryDefault);

            GUI.DragWindow();
        }
    }
}
