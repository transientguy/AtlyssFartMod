﻿using BepInEx.Configuration;
using FartMod.GasControllers.Burps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FartMod.FartModCore;

namespace FartMod.Core.GasCommandManagers
{
    public class BurpCommandManager : GasCommandManager<BurpController>
    {
        protected override string GetGasVerb()
        {
            return "burp";
        }

        protected override GasController GetCharacterGasController(ChatBehaviour chatBehaviour)
        {
            Player owningPlayer = chatBehaviour.GetComponent<Player>();
            if (owningPlayer)
            {
                GasController controller = BurpController.allBurpControllers.Find(x => x.owner == owningPlayer);
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

        protected override ConfigEntry<bool> GetChaosConfig()
        {
            return Configuration.BurpChaos;
        }

        protected override ConfigEntry<float> GetVolumeConfig()
        {
            return Configuration.BurpVolume;
        }

        protected override ConfigEntry<float> GetGlobalVolumeConfig()
        {
            return Configuration.GlobalBurpVolume;
        }

        protected override ConfigEntry<float> GetParticleSizeConfig()
        {
            return Configuration.BurpParticleSize;
        }
    }
}
