using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;

namespace FartMod
{
    public class FartEffectsManager : MonoBehaviour
    {
        private static List<AudioClip> sounds = new List<AudioClip>();
        private static GameObject particleSystemPrefab;

        public Player owner;
        private AudioSource audioSource;
        private ParticleSystem particleSystem;
        private bool effectPlaying;
        private AssetBundle bundle;

        private float counter;

        //Network Audio
        //Network Particles

        public void Initialize(AssetBundle bundle)
        {
            this.bundle = bundle;

            PreloadAudioClips();
            GetParticleSystemPrefab();
            GetAudioSource();
            GetParticleSystem();

            enabled = false;
        }

        private GameObject GetParticleSystemPrefab() 
        {
            if (!particleSystemPrefab)
                particleSystemPrefab = bundle.LoadAsset<GameObject>("FartParticle");

            return particleSystemPrefab;
        }

        private void PreloadAudioClips()
        {
            if (sounds.Any())
                return;

            Log("Checking Sounds");

            string audioPathName = "Audio";
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), audioPathName);
            if (Directory.Exists(path))
            {
                sounds.Clear();
                CollectAudioFiles(path);
            }
            else
            {
                Log($"Directory {path} does not exist! Creating.");
                Directory.CreateDirectory(path);
            }
        }

        private void CollectAudioFiles(string path)
        {
            Log($"checking folder {Path.GetFileName(path)}");
            string[] audioFiles = Directory.GetFiles(path);
            foreach (string file in audioFiles)
            {
                Log($"\tchecking single file {Path.GetFileName(file)}");
                AudioClip clip = AssetUtils.LoadAudioFromWebRequest(file, AudioType.UNKNOWN);

                if (clip)
                    sounds.Add(clip);
            }
        }

        private AudioSource GetAudioSource()
        {
            if (!audioSource)
                audioSource = FartController.AddAndGetComponent<AudioSource>(gameObject);

            return audioSource;
        }

        private ParticleSystem GetParticleSystem()
        {
            if (!particleSystem)
            {
                particleSystem = GetComponentInChildren<ParticleSystem>();

                if (!particleSystem)
                {
                    if (GetParticleSystemPrefab())
                        particleSystem = Instantiate(GetParticleSystemPrefab(), transform).GetComponent<ParticleSystem>();
                }
            }

            return particleSystem;
        }

        public void StartEffect() 
        {
            //Reset counter;
            counter = 0;

            //Grab random clip
            int randIndex = Random.Range(0, sounds.Count);
            AudioClip clip = sounds[randIndex];

            //Set clip and play
            GetAudioSource().clip = clip;
            GetAudioSource().Play();

            Player player = GetPlayer();

            //Grab audio source on player and copy values
            AudioSource audioSource = GetAudioSource();

            AudioSource playerAudioSource = player.GetComponentInChildren<AudioSource>();
            if (playerAudioSource)
            {
                audioSource.outputAudioMixerGroup = playerAudioSource.outputAudioMixerGroup;
                audioSource.spatialBlend = playerAudioSource.spatialBlend;
            }

            enabled = true;
        }

        private Vector3 AssDirection(Player player)
        {
            Vector3 averagePosition = Vector3.zero;

            DynamicBone[] assBones = player._pVisual._playerRaceModel._assDynamicBones;
            for (int i = 0; i < assBones.Length; i++)
            {
                DynamicBone assBone = assBones[i];
                averagePosition += assBone.m_Root.up;
            }

            return (averagePosition / assBones.Length);
        }

        private Vector3 AssPosition(Player player)
        {
            Vector3 averagePosition = Vector3.zero;

            DynamicBone[] assBones = player._pVisual._playerRaceModel._assDynamicBones;
            for (int i = 0; i < assBones.Length; i++)
            {
                DynamicBone assBone = assBones[i];
                averagePosition += assBone.transform.position;
            }

            return (averagePosition / assBones.Length);
        }

        private Player GetPlayer()
        {
            return owner;
        }

        private void Log(string message, bool force = false) 
        {
            FartModCore.Log(message, force);
        }

        private void SetEffectEnabled(bool b) 
        {
            StopAllCoroutines();

            if (b)
            {
                StartCoroutine(EyeConditionRoutine());
                StartCoroutine(JiggleRoutine());
            }
            else 
            {
                GetAudioSource().Stop();
            }

            SetJiggleForce(0);
            SetParticleEnabled(b);
            effectPlaying = b;
        }

        public void SetTransform(Transform t) 
        {
            Player player = GetPlayer();

            if (!player)
                return;

            t.forward = AssDirection(player);
            t.position = AssPosition(player) + t.forward * .75f;
        }

        private void Update() 
        {
            if (!GetAudioSource().clip) 
            {
                enabled = false;
                return;
            }

            if (counter >= GetAudioSource().clip.length) 
            {
                enabled = false;
                return;
            }

            Player player = GetPlayer();

            if (!player)
            {
                enabled = false;
                return;
            }

            transform.localScale = Vector3.one * .075f;
            SetTransform(transform);

            bool playEffects = true;

            if (playEffects)
            {
                if (!effectPlaying)
                    SetEffectEnabled(true);
            }
            else 
            {
                if (effectPlaying)
                    SetEffectEnabled(false);
            }

            counter += Time.deltaTime;
        }

        private void SetJiggleForce(float forceMultiplier) 
        {
            Player player = GetPlayer();

            if (!player)
                return;

            //Jiggle dynamic bones
            float randPower = Random.Range(0.0003f, 0.0006f);
            float forcePower = randPower * forceMultiplier;

            //Jiggle dat ass
            DynamicBone[] assBones = player._pVisual._playerRaceModel._assDynamicBones;
            for (int i = 0; i < assBones.Length; i++)
            {
                DynamicBone assBone = assBones[i];
                float multiplier = ((i + 1) % 2) == 0 ? 1 : -1;
                Vector3 force = player.transform.right * multiplier * forcePower;
                assBone.m_Force = force;
            }

            //Jiggle tail
            List<DynamicBone> dynamicBones = new List<DynamicBone>(player.gameObject.GetComponentsInChildren<DynamicBone>());
            DynamicBone tailBone = dynamicBones.Find(x => x.name.Contains("tail"));

            if (tailBone)
            {
                Vector3 force = player.transform.up * forcePower;
                tailBone.m_Force = force;
            }
            else
            {
                //Log("Tailbone not found?");
            }
        }

        private IEnumerator JiggleRoutine()
        {
            while (true)
            {
                SetJiggleForce(1);

                yield return new WaitForEndOfFrame();

                SetJiggleForce(0);

                yield return new WaitForSeconds(.15f);
            }
        }

        private void SetParticleEnabled(bool b) 
        {
            ParticleSystem particle = GetParticleSystem();

            if (particle)
            {
                if (b)
                {
                    particle.Play(true);
                }
                else
                {
                    particle.Stop(true);
                }
            }
            else
            {
                Log("NO PARTICLE", true);
            }
        }

        private void SetEyeConditions()
        {
            Player player = GetPlayer();

            if (!player)
                return;

            //Expression
            //Set eye condition
            player._pVisual._playerRaceModel.Set_EyeCondition(EyeCondition.Closed, 1f);

            //Set eye condition
            player._pVisual._playerRaceModel.Set_MouthCondition(MouthCondition.Closed, 1f);
        }

        private IEnumerator EyeConditionRoutine()
        {
            while (true)
            {
                SetEyeConditions();
                yield return new WaitForEndOfFrame();
            }
        }

        private void OnDisable() 
        {
            SetEffectEnabled(false);

            Player player = GetPlayer();

            if (!player)
                return;

            //Expression
            //Get eye condition
            List<EyeCondition> eyeConditions = new List<EyeCondition>();
            eyeConditions.Add(EyeCondition.Up);
            eyeConditions.Add(EyeCondition.Closed);
            EyeCondition eyeCond = eyeConditions[Random.Range(0, eyeConditions.Count)];
            player._pVisual._playerRaceModel.Set_EyeCondition(eyeCond, 1f);

            //Set eye condition
            player._pVisual._playerRaceModel.Set_MouthCondition(MouthCondition.Open, 1f);
        }
    }
}
