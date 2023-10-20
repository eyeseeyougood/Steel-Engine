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
        protected virtual void Init() { }

        public void ComponentInit(GameObject boundObject)
        {
            gameObject = boundObject;
            Init();
        }
    }
}