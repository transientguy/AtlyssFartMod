using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace FartMod
{
    public static class GasCharacterModelTypes
    {
        public static List<GasCharacterModelType> characterModelTypes = new List<GasCharacterModelType>();
        public static UnityEvent onModelTypesUpdated = new UnityEvent();

        public static void GetCharacterModelTypes() 
        {
            characterModelTypes.Clear();



            onModelTypesUpdated.Invoke();
        }

        public static GasCharacterModelType GetModelType(Animator animator) 
        {
            return new GasCharacterModelType();
        }
    }

    public class GasCharacterModelType
    {
        public List<string> animators = new List<string>();
        public string headBone;
        public List<string> assBones = new List<string>();
    }
}
