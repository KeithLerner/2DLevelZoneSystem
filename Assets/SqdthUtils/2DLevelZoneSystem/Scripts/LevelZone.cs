using System.Collections.Generic;
using SqdthUtils._2DLevelZoneSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SqdthUtils
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelZone : MonoBehaviour, ISnapToBounds
    {
        public enum ScrollDirection { Horizontal, Vertical, FollowPlayer, NoScroll }
        // == Behavior ==
        [Header("Behavior")]
        [Tooltip("Which direction the room will scroll in.")]
        public ScrollDirection scrollDirection;
        [Tooltip("Offset given to the camera.")]
        [SerializeField] private Vector3 cameraOffset;

        [Tooltip("Forces cameras to lock to edge centers when leaving the level zone.")] 
        [SerializeField] private bool forceEdgeCenters;
        public bool ForceEdgeCenters => forceEdgeCenters;
    
        // == Sizing ==
        public Vector3 Size { get => size; set => size = value; }
        [SerializeField] private Vector2 size = new Vector2(32, 32);
        private float ScreenAspect => 
            (float)LevelZoneSettings.Instance.TargetCameraAspectRatio.x / 
                   LevelZoneSettings.Instance.TargetCameraAspectRatio.y; 
        float CameraHeight => (Camera.main != null ? 
            Camera.main.orthographicSize : 
            LevelZoneSettings.Instance.TargetOrthographicCameraSize) * 2;
        private Vector3 CameraSize => new Vector3(CameraHeight * ScreenAspect, CameraHeight);
        internal Vector3 CameraOffset
        {
            get
            {
                return scrollDirection switch
                {
                    ScrollDirection.Horizontal   => Vector3.up    * cameraOffset.y,
                    ScrollDirection.Vertical     => Vector3.right * cameraOffset.x,
                    _                            =>                 cameraOffset
                };
            }
        }
        public Bounds CameraBounds => new Bounds(
            (BColl != null ? BColl.bounds.center : transform.position) + 
            CameraOffset, 
            scrollDirection switch 
            {
                ScrollDirection.FollowPlayer => Size + CameraSize,
                _ => CameraSize,
            }
        );
        
        // == Snapping ==
        [field: SerializeField] 
        public bool Lock { get; set; }
        public Bounds SnappingBounds
        {
            get
            {
                if (transform.parent.TryGetComponent(out LevelZone lz))
                {
                    return lz.BColl.bounds;
                }
                return BColl.bounds;
            }
        }
        public Vector2 SnappingOffset => Size / 2f;

        // == Object References ==
        private BoxCollider2D _bColl;
        public BoxCollider2D BColl
        {
            get
            {
                if (_bColl == null)
                    _bColl = GetComponent<BoxCollider2D>();
                return _bColl;
            }
        }
        private GameObject _playerGo;
        private Transform _camTransform;

        private void Start()
        {
            BColl.isTrigger = true;
            BColl.size = Size;
        
            // Disable editing the box collider via the inspector
            // Forces changes to be made via the script
            if (BColl.hideFlags != HideFlags.HideInInspector)
                BColl.hideFlags =  HideFlags.HideInInspector;
        
            if (_playerGo == null)
                _playerGo = GameObject.FindWithTag("Player");
        
            if (_camTransform == null && Camera.main != null)
                _camTransform = Camera.main.transform;
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
            _camTransform = player.PlayerCamera.transform;
        
            // Get target camera position
            Vector3 cPos = _camTransform.position; // camera position
            Vector3 laPos = transform.position + (Vector3)BColl.offset; // level zone position
            Vector3 pPos = _playerGo.transform.position; // Player position
            Vector3 targetCamPos = scrollDirection switch // target position for camera based on level zone scroll direction
            {
                // This level zone scrolls horizontally
                ScrollDirection.Horizontal => 
                    new Vector3(pPos.x, laPos.y),
            
                // This level zone scrolls vertically
                ScrollDirection.Vertical => 
                    new Vector3(laPos.x, pPos.y),
            
                ScrollDirection.FollowPlayer =>
                    new Vector3(pPos.x, pPos.y),
            
                // This level zone does not scroll
                _ => new Vector3(laPos.x, laPos.y)
            };
            
            // Add offset to target camera position
            targetCamPos += CameraOffset;
            
            // Set target camera position if player is not in zone bounds
            bool isInside = pPos.IsInsideLevelZone(this);
            if (!isInside)
            {
                // Update player's current zone to null
                // Allows zones to neighbor each other without making use of
                // entrances
                player.CurrentZone = null;

                // No scroll zones should keep their previously set target position
                if (scrollDirection != ScrollDirection.NoScroll)
                {
                    // Determine and store if camera offset is diagonal
                    bool diagonalOffset =
                        CameraOffset.x != 0 && CameraOffset.y != 0;

                    // Update target camera position depending on if this zone
                    // forces edge center transitions
                    // Diagonal offsets can not use camera position as reference
                    // because the camera moves away by adding offset to itself 
                    // from its nearest edge center/point
                    targetCamPos = forceEdgeCenters
                        ? this.GetNearestEdgeCenter(
                            diagonalOffset ? pPos : cPos)
                        : this.GetNearestEdgePoint(
                            diagonalOffset ? pPos : cPos);

                    // Add diagonal offsets back to target position
                    // linear offsets have better camera positioning and don't 
                    // require additional offset addition, in fact is causes camera
                    // jitters between neighboring level zones
                    if (diagonalOffset)
                    {
                        targetCamPos += CameraOffset;
                    }
                }
            }
            
            // Fix target camera position's Z value
            targetCamPos.z = cPos.z;
            
            // Set player's camera position
            _camTransform.position = Vector3.Lerp(
                cPos, targetCamPos, 
                Time.fixedDeltaTime * player.CameraSpeed
            );
        }

#if UNITY_EDITOR
        
        // == Visual debug details ==
        private const float ColorAlpha = .4f; 
        public bool DrawLevelZone => drawZone;
        [field: Header("Visualization")]
        [SerializeField] private bool drawZone = true;
        [SerializeField] private Color overrideLevelZoneColor = new Color(1,1,1,ColorAlpha);
        public Color LevelZoneColor => 
            overrideLevelZoneColor != new Color(1,1,1,ColorAlpha) ? 
                overrideLevelZoneColor :
                scrollDirection switch
                {
                    ScrollDirection.Horizontal => new Color(.2f, .2f, 1, ColorAlpha),
                    ScrollDirection.Vertical => new Color(1, .2f, .2f, ColorAlpha),
                    ScrollDirection.FollowPlayer => new Color(.2f, 1, .2f, ColorAlpha),
                    _ => new Color(.2f, .2f, .2f, ColorAlpha)
                };
        
        internal void RandomizeColor()
        {
            overrideLevelZoneColor = new Color(Random.value, Random.value, Random.value, ColorAlpha);
        }

        private void OnDrawGizmos()
        {
            // Early exit for don't draw room
            if (!DrawLevelZone || Application.isPlaying) return;

            Start();
            
            // Round position to camera bounds
            if (!Lock && transform.parent != null && // Don't round position when snap lock is on
                transform.parent.GetComponent<LevelZone>() != null)
            {
                (this as ISnapToBounds).RoundPositionToBounds(transform);
            }
            
            // Draw level zone area using specified color
            Gizmos.color = LevelZoneColor;
            Gizmos.DrawCube(BColl.bounds.center, BColl.size);
            
            // Early exit for zones not at the top of their parental hierarchy
            // The remaining debug visuals are only for the parent most level zone 
            if (transform.parent != null &&
                transform.parent.GetComponent<LevelZone>() != null) return;
            
            // Get list of perimeter points
            Vector3[] perimeterPoints = 
                CameraBounds.GetPerimeterPoints(GetAllFamilialCameraBounds());

            // Create list of perimeter edges
            List<Vector3> perimeterEdges = new List<Vector3>();
            for (var i = 0; i < perimeterPoints.Length; i++)
            {
                // Get points to make line from
                Vector3 a = perimeterPoints[i];
                int bi = i + 1;
                if (bi >= perimeterPoints.Length)
                    bi = 0;
                Vector3 b = perimeterPoints[bi];
                
                // Add points in point pair format
                perimeterEdges.Add(a);
                perimeterEdges.Add(b);
                
                // Fun extra debug line for those that like seeing the
                // numbered vertices of the full camera bounds perimeter
                //UnityEditor.Handles.Label(a, i.ToString());
            }
            
            // Draw perimeter edges
            if (perimeterPoints.Length == 0) return;
            UnityEditor.Handles.color = LevelZoneColor;
            UnityEditor.Handles.Label(perimeterPoints[0], gameObject.name, 
                new GUIStyle
                {
                    fontSize = 10,
                    alignment = TextAnchor.UpperRight
                }
            );
            UnityEditor.Handles.DrawAAPolyLine(
                LevelZoneSettings.Instance.DebugLineWidth, 
                perimeterEdges.ToArray());
            
            // Gets all camera bounds in this chain of children
            Bounds[] GetAllFamilialCameraBounds()
            {
                // Initialize return list
                List<Bounds> results = new List<Bounds>{ CameraBounds };
            
                // Get all child bounds
                LevelZone[] family = GetComponentsInChildren<LevelZone>();
                foreach (var child in family)
                {
                    // Get level zone camera bounds of i
                    results.Add(child.CameraBounds);
                }

                return results.ToArray();
            }
        }
    
#endif
        
    }
}