using OpenTK.Mathematics;
using Steel_Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MonkeyRotate : Component
{
    public override void Tick(float deltaTime)
    {
        gameObject.Rotate(0, 3 * deltaTime, 0);
    }
}