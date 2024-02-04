using Steel_Engine;

public class MoveTest : Component
{
    public override void Tick(float deltaTime)
    {
        gameObject.position += new OpenTK.Mathematics.Vector3(deltaTime, 0, 0);
    }
}