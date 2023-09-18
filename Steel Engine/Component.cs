using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public abstract class Component
    {
        public GameObject gameObject;
        public abstract void Tick(float deltaTime);

        public void ComponentInit(GameObject boundObject)
        {
            this.gameObject = boundObject;
        }
    }
}
