﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskyMod.Survivors.Croco
{
    public class RegenRework
    {
        public static BuffDef CrocoRegen2;
        public RegenRework()
        {
            CrocoRegen2 = ScriptableObject.CreateInstance<BuffDef>();
            CrocoRegen2.buffColor = RoR2Content.Buffs.CrocoRegen.buffColor;
            CrocoRegen2.canStack = true;
            CrocoRegen2.isDebuff = false;
            CrocoRegen2.name = "RiskyMod_CrocoRegen";
            CrocoRegen2.iconSprite = RoR2Content.Buffs.CrocoRegen.iconSprite;
            BuffAPI.Add(new CustomBuff(CrocoRegen2));

            IL.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "CrocoRegen")
                    );
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterBody, bool>>((hasBuff, self) =>
                {
                    return hasBuff || self.HasBuff(CrocoRegen2);
                });
            };

            SharedHooks.GetStatsCoefficient.HandleStatsActions += CrocoRegenStats;

            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                orig(self, damageInfo);
                if (NetworkServer.active && !damageInfo.rejected && self.body)
                {
                    if (self.body.HasBuff(CrocoRegen2.buffIndex))
                    {
                        self.body.ClearTimedBuffs(CrocoRegen2.buffIndex);
                    }
                }
            };

            ReplaceM1Regen();
            ReplaceBiteRegen();
        }

        private void ReplaceM1Regen()
        {
            IL.EntityStates.Croco.Slash.OnMeleeHitAuthority += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                        x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "CrocoRegen")
                       );
                c.Remove();
                c.Emit<RegenRework>(OpCodes.Ldsfld, nameof(CrocoRegen2));
                c.GotoNext(
                        x => x.MatchLdcR4(0.5f)
                       );
                c.Next.Operand = 1f;
            };
        }

        private void ReplaceBiteRegen()
        {
            IL.EntityStates.Croco.Bite.OnMeleeHitAuthority += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                        x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "CrocoRegen")
                       );
                c.Remove();
                c.Emit<RegenRework>(OpCodes.Ldsfld, nameof(CrocoRegen2));
                c.GotoNext(
                        x => x.MatchLdcR4(0.5f)
                       );
                c.Next.Operand = 1f;
            };
        }

        private static void CrocoRegenStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(CrocoRegen2.buffIndex))
            {
                args.baseRegenAdd += 0.1f * sender.healthComponent.fullHealth;
            }
        }
    }
}