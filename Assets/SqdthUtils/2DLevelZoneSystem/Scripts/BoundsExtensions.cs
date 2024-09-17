using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SqdthUtils
{
    /// <summary>
    /// Stores an edge between two points.
    /// </summary>
    internal struct Edge
    {
        public readonly Vector3 StartPosition;
        public readonly Vector3 EndPosition;

        public Edge(Vector3 startPosition, Vector3 endPosition)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        /*[Obsolete]
        public bool IntersectsXZ(Edge other)
        {
            float x1 = StartPosition.x; 
            float y1 = StartPosition.y;
            float x2 = EndPosition.x; 
            float y2 = EndPosition.y;
            
            float x3 = other.StartPosition.x; 
            float y3 = other.StartPosition.y;
            float x4 = other.EndPosition.x; 
            float y4 = other.EndPosition.y;
            
            float px =
                ((x1 * y2 - y1 * x2) * (y3 - y4) - (x1 - x2) * (x3 * y4 - y3 * x4)) /
                ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
            float py = 
                ((x1 * y2 - y1 * x2) * (x3 - x4) - (y1 - y2) * (x3 * y4 - y3 * x4)) /
                ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));

            return px + py != 0;
        }*/
    }
    
    public static class BoundsExtensions
    {
        /// <summary>
        /// Gets bounds corners.
        /// </summary>
        /// <param name="bounds"> Bounds to get corners of. </param>
        /// <returns> Corners in order:
        /// xyz, Xyz, XYz, xYz, xyZ, XyZ, XYZ, xYZ. </returns>
        public static Vector3[] GetCorners(this Bounds bounds)
        {
            float minX = bounds.min.x; float maxX = bounds.max.x;
            float minY = bounds.min.y; float maxY = bounds.max.y;
            float minZ = bounds.min.z; float maxZ = bounds.max.z;
            return new[]
            {
                bounds.min,
                new Vector3(maxX, minY, minZ),
                new Vector3(maxX, maxY, minZ),
                new Vector3(minX, maxY, minZ),
                
                new Vector3(minX, minY, maxZ),
                new Vector3(maxX, minY, maxZ),
                bounds.max,
                new Vector3(minX, maxY, maxZ)
            };
        }
        
        
    }
}
