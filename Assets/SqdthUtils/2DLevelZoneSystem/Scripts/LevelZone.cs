using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelZone : MonoBehaviour, ISnapToBounds
    {
        public static Vector2Int TargetAspectRatio { get; set; }
        public static float TargetOrthographicCameraSize { get; set; }
        public static float DebugLineWidth { get; set; }
    
        public enum ScrollDirection { Horizontal, Vertical, FollowPlayer, NoScroll }
        [Header("Behavior")]
        [Tooltip("Which direction the room will scroll in.")]
        public ScrollDirection scrollDirection;
        [Tooltip("Offset given to the camera.")]
        [SerializeField] private Vector2 cameraOffset;

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
        float CameraHeight => (Camera.main != null ? 
            Camera.main.orthographicSize : 
            TargetOrthographicCameraSize) * 2;
        public Vector2 CameraSize => new Vector2(CameraHeight * ScreenAspect, CameraHeight);
        public Vector2 CameraOffset
        {
            get
            {
                return scrollDirection switch
                {
                    ScrollDirection.Horizontal   => Vector2.up    * cameraOffset.y,
                    ScrollDirection.Vertical     => Vector2.right * cameraOffset.x,
                    _                            =>                 cameraOffset
                };
            }
        }
        public Bounds CameraBounds => new Bounds(BColl.bounds.center + (Vector3)CameraOffset, 
            scrollDirection switch 
            {
                ScrollDirection.FollowPlayer => Size + CameraSize,
                _ => CameraSize,
            }
        );
        
        // == Snapping ==
        public Bounds SnappingBounds
        {
            get
            {
                if (transform.parent.TryGetComponent(out LevelZone lz))
                {
                    if (lz.BColl == null)
                    {
                        lz.BColl = lz.GetComponent<BoxCollider2D>();
                    }
                    return lz.BColl.bounds;
                }
                return BColl.bounds;
            }
        }
        public Vector2 SnappingOffset => Size / 2f;

        // == Debug ==
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
        public void RandomizeColor()
        {
            overrideLevelZoneColor = new Color(Random.value, Random.value, Random.value, ColorAlpha);
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
                    new Vector2(pPos.x, laPos.y),
            
                // This level zone scrolls vertically
                ScrollDirection.Vertical => 
                    new Vector2(laPos.x, pPos.y),
            
                ScrollDirection.FollowPlayer =>
                    new Vector2(pPos.x, pPos.y),
            
                // This level zone does not scroll
                _ => new Vector2(laPos.x, laPos.y)
            };
            
            // Add offset to target camera position
            targetCamPos += (Vector3)CameraOffset;
            
            // Set target camera position if player is not in zone bounds
            bool isInside = IsInsideLevelZone(pPos);
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
                    // diagonal offsets can not use camera position as reference
                    // because the camera lerps away by adding offset to iself 
                    // from its nearest edge center/point
                    targetCamPos = forceEdgeCenters
                        ? GetNearestEdgeCenter(diagonalOffset ? pPos : cPos)
                        : GetNearestEdgePoint(diagonalOffset ? pPos : cPos);

                    // Add diagonal offsets back to target position
                    // linear offsets have better camera positioning and don't 
                    // require additional offset addition, in fact is causes camera
                    // jitters between neighboring level zones
                    if (diagonalOffset)
                    {
                        targetCamPos += (Vector3)CameraOffset;
                    }
                }
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

        #region Perimeter Outline
        
        private Bounds[] GetAllFamilialCameraBounds()
        {
            // Initialize return list
            List<Bounds> results = new List<Bounds>{ CameraBounds };
            
            // Get all child bounds
            LevelZone[] family = GetComponentsInChildren<LevelZone>();
            for (int i = 0; i < family.Length; i++)
            {
                // Get level zone camera bounds of i
                results.Add(family[i].CameraBounds);
            }

            return results.ToArray();
        }

        private Vector2[] GetAllFamilialCameraBoundsCorners()
        {
            // Initialize return list
            List<Vector2> results = new List<Vector2>();

            foreach (Bounds bounds in GetAllFamilialCameraBounds())
            {
                results.Add(bounds.min); // bottom left
                results.Add(new Vector2(bounds.min.x, bounds.max.y)); // top left
                results.Add(bounds.max); // top right
                results.Add(new Vector2(bounds.max.x, bounds.min.y)); // bottom right
            }

            return results.ToArray();
        }
        
        private struct Edge
        {
            public Vector2 Item1;
            public Vector2 Item2;

            public Edge(Vector2 start, Vector2 end)
            {
                Item1 = start;
                Item2 = end;
            }
        }

        private Vector2[] GetPerimeterPoints2(Bounds[] boundsArray)
        {
            // Set up storage for all points (corners and intersections) of
            // bounds union shape
            HashSet<Vector2> pointSet = new HashSet<Vector2>();
            
            // Get all edges and add valid corners of bounds to the point set
            List<Edge> horizontalEdges = new List<Edge>();
            List<Edge> verticalEdges = new List<Edge>();
            foreach (Bounds bounds in boundsArray)
            {
                // Bottom edge
                horizontalEdges.Add(
                    new Edge(bounds.min, new Vector2(bounds.max.x, bounds.min.y))
                ); 
                // Top edge
                horizontalEdges.Add(
                    new Edge(new Vector2(bounds.min.x, bounds.max.y), bounds.max)
                ); 
                // Left edge
                verticalEdges.Add(
                    new Edge(bounds.min, new Vector2(bounds.min.x, bounds.max.y))
                ); 
                // Right edge
                verticalEdges.Add(
                    new Edge(new Vector2(bounds.max.x, bounds.min.y), bounds.max)
                ); 
                
                // Get corners
                bool[] validCorner = new bool[] { true, true, true, true };
                Vector2[] corners = new Vector2[]
                {
                    bounds.min, 
                    new Vector2(bounds.min.x, bounds.max.y),
                    bounds.max, 
                    new Vector2(bounds.max.x, bounds.min.y)
                };
                
                // Check this bound against all other bounds
                foreach (Bounds boundsToCheckAgainst in boundsArray)
                {
                    // Determine if a corner is valid
                    // A corner is valid if it is not contained in other bounds (exclusive)
                    for (var i = 0; i < corners.Length; i++)
                    {
                        var corner = corners[i];
                        if (corner.x < boundsToCheckAgainst.max.x &&
                            corner.x > boundsToCheckAgainst.min.x &&
                            Mathf.Abs(corner.x - boundsToCheckAgainst.max.x) >
                            .0001f &&
                            corner.y < boundsToCheckAgainst.max.y &&
                            corner.y > boundsToCheckAgainst.min.y &&
                            Mathf.Abs(corner.y - boundsToCheckAgainst.max.y) >
                            .0001f)
                        {
                            validCorner[i] = false;
                        }
                    }
                }
                
                // Add valid corners to the point set
                if (bounds != CameraBounds &&
                    validCorner[0] &&
                    validCorner[1] &&
                    validCorner[2] &&
                    validCorner[3])
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawCube(bounds.center, bounds.size);
                    Gizmos.DrawCube(CameraBounds.center, CameraBounds.size);
                }
                for (int i = 0; i < corners.Length; i++)
                {
                    if (validCorner[i])
                    {
                        pointSet.Add(corners[i]);
                    }
                }
            }
            
            // Determine and Add intersecting points to point set
            foreach (var t in horizontalEdges)
            {
                // Get i horizontal edge
                Edge hEdge = t;
                
                // Get Y value of horizontal edge
                float hY = hEdge.Item1.y;
                
                // Check against each vertical edge
                foreach (var vEdge in verticalEdges)
                {
                    // Get X value of vertical edge
                    float vX = vEdge.Item1.x;
                    
                    // If vertical edge exists between horizontal edge points (exclusive)
                    bool containedX = hEdge.Item1.x < vX && vX < hEdge.Item2.x;
                    // If horizontal edge exists between vertical edge points (exclusive)
                    bool containedY = vEdge.Item1.y < hY && hY < vEdge.Item2.y;
                    
                    // If intersecting
                    if (containedX && containedY)
                    {
                        // Add nearest point on horizontal edge to point set
                        pointSet.Add(new Vector2(vX, hY));
                    }
                }
            }

            // Make a list from the point set
            List<Vector2> pointList = new List<Vector2>();
            foreach (Vector2 point in pointSet)
            {
                pointList.Add(point);
            }

            return MinimizeDistancePath(pointList).ToArray();
        }

        private static List<Vector2> MinimizeDistancePath(List<Vector2> points)
        {
            List<Vector2> path = new List<Vector2>();
            HashSet<Vector2> visited = new HashSet<Vector2>();

            // Start from the first point in the list
            Vector2 currentPoint = points[0];
            path.Add(currentPoint);
            visited.Add(currentPoint);

            // Repeat until all points are visited
            int iterationCount = 0;
            int maxIterCount = 3;
            while (visited.Count < points.Count &&
                   iterationCount < maxIterCount)
            {
                Vector2 closestPoint = Vector2.zero;
                float shortestDistance = Mathf.Infinity;

                // Find the closest unvisited point
                foreach (Vector2 point in points)
                {
                    bool axisAligned = (point - currentPoint).x != 0 ^
                                       (point - currentPoint).y != 0;
                    if (!visited.Contains(point) && axisAligned)
                    {
                        float distance = Vector2.Distance(currentPoint, point);
                        if (distance < shortestDistance)
                        {
                            closestPoint = point;
                            shortestDistance = distance;
                            iterationCount = 0;
                        }
                    }
                }

                // Move to the closest point
                currentPoint = closestPoint;
                path.Add(currentPoint);
                visited.Add(currentPoint);
                
                // Count iteration
                iterationCount++;
            }

            if (iterationCount >= maxIterCount)
            {
                return new List<Vector2>();
            }

            return path;
        }

        #endregion
        

        private void OnGUI()
        {
            Start();
            
            // Round position to camera bounds
            if (transform.parent.GetComponent<LevelZone>() != null)
            {
                (this as ISnapToBounds).RoundPositionToBounds(transform);
            }
        }

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
                    UnityEditor.Handles.DrawAAPolyLine(DebugLineWidth, 
                        bCollBounds.center + (Vector3)CameraOffset - Vector3.right * bCollBounds.extents.x, 
                        bCollBounds.center + (Vector3)CameraOffset + Vector3.right * bCollBounds.extents.x);
                    break;
            
                case ScrollDirection.Vertical:
                    UnityEditor.Handles.DrawAAPolyLine(DebugLineWidth, 
                        bCollBounds.center + (Vector3)CameraOffset - Vector3.up * bCollBounds.extents.y, 
                        bCollBounds.center + (Vector3)CameraOffset + Vector3.up * bCollBounds.extents.y);
                    break;
            
                default: break;
            }
            
            // Early exit for zones that aren't at their parental hierarchy
            // The remaining debug visuals are only for the parent most level zone 
            if (transform.parent.GetComponent<LevelZone>() != null) return;
            
            // Get list of perimeter points
            Vector2[] perimeterPoints = 
                GetPerimeterPoints2(GetAllFamilialCameraBounds());

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
                // numbered vertices of the full camera bounds
                UnityEditor.Handles.Label(a, i.ToString());
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
            UnityEditor.Handles.DrawAAPolyLine(DebugLineWidth, 
                perimeterEdges.ToArray());
        }

        /// <summary>
        /// <b> FOR LEVEL ZONE ENTRANCE USE ONLY. </b>
        /// </summary>
        public void DrawCameraBoundsGizmo() => OnDrawGizmosSelected();
        
        private void OnDrawGizmosSelected()
        {
            // Early exit for don't draw room
            if (!DrawLevelZone) return;

            Start();

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(CameraBounds.center, CameraBounds.size);
        }
    
#endif
        
    }
}