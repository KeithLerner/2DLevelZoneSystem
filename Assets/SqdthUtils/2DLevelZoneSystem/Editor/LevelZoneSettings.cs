using System;
using SqdthUtils._2DLevelZoneSystem.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR

using UnityEditor;

namespace SqdthUtils._2DLevelZoneSystem.Editor
{
    // [CreateAssetMenu( menuName = "2DLevelZoneSystem/LevelZoneSettings", 
    //     fileName = "LevelZoneSettings" )]
    public class LevelZoneSettings : ScriptableObject
    {
        public Camera defaultCameraSettings = new Camera();
        
        public static LevelZoneSettings Instance { get; private set; }
        
        [field: SerializeField] [Tooltip("NOT READY YET!!!!")]
        private bool cinemachineMode = false;
            
        [field: SerializeField]
        [Tooltip("The desired aspect ratio the game will be played at. " +
                 "This is used to draw camera bounding boxes surrounding level zones")]
        private Vector2Int targetAspectRatio = new Vector2Int(16, 9);

        [field: SerializeField]
        [Tooltip("The desired width of drawn debug lines.")]
        private float debugLineWidth = 8f;
        
        internal static LevelZoneSettings GetOrCreateSettings()
        {
            LevelZoneSettings settings = 
                AssetDatabase.LoadAssetAtPath<LevelZoneSettings>(Resources.KLevelZoneSettingsPath);
            UnityEngine.Resources.Load<LevelZoneSettings>(Resources
                .KLevelZoneSettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<LevelZoneSettings>();
                Instance = settings;
                AssetDatabase.CreateAsset(settings, Resources.KLevelZoneSettingsPath);
                AssetDatabase.SaveAssets();
                Debug.LogWarning("[2D Level Zone System Settings]: " +
                                 "Generating new 2D Level Zone System Settings.\n" +
                                 $"Located at {Resources.KLevelZoneSettingsPath}");
            }
            
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            hideFlags = HideFlags.HideInHierarchy;
            LevelZone.CinemachineMode = cinemachineMode;
            LevelZone.TargetAspectRatio = targetAspectRatio;
            LevelZone.DebugLineWidth = debugLineWidth;
        }
    }
}

#endif

namespace SqdthUtils._2DLevelZoneSystem
{
    public static class Resources
    {
        public const string KLevelZoneSettingsPath =
            "Assets/SqdthUtils/2DLevelZoneSystem/Resources/LevelZoneSettings.asset";
    }
}