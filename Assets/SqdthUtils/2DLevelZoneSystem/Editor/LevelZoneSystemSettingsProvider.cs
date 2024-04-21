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

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
            m_LevelZoneSystemSettings = LevelZoneSystemSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            // EditorGUILayout.Toggle("Split Screen Mode", false);
            // EditorGUILayout.Vector2IntField("Target Aspect Ratio", new Vector2Int(16, 9));
            
            EditorGUILayout.PropertyField(m_LevelZoneSystemSettings.FindProperty("SplitScreenMode"),
                new GUIContent("Split Screen Mode"));
            EditorGUILayout.PropertyField(m_LevelZoneSystemSettings.FindProperty("TargetAspectRatio"),
                new GUIContent("Target Aspect Ratio"));
            
            m_LevelZoneSystemSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new LevelZoneSystemSettingsProvider("Project/LevelZoneSystemSettingsProvider", SettingsScope.Project);

                provider.keywords = new[] { "Levels", "Zones", "Target Aspect Ratio", "Split Screen Mode" };
                
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}

#endif