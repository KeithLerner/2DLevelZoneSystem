using System.IO;
using System.Collections.Generic;
using SqdthUtils._2DLevelZoneSystem.Scripts;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using System.CodeDom;
using UnityEditor;

namespace SqdthUtils._2DLevelZoneSystem.Editor
{
    public class LevelZoneSettingsProvider : SettingsProvider
        {
            private SerializedObject _mLevelZoneSettings;
            private Camera _mCamera;
            private CameraEditor _mCameraEditor;

            public LevelZoneSettingsProvider(
                string path, SettingsScope scopes,
                IEnumerable<string> keywords = null
            ) : base(path, scopes, keywords) { }

            public static bool IsSettingsAvailable()
            {
                return File.Exists(Resources.KLevelZoneSettingsPath);
            }

            /// <summary>
            /// This function is called when the user clicks on the target element in the Settings window.
            /// </summary>
            /// <param name="searchContext"></param>
            /// <param name="rootElement"></param>
            public override void OnActivate(string searchContext,
                VisualElement rootElement)
            {
                _mLevelZoneSettings = LevelZoneSettings.GetSerializedSettings();
                if (LevelZoneSettings.Instance.defaultCameraSettings == null)
                {
                    LevelZoneSettings.Instance.defaultCameraSettings = 
                        Camera.main == null ? new Camera() : Camera.main;
                    Debug.LogWarning("[2D Level Zone System Settings]: " +
                                     "Creating new default camera settings.");
                }
            }

            public override void OnGUI(string searchContext)
            {
                EditorGUILayout.PropertyField(
                    _mLevelZoneSettings.FindProperty("cinemachineMode"),
                    new GUIContent("Cinemachine Mode"));
                EditorGUILayout.PropertyField(
                    _mLevelZoneSettings.FindProperty("targetAspectRatio"),
                    new GUIContent("Target Aspect Ratio"));
                EditorGUILayout.PropertyField(
                    _mLevelZoneSettings.FindProperty("debugLineWidth"),
                    new GUIContent("Debug Line Width"));
                
                if (LevelZoneSettings.Instance != null &&
                    LevelZoneSettings.Instance.defaultCameraSettings != null)
                {
                    //EditorGUILayout.BeginFoldoutHeaderGroup(false, new GUIContent("Default Camera Settings"));
                    UnityEditor.Editor.CreateEditor(
                        LevelZoneSettings.Instance.defaultCameraSettings,
                        typeof(CameraEditor)
                    ).OnInspectorGUI();
                    //EditorGUILayout.EndFoldoutHeaderGroup();
                }

                // var ce = ScriptableObject.CreateInstance<CameraEditor>();
                // UnityEditor.Editor.CreateEditor(ce);

                _mLevelZoneSettings.ApplyModifiedPropertiesWithoutUndo();
            }

            // Register the SettingsProvider
            [SettingsProvider]
            public static SettingsProvider CreateMyCustomSettingsProvider()
            {
                if (IsSettingsAvailable())
                {
                    var provider = new LevelZoneSettingsProvider(
                        "Project/2DLevelZoneSettings", SettingsScope.Project);

                    provider.label = "2D Level Zone System Settings";

                    provider.keywords = new[]
                    {
                        "2D", "Levels", "Zones", "Target Aspect Ratio",
                        "Cinemachine Mode", "Debug Line Width",
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