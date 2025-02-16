using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace FartMod
{
    public static class AssetUtils
    {
        private static void Log(string message)
        {
            FartModCore.Log(message);
        }

        private static bool IsInvalidPath(string path)
        {
            return path.EndsWith(".txt") || !path.Contains(".");
        }

        public static T GetWebRequest<T>(System.Func<UnityWebRequest> webRequest) where T : DownloadHandler
        {
            try
            {
                var www = webRequest();
                www.SendWebRequest();
                while (!www.isDone)
                {

                }

                if (www != null)
                {
                    DownloadHandler dh = www.downloadHandler;
                    if (dh != null && dh is T)
                        return (T)dh;
                }
                else
                {
                    Log("www is null " + www.url);
                }
            }
            catch
            {

            }

            return null;
        }

        public static string GetWebRequestPath(string path)
        {
            Log($"path: {path}");
            path = "file:///" + path.Replace("\\", "/");
            return path;
        }

        public static AssetBundle LoadAssetBundleFromWebRequest(string path)
        {
            if (IsInvalidPath(path))
                return null;

            path = GetWebRequestPath(path);
            DownloadHandlerAssetBundle dac = GetWebRequest<DownloadHandlerAssetBundle>(() => UnityWebRequestAssetBundle.GetAssetBundle(path));

            if (dac != null)
                return dac.assetBundle;

            return null;
        }

        public static AssetBundle LoadAssetBundle(string bundlePath)
        {
            string path = Path.Combine(BepInEx.Paths.PluginPath, bundlePath);

            if (!File.Exists(path))
                return null;

            return AssetBundle.LoadFromFile(path);
        }

        public static AudioClip LoadAudioFromWebRequest(string path, AudioType audioType)
        {
            if (IsInvalidPath(path))
                return null;

            path = GetWebRequestPath(path);
            DownloadHandlerAudioClip dac = GetWebRequest<DownloadHandlerAudioClip>(() => UnityWebRequestMultimedia.GetAudioClip(path, audioType));

            if (dac != null)
                return dac.audioClip;

            return null;
        }

        public static List<AudioClip> PreloadAudioClips(string folderName)
        {
            List<AudioClip> sounds = new List<AudioClip>();
            Log("Checking Sounds");

            string audioPathName = folderName;
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), audioPathName);
            if (Directory.Exists(path))
            {
                sounds.Clear();
                CollectAudioFiles(path);
            }
            else
            {
                Log($"Directory {path} does not exist! Creating.");
                Directory.CreateDirectory(path);
            }

            return sounds;
        }

        private static List<AudioClip> CollectAudioFiles(string path)
        {
            List<AudioClip> sounds = new List<AudioClip>();

            Log($"checking folder {Path.GetFileName(path)}");
            string[] audioFiles = Directory.GetFiles(path);
            foreach (string file in audioFiles)
            {
                Log($"\tchecking single file {Path.GetFileName(file)}");
                AudioClip clip = AssetUtils.LoadAudioFromWebRequest(file, AudioType.UNKNOWN);

                if (clip)
                    sounds.Add(clip);
            }

            return sounds;
        }

        public static List<string> GetAllLinesAtFile(string filePath) 
        {
            Log($"\tchecking single file {Path.GetFileName(filePath)}");
            return File.ReadAllLines(filePath).ToList();
        }

        public static Dictionary<string, List<string>> GetParameterDictionaryFromFile(string filePath)
        {
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
            List<string> lines = GetAllLinesAtFile(filePath);

            foreach(string str in lines) 
            {
                List<string> splitHeader = str.Split('=').ToList();

                if (splitHeader.Count >= 2) 
                {
                    string key = splitHeader[0].Trim();

                    if (!dictionary.ContainsKey(key))
                        dictionary.Add(key, GetParametersFromLine(splitHeader[1]));
                }
            }
            
            return dictionary;
        }

        public static List<string> GetParametersFromLine(string line)
        {
            string[] parameters = line.Split(',');
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = parameters[i].Trim();

            return parameters.ToList();
        }
    }
}
