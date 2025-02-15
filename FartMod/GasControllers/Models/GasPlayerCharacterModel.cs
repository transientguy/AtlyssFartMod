using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod
{
    public class GasPlayerCharacterModel : GasCharacterModel
    {
        public Player player;

        public override void Initialize(Component owningObject)
        {
            player = owningObject as Player;
        }

        public override void SetEyeCondition(EyeCondition eyeCondition, float time)
        {
            player._pVisual._playerRaceModel.Set_EyeCondition(eyeCondition, time);
        }

        public override void SetMouthCondition(MouthCondition mouthCondition, float time)
        {
            player._pVisual._playerRaceModel.Set_MouthCondition(mouthCondition, time);
        }

        public override Transform GetTransform()
        {
            return player.transform;
        }

        public override Animator GetAnimator()
        {
            return player._pVisual._visualAnimator;
        }

        public override Animator GetRaceAnimator()
        {
            return player._pVisual._playerRaceModel._raceAnimator;
        }

        public override Transform GetHeadTransform()
        {
            return player._pVisual._playerRaceModel._headBoneTransform;
        }

        public override void JiggleAss(float forcePower)
        {
            JiggleAssDynamicBones(player._pVisual._playerRaceModel._assDynamicBones, this, forcePower);
        }

        public static void JiggleAssDynamicBones(DynamicBone[] assBones, GasCharacterModel model, float forcePower)
        {
            for (int i = 0; i < assBones.Length; i++)
            {
                DynamicBone assBone = assBones[i];
                float multiplier = ((i + 1) % 2) == 0 ? 1 : -1;
                Vector3 force = model.GetTransform().right * multiplier * forcePower;
                assBone.m_Force = force;
            }
        }

        public override void JiggleTail(float forcePower)
        {
            List<DynamicBone> dynamicBones = new List<DynamicBone>(GetTransform().GetComponentsInChildren<DynamicBone>());
            DynamicBone tailBone = dynamicBones.Find(x => x.name.Contains("tail"));

            if (tailBone)
            {
                Vector3 force = GetTransform().up * forcePower;
                tailBone.m_Force = force;
            }
            else
            {
                //Log("Tailbone not found?");
            }
        }

        public override Vector3 AssDirection()
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

        public override Vector3 AssPosition()
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
    }
}
