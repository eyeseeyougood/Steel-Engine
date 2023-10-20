using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Steel_Engine.Common;

namespace Steel_Engine
{
    public class Rigidbody : Component
    {
        public Vector3 velocity { get; private set; }

        private Collider collider = null;

        bool oneTime = true;

        protected override void Init()
        {
            collider = gameObject.GetComponent<Collider>();
        }

        private void ApplyGravity()
        {
            Vector3 gravityForce = Time.timeScale * new Vector3(0, -1, 0) * 0.01f;
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
                    Vector3 collisionNormal = (collider.gameObject.position - col.gameObject.position).Normalized();
                    while (collider.CheckCollision(col))
                    {
                        gameObject.position += collisionNormal * 1f; // change based of how accurate / laggy it is
                    }
                }
            }
        }

        public override void Tick(float deltaTime)
        {
            ApplyGravity();
            ApplyVelocity();
            ResolveCollisions();
            Console.WriteLine(InfoManager.testSphere.mesh.vertices[0].colour);
        }
    }
}