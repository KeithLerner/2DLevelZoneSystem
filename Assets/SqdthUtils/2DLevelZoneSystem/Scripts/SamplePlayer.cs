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
        public float moveSpeed = 12f;

        private Rigidbody2D rb;

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            movement *= moveSpeed * Time.fixedDeltaTime;

            rb.velocity = movement;
        }
    }
}