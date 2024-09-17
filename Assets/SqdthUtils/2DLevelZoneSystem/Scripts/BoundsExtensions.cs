using System.Collections.Generic;
using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem
{
    public static class BoundsExtensions
    {
        /// <summary>
        /// Stores an edge between two points.
        /// </summary>
        private struct Edge
        {
            public readonly Vector3 StartPosition;
            public readonly Vector3 EndPosition;

            public Edge(Vector3 startPosition, Vector3 endPosition)
            {
                StartPosition = startPosition;
                EndPosition = endPosition;
            }
        }

        internal static List<List<Vector3>> GetPerimeters(this Bounds[] allBounds)
        {
            List<List<Vector3>> results = new List<List<Vector3>>();
            
            // Get perimeter points in hash sets
            List<HashSet<Vector3>> perimPoints = new List<HashSet<Vector3>>();
            

            return results;
        }
        
        /// <summary>
        /// Get points that make up corners on the perimeter of overlapping
        /// bounds.
        /// </summary>
        /// <param name="ignoreBounds"> A bounds to ignore, the root zone in a
        /// parental hierarchy usually. </param>
        /// <param name="boundsArray"> Overlapping bounds. </param>
        /// <returns></returns>
        internal static Vector3[] GetPerimeterPoints(this Bounds ignoreBounds, 
            Bounds[] boundsArray)
        {
            // Set up storage for all points (corners and intersections) of
            // bounds union shape
            HashSet<Vector3> pointSet = new HashSet<Vector3>();
            
            // Get all edges and add valid corners of bounds to the point set
            List<Edge> horizontalEdges = new List<Edge>();
            List<Edge> verticalEdges = new List<Edge>();
            foreach (Bounds bounds in boundsArray)
            {
                // Bottom edge
                horizontalEdges.Add(
                    new Edge(bounds.min, new Vector3(bounds.max.x, bounds.min.y))
                ); 
                // Top edge
                horizontalEdges.Add(
                    new Edge(new Vector3(bounds.min.x, bounds.max.y), bounds.max)
                ); 
                // Left edge
                verticalEdges.Add(
                    new Edge(bounds.min, new Vector3(bounds.min.x, bounds.max.y))
                ); 
                // Right edge
                verticalEdges.Add(
                    new Edge(new Vector3(bounds.max.x, bounds.min.y), bounds.max)
                ); 
                
                // Get corners
                bool[] validCorner = { true, true, true, true };
                Vector3[] corners = 
                {
                    bounds.min, 
                    new Vector3(bounds.min.x, bounds.max.y),
                    bounds.max, 
                    new Vector3(bounds.max.x, bounds.min.y)
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
                if (bounds != ignoreBounds &&
                    validCorner[0] &&
                    validCorner[1] &&
                    validCorner[2] &&
                    validCorner[3])
                {
                    // All corners invalid
                    // Gizmos.color = Color.magenta;
                    // Gizmos.DrawCube(bounds.center, bounds.size);
                    // Gizmos.DrawCube(CameraBounds.center, CameraBounds.size);
                    
                    UnityEditor.Handles.color = Color.yellow;
                    UnityEditor.Handles.Label(corners[0], "Not in camera bounds of parent zone", 
                        new GUIStyle
                        {
                            fontSize = 10,
                            alignment = TextAnchor.UpperRight
                        }
                    );
                    corners = new []
                    {
                        corners[0],
                        corners[1],
                        corners[2],
                        corners[3],
                        corners[0]
                    };
                    UnityEditor.Handles.DrawAAPolyLine(
                        LevelZoneSettings.Instance.DebugLineWidth, corners);
                }
                else for (int i = 0; i < corners.Length; i++)
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
                float hY = hEdge.StartPosition.y;
                
                // Check against each vertical edge
                foreach (var vEdge in verticalEdges)
                {
                    // Get X value of vertical edge
                    float vX = vEdge.StartPosition.x;
                    
                    // If vertical edge exists between horizontal edge points (exclusive)
                    bool containedX = hEdge.StartPosition.x < vX && vX < hEdge.EndPosition.x;
                    // If horizontal edge exists between vertical edge points (exclusive)
                    bool containedY = vEdge.StartPosition.y < hY && hY < vEdge.EndPosition.y;
                    
                    // If intersecting
                    if (containedX && containedY)
                    {
                        // Add nearest point on horizontal edge to point set
                        pointSet.Add(new Vector2(vX, hY));
                    }
                }
            }

            // Make a list from the point set
            List<Vector3> pointList = new List<Vector3>();
            foreach (Vector3 point in pointSet)
            {
                pointList.Add(point);
            }

            return GetShortestPath(pointList).ToArray();
        }

        private static List<Vector3> GetShortestPath(List<Vector3> points)
        {
            List<Vector3> path = new List<Vector3>();
            HashSet<Vector3> visited = new HashSet<Vector3>();

            // Start from the first point in the list
            Vector3 currentPoint = points[0];
            path.Add(currentPoint);
            visited.Add(currentPoint);

            // Repeat until all points are visited
            int iterationCount = 0;
            int maxIterCount = 3;
            while (visited.Count < points.Count &&
                   iterationCount < maxIterCount)
            {
                Vector3 closestPoint = Vector3.zero;
                float shortestDistance = Mathf.Infinity;

                // Find the closest unvisited point
                foreach (Vector3 point in points)
                {
                    bool axisAligned = (point - currentPoint).x != 0 ^
                                       (point - currentPoint).y != 0;
                    if (!visited.Contains(point) && axisAligned)
                    {
                        float distance = Vector3.Distance(currentPoint, point);
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
                return new List<Vector3>();
            }

            return path;
        }
    }
}