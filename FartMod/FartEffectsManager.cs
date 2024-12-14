using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;
using static UnityEngine.ParticleSystem;

namespace FartMod
{
    public class FartEffectsManager : MonoBehaviour
    {
        private static List<AudioClip> sounds = new List<AudioClip>();
        private static GameObject particleSystemPrefab;

        public Player owner;
        private AudioSource audioSource;

        private ParticleSystem particleSystem;
        private ParticleSystem gasParticle;

        private bool effectPlaying;
        private AssetBundle bundle;

        private float counter;

        private FartEffectsConfiguration configuration = new FartEffectsConfiguration();

        //Network Audio
        //Network Particles

        public void Initialize(AssetBundle bundle)
        {
            this.bundle = bundle;
            
            configuration = new FartEffectsConfiguration(this);
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

            if(audioSource)
                audioSource.volume = configuration.GetVolume();
            
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
                //audioSource.outputAudioMixerGroup = playerAudioSource.outputAudioMixerGroup;
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

        private Gradient GetGradientColorKeys(List<Color> colors, Gradient gradient)
        {
            List<GradientColorKey> gradientColorKeys = new List<GradientColorKey>();
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                GradientColorKey originalGradient = gradient.colorKeys[i];
                Color c = originalGradient.color;

                if (colors.Any())
                {
                    if (i < colors.Count)
                    {
                        c = colors[i];
                    }
                    else
                    {
                        c = colors[0];
                    }
                }

                GradientColorKey newGradient = new GradientColorKey(c, originalGradient.time);
                gradientColorKeys.Add(newGradient);
            }

            Gradient grad = new Gradient();
            grad.SetKeys(gradientColorKeys.ToArray(), gradient.alphaKeys);
            return grad;
        }

        private ParticleSystem GetGasParticle()
        {
            if (!gasParticle)
                gasParticle = GetParticleSystem().transform.Find("Gas").GetComponent<ParticleSystem>();

            return gasParticle;
        }

        private Gradient GetStartGradient()
        {
            return GetGasParticle().colorOverLifetime.color.gradientMax;
        }

        private Gradient GetEndGradient()
        {
            return GetGasParticle().colorOverLifetime.color.gradientMin;
        }

        private void SetEffectEnabled(bool b) 
        {
            StopAllCoroutines();

            if (b)
            {
                List<Color> startColors = configuration.GetStartColors();
                Gradient startGradient = GetGradientColorKeys(startColors, GetStartGradient());

                List<Color> endColors = configuration.GetEndColors();
                Gradient endGradient = GetGradientColorKeys(endColors, GetEndGradient());

                ColorOverLifetimeModule var = GetGasParticle().colorOverLifetime;
                var.color = new MinMaxGradient(endGradient, startGradient);

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

            audioSource.volume = configuration.GetVolume();
            GetParticleSystem().transform.localScale = Vector3.one * configuration.GetParticleSize();
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
                SetJiggleForce(1 * configuration.GetJiggleMultiplier());

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

    public class FartEffectsConfiguration
    {
        public FartEffectsManager owner;
        public float volume = 1;
        public string startColors;
        public string endColors;
        public float particleSize;
        public float jiggleMultiplier = 1;

        public FartEffectsConfiguration()
        {
            SetDefaults();
        }

        public FartEffectsConfiguration(FartEffectsManager owner)
        {
            this.owner = owner;
            SetDefaults();
        }

        private void SetDefaults() 
        {
            volume = (float)Configuration.FartVolume.DefaultValue;
            startColors = (string)Configuration.FartParticleStartColors.DefaultValue;
            endColors = (string)Configuration.FartParticleEndColors.DefaultValue;
            jiggleMultiplier = (float)Configuration.JiggleIntensity.DefaultValue;
            particleSize = (float)Configuration.FartParticleSize.DefaultValue;
        }

        private bool IsPlayer() 
        {
            if (owner)
                return owner.owner == Player._mainPlayer;

            return false;
        }

        public float GetVolume() 
        {
            float volume = this.volume;
            if (IsPlayer())
                volume = Configuration.FartVolume.Value;

            return volume + (Configuration.GlobalFartVolume.Value - 1);
        }

        public float GetJiggleMultiplier()
        {
            if (IsPlayer())
                return Configuration.JiggleIntensity.Value;

            return jiggleMultiplier;
        }

        public List<Color> GetStartColors()
        {
            if (IsPlayer())
                return Configuration.GetStartColors();

            return Configuration.GetColors(startColors);
        }

        public List<Color> GetEndColors()
        {
            if (IsPlayer())
                return Configuration.GetEndColors();

            return Configuration.GetColors(endColors);
        }

        public float GetParticleSize()
        {
            if (IsPlayer())
                return Configuration.FartParticleSize.Value;

            return particleSize;
        }
    }
}
