﻿using OpenTK.Graphics.ES11;
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
    public class CollisionData3D
    {
        public Vector3 mtv;
        public Vector3 collisionNormal;

        public static CollisionData3D Zero() { return new CollisionData3D(Vector3.Zero, Vector3.Zero); }

        public CollisionData3D(Vector3 mtv, Vector3 colNormal)
        {
            this.mtv = mtv;
            collisionNormal = colNormal;
        }
    }

    [SteelComponent]
    public class Collider : Component
    {
        public Vector3[] vertices;

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

        public virtual bool CheckSATCollision(Collider other, out Vector3 mtv, out Vector3 normal)
        {
            bool result = false;
            mtv = Vector3.Zero;
            normal = Vector3.Zero;
            return result;
        }

        public virtual Vector3[] FindCollisionPoints(Collider other)
        {
            return null;
        }

        public override void Cleanup()
        {
            if (CollisionManager.colliders.Contains(this))
            {
                CollisionManager.colliders.Remove(this);
            }
        }
    }


    public class BoxCollider : Collider
    {
        public Vector3 min;
        public Vector3 max;

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

        public override bool CheckSATCollision(Collider other, out Vector3 mtv, out Vector3 collisionNormal)
        {
            Vector3 mtvResult = Vector3.Zero;
            Vector3 colNormal = Vector3.Zero;
            bool result = false;

            switch (other.GetType().Name)
            {
                case "BoxCollider":
                    result = SAT3D(this, (BoxCollider)other, out Vector3 vec, out Vector3 normal);
                    mtvResult = vec;
                    colNormal = normal;
                    break;
            }

            mtv = mtvResult;
            collisionNormal = colNormal;
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

        public bool SAT3D(BoxCollider box1, BoxCollider box2, out Vector3 mtv, out Vector3 normal) // mtv = minimum translation vector
        {
            bool result = true;

            SteelPlane3D[] planes = FindPlanes3D(box1, box2);

            List<CollisionData3D> data = new List<CollisionData3D>();

            Vector3 collisionNormal = Vector3.Zero;
            Vector3 minimum = Vector3.Zero; // in this case i am making this the same as the mtv

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
                    /*
                    InfoManager.testObject.mesh.SetColour(Vector3.UnitX);
                    GameObject go = GameObject.QuickCopy(InfoManager.testObject);
                    go.position = vertex - diff;
                    SceneManager.gameObjects.Add(go);*/
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
                    /*
                    InfoManager.testObject.mesh.SetColour(Vector3.UnitZ);
                    GameObject go = GameObject.QuickCopy(InfoManager.testObject);
                    go.position = vertex-diff;
                    SceneManager.gameObjects.Add(go);
                    */
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
                if (!SAT2D(shape1, shape2, out Vector2 mtv2D))
                {
                    // there is a plane on which the objects don't overlap
                    result = false;
                    break;
                }
                else
                {
                    // the objects overlap on this plane
                    // so we will re-project the mtv2D back into 3D
                    Vector3 projectedMTV = new Vector3(mtv2D.X, mtv2D.Y, 0) * rotationMatrix.Inverted();
                    CollisionData3D colData = new CollisionData3D(projectedMTV, a);
                    data.Add(colData);
                    /*
                    InfoManager.testObject.mesh.SetColour(Vector3.UnitY);
                    GameObject go = GameObject.QuickCopy(InfoManager.testObject);
                    go.position = projectedMTV;
                    SceneManager.gameObjects.Add(go);

                    InfoManager.testObject.mesh.SetColour(new Vector3(1f, 0, 1f));
                    GameObject go1 = GameObject.QuickCopy(InfoManager.testObject);
                    go1.position = projectedMTV;
                    SceneManager.gameObjects.Add(go1);*/
                }
            }

            // find the minimum translation vector
            CollisionData3D currentData = CollisionData3D.Zero();
            if (data.Count > 0 && result)
            {
                minimum = data[0].mtv;
                currentData = data[0];
                foreach (CollisionData3D col in data)
                {
                    if (col.mtv.Length < minimum.Length) // OPTIMISATION -- length thingy again
                    {
                        minimum = col.mtv;
                        currentData = col;
                    }
                }
            }

            mtv = currentData.mtv;
            normal = currentData.collisionNormal;
            return result;
        }

        public bool SAT2D(SteelShape2D shape1, SteelShape2D shape2, out Vector2 mtv)
        {
            bool result = true;

            Vector2[] axes = FindAxes2D(shape1, shape2);

            List<Vector2> tvs = new List<Vector2>();

            Vector2 collisionNormal = Vector2.Zero; // in this case im making the collision normal the same as the mtv

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
                    // step 1: find which way the overlap is (box1 on box2 or box2 on box1)
                    // step 2: calculate the overlap distance
                    // possibilities:
                    // b1.1 --- b2.1 --- b1.2 --- b2.2
                    // b1.1 --- b2.1 --- b2.2 --- b1.2
                    // b2.1 --- b1.1 --- b2.2 --- b1.2
                    // b2.1 --- b1.1 --- b1.2 --- b2.2
                    // last edge case is that they are perfectly ontop of each other, but that just gets handled as if it were case 4
                    float dist = 0;
                    if (minDist < minDist1)
                    {
                        if (maxDist < maxDist1)
                        {
                            // case 1
                            dist = -(maxDist - minDist1);
                        }
                        else if (maxDist >= maxDist1)
                        {
                            // case 2
                            dist = -(maxDist1 - minDist);
                        }
                    }
                    else if (minDist1 <= minDist)
                    {
                        if (maxDist1 < maxDist)
                        {
                            // case 3
                            dist = maxDist1 - minDist;
                        }
                        else if (maxDist1 >= maxDist)
                        {
                            // case 4
                            dist = maxDist - minDist1;
                        }
                    }
                    tvs.Add(axis.Normalized() * dist);
                }
            }

            // step 3: find the smallest translation vector (calculating the mtv)
            if (tvs.Count > 0 && result)
            {
                collisionNormal = tvs[0];
                foreach (Vector2 tv in tvs)
                {
                    if (tv.Length < collisionNormal.Length) // OPTIMISATION -- if running too slow, can switch to LengthFast for more speed but less accuracy
                    {
                        collisionNormal = tv;
                    }
                }
            }

            mtv = collisionNormal;

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
                if (vert != prevVert1)
                {
                    axes.Add(vert - prevVert1);
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
