using FartMod.Core.GasCommandManagers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod
{
    public class BurpEffectsManager : GasEffectsManager
    {
        protected override GasEffectsConfiguration GetGasEffectsConfiguration()
        {
            if(model)
                return model.GetBurpEffectsConfiguration(this);

            return new BurpEffectsConfiguration(this);
        }

        protected override List<AudioClip> GetAudioClips()
        {
            return FartModCore.instance.burpCommands.GetAudioClips();
        }

        protected override Vector3 EffectDirection(GasCharacterModel model)
        {
            return model.GetHeadTransform().forward;
        }

        protected override Vector3 EffectPosition(GasCharacterModel model)
        {
            return model.GetHeadTransform().position;
        }

        protected override void SetEyeConditions(bool effectEnabled)
        {
            GasCharacterModel model = GetModel();

            if (!model)
                return;

            if (effectEnabled)
            {
                //Expression
                //Set mouth condition
                model.SetMouthCondition(MouthCondition.Open, 1f);
            }
            else
            {
                //Expression
                //Set mouth condition
                model.SetMouthCondition(MouthCondition.Closed, 1f);
            }
        }
    }

    public class BurpEffectsConfiguration : GasEffectsConfiguration
    {
        public BurpEffectsConfiguration(BurpEffectsManager owner) : base(owner)
        {

        }

        public override float GetVolume()
        {
            float volume = this.volume;
            if (IsPlayer())
                volume = Configuration.BurpVolume.Value;

            return volume + (Configuration.GlobalBurpVolume.Value - 1);
        }

        public override List<Color> GetStartColors()
        {
            if (IsPlayer())
                return Configuration.GetStartColors(Configuration.BurpParticleStartColors);

            return Configuration.GetColors(startColors);
        }

        public override List<Color> GetEndColors()
        {
            if (IsPlayer())
                return Configuration.GetEndColors(Configuration.BurpParticleEndColors);

            return Configuration.GetColors(endColors);
        }

        public override float GetParticleSize()
        {
            if (IsPlayer())
                return Configuration.BurpParticleSize.Value;

            return particleSize;
        }
    }
}
