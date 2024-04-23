using SqdthUtils._2DLevelZoneSystem.Editor;
using SqdthUtils._2DLevelZoneSystem.Scripts;
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
        private bool splitScreenMode = false;
            
        [field: SerializeField]
        [Tooltip("The desired aspect ratio the game will be played at. " +
                 "This is used to draw camera bounding boxes surrounding level zones")]
        private Vector2Int targetAspectRatio = new Vector2Int(16, 9);

        [field: SerializeField]
        [Tooltip("The desired width of drawn debug lines.")]
        private float debugLineWidth = 8f;
        
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
            LevelZone.LevelZoneSettings.SplitScreenMode = splitScreenMode;
            LevelZone.LevelZoneSettings.TargetAspectRatio = targetAspectRatio;
            LevelZone.LevelZoneSettings.DebugLineWidth = debugLineWidth;
        }
    }
}

#endif