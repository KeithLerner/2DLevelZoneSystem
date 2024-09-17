using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem
{
    public static class LevelZoneExtensions
    {
        /// <summary>
        /// Get center points on edges of the level zone
        /// </summary>
        /// <returns> Array of center points on the edges of the level zone.</returns>
        internal static Vector3[] GetEdgeCenters(this LevelZone lz)
        {
            // Get level zone data
            Bounds bounds = lz.BColl.bounds;
            LevelZone.ScrollDirection scrollDirection = lz.scrollDirection;
            
            // Get min and max bounding position
            Vector3 pos = bounds.center;
            Vector3 extents = bounds.extents;
            Vector3 minBounds = pos - extents;
            Vector3 maxBounds = pos + extents;
        
            return scrollDirection switch
            {
                LevelZone.ScrollDirection.Horizontal => new []
                {
                    new Vector3(minBounds.x, pos.y),
                    new Vector3(maxBounds.x, pos.y),
                },
                LevelZone.ScrollDirection.Vertical => new []
                {
                    new Vector3(pos.x, minBounds.y),
                    new Vector3(pos.x, maxBounds.y)
                },
                _ => new []
                {
                    new Vector3(minBounds.x, pos.y),
                    new Vector3(maxBounds.x, pos.y),
                    new Vector3(pos.x, minBounds.y),
                    new Vector3(pos.x, maxBounds.y)
                }
            };
        }

        /// <summary>
        /// Get the nearest edge center to a target location. Uses GetEdgeCenters.
        /// </summary>
        /// <param name="lz"> The bounds to get nearest edge center of. </param>
        /// <param name="targetPosition"> The target position to find the
        /// nearest edge center to. </param>
        /// <returns> A Vector2 of the nearest edge center to the targetPosition. </returns>
        internal static Vector3 GetNearestEdgeCenter(this LevelZone lz, 
            Vector3 targetPosition)
        {
            // Get details from owning level zone
            Vector3 centerPos = lz.BColl.bounds.center;

            // Get center points on edges of the bounding box
            Vector3[] edgePoints = lz.GetEdgeCenters();

            // Get nearest point
            Vector3 nearestPoint = centerPos;
            float nearestDistance = float.MaxValue;
            foreach (Vector3 edgePoint in edgePoints)
            {
                float distance = Vector3.Distance(
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
        
        public static Vector3 GetNearestEdgePoint(this LevelZone lz, 
            Vector3 targetPosition)
        {
            // Get level zone data
            Bounds bounds = lz.BColl.bounds;
            
            // Get min and max bounding position
            Vector3 pos = bounds.center;
            Vector3 extents = bounds.extents;
            
            // Clamp the specified point to the boundaries of the box
            float clampedX = Mathf.Clamp(targetPosition.x, 
                pos.x - extents.x, pos.x + extents.x);
            float clampedY = Mathf.Clamp(targetPosition.y, 
                pos.y - extents.y, pos.y + extents.y);
            float clampedZ = Mathf.Clamp(targetPosition.z, 
                pos.z - extents.z, pos.z + extents.z);
        
            // Return the clamped point as the nearest edge point
            return new Vector3(clampedX, clampedY, clampedZ);
        }
    
        /// <summary>
        /// Check if a position is inside of the level zone.
        /// </summary>
        /// <param name="targetPosition"> The position to check. </param>
        /// <returns></returns>
        public static bool IsInsideLevelZone(this Vector3 targetPosition, LevelZone lz)
        {
            // Get min and max bounding position
            Vector3 pos = lz.transform.position;
            Vector3 extents = lz.BColl.bounds.extents;
            Vector3 minBounds = pos - extents;
            Vector3 maxBounds = pos + extents;

            return targetPosition.x >= minBounds.x && targetPosition.x <= maxBounds.x &&
                   targetPosition.y >= minBounds.y && targetPosition.y <= maxBounds.y;
        }
    }
}