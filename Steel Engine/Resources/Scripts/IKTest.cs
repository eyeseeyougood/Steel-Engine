using OpenTK.Mathematics;
using Steel_Engine;

public class IKTest : Component
{
    private class IKPoint
    {
        public GameObject representative;
        public Vector3 position;
        public float dist;
    }

    private List<IKPoint> ikPoints = new List<IKPoint>();

    private Vector3 targetPos;
    private Vector3 origin = Vector3.Zero;

    protected override void Init()
    {
        InfoManager.testObject.mesh.SetColour(new Vector3(0.0f, 0.0f, 0.0f));
        SceneManager.ChangeClearColour(new Vector3(0.8f, 0.8f, 0.8f));

        int iterator = 0;
        while (iterator < 7)
        {
            IKPoint newPoint = new IKPoint();
            newPoint.dist = 0.75f;
            ikPoints.Add(newPoint);
            iterator++;
        }
    }

    public override void Tick(float deltaTime)
    {
        targetPos = gameObject.position;

        for (int index = ikPoints.Count - 1; index >= 0; index--)
        {
            if (index == ikPoints.Count - 1)
            {
                ikPoints[index].position = targetPos;
            }
            else
            {
                ikPoints[index].position = ikPoints[index+1].position+(ikPoints[index].position - ikPoints[index + 1].position).Normalized()*ikPoints[index].dist;
            }
        }
        for (int index = 0; index < ikPoints.Count; index++)
        {
            if (index == 0)
            {
                ikPoints[index].position = origin;
            }
            else
            {
                ikPoints[index].position = ikPoints[index - 1].position + (ikPoints[index].position - ikPoints[index - 1].position).Normalized() * ikPoints[index].dist;
            }
        }

        foreach (IKPoint p in ikPoints)
        {
            if (p.representative == null)
            {
                GameObject go = GameObject.QuickCopy(InfoManager.testObject);
                go.position = p.position;
                go.scale = new Vector3(0.1f, 0.1f, 0.1f);
                SceneManager.gameObjects.Add(go);
                p.representative = go;
            }
            else
            {
                p.representative.position = p.position;
            }
        }
    }
}