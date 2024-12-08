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

namespace FartMod
{
    [BepInPlugin("TransientGuy.Atlyss.FartMod", "FartMod", "1.2.3.4")]
    public class FartModCore : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

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

            //FartCommands.AddCommand("allanimations", "", AllAnims);

            Log("Fart Commands loaded");
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

        private void FartLoop(ChatBehaviour chatBehaviour) 
        {
            FartController controller = GetCharacterFartController(chatBehaviour);

            if (controller)
                controller.FartLoop();
        }

        private void FartOneshot(ChatBehaviour chatBehaviour)
        {
            FartController controller = GetCharacterFartController(chatBehaviour);

            if (controller)
                controller.FartOneshot();
        }

        private void FartLoopInfinite(ChatBehaviour chatBehaviour)
        {
            FartController controller = GetCharacterFartController(chatBehaviour);

            if (controller)
                controller.FartLoopInfinite();
        }

        private void StopFarting(ChatBehaviour chatBehaviour)
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
            public Action<ChatBehaviour> action;

            public ChatCommand(string commandKey, string commandMessage, Action<ChatBehaviour> action) 
            {
                this.commandKey = commandKey;
                this.action = action;
            }

            public void InvokeAction(ChatBehaviour chatBehaviour) 
            {
                action.Invoke(chatBehaviour);

                if(!string.IsNullOrEmpty(commandMessage))
                    chatBehaviour.New_ChatMessage("<color=#ea8e09> " + commandMessage + "</color>");
            }
        }

        public static class FartCommands
        {
            public static ChatBehaviour playerChat;
            public static List<ChatCommand> allChatCommands = new List<ChatCommand>();

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

            private static bool CheckCommandReceived(ChatBehaviour __instance, string _message) 
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
                            return false;
                        }
                    }
                }

                return true;
            }

            public static void AddCommand(string commandKey, string commandMessage, Action<ChatBehaviour> action) 
            {
                allChatCommands.Add(new ChatCommand(commandKey, commandMessage, action));
            }

            /*
            [HarmonyPatch(typeof(ChatBehaviour), "Send_ChatMessage")]
            public static class AddCommandsPatch
            {
                [HarmonyPrefix]
                public static bool Send_ChatMessage_Prefix(ChatBehaviour __instance, string _message)
                {
                    return CheckCommand(__instance, _message);
                }
            }
            */

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
                            CheckCommandReceived(chat, _message);
                        }
                    }
                }
            }
        }
    }
}
