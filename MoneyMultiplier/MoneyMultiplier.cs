using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using CharacterMaster = On.RoR2.CharacterMaster;

namespace MoneyMultiplier
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MoneyMultiplier : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Arch1t3ct";
        public const string PluginName = "MoneyMultiplier";
        public const string PluginVersion = "1.0.0";


        private static ConfigEntry<float> MultiplierConfig { get; set; }

        public static float Multiplier
        {
            get => MultiplierConfig.Value;
            protected set => MultiplierConfig.Value = value;
        }

        public void Awake()
        {
            MultiplierConfig = Config.Bind(
                "Game", "MoneyMultiplier", 2.0f, " Sets the Money Multiplier");
        }

        public void OnEnable()
        {
            On.RoR2.CharacterMaster.GiveMoney += CharacterMaster_GiveMoney;
        }

        public void OnDisable()
        {
            On.RoR2.CharacterMaster.GiveMoney -= CharacterMaster_GiveMoney;
        }

        private void CharacterMaster_GiveMoney(CharacterMaster.orig_GiveMoney orig, RoR2.CharacterMaster self,
            uint amount)
        {
            // Shouldn't technically need this network check, but I don't think it hurts
            if (NetworkServer.active)
            {
                amount = (uint)(amount * Multiplier);
            }

            orig(self, amount);
        }
        
        [ConCommand(commandName = "mod_mm_set_multiplier", flags = ConVarFlags.SenderMustBeServer,
            helpText = "Sets the Money Multiplier")]
        private static void SetMoneyMultiplier(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            if (int.TryParse(args[0], out var newMultiplier))
            {
                MultiplierConfig.Value = newMultiplier;
                Debug.Log($"Money Multiplier set to {MultiplierConfig.Value}");
                SendChatMessage();
            }
        }

        private static void SendChatMessage()
        {
            // Clients can't set the multiplier, so it doesn't matter really
            if (!NetworkServer.active) return;
            
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "Money Multiplier has been set to {0}",
                paramTokens = new[]
                {
                    MultiplierConfig.Value.ToString(CultureInfo.InvariantCulture)
                }
            });
        }
    }
}