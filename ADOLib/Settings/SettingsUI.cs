using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Action = System.Action;

namespace ADOLib.Settings
{
    /// <summary>
    /// UI Class used for displaying ADOLib setting interface.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        internal static List<string> Tabs = new List<string>();
        internal static Dictionary<string, Action> Settings = new Dictionary<string, Action>();
        internal static Dictionary<string, Action> Saves = new Dictionary<string, Action>();
        internal static Dictionary<string, Category> Categories = new Dictionary<string, Category>();

        GUIStyle Background = new GUIStyle();
        GUIStyle Title = new GUIStyle();
        GUIStyle Tab = new GUIStyle();
        GUIStyle TabSelected = new GUIStyle();
        
#pragma warning disable 1591
        public static Texture2D ButtonActivated = new Texture2D(2, 2);
        public static Texture2D ButtonNotActivated = new Texture2D(2, 2);
        public static Texture2D BG = new Texture2D(2, 2);
        public static bool UI;
        public static bool Escape;
        public int selectedTab = 0;
#pragma warning restore 1591

        /// <summary>
        /// <see cref="GUIStyle" /> can be used with labels, toggles.
        /// </summary>
        public static GUIStyle Text;
        
        /// <summary>
        /// <see cref="GUIStyle" /> can be used with buttons, selection grids.
        /// </summary>
        public static GUIStyle Selection;
        
        /// <summary>
        /// Activated version of <see cref="Selection"/>.
        /// </summary>
        public static GUIStyle SelectionActive;
        
        /// <summary>
        /// <see cref="GUIStyle" /> can be used with text area, text fields.
        /// </summary>
        public static GUIStyle TextInput;

        private Vector2 _position = new Vector2(0, 0);

        public void Start() {
            ADOLib.Log("SettingsUI Start");
            Category.RegisterCategories(AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()));
        }

        public void Awake() {
            BG.LoadImage(File.ReadAllBytes($"{ADOLib.PathStatic}settingsBG.png"));
            Background.normal.background = BG;

            ButtonActivated.LoadImage(File.ReadAllBytes($"{ADOLib.PathStatic}buttonActivated.png"));
            ButtonNotActivated.LoadImage(File.ReadAllBytes($"{ADOLib.PathStatic}buttonNotActivated.png"));

            Text = GUIExtended.Text;
            Selection = GUIExtended.Selection;
            SelectionActive = GUIExtended.SelectionActive;
            TextInput = GUIExtended.TextInput;
            ADOLib.Log("SettingsUI Awake");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && UI)
            {
                Saves[Tabs[selectedTab]]();
                UI = false;
                Escape = true;
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.BackQuote))
                {
                    Saves[Tabs[selectedTab]]();
                    UI = !UI;
                }
            }
        }

        void OnGUI() {
            Tab.border.Add( new Rect(5, 5, 5, 5));
            Tab.normal.textColor = Color.black;
            Tab.hover.textColor = Color.black;
            Tab.alignment = TextAnchor.MiddleCenter;
            Tab.fontSize = 20;
            Tab.richText = true;
            Tab.normal.background = ButtonNotActivated;
            
            TabSelected.normal.textColor = Color.white;
            TabSelected.hover.textColor = Color.white;
            TabSelected.alignment = TextAnchor.MiddleCenter;
            TabSelected.fontSize = 22;
            TabSelected.richText = true;
            TabSelected.normal.background = ButtonActivated;
            if (!UI) return;
            GUILayout.BeginArea(new Rect(250, 150,Screen.width - 500, Screen.height - 300));
            GUILayout.Label(Background.normal.background, GUILayout.Width(Screen.width - 300));
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(260, 160,Screen.width - 520, Screen.height - 320));
            GUILayout.Label("<b><color=#FFFFFF>Settings</color></b>", Title);
            GUILayout.BeginHorizontal();
            var width = Screen.width - 520;
            var select = 0;
            var oneWidth = (width + 10) / Mathf.Max(Tabs.Count + 1, 8);
            foreach (var tab in Tabs)
            {
                if (selectedTab == select)
                {
                    GUILayout.BeginArea(new Rect(select * oneWidth, 35, oneWidth, 35));
                    GUILayout.Button(tab, TabSelected, GUILayout.Width(oneWidth), GUILayout.Height(35));
                }
                else
                {
                    GUILayout.BeginArea(new Rect(select * oneWidth, 40, oneWidth, 30));
                    if (GUILayout.Button(tab, Tab, GUILayout.Width(oneWidth), GUILayout.Height(30)))
                        selectedTab = select;
                }

                GUILayout.EndArea();
                select += 1;
            }

            
            GUILayout.BeginArea(new Rect(0, 67, width, Screen.height - 300));
            GUILayout.Label(ButtonActivated ,GUILayout.Width(width));
            GUILayout.EndArea();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(260, 230,Screen.width - 520, Screen.height - 400));
            _position = GUILayout.BeginScrollView(_position);
            
            Settings[Tabs[selectedTab]]();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}