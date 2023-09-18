using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
namespace Steel_Engine
{
    public class BoxCollider : Collider
    {
        public Vector3 minBound;
        public Vector3 maxBound;

        public void RefreshBounds()
        {
            Vector3 newMin = Vector3.Zero;
            Vector3 newMax = Vector3.Zero;

            newMin = new Vector3(
                minBound.X < maxBound.X ? minBound.X : maxBound.X,
                minBound.Y < maxBound.Y ? minBound.Y : maxBound.Y,
                minBound.Z < maxBound.Z ? minBound.Z : maxBound.Z
                );

            newMax = new Vector3(
                minBound.X > maxBound.X ? minBound.X : maxBound.X,
                minBound.Y > maxBound.Y ? minBound.Y : maxBound.Y,
                minBound.Z > maxBound.Z ? minBound.Z : maxBound.Z
                );

            minBound = newMin;
            maxBound = newMax;
        }

        public Vector3[] GetTransformedBounds()
        {
            Vector3[] bounds = new Vector3[2];

            //bounds[0] = new Vector3((new Vector4(minBound, 1) * boundObject.GetTransform()).X, (new Vector4(minBound, 1) * boundObject.GetTransform()).Y, (new Vector4(minBound, 1) * boundObject.GetTransform()).Z);

            return bounds;
        }

        public override bool CheckCollision(Collider other)
        {

        }
    }
}
*/