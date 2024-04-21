using System;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace SqdthUtils._2DLevelZoneSystem.Editor
{
    [CreateAssetMenu( menuName = "2DLevelZoneSystem/LevelZoneSystemSettings", 
        fileName = "LevelZoneSystemSettings" )]
    public class LevelZoneSystemSettings : ScriptableObject
    {
        public const string k_LevelZoneSystemSettingsPath = 
            "Assets/SqdthUtils/2DLevelZoneSystem/Editor/LevelZoneSystemSettings.asset";

        [field: SerializeField] [Tooltip("NOT READY YET!!!!")]
        private bool SplitScreenMode = false;

        [field: SerializeField]
        [Tooltip("The desired aspect ratio the game will be played at. " +
                 "This is used to draw camera bounding boxes surrounding level zones")]
        private Vector2Int TargetAspectRatio = new Vector2Int(16, 9);
        
        internal static LevelZoneSystemSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LevelZoneSystemSettings>(k_LevelZoneSystemSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LevelZoneSystemSettings>();
                AssetDatabase.CreateAsset(settings, k_LevelZoneSystemSettingsPath);
                AssetDatabase.SaveAssets();
            }
            
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        private void OnValidate()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }
    }
}

#endif