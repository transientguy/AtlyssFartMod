using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace FartMod
{
    public class GasEffectsManager : MonoBehaviour
    {
        private static GameObject particleSystemPrefab;

        public Player owner;
        private AudioSource audioSource;

        private ParticleSystem particleSystem;
        private ParticleSystem gasParticle;

        private bool effectPlaying;
        private AssetBundle bundle;

        private float counter;

        protected GasEffectsConfiguration configuration = new GasEffectsConfiguration();

        //Network Audio
        //Network Particles

        protected virtual GasEffectsConfiguration GetGasEffectsConfiguration() 
        {
            return new GasEffectsConfiguration(this);
        }

        public void Initialize(AssetBundle bundle)
        {
            this.bundle = bundle;

            configuration = GetGasEffectsConfiguration();
            GetAudioClips();
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

        protected virtual List<AudioClip> GetAudioClips()
        {
            return new List<AudioClip>();
        }

        protected List<AudioClip> CollectAudioFilesFromPath(string audioPathName)
        {
            List<AudioClip> sounds = new List<AudioClip>();

            Log("Checking Sounds");

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), audioPathName);
            if (Directory.Exists(path))
            {
                sounds = CollectAudioFiles(path);
            }
            else
            {
                Log($"Directory {path} does not exist! Creating.");
                Directory.CreateDirectory(path);
            }

            return sounds;
        }

        private List<AudioClip> CollectAudioFiles(string path)
        {
            List<AudioClip> sounds = new List<AudioClip>();

            Log($"checking folder {Path.GetFileName(path)}");
            string[] audioFiles = Directory.GetFiles(path);
            foreach (string file in audioFiles)
            {
                Log($"\tchecking single file {Path.GetFileName(file)}");
                AudioClip clip = AssetUtils.LoadAudioFromWebRequest(file, AudioType.UNKNOWN);

                if (clip)
                    sounds.Add(clip);
            }

            return sounds;
        }

        private AudioSource GetAudioSource()
        {
            if (!audioSource)
                audioSource = FartController.AddAndGetComponent<AudioSource>(gameObject);

            if (audioSource)
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
            if (!GetAudioClips().Any()) 
            {
                Log("No clips found of either farts or burps!");
                return;
            }

            //Reset counter;
            counter = 0;

            //Grab random clip
            int randIndex = Random.Range(0, GetAudioClips().Count);
            AudioClip clip = GetAudioClips()[randIndex];

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

        protected virtual Vector3 EffectDirection(Player player)
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

        protected virtual Vector3 EffectPosition(Player player)
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

        protected Player GetPlayer()
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

        protected virtual void SetEffectEnabled(bool b)
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
            }
            else
            {
                GetAudioSource().Stop();
                SetEyeConditions(false);
            }

            SetParticleEnabled(b);
            effectPlaying = b;
        }

        public void SetTransform(Transform t)
        {
            Player player = GetPlayer();

            if (!player)
                return;

            t.forward = EffectDirection(player);
            t.position = EffectPosition(player) + t.forward * .75f;
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

        protected virtual void SetEyeConditions(bool effectEnabled)
        {
            Player player = GetPlayer();

            if (!player)
                return;

            if (effectEnabled)
            {
                //Expression
                //Set eye condition
                player._pVisual._playerRaceModel.Set_EyeCondition(EyeCondition.Closed, 1f);

                //Set eye condition
                player._pVisual._playerRaceModel.Set_MouthCondition(MouthCondition.Closed, 1f);
            }
            else 
            {
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

        private IEnumerator EyeConditionRoutine()
        {
            while (true)
            {
                SetEyeConditions(true);
                yield return new WaitForEndOfFrame();
            }
        }

        private void OnDisable()
        {
            SetEffectEnabled(false);
        }
    }

    public class GasEffectsConfiguration
    {
        public GasEffectsManager owner;
        public float volume = 1;
        public string startColors;
        public string endColors;
        public float particleSize;

        public GasEffectsConfiguration()
        {
            SetDefaults();
        }

        public GasEffectsConfiguration(GasEffectsManager owner)
        {
            this.owner = owner;
            SetDefaults();
        }

        protected virtual void SetDefaults()
        {
            volume = (float)Configuration.FartVolume.DefaultValue;
            startColors = (string)Configuration.FartParticleStartColors.DefaultValue;
            endColors = (string)Configuration.FartParticleEndColors.DefaultValue;
            particleSize = (float)Configuration.FartParticleSize.DefaultValue;
        }

        protected bool IsPlayer()
        {
            if (owner)
                return owner.owner == Player._mainPlayer;

            return false;
        }

        public virtual float GetVolume()
        {
            float volume = this.volume;
            if (IsPlayer())
                volume = Configuration.FartVolume.Value;

            return volume + (Configuration.GlobalFartVolume.Value - 1);
        }

        public virtual List<Color> GetStartColors()
        {
            if (IsPlayer())
                return Configuration.GetStartColors(Configuration.FartParticleStartColors);

            return Configuration.GetColors(startColors);
        }

        public virtual List<Color> GetEndColors()
        {
            if (IsPlayer())
                return Configuration.GetEndColors(Configuration.FartParticleEndColors);

            return Configuration.GetColors(endColors);
        }

        public virtual float GetParticleSize()
        {
            if (IsPlayer())
                return Configuration.FartParticleSize.Value;

            return particleSize;
        }
    }
}
