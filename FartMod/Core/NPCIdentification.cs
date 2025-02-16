using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace FartMod
{
    public static class NPCIdentification
    {
        public static Dictionary<string, List<string>> NPCIDS = new Dictionary<string, List<string>>();

        private static void Log(string message)
        {
            FartModCore.Log(message);
        }

        public static void InitializeDictionary() 
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NPC/NPC IDs.txt");
            if (File.Exists(path))
            {
                NPCIDS = AssetUtils.GetParameterDictionaryFromFile(path);
            }
            else
            {
                Log($"File {path} does not exist!");
            }
        }

        public static string StripNetID(string str) 
        {
            string output = Regex.Replace(str, @" ?\[.*?\]", string.Empty);
            output = output.Trim();
            //Debug.LogError("Stripped net ID = " + output);
            return output;
        }

        public static GameObject GetNPC(string key) 
        {
            key = key.Trim();

            //Debug.LogError("Looking for " + key);

            if (NPCIDS.ContainsKey(key)) 
            {
                foreach (string str in NPCIDS[key]) 
                {
                    GameObject g = GameObject.Find(str);

                    if (g) 
                    {
                        //Debug.LogError("found " + key + " was " + str);
                        return g;
                    }
                }
            }

            return GameObject.Find(key);
        }
    }
}
