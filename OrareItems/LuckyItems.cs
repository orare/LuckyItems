using System;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Reflection;
using R2API.AssetPlus;
using LuckyItems.Items;
using BepInEx.Configuration;

namespace LuckyItems
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("dev.ontrigger.itemstats", BepInDependency.DependencyFlags.SoftDependency)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(ItemDropAPI), nameof(ResourcesAPI), nameof(LanguageAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    public class LuckyItems : BaseUnityPlugin
    {
        const string ModVersion = "0.0.6";
        const string ModName = "Lucky Items";
        const string ModGuid = "mod.orare.luckyitems";

        public static AssetBundle bundle;
        public static string ModPrefix = "@LuckyItems:";

        public static ConfigEntry<float> configInitialStackChance;
        public static ConfigEntry<float> configAdditionalStackChance;

        public void Awake()
        {
            configInitialStackChance = Config.Bind("General Settings", "Initial Chance", 15f, "The chance % the inital item gives.");
            configAdditionalStackChance = Config.Bind("General Settings", "Additional Chance", 15f, "The amount of % chance subsequent stacks give.");

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LuckyItems.itemassets"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider(ModPrefix.Trim(':'), bundle);
                ResourcesAPI.AddProvider(provider);
            }

            ExtraShrineRoll.Init();
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dev.ontrigger.itemstats"))
                ExtraShrineRoll.AddItemStatsModDef();
        }
    }
}
