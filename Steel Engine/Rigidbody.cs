using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Steel_Engine
{
    public class Rigidbody : Component
    {
        public Vector3 velocity { get; private set; }

        public override void Tick(float deltaTime)
        {
            gameObject.position += new Vector3(0, velocity.Y, 0);

            velocity += new Vector3(0, -InfoManager.gravityStrength * deltaTime, 0);
        }
    }
}
