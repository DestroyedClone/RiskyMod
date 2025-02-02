﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using R2API;
using RoR2.Orbs;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static RiskyMod.Content.Assets;

namespace RiskyMod.Items.Uncommon
{
    public class Infusion
    {
        public static bool enabled = true;
        public static BuffDef InfusionBuff;
        public Infusion()
        {
            if (!enabled) return;
            ItemsCore.ModifyItemDefActions += ModifyItem;

            //Remove vanilla effect
            IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if(c.TryGotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Items), "Infusion")
                    ))
                {
                    c.Remove();
                    c.Emit<RiskyMod>(OpCodes.Ldsfld, nameof(RiskyMod.emptyItemDef));
                }
                else
                {
                    UnityEngine.Debug.LogError("RiskyMod: Infusion OnCharacterDeath IL Hook failed");
                }
            };

            AssistManager.HandleAssistInventoryActions += OnKillEffect;

            InfusionBuff = SneedUtils.SneedUtils.CreateBuffDef(
                "RiskyMod_InfusionBuff",
                true,
                false,
                false,
                Color.white,
                BuffIcons.Infusion
                );

            IL.RoR2.CharacterBody.RecalculateStats += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                     x => x.MatchCallvirt<Inventory>("get_infusionBonus")
                    ))
                {
                    c.Emit(OpCodes.Ldarg_0);//self
                    c.EmitDelegate<Func<int, CharacterBody, int>>((infusionBonus, self) =>
                    {
                        float newHP = 0f;
                        float infusionCount = (float)infusionBonus;
                        int hundredsFulfilled = 0;
                        while (infusionCount > 0f)
                        {
                            float killRequirement = 100f + 150f * hundredsFulfilled;
                            if (infusionCount <= killRequirement)
                            {
                                newHP += 100f * infusionCount / killRequirement;
                                infusionCount = 0f;
                            }
                            else
                            {
                                infusionCount -= killRequirement;
                                newHP += 100f;
                                hundredsFulfilled++;
                            }
                        }
                        int hpGained = Mathf.FloorToInt(newHP);
                        if (NetworkServer.active)
                        {
                            int currentInfusionBuffCount = self.GetBuffCount(InfusionBuff.buffIndex);
                            if (self.inventory && self.inventory.GetItemCount(RoR2Content.Items.Infusion) > 0)
                            {
                                if (hpGained != currentInfusionBuffCount)
                                {
                                    if (hpGained > currentInfusionBuffCount)
                                    {
                                        for (int i = 0; i < hpGained - currentInfusionBuffCount; i++)
                                        {
                                            self.AddBuff(InfusionBuff);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < currentInfusionBuffCount - hpGained; i++)
                                        {
                                            self.RemoveBuff(InfusionBuff);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < currentInfusionBuffCount; i++)
                                {
                                    self.RemoveBuff(InfusionBuff);
                                }
                            }
                        }
                        return hpGained;
                    });
                }
                else
                {
                    UnityEngine.Debug.LogError("RiskyMod: Infusion IL Hook failed");
                }
            };
        }
        private static void ModifyItem()
        {
            HG.ArrayUtils.ArrayAppend(ref ItemsCore.changedItemPickups, RoR2Content.Items.Infusion);
            HG.ArrayUtils.ArrayAppend(ref ItemsCore.changedItemDescs, RoR2Content.Items.Infusion);
        }

        private void OnKillEffect(CharacterBody attackerBody, Inventory attackerInventory, CharacterBody victimBody, CharacterBody killerBody)
        {
            int itemCount = attackerInventory.GetItemCount(RoR2Content.Items.Infusion);
            if (itemCount > 0)
            {
                if (!victimBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
                {
                    InfusionOrb infusionOrb = new InfusionOrb();
                    infusionOrb.origin = victimBody.corePosition;
                    infusionOrb.target = Util.FindBodyMainHurtBox(attackerBody);
                    infusionOrb.maxHpValue = itemCount;
                    OrbManager.instance.AddOrb(infusionOrb);
                }
            }
        }
    }
}
