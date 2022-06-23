﻿using R2API;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace RiskyMod.Items.Common
{
    public class CritGlasses
    {
        public static bool enabled = true;

        public CritGlasses()
        {
            if (!enabled) return;

            ItemsCore.ModifyItemDefActions += ModifyItem;

            //Remove Vanilla Effect
            IL.RoR2.CharacterBody.RecalculateStats += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if(c.TryGotoNext(
                     x => x.MatchLdfld<CharacterBody>("levelCrit")
                    )
                &&
                c.TryGotoNext(
                     x => x.MatchLdcR4(10f)
                    )
                )
                {
                    c.Next.Operand = 7f;
                }
                else
                {
                    UnityEngine.Debug.LogError("RiskyMod: CritGlasses IL Hook failed");
                }
            };
        }
        private static void ModifyItem()
        {
            HG.ArrayUtils.ArrayAppend(ref ItemsCore.changedItemDescs, RoR2Content.Items.CritGlasses);
            HG.ArrayUtils.ArrayAppend(ref ItemsCore.changedItemPickups, RoR2Content.Items.CritGlasses);
        }

    }
}
