﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;

namespace RiskyMod.Items.Uncommon
{
    public class LeechingSeed
    {
        public static bool enabled = true;
        public LeechingSeed()
        {
			if (!enabled) return;
			HG.ArrayUtils.ArrayAppend(ref ItemsCore.changedItemDescs, RoR2Content.Items.Seed);

			IL.RoR2.GlobalEventManager.OnHitEnemy += (il) =>
			{
				ILCursor c = new ILCursor(il);
				c.GotoNext(
					 x => x.MatchLdsfld(typeof(RoR2Content.Items), "Seed")
					);
				c.GotoNext(
					 x => x.MatchConvR4()
					);
				c.Index++;
				c.EmitDelegate<Func<float, float>>((heal) =>
				{
					return heal * 3f;
				});
			};

			//LanguageAPI.Add("ITEM_SEED_DESC", "Dealing damage <style=cIsHealing>heals</style> you for <style=cIsHealing>3 <style=cStack>(+3 per stack)</style> health</style>.");
		}
    }
}
