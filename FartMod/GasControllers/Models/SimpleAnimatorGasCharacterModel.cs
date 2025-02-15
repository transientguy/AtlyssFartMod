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
        public Animator animator;
        public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        public GasCharacterModelType modelType;

        public Transform headTransform;

        public List<Transform> assBones = new List<Transform>();
        public List<DynamicBone> dynamicAssBones = new List<DynamicBone>();

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
            }
        }

        private void OnDestroy() 
        {
            GasCharacterModelTypes.onModelTypesUpdated.RemoveListener(OnModelsUpdated);
        }

        public override void Initialize(Component owningObject)
        {
            animator = owningObject as Animator;
            skinnedMeshRenderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            OnModelsUpdated();
        }

        public override Vector3 AssDirection()
        {
            return base.AssDirection();
        }

        public override Vector3 AssPosition()
        {
            return base.AssPosition();
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

        }

        public override void JiggleTail(float forcePower)
        {

        }

        public override void SetEyeCondition(EyeCondition eyeCondition, float time)
        {

        }

        public override void SetMouthCondition(MouthCondition mouthCondition, float time)
        {

        }
    }
}
