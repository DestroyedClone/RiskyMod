﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace RiskyMod.Survivors.Bandit2
{
    public class BanditSpecialGracePeriod
    {
        public static bool enabled = true;
        public static float duration = 1.2f;
        public static GameObject resetEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect");
        public static GameObject skullEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2KillEffect");
        public BanditSpecialGracePeriod()
        {
            if (!enabled) return;
            //Remove Vanilla Effect
            IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdcI4(4),
                     x => x.MatchAnd(),
                     x => x.MatchLdcI4(4)
                    );
                c.EmitDelegate<Func<DamageType, DamageType>>((orig) =>
                {
                    return DamageType.Generic;
                });

                c.GotoNext(
                     x => x.MatchLdcI4(268435456),
                     x => x.MatchAnd(),
                     x => x.MatchLdcI4(268435456)
                    );
                c.EmitDelegate<Func<DamageType, DamageType>>((orig) =>
                {
                    return DamageType.Generic;
                });
            };
        }
    }
}
