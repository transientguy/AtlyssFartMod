using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using HarmonyLib;
using System;
using Mirror;
using BepInEx.Configuration;

namespace FartMod
{
    [BepInPlugin("TransientGuy.Atlyss.FartMod", "FartMod", "1.2.3.4")]
    public class FartModCore : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static ConfigFile GetConfig() => instance.Config;

        public static FartModCore instance;
        private FartController originalFartController;
        private AssetBundle bundle;

        public static void Log(string message, bool forcePlay = false)
        {
            Logger.LogInfo(message);
        }

        private void Awake()
        {
            instance = this;
            Logger = base.Logger;

            Configuration.BindConfiguration();
            Harmony val = new Harmony("FartMod");
            try
            {
                val.PatchAll();
            }
            catch (Exception value)
            {
                Log(value.ToString());
                throw;
            }

            GetOriginalFartController();
            InitCommands();
            Log("Fart Mod Initialized!", true);
        }

        private void InitCommands()
        {
            FartCommands.AddCommand("fart", "Rippin ass!", FartLoop);
            FartCommands.AddCommand("fartoneshot", "Rippin ass!", FartOneshot);

            FartCommands.AddCommand("fartinfinite", "Rippin ass!", FartLoopInfinite);
            FartCommands.AddCommand("stopfarting", "", StopFarting);

            FartCommands.AddHostCommand("rebind", "", Rebind);

            FartCommands.AddHostCommand("fartchaos", "", ToggleFartChaos);

            FartCommands.AddHostCommand("fartvolume", "", SetFartVolume);
            FartCommands.AddHostCommand("globalfartvolume", "", SetGlobalFartVolume);
            FartCommands.AddHostCommand("fartsize", "", SetFarticleSize);
            FartCommands.AddHostCommand("fartjiggle", "", SetFartJiggle);

            Log("Fart Commands loaded");
        }

        private void Rebind(ChatBehaviour chatBehaviour, List<string> parameters) 
        {
            Log("Config file rebound!");
            GetConfig().Reload();
        }

        private void ToggleFartChaos(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            Configuration.FartChaos.Value = !Configuration.FartChaos.Value;
            string onMessage = Configuration.FartChaos.Value ? "on" : "off";
            Log("Fart chaos " + onMessage + "!");
        }

        private float GetFloat(List<string> parameters, int index, float defaultValue, out bool success) 
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

        private void SetFartVolume(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            Configuration.FartVolume.Value = GetFloat(parameters, 0, Configuration.FartVolume.Value, out bool b);

            if (b)
                Log("Set fart volume to " + Configuration.FartVolume.Value + "!");
        }

        private void SetGlobalFartVolume(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            Configuration.GlobalFartVolume.Value = GetFloat(parameters, 0, Configuration.FartVolume.Value, out bool b);

            if (b)
                Log("Set global fart volume to " + Configuration.GlobalFartVolume.Value + "!");
        }

        private void SetFarticleSize(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            Configuration.FartParticleSize.Value = GetFloat(parameters, 0, Configuration.FartVolume.Value, out bool b);

            if (b)
                Log("Set fart particle size to " + Configuration.FartParticleSize.Value + "!");
        }

        private void SetFartJiggle(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            Configuration.JiggleIntensity.Value = GetFloat(parameters, 0, Configuration.FartVolume.Value, out bool b);

            if (b)
                Log("Set fart jiggle intensity to " + Configuration.JiggleIntensity.Value + "!");
        }

        private void AllAnims(ChatBehaviour chatBehaviour)
        {
            Player player = Player._mainPlayer;
            if (player)
            {
                Animator animator = player._pVisual._playerRaceModel._raceAnimator;

                for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
                {
                    AnimationClip ac = animator.runtimeAnimatorController.animationClips[i];

                    if (ac)
                        Log(ac.name + " " + i);
                }
            }
        }

        private void FartLoop(ChatBehaviour chatBehaviour, List<string> parameters) 
        {
            FartController controller = GetCharacterFartController(chatBehaviour);

            if (controller)
                controller.FartLoop();
        }

        private void FartOneshot(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            FartController controller = GetCharacterFartController(chatBehaviour);

            if (controller)
                controller.FartOneshot();
        }

        private void FartLoopInfinite(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            FartController controller = GetCharacterFartController(chatBehaviour);

            if (controller)
                controller.FartLoopInfinite();
        }

        private void StopFarting(ChatBehaviour chatBehaviour, List<string> parameters)
        {
            FartController controller = GetCharacterFartController(chatBehaviour);

            if (controller)
                controller.StopFarting();
        }

        private FartController GetCharacterFartController(ChatBehaviour chatBehaviour) 
        {
            Player owningPlayer = chatBehaviour.GetComponent<Player>();
            if (owningPlayer) 
            {
                FartController controller = FartController.allFartControllers.Find(x => x.owner == owningPlayer);
                if (!controller) 
                {
                    controller = Instantiate(GetOriginalFartController());
                    controller.gameObject.SetActive(true);
                    controller.SetOwner(owningPlayer, GetAssetBundle());
                }

                return controller;
            }

            return null;
        }

        private FartController GetOriginalFartController()
        {
            if (!originalFartController)
            {
                GameObject g = new GameObject("FartController");
                g.transform.SetParent(transform);
                originalFartController = g.AddComponent<FartController>();
                originalFartController.Initialize(GetAssetBundle());
                originalFartController.gameObject.SetActive(false);
            }

            return originalFartController;
        }

        private AssetBundle GetAssetBundle()
        {
            if (!bundle) 
            {
                string modPath = Path.GetDirectoryName(Info.Location);
                bundle = AssetUtils.LoadAssetBundle(Path.Combine(modPath, "Assets/atlyss"));
            }

            return bundle;
        }

        public class ChatCommand
        {
            public string commandKey;
            public string commandMessage;
            public Action<ChatBehaviour, List<string>> action;

            public ChatCommand(string commandKey, string commandMessage, Action<ChatBehaviour, List<string>> action) 
            {
                this.commandKey = commandKey;
                this.action = action;
            }

            public void InvokeAction(ChatBehaviour chatBehaviour) 
            {
                InvokeAction(chatBehaviour, new List<string>());
            }

            public void InvokeAction(ChatBehaviour chatBehaviour, List<string> parameters)
            {
                action.Invoke(chatBehaviour, parameters);

                if (!string.IsNullOrEmpty(commandMessage))
                    chatBehaviour.New_ChatMessage("<color=#ea8e09> " + commandMessage + "</color>");
            }
        }

        public static class FartCommands
        {
            public static ChatBehaviour playerChat;
            public static List<ChatCommand> allChatCommands = new List<ChatCommand>();
            public static List<ChatCommand> hostChatCommands = new List<ChatCommand>();

            public static ChatBehaviour GetPlayerChat() 
            {
                if (!playerChat) 
                {
                    Player player = Player._mainPlayer;

                    if (player)
                        playerChat = player.GetComponentInChildren<ChatBehaviour>();
                }

                return playerChat;
            }

            public static void AddCommand(string commandKey, string commandMessage, Action<ChatBehaviour, List<string>> action) 
            {
                allChatCommands.Add(new ChatCommand(commandKey, commandMessage, action));
            }

            public static void AddHostCommand(string commandKey, string commandMessage, Action<ChatBehaviour, List<string>> action)
            {
                hostChatCommands.Add(new ChatCommand(commandKey, commandMessage, action));
            }

            [HarmonyPatch(typeof(ChatBehaviour), "Send_ChatMessage")]
            public static class AddCommandsPatch
            {
                [HarmonyPrefix]
                public static bool Send_ChatMessage_Prefix(ChatBehaviour __instance, string _message)
                {
                    if (CheckHostCommand(__instance, _message))
                        return false;

                    return true;
                }
            }

            private static bool CheckHostCommand(ChatBehaviour __instance, string _message)
            {
                if (_message == null)
                    return false;

                if (!_message.Any())
                    return false;

                if (_message[0] != '/')
                    return false;

                _message = _message.Substring(1);
                List<string> parameters = _message.Split(' ').ToList();
                ChatCommand command = hostChatCommands.Find(x => parameters[0] == x.commandKey);

                if (command != null) 
                {
                    parameters.RemoveAt(0);
                    parameters = parameters.Where(x => !string.IsNullOrEmpty(x)).ToList();
                    command.InvokeAction(__instance, parameters);
                    return true;
                }

                return false;
            }

            //Patch received message for now as i have no idea how to send commands to the server otherwise
            [HarmonyPatch(typeof(ChatBehaviour), "InvokeUserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel")]
            public static class RecieveChatMessagePatch
            {
                [HarmonyPostfix]
                public static void RecieveChatMessage_Prefix(ChatBehaviour __instance, NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
                {
                    //Runs when any message is received
                    if (obj && obj is ChatBehaviour) 
                    {
                        ChatBehaviour playerChat = GetPlayerChat();
                        if (playerChat && playerChat._chatMessages.Any()) 
                        {
                            string _message = playerChat._chatMessages[playerChat._chatMessages.Count - 1];
                            ChatBehaviour chat = obj as ChatBehaviour;

                            bool command = CheckRPCCommandReceived(chat, _message);

                            //For farting chaos fun!
                            if (Configuration.FartChaos.Value && !command && chat != playerChat)
                                instance.FartLoop(chat, new List<string>());
                        }
                    }
                }
            }

            private static bool CheckRPCCommandReceived(ChatBehaviour __instance, string _message)
            {
                //Check if any command is within message for performance
                bool containsCommand = false;
                foreach (ChatCommand command in allChatCommands)
                {
                    if (_message.Contains(command.commandKey))
                    {
                        containsCommand = true;
                        break;
                    }
                }

                //Return if no command contained
                if (!containsCommand)
                    return false;

                //Split message by /
                string[] splitStr = _message.Split('/');
                foreach (string str in splitStr)
                {
                    //remove color code special character
                    string commandToCheck = str.Replace("<", "");

                    foreach (ChatCommand command in allChatCommands)
                    {
                        if (commandToCheck == command.commandKey)
                        {
                            command.InvokeAction(__instance);
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
