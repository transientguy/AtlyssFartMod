using System.IO;
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
    }
}
