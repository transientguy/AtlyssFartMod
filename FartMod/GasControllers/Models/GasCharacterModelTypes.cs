using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Models");
            if (Directory.Exists(path))
            {
                Log($"checking folder {Path.GetFileName(path)}");
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    BoneStructureGasCharacterModelType newModelType = new BoneStructureGasCharacterModelType();
                    newModelType.SetInfo(file);
                    characterModelTypes.Add(newModelType);
                }
            }
            else
            {
                Log($"Directory {path} does not exist! Creating.");
                Directory.CreateDirectory(path);
            }

            onModelTypesUpdated.Invoke();
        }

        private static void Log(string message)
        {
            FartModCore.Log(message);
        }

        public static GasCharacterModelType GetModelType(SimpleAnimatorGasCharacterModel model) 
        {
            return characterModelTypes.Find(x => x.IsMatch(model));
        }
    }

    public class BoneStructureGasCharacterModelType : GasCharacterModelType
    {
        public List<string> boneStructure = new List<string>();

        public void SetInfo(string file) 
        {
            name = Path.GetFileName(file);
            Dictionary<string, List<string>> data = AssetUtils.GetParameterDictionaryFromFile(file);

            string bonesKey = "Bones";
            if (data.ContainsKey(bonesKey)) 
            {
                boneStructure = data[bonesKey];
            }

            string headBoneKey = "HeadBone";
            if (data.ContainsKey(headBoneKey))
            {
                if (data[headBoneKey].Any())
                    headBone = data[headBoneKey][0];
            }

            string assBonesKey = "AssBones";
            if (data.ContainsKey(assBonesKey))
            {
                assBones = data[assBonesKey];
            }
        }

        public override bool IsMatch(SimpleAnimatorGasCharacterModel model)
        {
            if (model.skinnedMeshRenderers.Any()) 
            {
                SkinnedMeshRenderer smr = model.skinnedMeshRenderers[0];

                for (int i = 0; i < boneStructure.Count; i++)
                {
                    if (i < smr.bones.Length)
                    {
                        if (boneStructure[i] != smr.bones[i].name)
                            return false;
                    }
                    else 
                    {
                        return false;
                    }
                }
                
                return true;
            }

            return false;
        }

        public override Transform GetHeadBone(SimpleAnimatorGasCharacterModel model)
        {
            if (model.skinnedMeshRenderers.Any()) 
            {
                return Array.Find(model.skinnedMeshRenderers[0].bones, x => x.name == headBone);
            }

            return base.GetHeadBone(model);
        }

        public override List<Transform> GetAssBones(SimpleAnimatorGasCharacterModel model)
        {
            if (model.skinnedMeshRenderers.Any())
            {
                return Array.FindAll(model.skinnedMeshRenderers[0].bones, x => assBones.Contains(x.name)).ToList();
            }

            return new List<Transform>();
        }
    }

    public class GasCharacterModelType
    {
        public string name;
        public string headBone;
        public List<string> assBones = new List<string>();

        public GasCharacterModelType() 
        {

        }

        public virtual bool IsMatch(SimpleAnimatorGasCharacterModel model)
        {
            return false;
        }

        public virtual Transform GetHeadBone(SimpleAnimatorGasCharacterModel model) 
        {
            return model.GetTransform();
        }

        public virtual List<Transform> GetAssBones(SimpleAnimatorGasCharacterModel model)
        {
            return new List<Transform>();
        }
    }
}
