using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR

using UnityEditor;

namespace SqdthUtils._2DLevelZoneSystem.Editor
{
    public class LevelZoneSystemSettingsProvider : SettingsProvider
    {
        private SerializedObject m_LevelZoneSystemSettings;
        
        public LevelZoneSystemSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : 
            base(path, scopes, keywords) { }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(LevelZoneSystemSettings.k_LevelZoneSystemSettingsPath);
        }

        /// <summary>
        /// This function is called when the user clicks on the target element in the Settings window.
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="rootElement"></param>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_LevelZoneSystemSettings = LevelZoneSystemSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.PropertyField(m_LevelZoneSystemSettings.FindProperty("splitScreenMode"),
                new GUIContent("Split Screen Mode"));
            EditorGUILayout.PropertyField(m_LevelZoneSystemSettings.FindProperty("targetAspectRatio"),
                new GUIContent("Target Aspect Ratio"));
            EditorGUILayout.PropertyField(m_LevelZoneSystemSettings.FindProperty("debugLineWidth"),
                new GUIContent("Debug Line Width"));
            
            m_LevelZoneSystemSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new LevelZoneSystemSettingsProvider("Project/LevelZoneSystemSettingsProvider", SettingsScope.Project);

                provider.label = "2D Level Zone System Settings";
                
                provider.keywords = new[]
                {
                    "2D", "Levels", "Zones", "Target Aspect Ratio", "Split Screen Mode", "Debug Line Width",
                    "Draw Line Width"
                };
                
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}

#endif