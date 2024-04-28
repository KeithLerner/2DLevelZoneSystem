using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    public interface ISnapToBoundsOfParent
    {
        public Bounds ParentBounds { get; }
        public Vector2 Offset { get; }
        
        public void RoundPositionBounds(Transform target)
        {
            // Get position, min, and max bounding position
            Vector2 pos = target.position;
            Vector2 min = (Vector2)ParentBounds.min - Offset;
            Vector2 max = (Vector2)ParentBounds.max + Offset;
            
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
            newPos.z = target.parent.position.z;
            target.position = newPos;
        }
    }
}
