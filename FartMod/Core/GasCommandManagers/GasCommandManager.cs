﻿using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FartMod.FartModCore;

namespace FartMod.Core.GasCommandManagers
{
    public class GasCommandManager<T> where T : GasController
    {
        private GasController originalFartController;

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

            GetOriginalGasController();
            Log(GetGasVerbUpper() + " Commands loaded");
        }

        protected virtual GasController GetCharacterGasController(ChatBehaviour chatBehaviour)
        {
            Player owningPlayer = chatBehaviour.GetComponent<Player>();
            if (owningPlayer)
            {
                GasController controller = FartController.allFartControllers.Find(x => x.owner == owningPlayer);
                if (!controller)
                {
                    controller = GameObject.Instantiate(GetOriginalGasController());
                    controller.gameObject.SetActive(true);
                    controller.SetOwner(owningPlayer, FartModCore.instance.GetAssetBundle());
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

        public void GasLoop(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            GasController controller = GetCharacterGasController(chatBehaviour);

            if (controller)
                controller.FartLoop();
        }

        private void GasOneshot(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            GasController controller = GetCharacterGasController(chatBehaviour);

            if (controller)
                controller.FartOneshot();
        }

        private void GasLoopInfinite(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            GasController controller = GetCharacterGasController(chatBehaviour);

            if (controller)
                controller.FartLoopInfinite();
        }

        private void StopGas(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            GasController controller = GetCharacterGasController(chatBehaviour);

            if (controller)
                controller.StopFarting();
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
