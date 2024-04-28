using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    public class LevelZonePlayer : MonoBehaviour
    {
        public static Vector2 Size = new Vector2(1, 2);
        
        [field: SerializeField]
        public Camera PlayerCamera { get; protected set; }
        public LevelZone CurrentZone { get; set; }
    }
}
