using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    [SteelComponent]
    public class SpriteAnimator : Component
    {
        private SpriteAnimation loadedSpriteAnimation;
        public bool isPlaying { get; private set; }
        public int frameCount { get; private set; }
        public bool loop = false;
        public int frameSkip = 5;
        private int frameInc = 0;

        public void Play()
        {
            if (loadedSpriteAnimation == null)
            {
                Console.Error.WriteLine("ERROR: no animation is loaded but play attempt was made!");
                return;
            }
            isPlaying = true;
            frameCount = 0;
        }

        public void Play(int startFrame)
        {
            if (loadedSpriteAnimation == null)
            {
                Console.WriteLine("ERROR: no animation is loaded but play attempt was made!");
                return;
            }
            if (startFrame >= loadedSpriteAnimation.animation.Count)
            {
                Console.WriteLine("ERROR: starting frame is outside the bounds of the animation!");
                return;
            }
            isPlaying = true;
            frameCount = startFrame;
        }

        public void Stop()
        {
            isPlaying = false;
            frameCount = 0;
        }

        public void LoadSpriteAnimation(SpriteAnimation spriteAnimation)
        {
            loadedSpriteAnimation = spriteAnimation;
        }

        public override void Tick(float deltaTime)
        {
            if (isPlaying)
            {
                if (HandleAnimationEnd())
                {
                    return;
                }
                gameObject.LoadTexture(loadedSpriteAnimation.animation[frameCount]);
            }
            if (frameInc >= frameSkip)
            {
                frameCount++;
                frameInc = 0;
            }
            else
            {
                frameInc++;
            }
        }

        private bool HandleAnimationEnd() // the returned bool tells the program if to return or not
        {
            bool result = false;

            if (frameCount >= loadedSpriteAnimation.animation.Count)
            {
                if (loop)
                {
                    frameCount = 0;
                }
                else
                {
                    Stop();
                    result = true;
                }
            }

            return result;
        }
    }
}
