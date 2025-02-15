using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod
{
    public class GasController : MonoBehaviour
    {
        [Header("Components")]
        public GasCharacterModel model;
        protected AssetBundle bundle;

        [Header("Gas Effects")]
        protected GasEffectsManager fartEffectsManager;

        [Header("Animation Player")]
        private PlayableAnimationPlayer animPlayer;

        private void Log(string message, bool force = false)
        {
            FartModCore.Log(message, force);
        }

        protected AnimationClip GetAnimationClip(int index)
        {
            GasCharacterModel model = GetModel();
            if (model)
            {
                Animator animator = model.GetRaceAnimator();

                if (index < animator.runtimeAnimatorController.animationClips.Length)
                    return animator.runtimeAnimatorController.animationClips[index];
            }

            return null;
        }

        public void FartOneshot()
        {
            StopAllCoroutines();
            Fart();
        }

        public virtual void FartLoop()
        {
            PlayAnimation();
            StartCoroutine(LoopFartRoutine());
            StartCoroutine(StopLoopFartRoutine());
        }

        private IEnumerator LoopFartRoutine()
        {
            while (GetAnimationPlayer().currentClip == 0)
                yield return null;

            StartCoroutine(FartLoopInfiniteRoutine());
        }

        private void StopFartLoop()
        {
            StopAllCoroutines();
            GetAnimationPlayer().enabled = false;
        }

        private Animator GetPlayerAnimator()
        {
            if (GetModel())
                return GetModel().GetAnimator();

            return null;
        }

        private IEnumerator StopLoopFartRoutine()
        {
            Animator playerAnim = GetPlayerAnimator();
            List<CurrentAnimationMonitor> currentAnimationMonitors = new List<CurrentAnimationMonitor>();

            if (GetModel())
            {
                List<int> clipIndexes = new List<int>();

                //Alt idle clip
                clipIndexes.Add(1);

                List<AnimationClip> clipsToIgnore = new List<AnimationClip>();

                foreach (int i in clipIndexes)
                {
                    AnimationClip clip = GetAnimationClip(i);
                    if (clip)
                        clipsToIgnore.Add(clip);
                }

                playerAnim = GetModel().GetAnimator();

                for (int i = 0; i < playerAnim.layerCount; i++)
                {
                    //Ignore Weapon Hold Layer (2)
                    if (i == 2)
                        continue;

                    //Ignore Boob Layer (6)
                    if (i == 6)
                        continue;

                    //Ignore Shield Hold Layer (4)
                    if (i == 4)
                        continue;

                    CurrentAnimationMonitor animationMonitor = new CurrentAnimationMonitor(playerAnim, i);
                    animationMonitor.clipsToIgnore = clipsToIgnore;
                    currentAnimationMonitors.Add(animationMonitor);
                }
            }

            while (true)
            {
                if (!GetModel())
                {
                    StopFartLoop();
                    break;
                }

                if (!currentAnimationMonitors.Any())
                {
                    Log("No monitors");
                    StopFartLoop();
                    break;
                }

                CurrentAnimationMonitor monitor = currentAnimationMonitors.Find(x => x.IsDifferent());
                if (monitor != null)
                {
                    StopFartLoop();
                    //int index = Array.IndexOf(playerAnim.runtimeAnimatorController.animationClips, monitor.GetCurrentClip());
                    //Log("Interruption on layer " + monitor.Debug() + " " + index);
                    break;
                }

                yield return null;
            }
        }

        public void FartLoopInfinite()
        {
            StopAllCoroutines();
            StartCoroutine(FartLoopInfiniteRoutine());
        }

        public void StopFarting()
        {
            StopAllCoroutines();
        }

        private void Fart()
        {
            GetFartEffectsManager().StartEffect();
        }

        protected virtual GasEffectsManager GetFartEffectsManager()
        {
            if (!fartEffectsManager)
            {
                fartEffectsManager = AddAndGetComponent<GasEffectsManager>(gameObject);
                fartEffectsManager.model = GetModel();
                fartEffectsManager.Initialize(bundle);
            }

            return fartEffectsManager;
        }

        private PlayableAnimationPlayer GetAnimationPlayer()
        {
            if (!animPlayer)
            {
                animPlayer = AddAndGetComponent<PlayableAnimationPlayer>(gameObject);
                animPlayer.enabled = false;
            }

            return animPlayer;
        }

        protected GasCharacterModel GetModel()
        {
            return model;
        }

        private IEnumerator FartLoopInfiniteRoutine()
        {
            yield return new WaitForSeconds(.5f);

            while (true)
            {
                if (!GetModel())
                {
                    StopAllCoroutines();
                    break;
                }

                Fart();
                float delayTime = UnityEngine.Random.Range(2f, 5f);
                yield return new WaitForSeconds(delayTime);
            }
        }

        protected virtual void PlayAnimation()
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

        protected void PlayAnimation(List<AnimationSequence> clips)
        {
            if (!clips.Any())
            {
                Log("No anim clip!");
                return;
            }

            GasCharacterModel model = GetModel();

            if (model)
            {
                Animator playerAnim = model.GetAnimator();

                if (playerAnim)
                {
                    PlayableAnimationPlayer animPlayer = GetAnimationPlayer();
                    if (animPlayer)
                    {
                        animPlayer.StartAnimating(playerAnim, clips);
                        return;
                    }

                    Log("No anim player!");
                }

                Log("No animator on character!");
            }

            Log("No character!");
        }

        public void Initialize(AssetBundle bundle)
        {
            this.bundle = bundle;
            GetFartEffectsManager();
            GetAnimationPlayer();
        }

        public static T AddAndGetComponent<T>(GameObject gameObject) where T : Component
        {
            T final = gameObject.GetComponent<T>();

            if (!final)
                final = gameObject.AddComponent<T>();

            return final;
        }

        public bool CompareOwner(Component owningObject) 
        {
            if (!model)
                return false;

            return model.owningObject == owningObject;
        }

        public void SetOwner(Component owningObject, AssetBundle bundle)
        {
            if (!model)
            {
                model = GasCharacterModel.GetModelFromComponent(owningObject, gameObject);
            }
            else 
            {
                model.Initialize(owningObject);
            }

            if (owningObject)
            {
                name += " " + owningObject.name;
                transform.SetParent(owningObject.transform);
                GetFartEffectsManager().SetTransform(transform);
            }

            Initialize(bundle);
        }
    }
}
