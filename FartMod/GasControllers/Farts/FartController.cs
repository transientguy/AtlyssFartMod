using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod
{
    public class FartController : GasController
    {
        public static List<FartController> allFartControllers = new List<FartController>();
        
        private void Awake()
        {
            if(!allFartControllers.Contains(this))
                allFartControllers.Add(this);
        }

        private void OnDestroy() 
        {
            allFartControllers.Remove(this);
        }

        protected override GasEffectsManager GetFartEffectsManager()
        {
            if (!fartEffectsManager)
            {
                fartEffectsManager = AddAndGetComponent<FartEffectsManager>(gameObject);
                fartEffectsManager.model = GetModel();
                fartEffectsManager.Initialize(bundle);
            }

            return fartEffectsManager;
        }

        protected override void PlayAnimation()
        {
            List<AnimationSequence> animationSequences = new List<AnimationSequence>();

            AnimationClip butt1 = GetAnimationClip(77);
            if (butt1)
                animationSequences.Add(new AnimationSequence(butt1, false));

            AnimationClip butt2 = GetAnimationClip(76);
            if (butt2)
                animationSequences.Add(new AnimationSequence(butt2, true));
            
            PlayAnimation(animationSequences);
        }
    }
}