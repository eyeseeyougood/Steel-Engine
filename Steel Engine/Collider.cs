using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public abstract class Collider : Component
    {
        public abstract bool CheckCollision(Collider other);
    }
}