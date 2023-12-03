using Steel_Engine;
using Steel_Engine.GUI;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Steel_Engine.Common;

public class TestCode : Component
{
    private SpriteAnimator animator;

    protected override void Init()
    {
        SceneManager.ChangeClearColour(new Vector3(0.7f, 0.7f, 0.7f));

        Texture[] slices = Texture.SliceSpriteSheet(InfoManager.usingDataPath + @"\Textures\Animations\FreeKnight_v1\Colour2\Outline\120x80_PNGSheets\_AttackComboNoMovement.png", 120, 80);
        SpriteAnimation attackAnim = new SpriteAnimation();
        foreach (Texture slice in slices)
        {
            attackAnim.animation.Add(slice);
        }
        animator = gameObject.GetComponent<SpriteAnimator>();
        animator.LoadSpriteAnimation(attackAnim);
        animator.Play();
        animator.frameSkip = 5;
        animator.loop = true;
    }

    public override void Tick(float deltaTime)
    {
    }
}