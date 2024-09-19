using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem
{
    /// <summary>
    /// A simple player that inherits from LevelZonePlayer to work with the
    /// Level Zone system.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    internal class SamplePlayer : LevelZonePlayer
    {
        [Tooltip("Player movement speed.")]
        public float moveSpeed = 12f;

        /// <summary>
        /// Player RigidBody2D
        /// </summary>
        private Rigidbody2D _rb;

        // Start is called before the first frame update
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            Vector2 movement = new Vector2(
                Input.GetAxis("Horizontal"), 
                Input.GetAxis("Vertical")
            );
            movement *= moveSpeed * Time.fixedDeltaTime;

            _rb.velocity = movement;
        }
    }
}