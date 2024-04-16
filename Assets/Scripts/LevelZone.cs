using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelZone : MonoBehaviour
{
    public enum ScrollDirection { Horizontal, Vertical, FollowPlayer, NoScroll }
    [Header("Behavior")]
    [Tooltip("Which direction the room will scroll in.")]
    public ScrollDirection scrollDirection;
    [Tooltip("How far from zero the camera should align on the axis opposite scrolling direction.")]
    [SerializeField] private int camOffset;
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
    float ScreenAspect => (float)Screen.width / (float)Screen.height;
    float CameraHeight => Camera.main.orthographicSize * 2;
    public Vector2 CameraSize => new Vector2(CameraHeight * ScreenAspect, CameraHeight);
    public Bounds CameraBounds => new Bounds(transform.position, Size + CameraSize);


    private const float colorAlpha = .34f; 
    public bool DrawLevelZone => drawZone;
    [field: Header("Visualization")]
    [SerializeField] private bool drawZone = true;
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

    public BoxCollider2D bColl;
    private GameObject playerGO;
    private Transform camTransform;

    private void Start()
    {
        if (bColl == null)
            bColl = GetComponent<BoxCollider2D>();
        bColl.isTrigger = true;
        bColl.size = Size;
        
        // Disable editing the box collider via the inspector
        // Forces changes to be made via the script
        if (bColl.hideFlags != HideFlags.HideInInspector)
            bColl.hideFlags =  HideFlags.HideInInspector;
        
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
        Vector2 extents = (Vector2)bColl.bounds.extents;
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
        // Get min and max bounding position
        Vector2 pos = transform.position;
        Vector2 extents = (Vector2)bColl.bounds.extents;
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
        if (IsInsideLevelZone(targetPosition)) return Vector2.zero;
        
        // Get min and max bounding position
        Bounds bCollBounds = bColl.bounds;
        Vector2 pos = transform.position;
        Vector3 extents = bCollBounds.extents;

        // Clamp the specified point to the boundaries of the box
        float clampedX = Mathf.Clamp(targetPosition.x, pos.x - extents.x, pos.x + extents.x);
        float clampedY = Mathf.Clamp(targetPosition.y, pos.y - extents.y, pos.y + extents.y);

        // Return the clamped point as the nearest edge point
        return new Vector2(clampedX, clampedY);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        Vector3 cPos = camTransform.position; // camera position
        Vector2 laPos = transform.position + (Vector3)bColl.offset; // level zone position
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
                new Vector2(pPos.x + camOffset, pPos.y),
            
            // This level zone does not scroll
            _ => new Vector2(laPos.x, laPos.y),
        };
        targetCamPos.z = cPos.z;

        // Set camera to target position if not in zone bounds already
        bool isInside = IsInsideLevelZone(other.transform.position);
        Vector3 transitionCamPos = forceEdgeCenters ? 
            GetNearestEdgeCenter(laPos) :
            GetNearestEdgePoint(laPos);
        transitionCamPos.z = cPos.z;
        camTransform.position = Vector3.Lerp(
            cPos,
            isInside ? targetCamPos : transitionCamPos, 
            Time.fixedDeltaTime * (isInside ? 10 : 25)
        );
    }

    private void OnDrawGizmos()
    {
        if (!DrawLevelZone) return;

        Start();
        
        Gizmos.color = LevelZoneColor;
        Gizmos.DrawCube(transform.position, (Vector3)bColl.size);
    }

    public void DrawGizmosSelected() => OnDrawGizmosSelected();
    private void OnDrawGizmosSelected()
    {
        Start();
        
        // Draw camera frame bounds from zone
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, Size + CameraSize);
        Gizmos.color = Color.clear;
    }
}