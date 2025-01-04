using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FartMod.FartModCore;

namespace FartMod.Core.GasCommandManagers
{
    public class FartCommandManager : GasCommandManager<FartController>
    {
        public override void Initialize()
        {
            FartCommands.AddHostCommand(GetGasVerb() + "jiggle", "", SetFartJiggle);
            base.Initialize();
        }

        private void SetFartJiggle(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            Configuration.JiggleIntensity.Value = GetFloat(parameters, 0, Configuration.FartVolume.Value, out bool b);

            if (b)
                Log("Set fart jiggle intensity to " + Configuration.JiggleIntensity.Value + "!");
        }
    }
}
