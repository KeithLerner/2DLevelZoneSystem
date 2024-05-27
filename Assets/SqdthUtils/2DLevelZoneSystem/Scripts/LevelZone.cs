using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelZone : MonoBehaviour, ISnapToBounds
    {
        public static bool CinemachineMode { get; set; }
        public static Vector2Int TargetAspectRatio { get; set; }
        public static float DebugLineWidth { get; set; }
        public static float LineWidth { get; set; }
    
        public enum ScrollDirection { Horizontal, Vertical, FollowPlayer, NoScroll }
        [Header("Behavior")]
        [Tooltip("Which direction the room will scroll in.")]
        public ScrollDirection scrollDirection;
        [Tooltip("How far from zero the camera should align on the axis opposite scrolling direction.")]
        [SerializeField] private float camOffset;

        public Vector2 CamOffset
        {
            get
            {
                return scrollDirection switch
                {
                    ScrollDirection.Horizontal => Vector2.up    * camOffset,
                    ScrollDirection.Vertical   => Vector2.right * camOffset,
                    _                          => Vector2.zero
                };
            }
        }
        [Tooltip("Forces cameras to lock to edge centers when leaving the level zone.")] 
        [SerializeField] private bool forceEdgeCenters = false;
        public bool ForceEdgeCenters => forceEdgeCenters;
    
        // == Sizing ==
        public Vector2 Size 
        { 
            get => size;
            set => size = value;
        }
        [SerializeField] private Vector2 size = new Vector2(32, 32);
        private float ScreenAspect => (float)TargetAspectRatio.x / TargetAspectRatio.y; 
        float CameraHeight => Camera.main.orthographicSize * 2;
        public Vector2 CameraSize => new Vector2(CameraHeight * ScreenAspect, CameraHeight);
        public Bounds CameraBounds => new Bounds(transform.position + (Vector3)CamOffset, 
            Size + CameraSize);
        
        // == Snapping ==
        public Bounds SnappingBounds
        {
            get
            {
                if (transform.parent.TryGetComponent(out LevelZone lz))
                {
                    return lz.BColl.bounds;
                }
                return new Bounds();
            }
        }
        public Vector2 SnappingOffset => Size / 2f;

        // == Debug ==
        private const float colorAlpha = .4f; 
        public bool DrawLevelZone => drawZone;
        [field: Header("Visualization")]
        [SerializeField] private bool drawZone = true;
        public static bool DoesCameraOffset(ScrollDirection _scrollDirection)
        {
            return _scrollDirection == ScrollDirection.Horizontal || _scrollDirection == ScrollDirection.Vertical;
        }
        [SerializeField] private Color overrideLevelZoneColor = new Color(1,1,1,colorAlpha);
        public Color LevelZoneColor => 
            overrideLevelZoneColor != new Color(1,1,1,colorAlpha) ? 
                overrideLevelZoneColor :
                scrollDirection switch
                {
                    ScrollDirection.Horizontal => new Color(.2f, .2f, 1, colorAlpha),
                    ScrollDirection.Vertical => new Color(1, .2f, .2f, colorAlpha),
                    ScrollDirection.FollowPlayer => new Color(.2f, 1, .2f, colorAlpha),
                    _ => new Color(.2f, .2f, .2f, colorAlpha)
                };
        public void RandomizeColor()
        {
            overrideLevelZoneColor = new Color(Random.value, Random.value, Random.value, colorAlpha);
        }

        public BoxCollider2D BColl { get; private set; }
        private GameObject playerGO;
        private Transform camTransform;

        private void Start()
        {
            if (BColl == null)
                BColl = GetComponent<BoxCollider2D>();
            BColl.isTrigger = true;
            BColl.size = Size;
        
            // Disable editing the box collider via the inspector
            // Forces changes to be made via the script
            if (BColl.hideFlags != HideFlags.HideInInspector)
                BColl.hideFlags =  HideFlags.HideInInspector;
        
            if (playerGO == null)
                playerGO = GameObject.FindWithTag("Player");
        
            if (camTransform == null)
                camTransform = Camera.main?.transform;
        }
    
        /// <summary>
        /// Check if a position is inside of the level zone.
        /// </summary>
        /// <param name="targetPosition"> The position to check. </param>
        /// <returns></returns>
        private bool IsInsideLevelZone(Vector3 targetPosition)
        {
            // Get min and max bounding position
            Vector2 pos = transform.position;
            Vector2 extents = (Vector2)BColl.bounds.extents;
            Vector2 minBounds = pos - extents;
            Vector2 maxBounds = pos + extents;

            return targetPosition.x >= minBounds.x && targetPosition.x <= maxBounds.x &&
                   targetPosition.y >= minBounds.y && targetPosition.y <= maxBounds.y;
        }

        /// <summary>
        /// Get center points on edges of the level zone
        /// </summary>
        /// <returns> Array of center points on the edges of the level zone.</returns>
        public Vector2[] GetEdgeCenters()
        {
            if (BColl == null)
            {
                BColl = GetComponent<BoxCollider2D>();
            }
        
            // Get min and max bounding position
            Vector2 pos = transform.position;
            Vector2 extents = (Vector2)BColl.bounds.extents;
            Vector2 minBounds = pos - extents;
            Vector2 maxBounds = pos + extents;
        
            return scrollDirection switch
            {
                ScrollDirection.Horizontal => new Vector2[]
                {
                    new Vector2(minBounds.x, transform.position.y),
                    new Vector2(maxBounds.x, transform.position.y),
                },
                ScrollDirection.Vertical => new Vector2[]
                {
                    new Vector2(transform.position.x, minBounds.y),
                    new Vector2(transform.position.x, maxBounds.y)
                },
                _ => new Vector2[]
                {
                    new Vector2(minBounds.x, transform.position.y),
                    new Vector2(maxBounds.x, transform.position.y),
                    new Vector2(transform.position.x, minBounds.y),
                    new Vector2(transform.position.x, maxBounds.y)
                }
            };
        }

        /// <summary>
        /// Get the nearest edge center to a target location. Uses GetEdgeCenters.
        /// </summary>
        /// <param name="targetPosition"> The target position to find the nearest edge center to. </param>
        /// <returns> A Vector2 of the nearest edge center to the targetPosition. </returns>
        public Vector2 GetNearestEdgeCenter(Vector2 targetPosition)
        {
            // Get details from owning level zone
            Vector2 ownerPos = transform.position;

            // Get center points on edges of the bounding box
            Vector2[] edgePoints = GetEdgeCenters();

            // Get nearest point
            Vector2 nearestPoint = ownerPos;
            float nearestDistance = float.MaxValue;
            foreach (Vector2 edgePoint in edgePoints)
            {
                float distance = Vector2.Distance(
                    targetPosition, 
                    edgePoint
                );
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPoint = edgePoint;
                }
            }

            // Return nearest point
            return nearestPoint;
        }

        public Vector2 GetNearestEdgePoint(Vector2 targetPosition)
        {
            if (BColl == null)
            {
                BColl = GetComponent<BoxCollider2D>();
            }
        
            // Get min and max bounding position
            Bounds bCollBounds = BColl.bounds;
            Vector2 pos = bCollBounds.center;
            Vector3 extents = bCollBounds.extents;
            float clampedX;
            float clampedY;
        
            // Clamp the specified point to the boundaries of the box
            clampedX = Mathf.Clamp(targetPosition.x, pos.x - extents.x, pos.x + extents.x);
            clampedY = Mathf.Clamp(targetPosition.y, pos.y - extents.y, pos.y + extents.y);
        
            // Return the clamped point as the nearest edge point
            return new Vector2(clampedX, clampedY);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Get player or early return for non-players
            if (!other.TryGetComponent(out LevelZonePlayer player)) return;
            
            // Check that player is not in another zone or early return
            // Allows zones to neighbor each other without making use of
            // entrances
            if (player.CurrentZone == null) player.CurrentZone = this;
            if (player.CurrentZone != this) return;
	
            // Update camera transform to this player's transform
            camTransform = player.PlayerCamera.transform;
        
            // Get target camera position
            Vector3 cPos = camTransform.position; // camera position
            Vector2 laPos = transform.position + (Vector3)BColl.offset; // level zone position
            Vector2 pPos = playerGO.transform.position; // Player position
            Vector3 targetCamPos = scrollDirection switch // target position for camera based on level zone scroll direction
            {
                // This level zone scrolls horizontally
                ScrollDirection.Horizontal => 
                    new Vector2(pPos.x, laPos.y + camOffset),
            
                // This level zone scrolls vertically
                ScrollDirection.Vertical => 
                    new Vector2(laPos.x + camOffset, pPos.y),
            
                ScrollDirection.FollowPlayer =>
                    new Vector2(pPos.x, pPos.y),
            
                // This level zone does not scroll
                _ => new Vector2(laPos.x, laPos.y),
            };

            // Set camera target position if not in zone bounds
            bool isInside = IsInsideLevelZone(pPos);
            if (!isInside)
            {
                // Update target camera position depending on if this zone
                // forces edge center transitions
                targetCamPos = forceEdgeCenters ? 
                    GetNearestEdgeCenter(cPos) :
                    GetNearestEdgePoint(cPos);
                
                // Update player's current zone to null
                // Allows zones to neighbor each other without making use of
                // entrances
                player.CurrentZone = null;
            }
            
            // Fix target camera position's Z value
            targetCamPos.z = cPos.z;
            
            // Set player's camera position
            camTransform.position = Vector3.Lerp(
                cPos, targetCamPos, 
                Time.fixedDeltaTime * player.CameraSpeed
            );
        }

#if UNITY_EDITOR
    
        private void OnDrawGizmos()
        {
            // Early exit for don't draw room
            if (!DrawLevelZone) return;

            Start();
        
            // Draw level zone area using specified color
            Gizmos.color = LevelZoneColor;
            Gizmos.DrawCube(transform.position, (Vector3)BColl.size);

            // Draw camera scroll lines when applicable
            UnityEditor.Handles.color = Color.white;
            Bounds bCollBounds = BColl.bounds;
            switch (scrollDirection)
            {
                case ScrollDirection.Horizontal:
                    UnityEditor.Handles.DrawAAPolyLine(LineWidth, 
                        bCollBounds.center + Vector3.up * camOffset - Vector3.right * bCollBounds.extents.x, 
                        bCollBounds.center + Vector3.up * camOffset + Vector3.right * bCollBounds.extents.x);
                    break;
            
                case ScrollDirection.Vertical:
                    UnityEditor.Handles.DrawAAPolyLine(LineWidth, 
                        bCollBounds.center + Vector3.right * camOffset - Vector3.up * bCollBounds.extents.y, 
                        bCollBounds.center + Vector3.right * camOffset + Vector3.up * bCollBounds.extents.y);
                    break;
            
                default: break;
            }

            // Round position to camera bounds
            if (transform.parent.TryGetComponent(out LevelZone lz))
            {
                (this as ISnapToBounds).RoundPositionToBounds(transform);
            }
        }

        public void DrawGizmosSelected() => OnDrawGizmosSelected();
        private void OnDrawGizmosSelected()
        {
            // Early exit for don't draw room
            if (!DrawLevelZone) return;
            
            // Early exit for zones without active children using their snapping
            if (transform.childCount == 0) return;
            ISnapToBounds[] snapChildren =
                transform.GetComponentsInChildren<ISnapToBounds>(false);
            if (snapChildren.Length < 2) return;
            
            
            Start();
        
            // Draw camera frame bounds from zone
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position + 
                    (DoesCameraOffset(scrollDirection) ? CamOffset : Vector3.zero),
                CameraBounds.size
            );
            Gizmos.color = Color.clear;
        }
    
#endif
        
    }
}