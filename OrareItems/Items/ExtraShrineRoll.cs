using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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

        private static float ExtraShrineRollChance = 15f;

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
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(ExtraShrineRollChance * inv.GetItemCount(ExtraShrineRollItemIndex), characterBody.master))
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
                            Chat.AddMessage("EXTRA ROLL FAIL");
                        }
                        else
                        {
                            Chat.AddMessage("EXTRA ROLL SUCCESS. GAINED: " + pickupIndex.ToString());
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
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(ExtraShrineRollChance * inv.GetItemCount(ExtraShrineRollItemIndex), characterBody.master))
                    {
                        var purchaseInteraction = self.GetFieldValue<PurchaseInteraction>("purchaseInteraction");
                        uint amount = (uint)(characterBody.healthComponent.fullCombinedHealth * (float)purchaseInteraction.cost / 100f * self.goldToPaidHpRatio);
                        if (characterBody.master)
                        {
                            characterBody.master.GiveMoney(amount);
                            Chat.AddMessage("EXTRA BLOOD SHRINE MONEY: " + amount.ToString());
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
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(ExtraShrineRollChance * inv.GetItemCount(ExtraShrineRollItemIndex), characterBody.master))
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/MonstersOnShrineUseEncounter"), self.transform.position, Quaternion.identity);
                        UnityEngine.Networking.NetworkServer.Spawn(gameObject);
                        CombatDirector combarDirector = gameObject.GetComponent<CombatDirector>();
                        float monsterCredit = 100f * Stage.instance.entryDifficultyCoefficient;
                        DirectorCard directorCard = combarDirector.SelectMonsterCardForCombatShrine(monsterCredit);
                        if (directorCard != null)
                        {
                            Chat.AddMessage("Spawned an additional set of combat shrine monsters.");
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
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(ExtraShrineRollChance * inv.GetItemCount(ExtraShrineRollItemIndex), characterBody.master))
                    {
                        if(TeleporterInteraction.instance)
                        {
                            TeleporterInteraction.instance.AddShrineStack();
                            Chat.AddMessage("Added Extra Boss Shrine");
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
                    if (inv.GetItemCount(ExtraShrineRollItemIndex) > 0 && Util.CheckRoll(ExtraShrineRollChance * inv.GetItemCount(ExtraShrineRollItemIndex), characterBody.master))
                    {
                        self.InvokeMethod("SetWardEnabled", true);
                        var healingWard = self.GetFieldValue<HealingWard>("healingWard");
                        float networkradius = self.baseRadius += self.radiusBonusPerPurchase;
                        healingWard.Networkradius = networkradius;
                        Chat.AddMessage("Healing Shrine Increased");

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

            ItemDisplayRule[] itemDisplayRules = new ItemDisplayRule[1]; // keep this null if you don't want the item to show up on the survivor 3d model. You can also have multiple rules !
            itemDisplayRules[0].followerPrefab = ExtraShrineRollPrefab; // the prefab that will show up on the survivor
            itemDisplayRules[0].childName = "Chest"; // this will define the starting point for the position of the 3d model, you can see what are the differents name available in the prefab model of the survivors
            itemDisplayRules[0].localScale = new Vector3(0.15f, 0.15f, 0.15f); // scale the model
            itemDisplayRules[0].localAngles = new Vector3(0f, 180f, 0f); // rotate the model
            itemDisplayRules[0].localPos = new Vector3(-0.35f, -0.1f, 0f); // position offset relative to the childName, here the survivor Chest

            var extraShrineRoll = new R2API.CustomItem(ExtraShrineRollItemDef, itemDisplayRules);

            ExtraShrineRollItemIndex = ItemAPI.Add(extraShrineRoll); // ItemAPI sends back the ItemIndex of your item
        }


        private static void AddLanguageTokens()
        {
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_NAME", "Two For One");
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_PICKUP", "Chance to gain an extra roll on shrine use.");
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_DESC",
                "Shrines have a <style=cIsUtility>15%</style> <style=cStack>(+15% per stack)</style>chance to roll a second time at no additonal cost.");
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_LORE",
                "deepest lore.");
        }
    }

}
