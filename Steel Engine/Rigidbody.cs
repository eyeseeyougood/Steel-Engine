using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Steel_Engine.Common;

namespace Steel_Engine
{
    [SteelComponent]
    public class Rigidbody : Component
    {
        public Vector3 velocity { get; private set; }

        private Collider collider = null;

        private bool oneTime = true;

        protected override void Init()
        {
            collider = gameObject.GetComponent<Collider>();
        }

        private void ApplyGravity()
        {
            Vector3 gravityForce = Time.timeScale * new Vector3(0, -1, 0) * InfoManager.gravityStrength / 100f;
            velocity += gravityForce;
        }

        private void ApplyVelocity()
        {
            gameObject.position += velocity;
        }

        private void ResolveCollisions()
        {
            if (collider == null)
                return;

            foreach (Collider col in CollisionManager.colliders)
            {
                if (col == collider)
                    continue;

                if (collider.CheckCollision(col))
                {
                    // calculate collision normal
                    Vector3 collisionNormal = collider.CalculateCollisionNormal(col);
                    gameObject.position += collisionNormal; // change based of how accurate / laggy it is
                    velocity = new Vector3(velocity.X, Time.timeScale * 0.01f, velocity.Z);
                }
            }
        }

        public override void Tick(float deltaTime)
        {
            ApplyGravity();
            ApplyVelocity();
            ResolveCollisions();
        }
    }
}