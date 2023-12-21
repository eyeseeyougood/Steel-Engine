using OpenTK.Mathematics;
using Steel_Engine;
using Steel_Engine.Common;
using Steel_Engine.GUI;

public class RopeTest : Component
{
    public class RopePoint
    {
        public GameObject representative;
        public Vector3 position;
        public Vector3 previousPosition;
        public bool locked;

        public RopePoint() { }
        public RopePoint(bool _locked) { 
            this.locked = _locked;
            GenRep();
        }
        public RopePoint(Vector3 pos, bool _locked)
        {
            this.position = pos;
            this.previousPosition = pos;
            this.locked = _locked;
            GenRep();
        }

        private void GenRep()
        {
            GameObject go = GameObject.QuickCopy(InfoManager.testObject);
            go.position = position;
            go.scale = new Vector3(0.1f, 0.1f, 0.1f);
            SceneManager.gameObjects.Add(go);
            representative = go;
        }
    }

    public class RopeStick
    {
        public RopePoint pointA;
        public RopePoint pointB;
        public float length;

        public RopeStick() { }

        public RopeStick(RopePoint pointA, RopePoint pointB, float length)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.length = length;
        }
    }

    public List<RopePoint> points = new List<RopePoint>();
    public List<RopeStick> sticks = new List<RopeStick>();

    private int numIterations = 10;

    public GameObject cursor3D;

    private bool sim;

    protected override void Init()
    {
        InfoManager.testObject.mesh.SetColour(new Vector3(0.0f, 0.0f, 1.0f));
        SceneManager.ChangeClearColour(new Vector3(0.8f, 0.8f, 0.8f));

        cursor3D = GameObject.QuickCopy(InfoManager.testObject);
        SceneManager.gameObjects.Add(cursor3D);
    }

    public override void Tick(float deltaTime)
    {
        if (sim)
        {
            foreach (RopePoint p in points)
            {
                if (!p.locked)
                {
                    Vector3 pos = p.position;
                    p.position += p.position - p.previousPosition;
                    p.position += -Vector3.UnitY * InfoManager.gravityStrength * deltaTime;
                    p.previousPosition = pos;
                }

                p.representative.position = p.position;
            }

            for (int i = 0; i < numIterations; i++)
            {
                foreach (RopeStick s in sticks)
                {
                    Vector3 center = (s.pointA.position + s.pointB.position) / 2.0f;
                    Vector3 direction = (s.pointA.position - s.pointB.position).Normalized();
                    if (!s.pointA.locked)
                        s.pointA.position = center + direction * s.length / 2;
                    if (!s.pointB.locked)
                        s.pointB.position = center - direction * s.length / 2;
                }
            }
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Y))
        {
            sim = !sim;
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace))
        {
            RopePoint point = null;
            foreach (RopePoint p in points)
            {
                if ((p.position-cursor3D.position).Length < 0.48f)
                    point = p;
            }
            List<RopeStick> stick = new List<RopeStick>();
            foreach (RopeStick s in sticks)
            {
                if (s.pointA == point || s.pointB == point)
                {
                    stick.Add(s);
                }
            }
            int index = 0;
            while (index < stick.Count)
            {
                sticks.Remove(stick[index]);
                index++;
            }
            if (point != null)
            {
                SceneManager.gameObjects.Remove(point.representative);
                points.Remove(point);
            }
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up))
        {
            cursor3D.position += -Vector3.UnitZ;
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down))
        {
            cursor3D.position += Vector3.UnitZ;
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left))
        {
            cursor3D.position += -Vector3.UnitX;
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right))
        {
            cursor3D.position += Vector3.UnitX;
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.J))
        {
            cursor3D.position += -Vector3.UnitY;
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.K))
        {
            cursor3D.position += Vector3.UnitY;
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.P))
        {
            InfoManager.testObject.mesh.SetColour(new Vector3(0.0f, 0.0f, 0.0f));
            RopePoint p = new RopePoint(cursor3D.position, false);
            KeyValuePair<float, RopePoint> closestPoint = new KeyValuePair<float, RopePoint>(float.MaxValue, p);
            foreach (RopePoint i in points)
            {
                float dist = (p.position-i.position).Length;
                if (dist < closestPoint.Key)
                {
                    closestPoint = new KeyValuePair<float, RopePoint>(dist, i);
                }
            }
            if (points.Count > 0)
            {
                RopeStick stick = new RopeStick(p, closestPoint.Value, 2f);
                sticks.Add(stick);
            }
            points.Add(p);
        }

        if (InputManager.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.L))
        {
            InfoManager.testObject.mesh.SetColour(new Vector3(1.0f, 0.0f, 0.0f));
            RopePoint p = new RopePoint(cursor3D.position, true);
            KeyValuePair<float, RopePoint> closestPoint = new KeyValuePair<float, RopePoint>(float.MaxValue, p);
            foreach (RopePoint i in points)
            {
                float dist = (p.position - i.position).Length;
                if (dist < closestPoint.Key)
                {
                    closestPoint = new KeyValuePair<float, RopePoint>(dist, i);
                }
            }
            if (points.Count > 0)
            {
                RopeStick stick = new RopeStick(p, closestPoint.Value, 2f);
                sticks.Add(stick);
            }
            points.Add(p);
        }
    }
}