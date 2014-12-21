using System;
using System.Collections.Generic;
using UnityEngine;

namespace PartFilters
{
    using UnityEngine;
    using PartFilters.Categoriser;
    using PartFilters.FilterTabs;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Core : MonoBehaviour
    {
        CargoBay cargoBayFilt;
        ControlSurface contrSurfaceFilt;
        Intake intakeFilt;
        LandingLeg legFilt;
        LFLOxEngines rocketFilt;
        Parachute paraFilt;
        StorageEc batFilt;
        Wheel wheelFilt;

        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(Filters);

            cargoBayFilt = new CargoBay();
            contrSurfaceFilt = new ControlSurface();
            intakeFilt = new Intake();
            legFilt = new LandingLeg();
            rocketFilt = new LFLOxEngines();
            paraFilt = new Parachute();
            batFilt = new StorageEc();
            wheelFilt = new Wheel();
        }

        private void Filters()
        {
            cargoBayFilt.Filter();
            contrSurfaceFilt.Filter();
            intakeFilt.Filter();
            legFilt.Filter();
            rocketFilt.Filter();
            paraFilt.Filter();
            batFilt.Filter();
            wheelFilt.Filter();
        }
    }
}
