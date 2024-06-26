using UnityEngine;

namespace SqdthUtils._2DLevelZoneSystem.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ExamplePlayer : LevelZonePlayer
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