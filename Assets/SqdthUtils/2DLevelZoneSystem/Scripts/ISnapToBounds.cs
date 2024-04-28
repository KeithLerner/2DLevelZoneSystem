using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    public interface ISnapToBounds
    {
        public Bounds SnappingBounds { get; }
        public Vector2 SnappingOffset { get; }
        
        public void RoundPositionToBounds(Transform target)
        {
            // Get position, min, and max bounding position
            Vector2 pos = target.position;
            Vector2 min = (Vector2)SnappingBounds.min - SnappingOffset;
            Vector2 max = (Vector2)SnappingBounds.max + SnappingOffset;
            
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
            else if (Mathf.Abs(minDist - distTop) < .001f)
            {
                pos = new Vector2(pos.x, max.y);
            }

            Vector3 newPos = pos;
            newPos.z = SnappingBounds.center.z;
            target.position = newPos;
        }
        
        public Vector3[] GetValidMovementAxes(Bounds toSnapBounds)
        {
            // Start new Vector3[] for return values
            Vector3[] axes = new Vector3[2];
        
            // Get position, min, and max bounding position
            Vector2 pos = toSnapBounds.center;
            Vector2 extents = toSnapBounds.extents;
            Vector2 min = (Vector2)SnappingBounds.min - extents;
            Vector2 max = (Vector2)SnappingBounds.max + extents;
            
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
    }
}
