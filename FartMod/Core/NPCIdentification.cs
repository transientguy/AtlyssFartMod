using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NPC");
            if (Directory.Exists(path))
            {
                GetAllLinesAtPath(path);
            }
            else
            {
                Log($"Directory {path} does not exist! Creating.");
                Directory.CreateDirectory(path);
            }
        }

        public static List<string> GetAllLinesAtPath(string path)
        {
            List<string> allLines = new List<string>();

            Log($"checking folder {Path.GetFileName(path)}");
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                Log($"\tchecking single file {Path.GetFileName(file)}");
                NPCIDS = AssetUtils.GetParameterDictionaryFromFile(file);
            }

            return allLines;
        }

        public static GameObject GetNPC(string key) 
        {
            if (NPCIDS.ContainsKey(key)) 
            {
                foreach (string str in NPCIDS[key]) 
                {
                    GameObject g = GameObject.Find(str);

                    if (g)
                        return g;
                }
            }

            return GameObject.Find(key);
        }
    }
}
