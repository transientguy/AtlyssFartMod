using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod.GasControllers.Burps
{
    public class BurpEffectsManager : GasEffectsManager
    {
        private static List<AudioClip> sounds = new List<AudioClip>();

        protected override GasEffectsConfiguration GetGasEffectsConfiguration()
        {
            return new BurpEffectsConfiguration(this);
        }

        protected override List<AudioClip> GetAudioClips()
        {
            if (!sounds.Any())
                sounds = CollectAudioFilesFromPath("Burp Audio");

            return sounds;
        }

        protected override Vector3 EffectDirection(Player player)
        {
            return player._pVisual._playerRaceModel._headBoneTransform.forward;
        }

        protected override Vector3 EffectPosition(Player player)
        {
            return player._pVisual._playerRaceModel._headBoneTransform.position;
        }

        protected override void SetEyeConditions(bool effectEnabled)
        {
            Player player = GetPlayer();

            if (!player)
                return;

            if (effectEnabled)
            {
                //Expression
                //Set mouth condition
                player._pVisual._playerRaceModel.Set_MouthCondition(MouthCondition.Open, 1f);
            }
            else
            {
                //Expression
                //Set mouth condition
                player._pVisual._playerRaceModel.Set_MouthCondition(MouthCondition.Closed, 1f);
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
