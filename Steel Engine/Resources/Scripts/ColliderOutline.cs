using OpenTK.Mathematics;
using Steel_Engine;

public class ColliderOutline : Component
{
    private GameObject minSphere;
    private GameObject maxSphere;
    private BoxCollider col;
    private bool oneTime = false;

    protected override void Init()
    {
        col = gameObject.GetComponent<BoxCollider>();
        minSphere = new GameObject(RenderShader.ShadeFlat, RenderShader.ShadeFlat);
        maxSphere = new GameObject(RenderShader.ShadeFlat, RenderShader.ShadeFlat);
        minSphere.mesh = OBJImporter.LoadOBJ("Cube", true);
        minSphere.mesh.SetColour(Vector3.UnitZ);
        maxSphere.mesh = OBJImporter.LoadOBJ("Cube", true);
        maxSphere.mesh.SetColour(Vector3.UnitX);
        minSphere.scale = Vector3.One * 0.05f;
        maxSphere.scale = Vector3.One * 0.05f;
        minSphere.Load();
        maxSphere.Load();
    }

    public override void Tick(float deltaTime)
    {
        if (!oneTime)
        {
            SceneManager.gameObjects.Add(minSphere);
            SceneManager.gameObjects.Add(maxSphere);
        }

        if (!oneTime)
            oneTime = true;

        minSphere.position = col.min;
        maxSphere.position = col.max;
    }
}