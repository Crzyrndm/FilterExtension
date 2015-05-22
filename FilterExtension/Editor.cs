using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    using ConfigNodes;

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class Editor : MonoBehaviour
    {
        public static Editor instance;
        void Start()
        {
            instance = this;
            StartCoroutine(editorInit());
        }

        customSubCategory customTestCategory;
        PartCategorizer.Category testCategory;
        static ApplicationLauncherButton btn;

        IEnumerator editorInit()
        {
            while (PartCategorizer.Instance == null)
                yield return null;

            // stock filters
            // If I edit them later everything breaks
            // custom categories can't be created at this point
            // The event which most mods will be hooking into fires after this, so they still get their subCategories even though I clear the category
            foreach (PartCategorizer.Category C in PartCategorizer.Instance.filters)
            {
                customCategory cat = Core.Instance.Categories.FirstOrDefault(c => c.categoryName == C.button.categoryName);
                if (cat != null && cat.hasSubCategories() && cat.stockCategory)
                {
                    if (cat.behaviour == categoryTypeAndBehaviour.StockReplace)
                        C.subcategories.Clear();
                    cat.initialise();
                }
            }
            // custom categories
            // wait until the part menu is initialised
            while (!PartCategorizer.Ready)
                yield return null;

            // frames after the flag is set to wait before initialising. Minimum of two for things to work consistently
            for (int i = 0; i < 4; i++)
                yield return null;

            // run everything
            foreach (customCategory c in Core.Instance.Categories)
                if (!c.stockCategory)
                    c.initialise();

            // wait again so icon edits don't occur immediately and cause breakages
            for (int i = 0; i < 4; i++)
                yield return null;
            // edit names and icons of all subcategories
            foreach (PartCategorizer.Category c in PartCategorizer.Instance.filters)
                Core.Instance.namesAndIcons(c);

            // Remove any category with no subCategories (causes major breakages if selected).
            for (int i = 0; i < 4; i++)
                yield return null;
            List<PartCategorizer.Category> catsToDelete = PartCategorizer.Instance.filters.FindAll(c => c.subcategories.Count == 0);
            foreach (PartCategorizer.Category cat in catsToDelete)
            {
                //Core.Log("removing Category " + cat.button.categoryName);
                PartCategorizer.Instance.scrollListMain.scrollList.RemoveItem(cat.button.container, true);
                PartCategorizer.Instance.filters.Remove(cat);
            }

            // reveal categories because why not
            PartCategorizer.Instance.SetAdvancedMode();

            ConfigNode settings = GameDatabase.Instance.GetConfigNodes("FilterSettings")[0];
            bool test = false;
            if (settings.HasValue("testCategory") && bool.TryParse(settings.GetValue("testCategory"), out test))
            {
                if (test)
                {
                    customTestCategory = new customSubCategory("testCategory", "");
                    customTestCategory.filters.Add(new Filter(false));
                    customTestCategory.filters[0].checks.Add(defaultCheck());
                    customTestCategory.initialise(PartCategorizer.Instance.filters[0]);
                    testCategory = PartCategorizer.Instance.filters[0].subcategories.First(c => c.button.categoryName == "testCategory");
                    if (btn == null)
                        btn = ApplicationLauncher.Instance.AddModApplication(Toggle, Toggle, null, null, null, null, ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH, (Texture2D)null);
                    StartCoroutine(refresh());
                }
            }
            for (int i = 0; i < 4; i++)
                yield return null;
            Core.Instance.setSelectedCategory();
        }

        void Toggle()
        {
            showWindow = !showWindow;
        }

        IEnumerator refresh()
        {
            while (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                yield return new WaitForSeconds(1f);
                if (showWindow && testCategory.button.activeButton.State == RUIToggleButtonTyped.ButtonState.TRUE)
                    EditorPartList.Instance.Refresh();
            }
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0) && show)
            {
                Vector2 guiMousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                if (!otherWindowRect.Contains(guiMousePos))
                    show = false;
            }
        }

        Rect windowRect = new Rect(300, 200, 300, 0);
        Rect otherWindowRect = new Rect(0, 0, 0, 0);
        public void OnGUI()
        {
            if (customTestCategory == null)
                return;
            if (showWindow)
                windowRect = GUILayout.Window(65874985, windowRect, drawWindow, "");
            if (show && checkToEdit != null)
            {
                otherWindowRect.x = windowRect.x + windowRect.width;
                otherWindowRect.y = windowRect.y;
                otherWindowRect = GUILayout.Window(45684123, otherWindowRect, checkEditWindow, "");
            }
        }

        bool showWindow = false;
        int selectedFilter = 0;
        void drawWindow(int id)
        {
            if (customTestCategory.filters.Count == 0)
            {
                customTestCategory.filters.Add(new Filter(false));
                customTestCategory.filters[0].checks.Add(defaultCheck());
            }
            GUIContent[] filters = new GUIContent[customTestCategory.filters.Count];
            for (int i = 0; i < customTestCategory.filters.Count; i++)
                filters[i] = new GUIContent(i.ToString());
            GUILayout.BeginHorizontal();
            selectedFilter = GUILayout.SelectionGrid(selectedFilter, filters, filters.Length, GUILayout.Width(filters.Length * 40));
            if (GUILayout.Button("+"))
            {
                // add a new filter
                customTestCategory.filters.Add(new Filter(false));
                customTestCategory.filters[customTestCategory.filters.Count - 1].checks.Add(defaultCheck());
                selectedFilter = customTestCategory.filters.Count - 1;
            }
            GUILayout.EndHorizontal();
            if (customTestCategory.filters[selectedFilter].checks.Count > 0)
                drawFilter(customTestCategory.filters[selectedFilter]);

            GUILayout.Space(20);
            if (GUILayout.Button("Log subcategory node"))
                Debug.Log(customTestCategory.toConfigNode());

            GUI.DragWindow();
        }

        void drawFilter(Filter filter)
        {
            filter.invert = GUILayout.Toggle(filter.invert, "Invert filter result");
            foreach (Check c in filter.checks)
            {
                if (drawCheck(c))
                    filter.checks.Remove(c);
            }
            if (GUILayout.Button("Add Check"))
                filter.checks.Add(defaultCheck());
            // add check
        }

        bool drawCheck(Check check)
        {
            bool ret = false;
            GUILayout.BeginHorizontal();
            string s = "Type: " + check.type;
            if (check.type != "check")
                s += "\r\nValue: " + check.value;
            s += "\r\nInvert: " + check.invert.ToString();
            if (check.type == "moduleTitle" || check.type == "moduleName" || check.type == "resource" || check.type == "propellant" || check.type == "size" || check.type == "profile")
                s += "\r\nContains: " + check.contains.ToString();
            if (check.type == "size" || check.type == "crew" || check.type == "mass" || check.type == "cost" || check.type == "crash" || check.type == "maxTemp")
                s += "\r\nEquality: " + check.equality.ToString();
            if (GUILayout.Button(s))
            {
                checkToEdit = check;
                show = true;
            }
            if (GUILayout.Button("X", GUILayout.Height(60)))
                ret = true;
            GUILayout.EndHorizontal();
            return ret;
        }

        bool show = false;
        Check checkToEdit = null;
        void checkEditWindow(int id)
        {
            GUILayout.Label("Check Type");
            GUIContent[] content = new GUIContent[Enum.GetValues(typeof(CheckType)).Length];
            for (int i = 0; i < Enum.GetValues(typeof(CheckType)).Length; i++)
                content[i] = new GUIContent(((CheckType)i).ToString());

            checkToEdit.type = Check.getTypeString((CheckType)GUILayout.SelectionGrid((int)Check.getType(checkToEdit.type), content, 4));
            GUILayout.Space(20);

            if (checkToEdit.type != "check")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Value: ");
                checkToEdit.value = GUILayout.TextField(checkToEdit.value);
                GUILayout.EndHorizontal();
            }

            checkToEdit.invert = GUILayout.Toggle(checkToEdit.invert, "Invert Check");

            if (checkToEdit.type == "moduleTitle" || checkToEdit.type == "moduleName" || checkToEdit.type == "resource" || checkToEdit.type == "propellant" || checkToEdit.type == "size" || checkToEdit.type == "profile")
                checkToEdit.contains = GUILayout.Toggle(checkToEdit.contains, "Contains");
            if (checkToEdit.type == "size" || checkToEdit.type == "crew" || checkToEdit.type == "mass" || checkToEdit.type == "cost" || checkToEdit.type == "crash" || checkToEdit.type == "maxTemp")
            {
                GUIContent[] equalityContent = new GUIContent[3] {new GUIContent("="), new GUIContent("<"), new GUIContent(">")};
                checkToEdit.equality = (Check.Equality)GUILayout.SelectionGrid((int)checkToEdit.equality, equalityContent, 3);
            }
        }

        Check defaultCheck()
        {
            return new Check("category", "Pods");
        }
    }
}
