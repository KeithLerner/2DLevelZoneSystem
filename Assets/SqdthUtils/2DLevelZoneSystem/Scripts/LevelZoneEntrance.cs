using SqdthUtils._2DLevelZoneSystem;
using UnityEngine;

namespace SqdthUtils
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelZoneEntrance : MonoBehaviour, ISnapToBounds
    {
        public enum Transition { Full, Player, Camera }
        
        [field: Header("Bounds")]
        [field: SerializeField] public Vector2 Size { get; protected set; } = Vector2.one * 2;
        [field: SerializeField] public Transition TransitionStyle { get; protected set; } = Transition.Camera;
        [field: SerializeField] public bool TransitionToEdgeCenter { get; protected set; } = true; 
        
        // == Snapping ==
        [field: SerializeField]
        public bool Lock { get; set; }
        public Bounds SnappingBounds => owningZone.CameraBounds;
        public Vector2 SnappingOffset => -Size / 2f;
        
        // == References ==
        private GameObject playerGO;
        private Transform camTransform;
        private BoxCollider2D bColl;
        private LevelZone owningZone;


        private void Start()
        {
            if (bColl == null)
                bColl = GetComponent<BoxCollider2D>();
            if (!bColl.isTrigger)
            {
                bColl.isTrigger = true;
            }
            if (bColl.size != Size)
            {
                bColl.size = Size;
            }
        
            // Disable editing the box collider via the inspector
            // Forces changes to be made via the script
            if (bColl.hideFlags != HideFlags.HideInInspector)
                bColl.hideFlags =  HideFlags.HideInInspector;

            if (transform.parent == null || (transform.parent != null && transform.parent.GetComponent<LevelZone>() == null))
                Debug.LogWarning($"Level Zone Entrance attached to \"{gameObject.name}\" does not have a parent level zone. " +
                                 $"Null reference exceptions will occur.");
        
            if (owningZone == null)
                owningZone = transform.parent.GetComponent<LevelZone>();
        }

        public Vector3[] GetValidMovementAxes()
        {
            if (bColl == null || owningZone == null) Start();
        
            // Start new Vector3[]
            Vector3[] axes = new Vector3[2];
        
            // Get position, min, and max bounding position
            Vector2 pos = transform.position;
            Vector3 extents = bColl.bounds.extents;
            Vector2 min = owningZone.CameraBounds.min + extents;
            Vector2 max = owningZone.CameraBounds.max - extents;
            
            // Calculate the distances to each edge
            float distLeft = pos.x - min.x;
            float distRight = max.x - pos.x;
            float distBottom = pos.y - min.y;
            float distTop = max.y - pos.y;

            // Round the position to the nearest edge
            // If near to left or right edge
            if (Mathf.Abs(distLeft) < .001f ||
                Mathf.Abs(distRight) < .001f)
            {
                axes[0] = Vector3.up;
            }
        
            // If near to top or bottom edge
            if (Mathf.Abs(distTop) < .001f ||
                Mathf.Abs(distBottom) < .001f)
            {
                axes[1] = Vector3.right;
            }

            return axes;
        }

        protected virtual void TransitionCamera()
        {
            // Get end position
            Vector3 endPos;
            if (owningZone.scrollDirection ==
                LevelZone.ScrollDirection.NoScroll)
            {
                endPos = owningZone.CameraBounds.center;
            }
            else
            {
                endPos = TransitionToEdgeCenter || owningZone.ForceEdgeCenters ?
                    owningZone.GetNearestEdgeCenter(transform.position) :
                    owningZone.GetNearestEdgePoint(transform.position);
            }
            
            // Set position 
            endPos.z = camTransform.position.z;
            camTransform.position = endPos;
        }

        protected virtual void TransitionPlayer()
        {
            Vector3 endPos;
            if (owningZone.scrollDirection ==
                LevelZone.ScrollDirection.NoScroll)
            {
                endPos = owningZone.CameraBounds.center;
            }
            else
            {
                endPos = TransitionToEdgeCenter || owningZone.ForceEdgeCenters ?
                    owningZone.GetNearestEdgeCenter(transform.position) :
                    owningZone.GetNearestEdgePoint(transform.position);
            }
            
            endPos.z = owningZone.transform.position.z;
            playerGO.transform.position = endPos;
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {
            // Get and check for player
            if (!other.TryGetComponent(out LevelZonePlayer player)) return;
        
            // Get player's GameObject
            playerGO = other.gameObject;
        
            // Get player's camera's transform
            camTransform = player.PlayerCamera.transform;
        
            // Only apply transition if velocity is in direction of room (entering room)
            // Relies on player using a rigidbody, ignored if player doesn't use a rigidbody
            Rigidbody2D rb = playerGO.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (Vector2.Dot(rb.velocity,
                        owningZone.transform.position - rb.transform.position) <= 0)
                {
                    return;
                }
            }

            // Only apply transition if velocity is in direction of room (entering room)
            // Relies on player using a character controller, ignored if player doesn't use a character controller
            CharacterController cc = playerGO.GetComponent<CharacterController>();
            if (cc != null)
            {
                if (Vector2.Dot(cc.velocity,
                        owningZone.transform.position - cc.transform.position) <= 0)
                {
                    return;
                }
            }
        
            // Transition camera
            if (TransitionStyle == Transition.Camera ||
                TransitionStyle == Transition.Full)
                TransitionCamera();

            // Transition player
            if (TransitionStyle == Transition.Player ||
                TransitionStyle == Transition.Full)
                TransitionPlayer();
        }
    
#if UNITY_EDITOR

        protected void OnDrawGizmos()
        {
            Start();
        
            if (owningZone == null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(transform.position, bColl.size.magnitude);
            }
            else 
            {
                // Draw debug stuff
                if (owningZone.DrawLevelZone)
                {
                    Vector3 pos = transform.position;
                    Vector3 transitionPos = 
                        TransitionToEdgeCenter || owningZone.ForceEdgeCenters ? 
                        owningZone.GetNearestEdgeCenter(pos) :
                        owningZone.GetNearestEdgePoint(pos);
                    transitionPos.z = owningZone.transform.position.z;
                
                    // Draw zone entrance
                    Gizmos.color = owningZone.LevelZoneColor;
                    Gizmos.DrawCube(transform.position, bColl.size);
                
                    // Draw line to next camera point
                    float debugLineWidth = 
                        LevelZoneSettings.Instance.DebugLineWidth;
                    UnityEditor.Handles.color = owningZone.LevelZoneColor;
                    UnityEditor.Handles.DrawAAPolyLine(
                        debugLineWidth, pos, transitionPos);
                }
            
                // Round position to camera bounds
                if (!((ISnapToBounds)this).Lock) // Don't round position when snap lock is on
                {
                    ((ISnapToBounds)this).RoundPositionToBounds(transform);
                }
            }
        }
    
        protected void OnDrawGizmosSelected()
        {
            Start();

            // Early exit for don't draw level zone
            if (!owningZone.DrawLevelZone) return;
        
            // Set up drawing entrance
            if (owningZone != null)
            { 
                // Entrance color
                Gizmos.color = Color.grey;
            
                // Draw zone entrance
                Gizmos.DrawWireCube(transform.position, bColl.size);

                // Call parent's OnDrawGizmoSelected
                transform.parent.GetComponent<LevelZone>().DrawCameraBoundsGizmo();
            }
            else
            {
                // Entrance color
                Gizmos.color = Color.magenta;
            }
        }
    
#endif
        
    }
}
