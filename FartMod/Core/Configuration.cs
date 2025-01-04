using BepInEx.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace FartMod
{
    internal static class Configuration
    {
        //Farting
        public static ConfigEntry<float> FartVolume = null;
        public static ConfigEntry<float> FartParticleSize = null;
        public static ConfigEntry<float> JiggleIntensity = null;

        public static ConfigEntry<string> FartParticleStartColors = null;
        public static ConfigEntry<string> FartParticleEndColors = null;

        public static ConfigEntry<float> GlobalFartVolume = null;
        public static ConfigEntry<bool> FartChaos = null;

        //Burping
        public static ConfigEntry<float> BurpVolume = null;
        public static ConfigEntry<float> BurpParticleSize = null;

        public static ConfigEntry<string> BurpParticleStartColors = null;
        public static ConfigEntry<string> BurpParticleEndColors = null;

        public static ConfigEntry<float> GlobalBurpVolume = null;
        public static ConfigEntry<bool> BurpChaos = null;

        internal static void BindConfiguration()
        {
            //Player burp effects
            FartVolume = FartModCore.GetConfig().Bind("Fart Effects", "FartVolume", 0.08f, "The volume of your character's farts");
            FartParticleSize = FartModCore.GetConfig().Bind("Fart Effects", "FartParticleSize", .075f, "The size of the fart particle effect.");
            JiggleIntensity = FartModCore.GetConfig().Bind("Fart Effects", "JiggleIntensity", 1f, "Multiplier for how intense your butt jiggles from farts.");

            string defaultStartColors = "CFFF4E, 77F131, 349300";
            FartParticleStartColors = FartModCore.GetConfig().Bind("Fart Effects", "FartParticleStartColors", defaultStartColors, "Start color of fart particles.");

            string defaultEndColors = "CFFF4E, DCFF40, 92B204";
            FartParticleEndColors = FartModCore.GetConfig().Bind("Fart Effects", "FartParticleEndColors", defaultEndColors, "End color of fart particles.");

            FartChaos = FartModCore.GetConfig().Bind("Global", "FartChaos", false, "Make ALL multiplayer characters fart when they chat.");
            GlobalFartVolume = FartModCore.GetConfig().Bind("Global", "GlobalFartVolume", 1f, "Volume of ALL multiplayer character farts.");



            //Player burp effects
            BurpVolume = FartModCore.GetConfig().Bind("Burp Effects", "BurpVolume", 0.08f, "The volume of your character's burps");
            BurpParticleSize = FartModCore.GetConfig().Bind("Burp Effects", "BurpParticleSize", .075f, "The size of the burp particle effect.");

            BurpParticleStartColors = FartModCore.GetConfig().Bind("Burp Effects", "BurpParticleStartColors", defaultStartColors, "Start color of burp particles.");

            BurpParticleEndColors = FartModCore.GetConfig().Bind("Burp Effects", "BurpParticleEndColors", defaultEndColors, "End color of burp particles.");

            BurpChaos = FartModCore.GetConfig().Bind("Global", "BurpChaos", false, "Make ALL multiplayer characters burp when they chat.");
            GlobalBurpVolume = FartModCore.GetConfig().Bind("Global", "GlobalBurpVolume", 1f, "Volume of ALL multiplayer character burp.");
        }

        public static List<Color> GetStartColors(ConfigEntry<string> config) 
        {
            return GetConfigColors(config);
        }

        public static List<Color> GetEndColors(ConfigEntry<string> config)
        {
            return GetConfigColors(config);
        }

        private static List<Color> GetConfigColors(ConfigEntry<string> config) 
        {
            return GetColors(config.Value);
        }

        public static List<Color> GetColors(string config)
        {
            config = config.Replace(" ", "");
            List<string> colorHexa = config.Split(',').ToList();
            List<Color> colors = new List<Color>();

            foreach (string str in colorHexa)
                colors.Add(FromHex(str));

            return colors;
        }

        public static Color FromHex(string hex)
        {
            if (hex.Length < 6)
            {
                Debug.LogError("Needs a string with a length of at least 6");
                return Color.green;
            }

            var r = hex.Substring(0, 2);
            var g = hex.Substring(2, 2);
            var b = hex.Substring(4, 2);
            string alpha;
            if (hex.Length >= 8)
                alpha = hex.Substring(6, 2);
            else
                alpha = "FF";

            return new Color((int.Parse(r, NumberStyles.HexNumber) / 255f),
                            (int.Parse(g, NumberStyles.HexNumber) / 255f),
                            (int.Parse(b, NumberStyles.HexNumber) / 255f),
                            (int.Parse(alpha, NumberStyles.HexNumber) / 255f));
        }
    }
}