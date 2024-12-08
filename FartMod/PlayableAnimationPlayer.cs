using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;
using System.Linq;

namespace FartMod
{
    public class PlayableAnimationPlayer : MonoBehaviour
    {
        public Animator animator;
        private PlayableGraph playableGraph;
        private AnimationClipPlayable anim;
        public int currentClip;
        private List<AnimationSequence> animationSequence = new List<AnimationSequence>();

        //Network animation

        private AnimationSequence GetCurrentClip() 
        {
            if (animationSequence.Any()) 
            {
                AnimationSequence sequence;
                if (currentClip < animationSequence.Count)
                {
                    sequence = animationSequence[currentClip];
                }
                else 
                {
                    sequence = animationSequence[animationSequence.Count - 1];
                }

                return sequence;
            }

            return null;
        }

        public void StartAnimating(Animator animator, List<AnimationSequence> animationSequence) 
        {
            currentClip = 0;
            this.animator = animator;
            this.animationSequence = animationSequence;

            if (animator && animationSequence.Any())
            {
                enabled = true;
                PlayCurrentClip();
            }
            else 
            {
                enabled = false;
            }
        }

        private void PlayCurrentClip() 
        {
            DestroyGraph();
            anim = AnimationPlayableUtilities.PlayClip(animator, GetCurrentClip().animationClip, out playableGraph);
        }

        private void Update()
        {
            if (!animator) 
            {
                enabled = false;
                return;
            }

            try
            {
                if (anim.GetTime() >= anim.GetAnimationClip().length) 
                {
                    currentClip++;
                    
                    if (currentClip < animationSequence.Count)
                    {
                        PlayCurrentClip();
                    }
                    else 
                    {
                        AnimationSequence current = GetCurrentClip();
                        bool animInfinite = current != null && current.infinite;
                        
                        if (!animInfinite)
                            enabled = false;
                    }
                }
            }
            catch
            {
                FartModCore.Log("Animation playing failed");
                enabled = false;
            }
        }

        private void DestroyGraph() 
        {
            if (animator) 
            {
                animator.Rebind();
                animator.Update(0);
            }

            if (playableGraph.IsValid())
            {
                // Destroys all Playables and Outputs created by the graph.
                playableGraph.Destroy();
            }
        }

        private void OnDisable()
        {
            DestroyGraph();
        }
    }

    public class AnimationSequence
    {
        public AnimationClip animationClip;
        public bool infinite;

        public AnimationSequence(AnimationClip animationClip, bool infinite)
        {
            this.animationClip = animationClip;
            this.infinite = infinite;
        }
    }
}
