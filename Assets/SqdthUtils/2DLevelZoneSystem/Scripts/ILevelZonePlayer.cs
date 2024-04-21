using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    public interface ILevelZonePlayer
    {
        public static Vector2 PlayerSize = new Vector2(1, 2);
        public Camera PlayerCamera { get; protected set; }
    }
}
