using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace LuckyItems.Items
{
    public static class ExtraShrineRoll
    {
        private static GameObject ExtraShrineRollPrefab;
        private static ItemIndex ExtraShrineRollItemIndex;

        public static void Init()
        {
            ExtraShrineRollPrefab = LuckyItems.bundle.LoadAsset<GameObject>("Assets/Items/ShrineItem/shrine_item.prefab");

            AddExtraShrineRollItem();
            AddLanguageTokens();

            On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
            On.RoR2.ShrineBloodBehavior.AddShrineStack += ShrineBloodBehavior_AddShrineStack;
            On.RoR2.ShrineCombatBehavior.AddShrineStack += ShrineCombatBehavior_AddShrineStack;
            On.RoR2.ShrineBossBehavior.AddShrineStack += ShrineBossBehavior_AddShrineStack;
            On.RoR2.ShrineHealingBehavior.AddShrineStack += ShrineHealingBehavior_AddShrineStack;

        }


        //Chance Shrine
        private static void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
        {
            orig(self, activator);
            var characterBody = activator.GetComponent<CharacterBody>();
            if(characterBody)
            {
                var inv = characterBody.inventory;
                if(inv)
                {
                    Chat.AddMessage($"{LuckyItems.configInitialStackChance.Value + LuckyItems.configAdditionalStackChance.Value * (inv.GetItemCount(ExtraShrineRollItemIndex) - 1)}");
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(LuckyItems.configInitialStackChance.Value + LuckyItems.configAdditionalStackChance.Value * (inv.GetItemCount(ExtraShrineRollItemIndex) - 1), characterBody.master))
                    {
                        Xoroshiro128Plus rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
                        PickupIndex none = PickupIndex.none;
                        PickupIndex value = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
                        PickupIndex value2 = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
                        PickupIndex value3 = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
                        PickupIndex value4 = rng.NextElementUniform<PickupIndex>(Run.instance.availableEquipmentDropList);
                        WeightedSelection<PickupIndex> weightedSelection = new WeightedSelection<PickupIndex>(8);
                        weightedSelection.AddChoice(none, self.failureWeight);
                        weightedSelection.AddChoice(value, self.tier1Weight);
                        weightedSelection.AddChoice(value2, self.tier2Weight);
                        weightedSelection.AddChoice(value3, self.tier3Weight);
                        weightedSelection.AddChoice(value4, self.equipmentWeight);
                        PickupIndex pickupIndex = weightedSelection.Evaluate(rng.nextNormalizedFloat);
                        bool flag = pickupIndex == PickupIndex.none;
                        if (flag)
                        {
                            Chat.AddMessage("Lucky Default Sphere has rolled the shrine an additional time for: Nothing.");
                        }
                        else
                        {
                            Chat.AddMessage($"Lucky Default Sphere has rolled the shrine an additional time for: {ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickupIndex).itemIndex).name}");
                            PickupDropletController.CreatePickupDroplet(pickupIndex, self.dropletOrigin.position, self.dropletOrigin.forward * 20f);
                        }
                    }
                }
            }
        }

        //Blood Shrine
        private static void ShrineBloodBehavior_AddShrineStack(On.RoR2.ShrineBloodBehavior.orig_AddShrineStack orig, ShrineBloodBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            var characterBody = interactor.GetComponent<CharacterBody>();
            if (characterBody)
            {
                var inv = characterBody.inventory;
                if (inv)
                {
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(LuckyItems.configInitialStackChance.Value + LuckyItems.configAdditionalStackChance.Value * (inv.GetItemCount(ExtraShrineRollItemIndex) - 1), characterBody.master))
                    {
                        var purchaseInteraction = self.GetFieldValue<PurchaseInteraction>("purchaseInteraction");
                        uint amount = (uint)(characterBody.healthComponent.fullCombinedHealth * (float)purchaseInteraction.cost / 100f * self.goldToPaidHpRatio);
                        if (characterBody.master)
                        {
                            characterBody.master.GiveMoney(amount);
                            Chat.AddMessage($"Lucky Default Sphere has given you an additonal {amount} gold.");
                        }
                    }
                }
            }
        }

        //Combat Shrine
        private static void ShrineCombatBehavior_AddShrineStack(On.RoR2.ShrineCombatBehavior.orig_AddShrineStack orig, ShrineCombatBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            var characterBody = interactor.GetComponent<CharacterBody>();
            if (characterBody)
            {
                var inv = characterBody.inventory;
                if (inv)
                {
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(LuckyItems.configInitialStackChance.Value + LuckyItems.configAdditionalStackChance.Value * (inv.GetItemCount(ExtraShrineRollItemIndex) - 1), characterBody.master))
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/MonstersOnShrineUseEncounter"), self.transform.position, Quaternion.identity);
                        UnityEngine.Networking.NetworkServer.Spawn(gameObject);
                        CombatDirector combarDirector = gameObject.GetComponent<CombatDirector>();
                        float monsterCredit = 100f * Stage.instance.entryDifficultyCoefficient;
                        DirectorCard directorCard = combarDirector.SelectMonsterCardForCombatShrine(monsterCredit);
                        if (directorCard != null)
                        {
                            Chat.AddMessage("Lucky Default Sphere has spawned an additional set of combat shrine monsters.");
                            combarDirector.CombatShrineActivation(interactor, monsterCredit, directorCard);
                            return;
                        }
                        UnityEngine.Networking.NetworkServer.Destroy(gameObject);
                    }
                }
            }
        }
        //Mountain Shrine
        private static void ShrineBossBehavior_AddShrineStack(On.RoR2.ShrineBossBehavior.orig_AddShrineStack orig, ShrineBossBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            var characterBody = interactor.GetComponent<CharacterBody>();
            if (characterBody)
            {
                var inv = characterBody.inventory;
                if (inv)
                {
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(LuckyItems.configInitialStackChance.Value + LuckyItems.configAdditionalStackChance.Value * (inv.GetItemCount(ExtraShrineRollItemIndex) - 1), characterBody.master))
                    {
                        if(TeleporterInteraction.instance)
                        {
                            TeleporterInteraction.instance.AddShrineStack();
                            Chat.AddMessage("Lucky Default Sphere has added an extra teleporter boss.");
                        }
                    }
                }
            }
        }

        //Shrine of the Woods
        private static void ShrineHealingBehavior_AddShrineStack(On.RoR2.ShrineHealingBehavior.orig_AddShrineStack orig, ShrineHealingBehavior self, Interactor activator)
        {
            orig(self, activator);
            var characterBody = activator.GetComponent<CharacterBody>();
            if (characterBody)
            {
                var inv = characterBody.inventory;
                if (inv)
                {
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(LuckyItems.configInitialStackChance.Value + LuckyItems.configAdditionalStackChance.Value * (inv.GetItemCount(ExtraShrineRollItemIndex) - 1), characterBody.master))
                    {
                        self.InvokeMethod("SetWardEnabled", true);
                        var healingWard = self.GetFieldValue<HealingWard>("healingWard");
                        float networkradius = self.baseRadius += self.radiusBonusPerPurchase;
                        healingWard.Networkradius = networkradius;
                        Chat.AddMessage("Lucky Default Sphere has increased the healing shrine range an additonal time.");

                    }
                }
            }
        }

        private static void AddExtraShrineRollItem()
        {
            var ExtraShrineRollItemDef = new ItemDef
            {
                name = "ExtraShrineRoll", // its the internal name, no spaces, apostrophes and stuff like that
                tier = ItemTier.Tier2,
                pickupModelPath = LuckyItems.ModPrefix + "Assets/Items/ShrineItem/shrine_item.prefab",
                pickupIconPath = LuckyItems.ModPrefix + "Assets/Items/ShrineItem/shrine_item_pic.png",
                nameToken = "EXTRASHRINEROLL_NAME", // stylised name
                pickupToken = "EXTRASHRINEROLL_PICKUP",
                descriptionToken = "EXTRASHRINEROLL_DESC",
                loreToken = "EXTRASHRINEROLL_LORE",
                tags = new[]
                {
                    ItemTag.Utility
                }
            };

            ItemDisplayRule[] itemDisplayRules = new ItemDisplayRule[0];

            var extraShrineRoll = new R2API.CustomItem(ExtraShrineRollItemDef, itemDisplayRules);

            ExtraShrineRollItemIndex = ItemAPI.Add(extraShrineRoll); // ItemAPI sends back the ItemIndex of your item
        }


        private static void AddLanguageTokens()
        {
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_NAME", "Lucky Default Sphere");
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_PICKUP", "Chance to gain an extra roll on shrine use.");
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_DESC",
                $"Shrines have a <style=cIsUtility>{LuckyItems.configInitialStackChance.Value}%</style> <style=cStack>(+{LuckyItems.configAdditionalStackChance.Value}% per stack)</style>chance to roll a second time at no additonal cost.");
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_LORE",
                "This item has has a dark, secret past.");
        }

        public static void AddItemStatsModDef()
        {

            ItemStatDef statDef = new ItemStatDef
            {
                Stats = new List<ItemStat>
                {
                    new ItemStat(
                    (itemCount, ctx) => LuckyItems.configInitialStackChance.Value + (itemCount-1) * LuckyItems.configAdditionalStackChance.Value,
                    (value, ctx) => $"Chance Proc: {value.FormatPercentage(maxValue:1f)}"
                    )
                }
            };
            ItemStatsMod.AddCustomItemStatDef(ExtraShrineRollItemIndex, statDef);


        }

    }

}
