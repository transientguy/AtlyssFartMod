using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;
using static UnityEngine.ParticleSystem;

namespace FartMod
{
    public class FartEffectsManager : GasEffectsManager
    {
        private static List<AudioClip> sounds = new List<AudioClip>();

        protected override List<AudioClip> GetAudioClips()
        {
            if (!sounds.Any())
                sounds = CollectAudioFilesFromPath("Audio");

            return sounds;
        }

        protected override GasEffectsConfiguration GetGasEffectsConfiguration()
        {
            return new FartEffectsConfiguration(this);
        }

        protected override Vector3 EffectDirection(Player player)
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

        protected override Vector3 EffectPosition(Player player)
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

        protected override void SetEffectEnabled(bool b) 
        {
            base.SetEffectEnabled(b);

            if (b)
                StartCoroutine(JiggleRoutine());

            SetJiggleForce(0);
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
                SetJiggleForce(1 * (configuration as FartEffectsConfiguration).GetJiggleMultiplier());

                yield return new WaitForEndOfFrame();

                SetJiggleForce(0);

                yield return new WaitForSeconds(.15f);
            }
        }
    }

    public class FartEffectsConfiguration : GasEffectsConfiguration
    {
        public float jiggleMultiplier = 1;

        public FartEffectsConfiguration(FartEffectsManager owner) : base(owner) 
        {

        }

        protected override void SetDefaults() 
        {
            base.SetDefaults();
            jiggleMultiplier = (float)Configuration.JiggleIntensity.DefaultValue;
        }

        public float GetJiggleMultiplier()
        {
            if (IsPlayer())
                return Configuration.JiggleIntensity.Value;

            return jiggleMultiplier;
        }
    }
}