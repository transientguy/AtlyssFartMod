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
    public class FartController : MonoBehaviour
    {
        public static List<FartController> allFartControllers = new List<FartController>();
        
        [Header("Components")]
        public Player owner;
        private AssetBundle bundle;

        [Header("Fart Effects")]
        private FartEffectsManager fartEffectsManager;

        [Header("Animation Player")]
        private PlayableAnimationPlayer animPlayer;

        public class CurrentAnimationMonitor 
        {
            public Animator animator;
            public int layer;
            public AnimationClip currentClip;
            public List<AnimationClip> clipsToIgnore = new List<AnimationClip>();

            public CurrentAnimationMonitor(Animator animator, int layer) 
            {
                this.animator = animator;
                this.layer = layer;
                currentClip = GetCurrentClip();
            }

            public AnimationClip GetCurrentClip() 
            {
                AnimatorClipInfo[] animatorInfo = this.animator.GetCurrentAnimatorClipInfo(layer);

                if (animatorInfo.Any())
                    return animatorInfo[0].clip;

                return null;
            }

            public string Debug() 
            {
                string message = animator.GetLayerName(layer);

                AnimationClip clip = GetCurrentClip();
                if (clip)
                    message += " " + clip.name;

                return message;
            }

            public bool IsDifferent() 
            {
                AnimationClip clip = GetCurrentClip();

                if (clipsToIgnore.Contains(clip))
                    return false;

                return currentClip != clip;
            }
        }

        private void Awake()
        {
            if(!allFartControllers.Contains(this))
                allFartControllers.Add(this);
        }

        private void OnDestroy() 
        {
            allFartControllers.Remove(this);
        }

        private void Log(string message, bool force = false) 
        {
            FartModCore.Log(message, force);
        }

        private AnimationClip GetAnimationClip(int index)
        {
            Player player = GetPlayer();
            if (player)
            {
                Animator animator = player._pVisual._playerRaceModel._raceAnimator;

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

        public void FartLoop()
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
            if (GetPlayer())
                return GetPlayer()._pVisual._visualAnimator;

            return null;
        }

        private IEnumerator StopLoopFartRoutine()
        {
            Animator playerAnim = GetPlayerAnimator();
            List<CurrentAnimationMonitor> currentAnimationMonitors = new List<CurrentAnimationMonitor>();

            if (GetPlayer()) 
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

                playerAnim = GetPlayer()._pVisual._visualAnimator;

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
                if (!GetPlayer())
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

        private FartEffectsManager GetFartEffectsManager()
        {
            if (!fartEffectsManager)
            {
                fartEffectsManager = AddAndGetComponent<FartEffectsManager>(gameObject);
                fartEffectsManager.owner = GetPlayer();
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

        private Player GetPlayer()
        {
            return owner;
        }

        private IEnumerator FartLoopInfiniteRoutine()
        {
            yield return new WaitForSeconds(.5f);

            while (true)
            {
                if (!GetPlayer())
                {
                    StopAllCoroutines();
                    break;
                }

                Fart();
                float delayTime = UnityEngine.Random.Range(2f, 5f);
                yield return new WaitForSeconds(delayTime);
            }
        }

        private void PlayAnimation()
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

        private void PlayAnimation(AnimationClip clip)
        {
            List<AnimationSequence> animationSequences = new List<AnimationSequence>();
            animationSequences.Add(new AnimationSequence(clip, false));
            PlayAnimation(animationSequences);
        }

        private void PlayAnimation(List<AnimationSequence> clips)
        {
            if (!clips.Any())
            {
                Log("No anim clip!");
                return;
            }

            Player player = GetPlayer();

            if (player)
            {
                Animator playerAnim = player._pVisual._visualAnimator;

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

        public void SetOwner(Player owner, AssetBundle bundle)
        {
            this.owner = owner;

            if (owner)
            {
                name += " " + owner.name;
                transform.SetParent(owner.transform);
                GetFartEffectsManager().SetTransform(transform);
            }
            
            Initialize(bundle);
        }
    }
}
