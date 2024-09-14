using UnityEngine;

namespace SqdthUtils
{
    public class LevelZonePlayer : MonoBehaviour
    {
        [field: SerializeField]
        [Tooltip("The camera of this player.")]
        public Camera PlayerCamera { get; protected set; }
        
        [field: SerializeField] 
        [Tooltip("The speed that the player camera's transform moves at.")]
        public float CameraSpeed { get; set; } = 45f;
        
        /// <summary>
        /// Level zone the player is currently in. <b>Null</b> if not in a zone.
        /// </summary>
        public LevelZone CurrentZone { get; internal set; }

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
