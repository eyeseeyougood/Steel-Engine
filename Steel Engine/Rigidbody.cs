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
        public Vector3 angularVelocity { get; private set; }

        public float mass = 1f;
        public float drag = 0.1f;
        public float angularDrag = 0.1f;
        public float bounciness = 1.5f;

        public Vector3 centerOfMass;

        public Dictionary<Vector3, Vector3> forces = new Dictionary<Vector3, Vector3>(); // pos (relative to center of mass), dir & mag

        protected Collider collider = null;

        private bool oneTime = false;

        public delegate void Collision(Collider other);
        public static event Collision onCollisionEnter = new Collision(OCE);

        private static void OCE(Collider o) { }

        protected override void Init()
        {
            collider = gameObject.GetComponent<Collider>();
            //velocity = new Vector3(0, -0.1f, 0);
            //AddRelativeForce(new Vector3(-1, 0, 0), new Vector3(0, 0.1f, 0));
        }

        public void SetVelocity(Vector3 vel)
        {
            velocity = vel;
        }

        public void AddRelativeForce(Vector3 position, Vector3 force)
        {
            if (forces.ContainsKey(position))
            {
                forces[position] += force/mass;
            }
            else
            {
                forces.Add(position, force/mass);
            }
        }

        protected void ApplyGravity()
        {
            Vector3 gravityForce = mass * Time.timeScale * new Vector3(0, -1, 0) * InfoManager.gravityStrength / 100f;
            AddRelativeForce(new Vector3(0, 0, 0), gravityForce);
        }

        protected void ApplyAmbientGravity(Vector3[] collisionPoints)
        {
            foreach (Vector3 vert in collider.vertices)
            {
                if (!collisionPoints.ToList().ContainsAnInstanceEqualTo(vert))
                {
                    Vector3 gravityForce = mass * Time.timeScale * new Vector3(0, -1, 0) * InfoManager.gravityStrength / 100f;
                    AddRelativeForce(vert-gameObject.position, gravityForce/collisionPoints.Length);
                }
            }
        }

        private void ApplyVelocity()
        {
            gameObject.position += velocity;
        }

        private void CalculateVelocity()
        {
            foreach (KeyValuePair<Vector3, Vector3> force in forces)
            {
                velocity += force.Value;
            }
        }

        private void ApplyAngularVelocity()
        {
            gameObject.Rotate(angularVelocity);
        }

        protected virtual void ResolveCollisions()
        {
            if (collider == null)
                return;

            /*
            if (oneTime)
                return;
            */

            oneTime = true;

            bool collided = false;
            foreach (Collider col in CollisionManager.colliders)
            {
                if (col == collider)
                    continue;

                if (collider.CheckSATCollision(col, out Vector3 mtv, out Vector3 collisionNormal))
                {
                    Console.WriteLine($"COLLIDING: {mtv}");
                    Console.WriteLine($"#ofColliders: {CollisionManager.colliders.Count}");
                    // apply impulse
                    gameObject.position += mtv;

                    // calculate collision normal

                    Vector3[] collisionPoints = collider.FindCollisionPoints(col);

                    foreach (Vector3 point in collisionPoints)
                    {
                        float gravityForce = mass * Time.timeScale * InfoManager.gravityStrength / 100f;
                        AddRelativeForce(point - gameObject.position, collisionNormal.Normalized()*gravityForce / collisionPoints.Length * bounciness);
                    }

                    // apply ambient gravity
                    if (collisionPoints.Length > 0)
                        ApplyAmbientGravity(collisionPoints);

                    // apply friction

                    onCollisionEnter.Invoke(col);
                    collided = true;
                }
                else
                {
                    Console.WriteLine("NOT COLLIDING");
                }
            }
            
            if (!collided)
            {
                ApplyGravity();
            }
            
        }

        public void CalculateAngularVelocity()
        {
            Dictionary<Vector3, Vector3> temp = new Dictionary<Vector3, Vector3>();
            foreach (KeyValuePair<Vector3, Vector3> force in forces)
            {
                angularVelocity += CalculateTorque(force.Key, force.Value);
                temp.Add(force.Key, force.Value);
            }
            foreach (KeyValuePair<Vector3, Vector3> force in temp)
            {
                forces.Remove(force.Key);
            }
        }

        public Vector3 CalculateTorque(Vector3 pos, Vector3 force)
        {
            Vector3 T = Vector3.Cross(pos, force);
            T /= mass;
            return T;
        }

        public void ApplyAirResistance()
        {
            if ((angularVelocity - (angularVelocity * 0.01f)).Length > 0)
            {
                angularVelocity -= angularVelocity * 0.01f * angularDrag;
            }
            if ((velocity - (velocity * 0.01f)).Length > 0)
            {
                velocity -= velocity * 0.01f * drag;
            }
        }

        public void ApplyFrictionalForces()
        {
            ApplyAirResistance();
        }

        public override void Tick(float deltaTime) { }

        public override void LateTick(float deltaTime)
        {
            Console.WriteLine(velocity);
            centerOfMass = gameObject.position;
            CalculateVelocity();
            ApplyVelocity();
            CalculateAngularVelocity();
            ApplyAngularVelocity();
            ApplyFrictionalForces();
            ResolveCollisions();
        }
    }
}