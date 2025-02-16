using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod
{
    public class SimpleAnimatorGasCharacterModel : GasCharacterModel
    {
        public GameObject owningGameObject;
        public Animator animator;
        public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        public GasCharacterModelType modelType;

        public Transform headTransform;

        public List<Transform> assBones = new List<Transform>();
        public List<DynamicBone> dynamicAssBones = new List<DynamicBone>();

        public override void Initialize(Component owningObject)
        {
            base.Initialize(owningObject);
            animator = owningObject as Animator;
            skinnedMeshRenderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            OnModelsUpdated();
        }

        public override bool CompareOwner(Component owningObject)
        {
            if (owningGameObject && owningObject)
                return owningGameObject == owningObject.gameObject;

            return base.CompareOwner(owningObject);
        }

        private void Awake()
        {
            GasCharacterModelTypes.onModelTypesUpdated.AddListener(OnModelsUpdated);
        }

        private void OnModelsUpdated() 
        {
            modelType = GasCharacterModelTypes.GetModelType(this);

            if (modelType != null) 
            {
                headTransform = modelType.GetHeadBone(this);
                assBones = modelType.GetAssBones(this);
                dynamicAssBones = assBones.Select(x => x.GetComponent<DynamicBone>()).ToList();
                dynamicAssBones = dynamicAssBones.Where(x => x).ToList();
            }
        }

        public override FartEffectsConfiguration GetFartEffectsConfiguration(FartEffectsManager controller)
        {
            //Check for NPC config first
            FartEffectsConfiguration config = NPCFartConfig.GetFartConfiguration(this, controller);
            if (config != null)
                return config;

            return base.GetFartEffectsConfiguration(controller);
        }

        public override BurpEffectsConfiguration GetBurpEffectsConfiguration(BurpEffectsManager controller)
        {
            //Check for NPC config first
            BurpEffectsConfiguration config = NPCFartConfig.GetBurpConfiguration(this, controller);
            if (config != null)
                return config;

            return base.GetBurpEffectsConfiguration(controller);
        }

        private void OnDestroy() 
        {
            GasCharacterModelTypes.onModelTypesUpdated.RemoveListener(OnModelsUpdated);
        }

        public override Vector3 AssDirection()
        {
            if(dynamicAssBones.Any())
                return GasPlayerCharacterModel.AssDirectionFromDynamicBones(dynamicAssBones, this);

            return GasPlayerCharacterModel.AssDirectionFromNormalBones(assBones, this);
        }

        public override Vector3 AssPosition()
        {
            if (dynamicAssBones.Any())
                return GasPlayerCharacterModel.AssPositionFromDynamicBones(dynamicAssBones, this);
            
            return GasPlayerCharacterModel.AssPositionFromNormalBones(assBones, this);
        }

        public override Animator GetAnimator()
        {
            return animator;
        }

        public override Transform GetHeadTransform()
        {
            return headTransform;
        }

        public override Animator GetRaceAnimator()
        {
            return animator;
        }

        public override void JiggleAss(float forcePower)
        {
            GasPlayerCharacterModel.JiggleAssDynamicBones(dynamicAssBones, this, forcePower);
        }

        public override void JiggleTail(float forcePower)
        {
            GasPlayerCharacterModel.JiggleTailDynamicBones(this, forcePower);
        }

        public override void SetEyeCondition(EyeCondition eyeCondition, float time)
        {

        }

        public override void SetMouthCondition(MouthCondition mouthCondition, float time)
        {

        }
    }
}
