using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    public class LevelZonePlayer : MonoBehaviour
    {
        public Vector2 Size { get; set; } = new Vector2(1, 2);
        
        [field: SerializeField]
        public Camera PlayerCamera { get; protected set; }
        [field: SerializeField] [Tooltip("The speed that the player camera moves at.")]
        public float CameraSpeed { get; set; } = 45f;
        public LevelZone CurrentZone { get; set; }

        private void Start()
        {
            if (PlayerCamera == null)
            {
                PlayerCamera = new GameObject($"{gameObject.name}PlayerCamera",
                    typeof(Camera)).GetComponent<Camera>();
            }
        }
    }
}
