using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelZoneEntrance : MonoBehaviour
{
    [field: Header("Bounds")]
    [field: SerializeField] public Vector2 Size { get; private set; } = Vector2.one;
    [field: SerializeField] public bool TransitionToEdgeCenter { get; private set; } = true; 
    
    private GameObject playerGO;
    private Transform camTransform;
    private BoxCollider2D bColl;
    private LevelZone owningZone;
    

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

        if (owningZone == null)
            owningZone = transform.parent.GetComponent<LevelZone>();
    }

    public Vector3[] GetValidMovementAxes()
    {
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

    public void RoundPositionToOwningCameraBounds()
    {
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

        // Find the minimum distance
        float minDist = Mathf.Min(distLeft, distRight, distBottom, distTop);

        // Round the position to the nearest edge
        if (Mathf.Abs(minDist - distLeft) < .001f)
        {
            pos = new Vector2(min.x, pos.y);
        }
        else if (Mathf.Abs(minDist - distRight) < .001f)
        {
            pos = new Vector2(max.x, pos.y);
        }
        else if (Mathf.Abs(minDist - distBottom) < .001f)
        {
            pos = new Vector2(pos.x, min.y);
        }
        else // minDist == distTop
        {
            pos = new Vector2(pos.x, max.y);
        }

        Vector3 newPos = pos;
        newPos.z = owningZone.transform.position.z;
        transform.position = newPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        // Only apply if velocity is in direction of room (entering room)
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

        // Only apply if velocity is in direction of room (entering room)
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
        Vector3 edgePoint = 
            owningZone.GetNearestEdgeCenter(transform.position);
        edgePoint.z = camTransform.position.z;
        camTransform.position = edgePoint;
        
        // Transition player (Doesn't work yet)
        //StartCoroutine(nameof(TransitionPlayer));
    }

    /*
    private IEnumerator TransitionPlayer()
    {
        // HARD SET VELOCITY DURING TRANSITION
        // NOTHING ELSE SHOULD BE HAPPENING DURING THIS TIME
        Rigidbody2D rb = playerGO.GetComponent<Rigidbody2D>();
        Vector2 vel = rb.velocity;
        rb.isKinematic = true;
        rb.velocity = vel;

        Debug.Log("Starting player transition");
        
        // Wait for end of transition
        //yield return new WaitForSeconds(transitionTime);
        
        Debug.Log("Ending player transition");
        
        // Restore rigidbody mode and velocity
        rb.isKinematic = false;
        rb.velocity = vel;
        
        yield return null;
    } 
    */
    
#if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        Start();
        
        if (owningZone == null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position, bColl.size.sqrMagnitude);
        }
        else 
        {
            if (owningZone.DrawLevelZone)
            {
                Vector3 pos = transform.position;
                Vector3 transitionPos = TransitionToEdgeCenter || owningZone.ForceEdgeCenters ? 
                    owningZone.GetNearestEdgeCenter(pos) :
                    owningZone.GetNearestEdgePoint(pos);
                transitionPos.z = owningZone.transform.position.z;
                
                // Draw zone entrance
                Gizmos.color = owningZone.LevelZoneColor;
                Gizmos.DrawCube(transform.position, bColl.size);
                
                // Draw line to next camera point
                UnityEditor.Handles.color = owningZone.LevelZoneColor;
                UnityEditor.Handles.DrawAAPolyLine(owningZone.LineWidth, pos, transitionPos);
            }
            
            // Round position to camera bounds
            RoundPositionToOwningCameraBounds();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Start();
        
        // Set up drawing entrance
        if (owningZone != null)
        { 
            // Entrance color
            Gizmos.color = Color.white;
            
            // Draw zone entrance
            Gizmos.DrawWireCube(transform.position, bColl.size);

            // Call parent's OnDrawGizmoSelected
            transform.parent.GetComponent<LevelZone>().DrawGizmosSelected();
        }
        else
        {
            // Entrance color
            Gizmos.color = Color.magenta;
        }
    }
    
#endif
    
}
