﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskyMod.VoidLocus
{
    public class PillarsDropItems
    {
        public static bool enabled = true;
        private static Vector3 rewardPositionOffset = new Vector3(0f, 3f, 0f);

        public static float whiteChance = 50f;
        public static float greenChance = 40f;
        public static float redChance = 10f;

        public static float pearlOverwriteChance = 0f;
        public static float lunarOverwriteChance = 0f;
        public static float noOverwriteChance = 100f;

        public PillarsDropItems()
        {
            if (!enabled) return;

            On.RoR2.HoldoutZoneController.OnDisable += (orig, self) =>
            {
                orig(self);

                if (NetworkServer.active)
                {
                    SceneDef sd = RoR2.SceneCatalog.GetSceneDefForCurrentScene();
                    if (sd)
                    {
                        if (sd.baseSceneName.Equals("voidstage"))
                        {
                            HoldoutZoneController holdoutZone = self;
                            PickupIndex pickupIndex = SelectItem();
                            ItemTier tier = PickupCatalog.GetPickupDef(pickupIndex).itemTier;
                            if (pickupIndex != PickupIndex.none)
                            {
                                int participatingPlayerCount = Run.instance.participatingPlayerCount;
                                if (participatingPlayerCount != 0 && holdoutZone.transform)
                                {
                                    int num = participatingPlayerCount;

                                    bool itemShareActive = false;
                                    switch (tier)
                                    {
                                        case ItemTier.Tier1:
                                            if (RiskyMod.ShareSuiteCommon)
                                            {
                                                num = 1;
                                                itemShareActive = true;
                                            }
                                            break;
                                        case ItemTier.Tier2:
                                            if (RiskyMod.ShareSuiteUncommon)
                                            {
                                                num = 1;
                                                itemShareActive = true;
                                            }
                                            break;
                                        case ItemTier.Tier3:
                                            if (RiskyMod.ShareSuiteLegendary)
                                            {
                                                num = 1;
                                                itemShareActive = true;
                                            }
                                            break;
                                        default: break;
                                    }

                                    float angle = 360f / (float)num;
                                    Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);

                                    int k = 0;
                                    while (k < num)
                                    {
                                        bool overwritePickup = false;
                                        PickupIndex pickupOverwrite = PickupIndex.none;
                                        if (tier != ItemTier.Tier3) //Don't overwrite legendaries
                                        {
                                            bool replaceLunar = lunarOverwriteChance > 0f;
                                            bool replaceBoss = pearlOverwriteChance > 0f;
                                            if (itemShareActive)
                                            {
                                                replaceLunar = replaceLunar && RiskyMod.ShareSuiteLunar;
                                                replaceBoss = replaceBoss && RiskyMod.ShareSuiteBoss;
                                            }
                                            else
                                            {
                                                replaceLunar = replaceLunar && !RiskyMod.ShareSuiteLunar;
                                                replaceBoss = replaceBoss && !RiskyMod.ShareSuiteBoss;
                                            }

                                            if (replaceLunar || replaceBoss)
                                            {
                                                float pearlChance = (replaceBoss ? pearlOverwriteChance : 0f);
                                                float lunarChance = (replaceLunar ? lunarOverwriteChance : 0f);
                                                float total = noOverwriteChance + pearlChance + lunarChance;
                                                if (Run.instance.bossRewardRng.RangeFloat(0f, total) < pearlChance)
                                                {
                                                    pickupOverwrite = SelectPearl();
                                                }
                                                else
                                                {
                                                    total -= pearlOverwriteChance;
                                                    if (Run.instance.bossRewardRng.RangeFloat(0f, total) < lunarChance)
                                                    {
                                                        pickupOverwrite = SelectLunar();
                                                    }
                                                }

                                                if (pickupOverwrite == PickupIndex.none) overwritePickup = false;
                                            }
                                        }
                                        PickupDropletController.CreatePickupDroplet(overwritePickup ? pickupOverwrite : pickupIndex, holdoutZone.transform.position + rewardPositionOffset, vector);
                                        k++;
                                        vector = rotation * vector;
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static PickupIndex SelectPearl()
        {
            PickupIndex pearlIndex = PickupCatalog.FindPickupIndex(RoR2Content.Items.Pearl.itemIndex);
            PickupIndex shinyPearlIndex = PickupCatalog.FindPickupIndex(RoR2Content.Items.ShinyPearl.itemIndex);
            bool pearlAvailable = pearlIndex != PickupIndex.none && Run.instance.IsItemAvailable(RoR2Content.Items.Pearl.itemIndex);
            bool shinyPearlAvailable = shinyPearlIndex != PickupIndex.none && Run.instance.IsItemAvailable(RoR2Content.Items.ShinyPearl.itemIndex);

            PickupIndex toReturn = PickupIndex.none;
            if (pearlAvailable && shinyPearlAvailable)
            {
                toReturn = pearlIndex;
                if (Run.instance.bossRewardRng.RangeFloat(0f, 100f) <= 20f)
                {
                    toReturn = shinyPearlIndex;
                }
            }
            else
            {
                if (pearlAvailable)
                {
                    toReturn = pearlIndex;
                }
                else if (shinyPearlAvailable)
                {
                    toReturn = shinyPearlIndex;
                }
            }
            return toReturn;
        }

        private static PickupIndex SelectLunar()
        {
            List<PickupIndex> list = Run.instance.availableLunarCombinedDropList;
            Xoroshiro128Plus bossRewardRng = Run.instance.bossRewardRng;
            PickupIndex selectedPickup = bossRewardRng.NextElementUniform<PickupIndex>(list);
            return selectedPickup;
        }

        //Yellow Chance is handled after selecting item
        private static PickupIndex SelectItem()
        {
            List<PickupIndex> list;
            Xoroshiro128Plus bossRewardRng = Run.instance.bossRewardRng;
            PickupIndex selectedPickup = PickupIndex.none;

            float total = whiteChance + greenChance + redChance;

            if (bossRewardRng.RangeFloat(0f, total) <= whiteChance)//drop white
            {
                list = Run.instance.availableTier1DropList;
            }
            else
            {
                total -= whiteChance;
                if (bossRewardRng.RangeFloat(0f, total) <= greenChance)//drop green
                {
                    list = Run.instance.availableTier2DropList;
                }
                else
                {
                    total -= greenChance;
                    list = Run.instance.availableTier3DropList;
                }
            }
            if (list.Count > 0)
            {
                selectedPickup = bossRewardRng.NextElementUniform<PickupIndex>(list);
            }
            return selectedPickup;
        }
    }
}
