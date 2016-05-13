using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace FilterExtensions
{
    using Utility;
    using KSP.UI.Screens;
    using KSP.UI;

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class Settings : MonoBehaviour
    {
        static Canvas settingsCanvasPrefab;
        static Canvas windowInstance;
        RectTransform windowPosition;


        private static ApplicationLauncherButton btnLauncher;

        public const string RelativeSettingsPath = "GameData/000_FilterExtensions/PluginData/";

        public static bool hideUnpurchased = true;
        public static bool debug = false;
        public static bool setAdvanced = true;
        public static bool replaceFbM = true;
        public static string categoryDefault = string.Empty;
        public static string subCategoryDefault = string.Empty;

        public void Start()
        {
            if (btnLauncher == null)
                btnLauncher = ApplicationLauncher.Instance.AddModApplication(toggleSettingsVisible, toggleSettingsVisible,
                                                                        null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER,
                                                                        GameDatabase.Instance.GetTexture("000_FilterExtensions/Icons/FilterCreator", false));

            LoadSettings();

            if (settingsCanvasPrefab == null)
                KSPAssets.Loaders.AssetLoader.LoadAssets(bundleLoaded, KSPAssets.Loaders.AssetLoader.GetAssetDefinitionWithName("000_FilterExtensions/fesettings", "SettingsPanel"));
            else
                InstantiateCanvas();
        }
        
        public void OnDestroy()
        {
            SaveSettings();
            windowInstance = null;
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

        void bundleLoaded(KSPAssets.Loaders.AssetLoader.Loader loader)
        {
            for (int i = 0; i < loader.definitions.Length; ++i)
            {
                UnityEngine.Object obj = loader.objects[i];
                if (obj == null || obj.name != "SettingsPanel")
                    continue;

                Canvas c = (obj as GameObject).GetComponent<Canvas>();
                if (c != null)
                {
                    settingsCanvasPrefab = c;
                    break;
                }
            }
            if (settingsCanvasPrefab == null)
            {
                Core.Log("No settings canvas prefab found", Core.LogLevel.Warn);
                return;
            }
            settingsCanvasPrefab.enabled = false;
            InstantiateCanvas();
        }

        static void toggleSettingsVisible()
        {
            if (windowInstance != null)
                windowInstance.enabled = !windowInstance.enabled;
        }

        private void InstantiateCanvas()
        {
            windowInstance = Instantiate(settingsCanvasPrefab);
            windowPosition = windowInstance.transform.GetChild(0) as RectTransform; // windowInstance.gameObject.GetChild("Panel");
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener((x) => windowDrag(x));
            windowPosition.gameObject.GetComponent<EventTrigger>().triggers.Add(entry);


            Toggle[] boolSettings = windowInstance.GetComponentsInChildren<Toggle>();
            foreach (Toggle t in boolSettings)
            {
                switch (t.name)
                {
                    case "tgl_Debug":
                        t.isOn = Settings.debug;
                        t.onValueChanged.AddListener(dbg_toggleChanged);
                        break;
                    case "tgl_unpurchased":
                        t.isOn = Settings.hideUnpurchased;
                        t.onValueChanged.AddListener(hide_toggleChanged);
                        break;
                    case "tgl_advanced":
                        t.isOn = Settings.setAdvanced;
                        t.onValueChanged.AddListener(setAdv_toggleChanged);
                        break;
                    case "tgl_replaceFBM":
                        t.isOn = Settings.hideUnpurchased;
                        t.onValueChanged.AddListener(rplFbM_toggleChanged);
                        break;
                }
            }
            InputField[] textSettings = windowInstance.GetComponentsInChildren<InputField>();
            foreach (InputField input in textSettings)
            {
                switch (input.name)
                {
                    case "input_category":
                        input.text = Settings.categoryDefault;
                        input.onValueChange.AddListener(cat_txtInputChanged);
                        break;
                    case "input_subcategory":
                        input.text = Settings.subCategoryDefault;
                        input.onValueChange.AddListener(subCat_txtInputChanged);
                        break;
                }
            }
        }

        public void dbg_toggleChanged(bool newValue)
        {
            Settings.debug = newValue;
        }

        public void hide_toggleChanged(bool newValue)
        {
            Settings.hideUnpurchased = newValue;
        }

        public void setAdv_toggleChanged(bool newValue)
        {
            Settings.setAdvanced = newValue;
        }

        public void rplFbM_toggleChanged(bool newValue)
        {
            Settings.replaceFbM = newValue;
        }

        public void cat_txtInputChanged(string newValue)
        {
            Settings.categoryDefault = newValue;
        }

        public void subCat_txtInputChanged(string newValue)
        {
            Settings.subCategoryDefault = newValue;
        }

        public void windowDrag(UnityEngine.EventSystems.BaseEventData data)
        {
            windowPosition.position += new Vector3(((PointerEventData)data).delta.x, ((PointerEventData)data).delta.y);
        }
    }
}
