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
using FartMod.GasControllers.Burps;
using FartMod.Core.GasCommandManagers;

namespace FartMod
{
    [BepInPlugin("TransientGuy.Atlyss.FartMod", "FartMod", "1.2.3.4")]
    public class FartModCore : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static ConfigFile GetConfig() => instance.Config;

        public static FartModCore instance;
        public FartCommandManager fartCommands = new FartCommandManager();
        public BurpCommandManager burpCommands = new BurpCommandManager();
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

            GasCharacterModelTypes.GetCharacterModelTypes();
            fartCommands.Initialize();
            burpCommands.Initialize();
            InitCommands();

            Log("Fart Mod Initialized!", true);
        }

        private void InitCommands()
        {
            FartCommands.AddHostCommand("rebind", "", Rebind);
            FartCommands.AddHostCommand("allAnims", "", AllAnims);
            FartCommands.AddHostCommand("updatemodels", "", UpdateModels);
        }

        private void UpdateModels(ChatBehaviour chatBehaviour, List<string> parameters) 
        {
            GasCharacterModelTypes.GetCharacterModelTypes();
        }

        private void Rebind(ChatBehaviour chatBehaviour, List<string> parameters) 
        {
            Log("Config file rebound!");
            GetConfig().Reload();
        }

        private void AllAnims(ChatBehaviour chatBehaviour, List<string> parameters)
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

        public AssetBundle GetAssetBundle()
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
                            if (!command && chat != playerChat) 
                            {
                                bool all = false;

                                if(Configuration.FartChaos.Value || all)
                                    instance.fartCommands.GasLoop(chat, new List<string>());

                                if (Configuration.BurpChaos.Value || all)
                                    instance.burpCommands.GasLoop(chat, new List<string>());
                            }
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
