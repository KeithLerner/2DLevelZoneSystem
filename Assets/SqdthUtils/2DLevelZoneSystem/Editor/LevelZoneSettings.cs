using SqdthUtils._2DLevelZoneSystem.Scripts;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SqdthUtils._2DLevelZoneSystem.Editor
{
    [CreateAssetMenu( menuName = "2DLevelZoneSystem/LevelZoneSettings", 
        fileName = "LevelZoneSettings" )]
    public class LevelZoneSettings : ScriptableObject
    {
        public static LevelZoneSettings Instance { get; private set; }
        
        [field: SerializeField]
        [Tooltip("The desired aspect ratio the game will be played at. " +
                 "This is used to draw camera bounding boxes surrounding level zones")]
        private Vector2Int targetCameraAspectRatio = new Vector2Int(16, 9);
        
        [field: SerializeField]
        [Tooltip("The desired size orthographic cameras. " +
                 "Used to draw debug bounding boxes.")]
        private float targetOrthographicCameraSize = 8f;

        [field: SerializeField] [field: Min(1f)]
        [Tooltip("The desired width of drawn debug lines.")]
        private float debugLineWidth = 8f;
        
        internal static SerializedObject GetSerializedSettings()
        {
            if (Instance == null)
                Instance = 
                    AssetDatabase.LoadAssetAtPath<LevelZoneSettings>(Resources
                        .KLevelZoneSettingsAssetPath);
            
            if (Instance == null)
            {
                Instance = CreateInstance<LevelZoneSettings>();
                AssetDatabase.CreateAsset(Instance, Resources.KLevelZoneSettingsAssetPath);
                AssetDatabase.SaveAssets();
                Debug.LogWarning("[2D Level Zone System Settings] " +
                                 "Generating new 2D Level Zone System Settings.\n" +
                                 $"Located at {Resources.KLevelZoneSettingsAssetPath}");
            }
            
            return new SerializedObject(Instance);
        }

        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            //hideFlags = HideFlags.HideInHierarchy;
            LevelZone.TargetAspectRatio = targetCameraAspectRatio;
            LevelZone.TargetOrthographicCameraSize =
                targetOrthographicCameraSize;
            LevelZone.DebugLineWidth = debugLineWidth;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}

#endif

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