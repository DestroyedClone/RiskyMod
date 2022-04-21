﻿using RoR2;
using UnityEngine;

namespace RiskyMod.Tweaks.RunScaling
{
    public class MonsterGoldRewards
    {
        public static bool enabled = true;
		public static bool scaleToChests = true;
		public static bool scaleToInflation = true;
		private static int stageChestCost = 25;
		public MonsterGoldRewards()
        {
            if (!enabled) return;


			On.RoR2.Stage.Start += (orig, self) =>
			{
				stageChestCost = Run.instance.GetDifficultyScaledCost(25);
				orig(self);
			};


			On.RoR2.DeathRewards.OnKilledServer += (orig, self, damageReport) =>
			{
				if (Run.instance.gameModeIndex != RiskyMod.simulacrumIndex)
				{
					float chestRatio = scaleToChests ? stageChestCost / (float)Run.instance.GetDifficultyScaledCost(25) : 1f;
					float inflationRatio = scaleToInflation ? 1.4f / (1f + 0.4f * Run.instance.difficultyCoefficient) : 1f;	//Couldn't find actual code, but wiki claims Combat Director spawning crerdits gets multiplied by this.

					int goldRewardRaw = (int)Mathf.Max(Mathf.Round(self.goldReward * chestRatio * inflationRatio), 1f);
					self.goldReward = (uint)goldRewardRaw;
				}
				orig(self, damageReport);
			};
		}
    }
}
