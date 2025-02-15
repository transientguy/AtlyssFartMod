using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;
using static UnityEngine.ParticleSystem;
using FartMod.Core.GasCommandManagers;

namespace FartMod
{
    public class FartEffectsManager : GasEffectsManager
    {
        protected override List<AudioClip> GetAudioClips()
        {
            return FartModCore.instance.fartCommands.GetAudioClips();
        }

        protected override GasEffectsConfiguration GetGasEffectsConfiguration()
        {
            return new FartEffectsConfiguration(this);
        }

        protected override Vector3 EffectDirection(GasCharacterModel model)
        {
            return base.EffectDirection(model);
        }

        protected override Vector3 EffectPosition(GasCharacterModel model)
        {
            return base.EffectPosition(model);
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
            GasCharacterModel model = GetModel();

            if (!model)
                return;

            //Jiggle dynamic bones
            float randPower = Random.Range(0.0003f, 0.0006f);
            float forcePower = randPower * forceMultiplier;

            //Jiggle dat ass
            model.JiggleAss(forcePower);

            //Jiggle tail
            model.JiggleTail(forcePower);
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