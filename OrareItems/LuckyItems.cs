using System;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Reflection;
using LuckyItems.Items;

namespace LuckyItems
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.bepis.r2api")]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(ItemDropAPI), nameof(ResourcesAPI), nameof(LanguageAPI), nameof(InventoryAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    public class LuckyItems : BaseUnityPlugin
    {
        #if DEBUG
            const string ModVersion = "1.0.0.2";
        #else
            const string ModVersion = "1.0.0";
        #endif
        const string ModName = "Lucky Items";
        const string ModGuid = "mod.orare.luckyitems";

        public static AssetBundle bundle;
        public static string ModPrefix = "@LuckyItems:";


        public void Awake()
        {

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LuckyItems.itemassets"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider(ModPrefix.Trim(':'), bundle);
                ResourcesAPI.AddProvider(provider);
            }

            ExtraShrineRoll.Init();
            ExtraItemPickup.Init();
        }
    }
}
