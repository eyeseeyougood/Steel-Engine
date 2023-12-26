using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using Steel_Engine.Common;
using Steel_Engine.GUI;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    [SteelComponent]
    public class Collider : Component
    {
        protected override void Init()
        {
            CollisionManager.colliders.Add(this);
        }

        public override void Tick(float deltaTime)
        {

        }

        public virtual Vector3 CalculateCollisionNormal(Collider other)
        {
            return Vector3.Zero;
        }

        public virtual bool CheckSATCollision(Collider other)
        {
            bool result = false;
            return result;
        }

        public virtual Vector3[] FindCollisionPoints(Collider other)
        {
            return null;
        }
    }


    public class BoxCollider : Collider
    {
        public Vector3 min;
        public Vector3 max;

        public Vector3[] vertices;

        protected override void Init()
        {
            base.Init();
            min = -Vector3.One;
            max = Vector3.One;
            vertices = new Vector3[8];
            vertices[0] = min;
            vertices[1] = new Vector3(-1, -1, 1);
            vertices[2] = new Vector3(1, -1, 1);
            vertices[3] = new Vector3(1, -1, -1);
            vertices[4] = new Vector3(1, 1, -1);
            vertices[5] = new Vector3(-1, 1, 1);
            vertices[6] = new Vector3(-1, 1, -1);
            vertices[7] = max;
            // transform vertices by position, scale, rotation
            List<Vector3> finalVertices = new List<Vector3>();
            foreach (Vector3 vert in vertices)
            {
                Vector3 transformed = vert * gameObject.scale;
                transformed = Vector3.Transform(transformed, gameObject.qRotation);
                transformed = transformed + gameObject.position;
                finalVertices.Add(transformed);
            }
            vertices = finalVertices.ToArray();
        }

        public override void Tick(float deltaTime)
        {
            min = -Vector3.One;
            max = Vector3.One;
            vertices = new Vector3[8];
            vertices[0] = min;
            vertices[1] = new Vector3(-1, -1, 1);
            vertices[2] = new Vector3(1, -1, 1);
            vertices[3] = new Vector3(1, -1, -1);
            vertices[4] = new Vector3(1, 1, -1);
            vertices[5] = new Vector3(-1, 1, 1);
            vertices[6] = new Vector3(-1, 1, -1);
            vertices[7] = max;
            // transform vertices by position, scale, rotation
            List<Vector3> finalVertices = new List<Vector3>();
            foreach (Vector3 vert in vertices)
            {
                Vector3 transformed = vert * gameObject.scale;
                transformed = Vector3.Transform(transformed, gameObject.qRotation);
                transformed = transformed + gameObject.position;
                finalVertices.Add(transformed);
            }
            vertices = finalVertices.ToArray();
        }

        public override bool CheckSATCollision(Collider other)
        {
            bool result = false;

            switch (other.GetType().Name)
            {
                case "BoxCollider":
                    result = SAT3D(this, (BoxCollider)other, out Vector3 vec);
                    break;
            }

            return result;
        }

        public override Vector3[] FindCollisionPoints(Collider other)
        {
            Vector3[] result = null;

            switch (other.GetType().Name)
            {
                case "BoxCollider":
                    result = FindCollisionPointsBox((BoxCollider)other);
                    break;
            }

            return result;
        }

        public bool SAT3D(BoxCollider box1, BoxCollider box2, out Vector3 mtv) // mtv = minimum translation vector
        {
            bool result = true;

            SteelPlane3D[] planes = FindPlanes3D(box1, box2);

            Vector3 totalTV = new Vector3(0, 0, 0);

            foreach (SteelPlane3D plane in planes)
            {
                // get plane normal
                Vector3 planeNormal = Vector3.Cross(plane.points[1] - plane.points[0], plane.points[2] - plane.points[0]);
                planeNormal.Normalize();

                // project box1 onto the plane
                Vector3[] projectedVertices = new Vector3[8];
                int index = 0;
                foreach (Vector3 vertex in box1.vertices)
                {
                    float dist = Vector3.Dot(vertex, planeNormal);
                    Vector3 diff = dist * planeNormal;
                    projectedVertices[index] = vertex - diff;
                    index++;
                }

                // project box2 onto the plane
                Vector3[] projectedVertices1 = new Vector3[8];
                index = 0;
                foreach (Vector3 vertex in box2.vertices)
                {
                    float dist = Vector3.Dot(vertex, planeNormal);
                    Vector3 diff = dist * planeNormal;
                    projectedVertices1[index] = vertex - diff;
                    index++;
                }

                // Find orthonormal bases
                Vector3 a = (plane.points[1] - plane.points[0]).Normalized();
                Vector3 b = Vector3.Cross(a, planeNormal).Normalized();
                // Find rotation matrix
                Matrix3 rotationMatrix = new Matrix3(a, b, planeNormal).Inverted();
                // Convert Vertices to 2D space
                string debug1 = "";
                string debug2 = "";
                List<Vector2> convertedVertices = new List<Vector2>();
                foreach (Vector3 vertex in projectedVertices)
                {
                    Vector3 rotated = vertex * rotationMatrix;
                    Vector2 converted = new Vector2(rotated.X, rotated.Y);
                    convertedVertices.Add(converted);
                    debug1 += $",({converted.X}, {converted.Y})";
                }

                List<Vector2> convertedVertices1 = new List<Vector2>();
                foreach (Vector3 vertex in projectedVertices1)
                {
                    Vector3 rotated = vertex * rotationMatrix;
                    Vector2 converted = new Vector2(rotated.X, rotated.Y);
                    convertedVertices1.Add(converted);
                    debug2 += $",({converted.X}, {converted.Y})";
                }

                // Create shapes
                SteelShape2D shape1 = new SteelShape2D(convertedVertices.ToArray());
                SteelShape2D shape2 = new SteelShape2D(convertedVertices1.ToArray());

                // perform 2D SAT on projected shapes
                if (!SAT2D(shape1, shape2, out float bum))
                {
                    // there is a plane on which the objects don't overlap
                    result = false;
                    break;
                }
            }

            if (result)
            {
                mtv = Vector3.Zero;
            }

            mtv = Vector3.Zero;
            return result;
        }

        public bool SAT2D(SteelShape2D shape1, SteelShape2D shape2, out float overlapDist)// overlapDist will be -1 if there isn't an overlap
        {
            bool result = true;

            Vector2[] axes = FindAxes2D(shape1, shape2);

            Vector2 collisionNormal = Vector2.Zero;

            foreach (Vector2 axis in axes)
            {
                // project box1 onto axis
                float minDist = float.MaxValue;
                float maxDist = float.MinValue;
                foreach (Vector2 vertex in shape1.vertices)
                {
                    float dist = Vector2.Dot(vertex, axis.Normalized());
                    if (dist < minDist) { minDist = dist; }
                    if (dist > maxDist) { maxDist = dist; }
                }

                // project box2 onto axis
                float minDist1 = float.MaxValue;
                float maxDist1 = float.MinValue;
                foreach (Vector2 vertex in shape2.vertices)
                {
                    float dist1 = Vector2.Dot(vertex, axis.Normalized());
                    if (dist1 < minDist1) { minDist1 = dist1; }
                    if (dist1 > maxDist1) { maxDist1 = dist1; }
                }

                // check for overlap
                if (!(minDist1 <= maxDist && minDist1 >= minDist || maxDist1 <= maxDist && maxDist1 >= minDist) && !(minDist <= maxDist1 && minDist >= minDist1 || maxDist <= maxDist1 && maxDist >= minDist1))
                {
                    // there isn't an overlap
                    result = false;
                    break;
                }
                else
                {
                    // there is an overlap on this axis
                    //if ()
                }
            }

            if (result)
            {
                // there is an overlap
                overlapDist = -1f;
            }
            else
            {
                overlapDist = -1f;
            }

            return result;
        }

        public SteelPlane3D[] FindPlanes3D(BoxCollider box1, BoxCollider box2)
        {
            List<SteelPlane3D> planes = new List<SteelPlane3D>();
            SteelPlane3D up = new SteelPlane3D(box1.vertices[5], box1.vertices[7], box1.vertices[6]);
            SteelPlane3D right = new SteelPlane3D(box1.vertices[2], box1.vertices[7], box1.vertices[3]);
            SteelPlane3D forward = new SteelPlane3D(box1.vertices[1], box1.vertices[5], box1.vertices[2]);
            SteelPlane3D up1 = new SteelPlane3D(box2.vertices[5], box2.vertices[7], box2.vertices[6]);
            SteelPlane3D right1 = new SteelPlane3D(box2.vertices[2], box2.vertices[7], box2.vertices[3]);
            SteelPlane3D forward1 = new SteelPlane3D(box2.vertices[1], box2.vertices[5], box2.vertices[2]);
            planes.Add(up);
            planes.Add(up1);
            planes.Add(right);
            planes.Add(right1);
            planes.Add(forward);
            planes.Add(forward1);

            // cleanup duplicate planes
            List<SteelPlane3D> finalPlanes = new List<SteelPlane3D>();
            foreach (SteelPlane3D plane in planes)
            {
                if (!finalPlanes.ContainsAnInstanceEqualTo(plane))
                {
                    finalPlanes.Add(plane);
                }
            }

            return planes.ToArray();
        }

        public Vector2[] FindAxes2D(SteelShape2D shape1, SteelShape2D shape2)
        {
            List<Vector2> axes = new List<Vector2>();
            Vector2 prevVert = shape1.vertices[0];
            foreach (Vector2 vert in shape1.vertices)
            {
                if (vert != prevVert)
                {
                    axes.Add(vert - prevVert);
                    prevVert = vert;
                }
            }
            Vector2 prevVert1 = shape2.vertices[0];
            foreach (Vector2 vert in shape2.vertices)
            {
                if (vert != prevVert)
                {
                    axes.Add(vert - prevVert);
                    prevVert1 = vert;
                }
            }

            // cleanup duplicate axes
            List<Vector2> finalAxes = new List<Vector2>();
            foreach (Vector2 axis in axes)
            {
                if (!finalAxes.ContainsAnInstanceEqualTo(axis))
                {
                    finalAxes.Add(axis);
                }
            }

            return finalAxes.ToArray();
        }

        // resolving boxes
        private Vector3[] FindCollisionPointsBox(BoxCollider other) // returns the points in a collision
        {
            List<Vector3> result = new List<Vector3>();

            foreach (Vector3 i in vertices)
            {
                if (MathFExtentions.PointCubeIntersection(i, other))
                {
                    result.Add(i);
                }
            }

            return result.ToArray(); // only returns points on this cube not other cubes
        }
    }

    public static class CollisionManager
    {
        public static List<Collider> colliders = new List<Collider>();
    }
}
