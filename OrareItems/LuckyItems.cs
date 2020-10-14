﻿using System;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Reflection;
using R2API.AssetPlus;
using LuckyItems.Items;

namespace LuckyItems
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.bepis.r2api")]

    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(ItemDropAPI), nameof(ResourcesAPI), nameof(LanguageAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    public class LuckyItems : BaseUnityPlugin
    {
        const string ModVersion = "0.0.5";
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
        }
    }
}
