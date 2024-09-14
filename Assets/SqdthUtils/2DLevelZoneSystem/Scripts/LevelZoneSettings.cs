using UnityEngine;
using UnityEditor;
using Resources = SqdthUtils._2DLevelZoneSystem.Resources;

namespace SqdthUtils
{
    [CreateAssetMenu( menuName = "2DLevelZoneSystem/LevelZoneSettings", 
        fileName = "LevelZoneSettings" )]
    public class LevelZoneSettings : ScriptableObject
    {
        private static LevelZoneSettings _instance;
        public static LevelZoneSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = GetOrCreateSettings();
                return _instance;
            }
        }

        [field: SerializeField]
        [Tooltip("The desired aspect ratio the game will be played at. " +
                 "This is used to draw camera bounding boxes surrounding level zones")]
        private Vector2Int targetCameraAspectRatio = new Vector2Int(16, 9);
        public Vector2Int TargetCameraAspectRatio => targetCameraAspectRatio; 
        
        [field: SerializeField]
        [Tooltip("The desired size orthographic cameras. " +
                 "Used to draw debug bounding boxes.")]
        private float targetOrthographicCameraSize = 8f;
        public float TargetOrthographicCameraSize =>
            targetOrthographicCameraSize;

        [field: SerializeField] [field: Min(1f)]
        [Tooltip("The desired width of drawn debug lines.")]
        private float debugLineWidth = 8f;
        public float DebugLineWidth => debugLineWidth;
        
        private static LevelZoneSettings GetOrCreateSettings()
        {
            // Try getting settings asset
            string path = Resources.KLevelZoneSettingsAssetPath;
            LevelZoneSettings settings = 
                AssetDatabase.LoadAssetAtPath<LevelZoneSettings>(path);
            
            // Create settings if no settings found
            if (settings == null)
            {
                // Set hide flags
                settings.hideFlags = HideFlags.HideInHierarchy;
            
                // Set default settings values
                settings = CreateInstance<LevelZoneSettings>();
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                
                // Debug about no settings found
                Debug.LogWarning(
                    "[2D Level Zone System Settings] " +
                     "Generating new 2D Level Zone System Settings.\n" +
                     $"Located at {Resources.KLevelZoneSettingsAssetPath}"
                );
            }
            
            return settings;
        }
        
#if UNITY_EDITOR        
        
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
        
#endif
        
    }
}

namespace SqdthUtils._2DLevelZoneSystem
{
    public static class Resources
    {
        public const string KLevelZoneSettingsFolderPath =
            "Assets/SqdthUtils/2DLevelZoneSystem/Resources/";
        public const string KLevelZoneSettingsAssetPath =
            KLevelZoneSettingsFolderPath + "LevelZoneSettings.asset";
    }
}