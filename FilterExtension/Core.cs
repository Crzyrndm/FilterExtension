using System;
using System.Collections.Generic;
using UnityEngine;

namespace PartFilters
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Core : MonoBehaviour
    {
        Wheel wheelFilt;
        ControlSurface contrSurfaceFilt;

        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(Filters);

            wheelFilt = new Wheel();
            contrSurfaceFilt = new ControlSurface();
        }

        private void Filters()
        {
            wheelFilt.WheelFilter();
            contrSurfaceFilt.SurfaceFilter();
        }
    }
}
