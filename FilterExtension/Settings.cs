using KSP.UI.Screens;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FilterExtensions
{
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
            StartCoroutine(LoadBundleAssets());            
        }
        
        public void OnDestroy()
        {
            SaveSettings();
            windowInstance = null;
        }

        public IEnumerator LoadBundleAssets()
        {
            if (settingsCanvasPrefab == null)
            {
                while (!Caching.ready)
                    yield return null;
                using (WWW www = new WWW("file://" + KSPUtil.ApplicationRootPath + Path.DirectorySeparatorChar
                    + "GameData" + Path.DirectorySeparatorChar + "000_FilterExtensions" + Path.DirectorySeparatorChar + "fesettings.ksp"))
                {
                    yield return www;

                    AssetBundle assetBundle = www.assetBundle;
                    GameObject[] objects = assetBundle.LoadAllAssets<GameObject>();
                    for (int i = 0; i < objects.Length; ++i)
                    {
                        if (objects[i].name == "SettingsPanel")
                        {
                            settingsCanvasPrefab = objects[i].GetComponent<Canvas>();
                            settingsCanvasPrefab.enabled = false; // only show once the toolbar button is pressed
                            Core.Log("settings Canvas prefab loaded", Core.LogLevel.Warn);
                        }
                    }
                    InstantiateCanvas();
                    yield return new WaitForSeconds(10.0f);
                    assetBundle.Unload(false);
                }
            }
            else
            {
                InstantiateCanvas();
            }
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

        static void toggleSettingsVisible()
        {
            if (windowInstance != null)
                windowInstance.enabled = !windowInstance.enabled;
        }

        private void InstantiateCanvas()
        {
            windowInstance = Instantiate(settingsCanvasPrefab);
            windowPosition = windowInstance.transform.GetChild(0) as RectTransform;

            // drag events
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
