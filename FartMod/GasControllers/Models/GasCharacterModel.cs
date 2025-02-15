using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod
{
    public class GasCharacterModel : MonoBehaviour
    {
        public GameObject owningObject;

        public virtual void Initialize(Component owningObject)
        {
            this.owningObject = owningObject.gameObject;
        }

        public bool IsValid()
        {
            return owningObject;
        }

        public virtual void OnDelete() 
        {

        }

        public virtual Animator GetAnimator() 
        {
            return null;
        }

        public virtual Animator GetRaceAnimator() 
        {
            return null;
        }

        public virtual Transform GetTransform()
        {
            return owningObject.transform;
        }

        public virtual Transform GetHeadTransform()
        {
            return owningObject.transform;
        }

        public virtual Vector3 AssDirection()
        {
            return -owningObject.transform.forward;
        }

        public virtual Vector3 AssPosition()
        {
            return owningObject.transform.position;
        }

        public virtual void SetEyeCondition(EyeCondition eyeCondition, float time)
        {

        }

        public virtual void SetMouthCondition(MouthCondition mouthCondition, float time)
        {

        }

        public virtual void JiggleAss(float forcePower)
        {

        }

        public virtual void JiggleTail(float forcePower) 
        {

        }

        public static GasCharacterModel GetModelFromComponent(Component owningObject, GameObject ownerGameObject) 
        {
            GasCharacterModel model;

            Player p;
            owningObject.gameObject.TryGetComponent(out p);
            if (p) 
            {
                model = ownerGameObject.AddComponent<GasPlayerCharacterModel>();
                model.Initialize(p);
                return model;
            }

            Animator anim = owningObject.GetComponentInChildren<Animator>();
            if (anim) 
            {
                model = ownerGameObject.AddComponent<SimpleAnimatorGasCharacterModel>();
                model.Initialize(anim);
                return model;
            }

            model = ownerGameObject.AddComponent<GasCharacterModel>();
            model.Initialize(owningObject);
            return model;
        }
    }
}
