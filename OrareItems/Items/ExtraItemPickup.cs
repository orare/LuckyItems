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

        public static void Init()
        {
            ExtraItemPickupPrefab = LuckyItems.bundle.LoadAsset<GameObject>("Assets/Items/OnPickupItem/pickup_item.prefab");

            AddExtraItemPickupItem();
            AddLanguageTokens();

            On.RoR2.UI.NotificationQueue.OnItemPickup += NotificationQueue_OnItemPickup;
        }

        private static void NotificationQueue_OnItemPickup(On.RoR2.UI.NotificationQueue.orig_OnItemPickup orig, RoR2.UI.NotificationQueue self, CharacterMaster characterMaster, ItemIndex itemIndex)
        {
            orig(self, characterMaster, itemIndex);
            Chat.AddMessage("ITEM PICKUP");
        }

        private static void AddExtraItemPickupItem()
        {
            var ExtraItemPickupItemDef = new ItemDef
            {
                name = "ExtraItemPickup",
                tier = ItemTier.Tier2,
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

            var ExtraShrinePickup = new R2API.CustomItem(ExtraItemPickupItemDef, itemDisplayRules);

            ExtraItemPickupItemIndex = ItemAPI.Add(ExtraShrinePickup); // ItemAPI sends back the ItemIndex of your itemf
        }


        private static void AddLanguageTokens()
        {
            R2API.LanguageAPI.Add("EXTRAITEMPICKUP_NAME", "Lucky Default Cube");
            R2API.LanguageAPI.Add("EXTRAITEMPICKUP_PICKUP", "Chance on item pickup to increase the stack count of one of your items.");
            R2API.LanguageAPI.Add("EXTRASHRINEROLL_DESC",
                $"NO DESC",
                "This item has has a dark, secret past.");
        }




    }
}
