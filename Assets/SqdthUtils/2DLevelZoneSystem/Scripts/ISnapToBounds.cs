using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    public interface ISnapToBounds
    {
        public Bounds SnappingBounds { get; }
        public Vector2 SnappingOffset { get; }

        public void RoundPositionToBounds(Transform target)
        {
            target.position = RoundLocationToBounds(target.position);
        }
        
        public Vector3 RoundLocationToBounds(Vector3 location)
        {
            // Get position, min, and max bounding position
            Vector2 min = (Vector2)SnappingBounds.min - SnappingOffset;
            Vector2 max = (Vector2)SnappingBounds.max + SnappingOffset;
            
            // Calculate the distances to each edge
            float distLeft = location.x - min.x;
            float distRight = max.x - location.x;
            float distBottom = location.y - min.y;
            float distTop = max.y - location.y;

            // Find the minimum distance
            float minDist = Mathf.Min(distLeft, distRight, distBottom, distTop);

            // Round the position to the nearest edge
            if (Mathf.Abs(minDist - distLeft) < .001f)
            {
                location = new Vector3(min.x, location.y);
            }
            else if (Mathf.Abs(minDist - distRight) < .001f)
            {
                location = new Vector3(max.x, location.y);
            }
            else if (Mathf.Abs(minDist - distBottom) < .001f)
            {
                location = new Vector3(location.x, min.y);
            }
            else if (Mathf.Abs(minDist - distTop) < .001f)
            {
                location = new Vector3(location.x, max.y);
            }
            
            location.z = SnappingBounds.center.z;
            return location;
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
