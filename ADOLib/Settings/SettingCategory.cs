
using System;
using UnityEngine;
using UnityModManagerNet;
using static ADOLib.Settings.SettingsUI;

namespace ADOLib.Settings
{
    [Category("Settings", "Settings", priority: Int32.MaxValue)]
    public class SettingCategory : Category
    {
        public override UnityModManager.ModEntry ModEntry => ADOLib.ModEntry;

        public override void OnGUI()
        {
            GUILayout.Label("<b><color=#000000><size=35>Settings</size></color></b>");
            GUILayout.Label("Language/언어", Text);
            GUILayout.BeginHorizontal();
            if (Translation.Setting.Language == SystemLanguage.Korean)
            {
                if (GUILayout.Button("English", Selection, GUILayout.Width(300), GUILayout.Height(30))) Translation.Setting.Language = SystemLanguage.English;
                GUILayout.Button("한국어", SelectionActive, GUILayout.Width(300), GUILayout.Height(30));
            }
            else
            {
                GUILayout.Button("English", SelectionActive, GUILayout.Width(300), GUILayout.Height(30));
                if (GUILayout.Button("한국어", Selection, GUILayout.Width(300), GUILayout.Height(30))) Translation.Setting.Language = SystemLanguage.Korean;
            }
            GUILayout.EndHorizontal();
        }
    }
}
