using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FartMod.FartModCore;

namespace FartMod.Core.GasCommandManagers
{
    public class GasCommandManager<T> where T : GasController
    {
        private GasController originalFartController;
        public List<AudioClip> sounds = new List<AudioClip>();

        public virtual List<AudioClip> GetAudioClips()
        {
            return new List<AudioClip>();
        }

        protected List<AudioClip> CollectAudioFilesFromPath(string audioPathName)
        {
            List<AudioClip> sounds = new List<AudioClip>();

            Log("Checking Sounds");

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), audioPathName);
            if (Directory.Exists(path))
            {
                sounds = CollectAudioFiles(path);
            }
            else
            {
                Log($"Directory {path} does not exist! Creating.");
                Directory.CreateDirectory(path);
            }

            return sounds;
        }

        private List<AudioClip> CollectAudioFiles(string path)
        {
            List<AudioClip> sounds = new List<AudioClip>();

            Log($"checking folder {Path.GetFileName(path)}");
            string[] audioFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (string file in audioFiles)
            {
                Log($"\tchecking single file {Path.GetFileName(file)}");
                AudioClip clip = AssetUtils.LoadAudioFromWebRequest(file, AudioType.UNKNOWN);

                if (clip)
                    sounds.Add(clip);
            }

            return sounds;
        }

        protected virtual string GetGasVerb()
        {
            return "fart";
        }

        public virtual void Initialize()
        {
            string verb = GetGasVerb();

            FartCommands.AddCommand(verb, "", GasLoop);
            FartCommands.AddCommand(verb + "oneshot", "", GasOneshot);

            FartCommands.AddCommand(verb + "infinite", "", GasLoopInfinite);
            FartCommands.AddCommand("stop" + verb + "ing", "", StopGas);

            FartCommands.AddHostCommand(verb + "chaos", "", ToggleGasChaos);

            FartCommands.AddHostCommand(verb + "volume", "", SetGasVolume);
            FartCommands.AddHostCommand("global" + verb + "volume", "", SetGlobalGasVolume);
            FartCommands.AddHostCommand(verb + "size", "", SetGasParticleSize);

            //NPCs
            string npcStr = "npc";
            FartCommands.AddCommand(npcStr + verb, "", NPCGasLoopInfinite);
            FartCommands.AddCommand(npcStr + "stop" + verb + "ing", "", NPCStopGas);

            string targetStr = "target";
            FartCommands.AddCommand(targetStr + verb, "", TargetGasLoopInfinite);
            FartCommands.AddCommand(targetStr + "stop" + verb + "ing", "", TargetStopGas);

            GetOriginalGasController();
            GetAudioClips();
            Log(GetGasVerbUpper() + " Commands loaded");
        }

        protected virtual GasController GetCharacterGasController(Component owningObject)
        {
            if (owningObject)
            {
                GasController controller = FartController.allFartControllers.Find(x => x.CompareOwner(owningObject));
                if (!controller)
                {
                    controller = GameObject.Instantiate(GetOriginalGasController());
                    controller.gameObject.SetActive(true);
                    controller.SetOwner(owningObject, FartModCore.instance.GetAssetBundle());
                }

                return controller;
            }

            return null;
        }

        protected virtual ConfigEntry<bool> GetChaosConfig()
        {
            return Configuration.FartChaos;
        }

        protected virtual ConfigEntry<float> GetVolumeConfig()
        {
            return Configuration.FartVolume;
        }

        protected virtual ConfigEntry<float> GetGlobalVolumeConfig()
        {
            return Configuration.GlobalFartVolume;
        }

        protected virtual ConfigEntry<float> GetParticleSizeConfig()
        {
            return Configuration.FartParticleSize;
        }

        private void ToggleGasChaos(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            GetChaosConfig().Value = !GetChaosConfig().Value;
            string onMessage = GetChaosConfig().Value ? "on" : "off";
            Log(GetGasVerbUpper() + " chaos " + onMessage + "!");
        }

        private void SetGasVolume(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            GetVolumeConfig().Value = GetFloat(parameters, 0, GetVolumeConfig().Value, out bool b);

            if (b)
                Log("Set " + GetGasVerb() + " volume to " + GetVolumeConfig().Value + "!");
        }

        private void SetGlobalGasVolume(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            GetGlobalVolumeConfig().Value = GetFloat(parameters, 0, GetGlobalVolumeConfig().Value, out bool b);

            if (b)
                Log("Set global " + GetGasVerb() + " volume to " + GetGlobalVolumeConfig().Value + "!");
        }

        private void SetGasParticleSize(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            GetParticleSizeConfig().Value = GetFloat(parameters, 0, GetParticleSizeConfig().Value, out bool b);

            if (b)
                Log("Set " + GetGasVerb() + " particle size to " + GetParticleSizeConfig().Value + "!");
        }

        private string GetGasVerbUpper()
        {
            string str = GetGasVerb();

            if (str.Length == 1)
            {
                char.ToUpper(str[0]);
            }
            else
            {
                str = char.ToUpper(str[0]) + str.Substring(1);
            }

            return str;
        }

        protected float GetFloat(List<string> parameters, int index, float defaultValue, out bool success)
        {
            if (index >= parameters.Count)
            {
                Log("Not enough parameters given for command");
                success = false;
                return defaultValue;
            }

            if (float.TryParse(parameters[index], out float value))
            {
                success = true;
                return value;
            }
            else
            {
                Log("Given parameter " + parameters[index] + " is of incorrect type");
                success = false;
                return defaultValue;
            }
        }

        public void GasLoop(Component owningObject, List<string> parameters)
        {
            GasController controller = GetCharacterGasController(owningObject);

            if (controller)
                controller.FartLoop();
        }

        private void GasOneshot(Component owningObject, List<string> parameters)
        {
            GasController controller = GetCharacterGasController(owningObject);

            if (controller)
                controller.FartOneshot();
        }

        private void GasLoopInfinite(Component owningObject, List<string> parameters)
        {
            GasController controller = GetCharacterGasController(owningObject);

            if (controller)
                controller.FartLoopInfinite();
        }

        private void StopGas(Component owningObject, List<string> parameters)
        {
            GasController controller = GetCharacterGasController(owningObject);

            if (controller)
                controller.StopFarting();
        }

        private void NPCGasLoopInfinite(Component owningObject, List<string> parameters)
        {
            if (parameters.Any()) 
            {
                string NPCKey = parameters[0];
                GameObject NPC = NPCIdentification.GetNPC(NPCKey);

                if (NPC)
                    GasLoopInfinite(NPC.transform, parameters);
            }
        }

        private void NPCStopGas(Component owningObject, List<string> parameters)
        {
            if (parameters.Any())
            {
                string NPCKey = parameters[0];
                GameObject NPC = NPCIdentification.GetNPC(NPCKey);

                if (NPC)
                    NPCStopGas(NPC.transform, parameters);
            }
        }

        public StatusEntity GetTarget(Component owningObject)
        {
            Player player = owningObject.GetComponent<Player>();

            if (player)
                return player._pTargeting._foundEntity;

            return null;
        }

        private void TargetGasLoopInfinite(Component owningObject, List<string> parameters)
        {
            StatusEntity target = GetTarget(owningObject);

            if (target)
                GasLoopInfinite(target, parameters);
        }

        private void TargetStopGas(Component owningObject, List<string> parameters)
        {
            StatusEntity target = GetTarget(owningObject);

            if (target)
                StopGas(target, parameters);
        }

        protected GasController GetOriginalGasController()
        {
            if (!originalFartController)
            {
                GameObject g = new GameObject(GetGasVerbUpper() + "Controller");
                g.transform.SetParent(FartModCore.instance.transform);
                originalFartController = g.AddComponent<T>();
                originalFartController.Initialize(FartModCore.instance.GetAssetBundle());
                originalFartController.gameObject.SetActive(false);
            }

            return originalFartController;
        }
    }
}
