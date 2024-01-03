using Steel_Engine.GUI;
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
        public virtual void LateTick(float deltaTime) { }

        public virtual void OnDrawInspectorPanel(GUIElement panel)
        {
            // make adjustments to panel (example)
        }

        public virtual void OnInspectorPanelDrawn(GUIElement panel)
        {
            // make adjustments to sub-objects (example)
        }

        public virtual void Cleanup() { }

        protected virtual void Init() { }

        public void ComponentInit(GameObject boundObject)
        {
            gameObject = boundObject;
            Init();
        }
    }
}