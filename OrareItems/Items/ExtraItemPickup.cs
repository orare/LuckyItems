using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace LuckyItems.Items
{
    public static class ExtraItemPickup
    {

        private static GameObject ExtraItemPickupPrefab;
        private static ItemIndex ExtraItemPickupItemIndex;
        private static float ItemProcChance = 50f;
        public static void Init()
        {
            ExtraItemPickupPrefab = LuckyItems.bundle.LoadAsset<GameObject>("Assets/Items/OnPickupItem/pickup_item.prefab");

            AddExtraItemPickupItem();
            AddLanguageTokens();

            On.RoR2.UI.NotificationQueue.OnItemPickup += NotificationQueue_OnItemPickup;
        }

        private static void AddExtraItemPickupItem()
        {
            var ExtraItemPickupItemDef = new ItemDef
            {
                name = "ExtraItemPickup",
                tier = ItemTier.Tier3,
                pickupModelPath = LuckyItems.ModPrefix + "Assets/Items/OnPickupItem/pickup_item.prefab",
                pickupIconPath = LuckyItems.ModPrefix + "Assets/Items/OnPickupItem/pickup_item_pic.png",
                nameToken = "EXTRAITEMPICKUP_NAME",
                pickupToken = "EXTRAITEMPICKUP_PICKUP",
                descriptionToken = "EXTRAITEMPICKUP_DESC",
                loreToken = "EXTRAITEMPICKUP_LORE",
                tags = new[]
                {
                    ItemTag.Utility,
                    ItemTag.AIBlacklist
                }
            };

            ItemDisplayRule[] itemDisplayRules = new ItemDisplayRule[0];

            var ExtraItemPickup = new R2API.CustomItem(ExtraItemPickupItemDef, itemDisplayRules);

            ExtraItemPickupItemIndex = ItemAPI.Add(ExtraItemPickup);
        }

        private static void AddLanguageTokens()
        {
            R2API.LanguageAPI.Add("EXTRAITEMPICKUP_NAME", "Lucky Default Cube");
            R2API.LanguageAPI.Add("EXTRAITEMPICKUP_PICKUP", "Chance on item pickup to increase the stack count of one of your items.");
            R2API.LanguageAPI.Add("EXTRAITEMPICKUP_DESC",
                $"On item pickup there is a 50% chance to increase the stack count of one of your items. <style=cStack>(Stacks increase the chance to give higher tiered items)</style>.");
            R2API.LanguageAPI.Add("EXTRAITEMPICKUP_LORE", "This item has has a dark, secret past.");
        }
        private static void NotificationQueue_OnItemPickup(On.RoR2.UI.NotificationQueue.orig_OnItemPickup orig, RoR2.UI.NotificationQueue self, CharacterMaster characterMaster, ItemIndex itemIndex)
        {
            orig(self, characterMaster, itemIndex);
            ItemTier itemTier = ItemTier.NoTier;
            if(characterMaster && characterMaster.inventory)
            {
                int itemCount = characterMaster.inventory.GetItemCount(ExtraItemPickupItemIndex);
                if(itemCount > 0 && Util.CheckRoll(ItemProcChance, characterMaster))
                {
                    itemTier = SelectItemTier(characterMaster.inventory);
                    if (itemTier == ItemTier.NoTier)
                        return;

                    List<ItemIndex> currentItems = characterMaster.inventory.itemAcquisitionOrder;
                    ItemIndex itemIndexSelection = SelectItemFromTier(currentItems, itemTier);
                    if (itemIndexSelection == ItemIndex.None)
                        return;
                    var item = ItemCatalog.GetItemDef(itemIndexSelection);

                    characterMaster.inventory.GiveItem(itemIndexSelection);

                    Chat.AddMessage($"+1 to: {Language.GetString(item.nameToken)}");
                }
            }
        }

        private static ItemTier SelectItemTier(Inventory inv)
        {
            int itemCount = inv.GetItemCount(ExtraItemPickupItemIndex);
            ItemTier itemTier = ItemTier.NoTier;
            var tier1Chance = 0.8f;
            var tier2Chance = 0.2f;
            var tier3Chance = 0.01f;
            if (itemCount > 0)
            {
                tier2Chance *= itemCount;
                tier3Chance *= Mathf.Pow(itemCount, 2f);
                WeightedSelection<ItemTier> weightedSelection = new WeightedSelection<ItemTier>(8);
                if (inv.GetTotalItemCountOfTier(ItemTier.Tier1) > 0)
                    weightedSelection.AddChoice(ItemTier.Tier1, tier1Chance);
                if (inv.GetTotalItemCountOfTier(ItemTier.Tier2) > 0)
                    weightedSelection.AddChoice(ItemTier.Tier2, tier2Chance);
                if (inv.GetTotalItemCountOfTier(ItemTier.Tier3) > 0)
                    weightedSelection.AddChoice(ItemTier.Tier3, tier3Chance);
                itemTier = weightedSelection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
            }
            return itemTier;
        }

        private static ItemIndex SelectItemFromTier(List<ItemIndex> itemIndexs, ItemTier itemTier)
        {
            List<ItemIndex> tieredItems = new List<ItemIndex>();
            var itemIndex = ItemIndex.None;
            foreach (var item in itemIndexs)
            {
                var itemDef = ItemCatalog.GetItemDef(item);
                if(itemDef.tier == itemTier)
                {
                    tieredItems.Add(item);
                }
            }
            if(tieredItems.Count > 0 )
            {
                Xoroshiro128Plus rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);

                itemIndex = rng.NextElementUniform(tieredItems);
            }


            return itemIndex;
        }

      
    }
}
