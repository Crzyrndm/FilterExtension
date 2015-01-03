using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace FilterCreator
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class AppLauncherEditor : MonoBehaviour
    {
        private static ApplicationLauncherButton btnLauncher;
        private static Rect window;
        private static string iconPath = "";

        internal static bool bDisplayEditor = false;

        void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(this.OnAppLauncherReady);
            window = new Rect(Screen.width - 180, 40, 30, 30);
        }

        void OnDestroy()
        {
            if (btnLauncher != null)
                ApplicationLauncher.Instance.RemoveModApplication(btnLauncher);
            btnLauncher = null;
        }

        private void OnAppLauncherReady()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(this.OnAppLauncherReady);
            btnLauncher = ApplicationLauncher.Instance.AddModApplication(OnToggleTrue, OnToggleFalse,
                                                                        null, null, null, null,
                                                                        ApplicationLauncher.AppScenes.ALWAYS,
                                                                        GameDatabase.Instance.GetTexture(iconPath, false));
        }

        void OnGameSceneChange(GameScenes scene)
        {
            ApplicationLauncher.Instance.RemoveModApplication(btnLauncher);
        }

        private void OnToggleTrue()
        {
            bDisplayEditor = true;
        }

        private void OnToggleFalse()
        {
            bDisplayEditor = false;
        }
    }
}
