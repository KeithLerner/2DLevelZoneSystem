using System.Collections;
using SqdthUtils._2DLevelZoneSystem;
using UnityEngine;

namespace SqdthUtils
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelZoneEntrance : MonoBehaviour, ISnapToBounds
    {
        public enum Transition { Full, Player, Camera }
        
        [field: Header("Bounds")]
        [field: SerializeField] 
        public Vector2 Size { get; protected set; } = Vector2.one * 2;
        [field: SerializeField] 
        public Transition TransitionStyle { get; protected set; } = Transition.Camera;
        [field: SerializeField] 
        public bool TransitionToEdgeCenter { get; protected set; } = true; 
        
        // == Snapping ==
        [field: SerializeField]
        public bool Lock { get; set; }
        public Bounds SnappingBounds => OwningZone.CameraBounds;
        public Vector2 SnappingOffset => -Size / 2f;
        
        // == Object References ==
        private GameObject _playerGo;
        private Transform _camTransform;
        private BoxCollider2D _bColl;
        private BoxCollider2D BColl
        {
            get
            {
                if (_bColl == null)
                    _bColl = GetComponent<BoxCollider2D>();
                return _bColl;
            }
        }
        private LevelZone _owningZone;
        private LevelZone OwningZone
        {
            get
            {
                if (_owningZone == null)
                    _owningZone = transform.parent.GetComponent<LevelZone>();
                return _owningZone;
            }
        }


        private void Start()
        {
            if (transform.parent == null || 
                (transform.parent != null && 
                    transform.parent.GetComponent<LevelZone>() == null)
                ) Debug.LogWarning(
                $"Level Zone Entrance attached to \"{gameObject.name}\" " +
                "does not have a parent level zone. Null reference exceptions " +
                "will occur.");
        }

        public Vector3[] GetValidMovementAxes()
        {
            // Start new Vector3[]
            Vector3[] axes = new Vector3[2];
        
            // Get position, min, and max bounding position
            Vector2 pos = transform.position;
            Vector3 extents = BColl.bounds.extents;
            Vector2 min = OwningZone.CameraBounds.min + extents;
            Vector2 max = OwningZone.CameraBounds.max - extents;
            
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

        protected virtual IEnumerator TransitionCamera()
        {
            // Get end position
            Vector3 endPos;
            if (OwningZone.scrollDirection ==
                LevelZone.ScrollDirection.NoScroll)
            {
                endPos = OwningZone.CameraBounds.center;
            }
            else
            {
                endPos = TransitionToEdgeCenter || OwningZone.ForceEdgeCenters ?
                    OwningZone.GetNearestEdgeCenter(transform.position) :
                    OwningZone.GetNearestEdgePoint(transform.position);
            }
            
            // Set position 
            endPos.z = _camTransform.position.z;
            _camTransform.position = endPos;
            
            yield return null;
        }

        protected virtual IEnumerator TransitionPlayer()
        {
            Vector3 endPos;
            if (OwningZone.scrollDirection ==
                LevelZone.ScrollDirection.NoScroll)
            {
                endPos = OwningZone.CameraBounds.center;
            }
            else
            {
                endPos = TransitionToEdgeCenter || OwningZone.ForceEdgeCenters ?
                    OwningZone.GetNearestEdgeCenter(transform.position) :
                    OwningZone.GetNearestEdgePoint(transform.position);
            }
            
            endPos.z = OwningZone.transform.position.z;
            _playerGo.transform.position = endPos;

            yield return null;
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {
            // Get and check for player
            if (!other.TryGetComponent(out LevelZonePlayer player)) return;
        
            // Get player's GameObject
            _playerGo = player.gameObject;
        
            // Get player's camera's transform
            _camTransform = player.PlayerCamera.transform;
        
            // Only apply transition if velocity is in direction of room (entering room)
            // Relies on player using a rigidbody, ignored if player doesn't use a rigidbody
            Rigidbody2D rb = _playerGo.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (Vector3.Dot(rb.velocity,
                        OwningZone.transform.position - rb.transform.position) <= 0)
                {
                    return;
                }
            }

            // Only apply transition if velocity is in direction of room (entering room)
            // Relies on player using a character controller, ignored if player doesn't use a character controller
            CharacterController cc = _playerGo.GetComponent<CharacterController>();
            if (cc != null)
            {
                if (Vector3.Dot(cc.velocity,
                        OwningZone.transform.position - cc.transform.position) <= 0)
                {
                    return;
                }
            }
        
            // Transition camera
            if (TransitionStyle is Transition.Camera or Transition.Full)
                StartCoroutine(nameof(TransitionCamera));

            // Transition player
            if (TransitionStyle is Transition.Player or Transition.Full)
                StartCoroutine(nameof(TransitionPlayer));
        }
    
#if UNITY_EDITOR

        protected void OnDrawGizmos()
        {
            // Draw debug stuff
            if (OwningZone.DrawLevelZone)
            {
                Vector3 pos = transform.position;
                Vector3 transitionPos = 
                    TransitionToEdgeCenter || OwningZone.ForceEdgeCenters ? 
                        OwningZone.GetNearestEdgeCenter(pos) :
                        OwningZone.GetNearestEdgePoint(pos);
                transitionPos.z = OwningZone.transform.position.z;
            
                // Draw zone entrance
                Gizmos.color = OwningZone.LevelZoneColor;
                Gizmos.DrawCube(pos, BColl.size);
            
                // Draw line to next camera point
                float debugLineWidth = 
                    LevelZoneSettings.Instance.DebugLineWidth;
                UnityEditor.Handles.color = OwningZone.LevelZoneColor;
                UnityEditor.Handles.DrawAAPolyLine(
                    debugLineWidth, pos, transitionPos);
            }
        
            // Round position to camera bounds
            if (!((ISnapToBounds)this).Lock) // Don't round position when snap lock is on
            {
                ((ISnapToBounds)this).RoundPositionToBounds(transform);
            }
        }
    
        protected void OnDrawGizmosSelected()
        {
            // Early exit for don't draw level zone
            if (!OwningZone.DrawLevelZone) return;
        
            // Entrance color
            Gizmos.color = Color.grey;
        
            // Draw zone entrance
            Gizmos.DrawWireCube(transform.position, BColl.size);

            // Call parent's OnDrawGizmoSelected
            transform.parent.GetComponent<LevelZone>().DrawCameraBoundsGizmo();
        }
    
#endif
        
    }
}
