using Steel_Engine;
using Steel_Engine.GUI;
using OpenTK.Mathematics;

public class DirectionalAudio : Component
{
    public float soundStrength = 5;
    private AudioSource audioSource;
    private bool test = true;

    protected override void Init()
    {
        audioSource = new AudioSource("The Lobster", 0.5f);

        //TESTCODE
        //SceneManager.gameObjects[1].GetComponent<Rigidbody>().isKinematic = true;
    }

    public override void Tick(float deltaTime)
    {
        if (test)
        {
            audioSource.Play();
            AudioManager.PlayDSO();
            test = false;
        }

        // calculate direction vector
        Vector3 directionVec = InfoManager.engineCamera.Position - gameObject.position;
        Vector3 normal = directionVec.Normalized();

        Vector3 directionVectorLocal = TransformToLocalSpace(normal, InfoManager.engineCamera.GetViewMatrix().ExtractRotation());

        // assign pan value
        float panValue = -directionVectorLocal.X;

        // offset pan value
        panValue = MathF.Max(-1, MathF.Min(1, panValue));

        // apply panning
        audioSource.SetPanning(panValue);

        // calculate distance drop-off
        // calculate distance
        float distance = directionVec.Length;

        // calculate volume
        float volume = 1 - (distance / soundStrength);

        // clamp volume so it isn't negative
        if (volume < 0)
        {
            volume = 0;
        }

        // apply volume
        audioSource.SetVolume(volume);
    }

    private Vector3 TransformToLocalSpace(Vector3 vector, Quaternion rotation)
    {
        // Transform the vector from world space to local space
        Vector4 homogenousVector = new Vector4(vector.X, vector.Y, vector.Z, 1.0f);
        Vector4 transformedVector = Vector4.Transform(homogenousVector, rotation);
        return new Vector3(transformedVector.X, transformedVector.Y, transformedVector.Z);
    }
}