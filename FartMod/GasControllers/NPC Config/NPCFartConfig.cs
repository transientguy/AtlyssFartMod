using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace FartMod
{
    public static class NPCFartConfig
    {
        public static List<NPCConfigEntry> configEntries = new List<NPCConfigEntry>();

        private static void Log(string message, bool force = false)
        {
            FartModCore.Log(message, force);
        }

        public static void LoadConfigEntries() 
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NPC/Config");
            if (Directory.Exists(path))
            {
                Log($"checking folder {Path.GetFileName(path)}");
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    NPCConfigEntry config = configEntries.Find(x => x.name == Path.GetFileName(file));

                    if (config == null) 
                    {
                        config = new NPCConfigEntry();
                        configEntries.Add(config);
                    }

                    config.LoadInfo(file);
                }

                //configEntries = configEntries.Where(x => files.Contains(x.name)).ToList();
            }
            else
            {
                Log($"Directory {path} does not exist! Creating.");
                Directory.CreateDirectory(path);
            }
        }

        public static NPCFartEffectsConfiguration GetFartConfiguration(SimpleAnimatorGasCharacterModel model, FartEffectsManager controller) 
        {
            NPCConfigEntry configEntry = configEntries.Find(x => x.IsMatch(model));
            
            if(configEntry != null)
                return configEntry.GetFartConfiguration(controller);

            return null;
        }

        public static NPCBurpEffectsConfiguration GetBurpConfiguration(SimpleAnimatorGasCharacterModel model, BurpEffectsManager controller)
        {
            NPCConfigEntry configEntry = configEntries.Find(x => x.IsMatch(model));

            if (configEntry != null)
                return configEntry.GetBurpConfiguration(controller);

            return null;
        }
    }

    public class NPCConfigEntry
    {
        public string name;
        public List<string> NPCNames = new List<string>();

        //Burp config
        public SubGasConfig burpConfig = new SubGasConfig();

        //Fart config
        public SubGasConfig fartConfig = new SubGasConfig();

        public bool IsMatch(SimpleAnimatorGasCharacterModel model) 
        {
            if (model.owningGameObject) 
            {
                string objectName = model.owningGameObject.name;

                if (NPCNames.Contains(objectName))
                    return true;

                string noNetIDObjectName = NPCIdentification.StripNetID(model.owningGameObject.name);

                if (NPCNames.Contains(noNetIDObjectName))
                    return true;

                foreach (string str in NPCNames) 
                {
                    if (NPCIdentification.NPCIDS.ContainsKey(str)) 
                    {
                        if (NPCIdentification.NPCIDS[str].Contains(objectName))
                            return true;

                        if (NPCIdentification.NPCIDS[str].Contains(noNetIDObjectName))
                            return true;
                    }
                }
            }

            return false;
        }

        public void LoadInfo(string file) 
        {
            name = Path.GetFileName(file);
            Dictionary<string, List<string>> data = AssetUtils.GetParameterDictionaryFromFile(file);

            NPCNames = GetStringListFromDictionary(data, "Names");

            burpConfig.volume = GetFloatFromDictionary(data, "BurpVolume", (float) Configuration.BurpVolume.DefaultValue);
            burpConfig.particleSize = GetFloatFromDictionary(data, "BurpParticleSize", (float)Configuration.BurpParticleSize.DefaultValue);
            burpConfig.startColors = GetColorsFromDictionary(data, "BurpParticleStartColors", (string)Configuration.BurpParticleStartColors.DefaultValue);
            burpConfig.endColors = GetColorsFromDictionary(data, "BurpParticleEndColors", (string)Configuration.BurpParticleEndColors.DefaultValue);

            fartConfig.volume = GetFloatFromDictionary(data, "FartVolume", (float)Configuration.FartVolume.DefaultValue);
            fartConfig.particleSize = GetFloatFromDictionary(data, "FartParticleSize", (float)Configuration.FartParticleSize.DefaultValue);
            fartConfig.jiggleIntensity = GetFloatFromDictionary(data, "JiggleIntensity", (float)Configuration.JiggleIntensity.DefaultValue);
            fartConfig.startColors = GetColorsFromDictionary(data, "FartParticleStartColors", (string)Configuration.FartParticleStartColors.DefaultValue);
            fartConfig.endColors = GetColorsFromDictionary(data, "FartParticleEndColors", (string)Configuration.FartParticleEndColors.DefaultValue);
        }

        private float GetFloatFromDictionary(Dictionary<string, List<string>> data, string key, float defaultVal) 
        {
            float value = defaultVal;

            if (data.ContainsKey(key)) 
            {
                if (data[key].Any())
                    float.TryParse(data[key][0], out value);
            }

            return value;
        }

        private List<Color> GetColorsFromDictionary(Dictionary<string, List<string>> data, string key, string defaultColorHexa)
        {
            if (data.ContainsKey(key))
            {
                List<string> hexas = data[key];
                return Configuration.GetColorsFromHexaList(hexas);
            }

            return Configuration.GetColors(defaultColorHexa);
        }

        private List<string> GetStringListFromDictionary(Dictionary<string, List<string>> data, string key)
        {
            if (data.ContainsKey(key))
                return data[key];

            return new List<string>();
        }

        public NPCFartEffectsConfiguration GetFartConfiguration(FartEffectsManager controller) 
        {
            NPCFartEffectsConfiguration fartEffect = new NPCFartEffectsConfiguration(controller);
            fartEffect.entry = this;
            return fartEffect;
        }

        public NPCBurpEffectsConfiguration GetBurpConfiguration(BurpEffectsManager controller)
        {
            NPCBurpEffectsConfiguration burpEffect = new NPCBurpEffectsConfiguration(controller);
            burpEffect.entry = this;
            return burpEffect;
        }
    }

    public class SubGasConfig 
    {
        public float volume;
        public List<Color> startColors = new List<Color>();
        public List<Color> endColors = new List<Color>();
        public float particleSize;
        public float jiggleIntensity;
    }

    public class NPCFartEffectsConfiguration : FartEffectsConfiguration
    {
        public NPCConfigEntry entry;

        public NPCFartEffectsConfiguration(FartEffectsManager owner) : base(owner)
        {

        }

        public override float GetVolume()
        {
            float volume = this.volume;
            if (entry != null)
                volume = entry.fartConfig.volume;

            return volume + (Configuration.GlobalFartVolume.Value - 1);
        }

        public override List<Color> GetStartColors()
        {
            if (entry != null)
                return entry.fartConfig.startColors;

            return Configuration.GetColors(startColors);
        }

        public override List<Color> GetEndColors()
        {
            if (entry != null)
                return entry.fartConfig.endColors;

            return Configuration.GetColors(endColors);
        }

        public override float GetParticleSize()
        {
            if (entry != null)
                return entry.fartConfig.particleSize;

            return particleSize;
        }

        public override float GetJiggleMultiplier()
        {
            if (entry != null)
                return entry.fartConfig.jiggleIntensity;

            return jiggleMultiplier;
        }
    }

    public class NPCBurpEffectsConfiguration : BurpEffectsConfiguration
    {
        public NPCConfigEntry entry;

        public NPCBurpEffectsConfiguration(BurpEffectsManager owner) : base(owner)
        {

        }

        public override float GetVolume()
        {
            float volume = this.volume;
            if (entry != null)
                volume = entry.burpConfig.volume;

            return volume + (Configuration.GlobalBurpVolume.Value - 1);
        }

        public override List<Color> GetStartColors()
        {
            if (entry != null)
                return entry.burpConfig.startColors;

            return Configuration.GetColors(startColors);
        }

        public override List<Color> GetEndColors()
        {
            if (entry != null)
                return entry.burpConfig.endColors;

            return Configuration.GetColors(endColors);
        }

        public override float GetParticleSize()
        {
            if (entry != null)
                return entry.burpConfig.particleSize;

            return particleSize;
        }
    }
}
