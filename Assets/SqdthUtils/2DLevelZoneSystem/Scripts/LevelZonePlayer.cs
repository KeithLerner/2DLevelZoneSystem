using UnityEngine;

namespace SqdthUtils
{
    public class LevelZonePlayer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The camera of this player.")]
        private Camera playerCamera;

        public Camera PlayerCamera
        {
            get
            {
                if (playerCamera == null)
                {
                    playerCamera = 
                        new GameObject($"{gameObject.name}PlayerCamera",
                            typeof(Camera)).GetComponent<Camera>();
                }
                return playerCamera;
            }
        }

        [field: SerializeField] 
        [Tooltip("The speed that the player camera's transform moves at.")]
        public float CameraSpeed { get; set; } = 45f;
        
        /// <summary>
        /// Level zone the player is currently in. <b>Null</b> if not in a zone.
        /// </summary>
        public LevelZone CurrentZone { get; internal set; }
    }
}
