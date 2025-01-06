using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod.GasControllers.Burps
{
    public class BurpController : GasController
    {
        public static List<BurpController> allBurpControllers = new List<BurpController>();

        private void Awake()
        {
            if (!allBurpControllers.Contains(this))
                allBurpControllers.Add(this);
        }

        private void OnDestroy()
        {
            allBurpControllers.Remove(this);
        }

        protected override GasEffectsManager GetFartEffectsManager()
        {
            if (!fartEffectsManager)
            {
                fartEffectsManager = AddAndGetComponent<BurpEffectsManager>(gameObject);
                fartEffectsManager.owner = GetPlayer();
                fartEffectsManager.Initialize(bundle);
            }

            return fartEffectsManager;
        }

        protected override void PlayAnimation()
        {
            List<AnimationSequence> animationSequences = new List<AnimationSequence>();

            AnimationClip boob1 = GetAnimationClip(75);
            if (boob1)
                animationSequences.Add(new AnimationSequence(boob1, false));

            AnimationClip boob2 = GetAnimationClip(74);
            if (boob2)
                animationSequences.Add(new AnimationSequence(boob2, true));

            PlayAnimation(animationSequences);
        }

        public override void FartLoop()
        {
            FartOneshot();
        }
    }
}
