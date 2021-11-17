﻿using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using System;

namespace RiskyMod.Items.Uncommon
{
    public class WillOWisp
    {
        public static bool enabled = true;
        public WillOWisp()
        {
            if (!enabled) return;
            HG.ArrayUtils.ArrayAppend(ref ItemsCore.changedItemDescs, RoR2Content.Items.ExplodeOnDeath);

            IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Items), "ExplodeOnDeath")
                    );

                //Disable Proc Coefficient
                if (RiskyMod.disableProcChains)
                {
                    c.GotoNext(
                        x => x.MatchStfld<DelayBlast>("position")
                        );
                    c.Index--;
                    c.EmitDelegate<Func<DelayBlast, DelayBlast>>((db) =>
                    {
                        db.procCoefficient = 0f;
                        return db;
                    });
                }

                //Disable Radius Scaling
                c.GotoNext(
                     x => x.MatchStfld<RoR2.DelayBlast>("radius")
                    );
                c.EmitDelegate<Func<float, float>>((oldRadius) =>
                {
                    return 16f;
                });

                //Disable falloff
                c.GotoNext(
                    x => x.MatchStfld<DelayBlast>("falloffModel")
                    );

                c.EmitDelegate<Func<BlastAttack.FalloffModel, BlastAttack.FalloffModel>>((model) =>
                {
                    return BlastAttack.FalloffModel.None;
                });
            };

            //LanguageAPI.Add("ITEM_EXPLODEONDEATH_DESC", "On killing an enemy, spawn a <style=cIsDamage>lava pillar</style> in a <style=cIsDamage>16m</style> radius for <style=cIsDamage>350%</style> <style=cStack>(+280% per stack)</style> base damage.");
        }
    }
}
