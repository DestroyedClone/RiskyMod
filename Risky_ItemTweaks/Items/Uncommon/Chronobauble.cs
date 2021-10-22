﻿using R2API;
using RoR2;

namespace Risky_ItemTweaks.Items.Uncommon
{
    public class Chronobauble
    {
        public static bool enabled = true;
        public static void Modify()
        {
            if (!enabled) return;

            //Effect handled in SharedHooks.GetStatsCoefficient

            LanguageAPI.Add("ITEM_SLOWONHIT_PICKUP", "Impair enemies on hit.");
            LanguageAPI.Add("ITEM_SLOWONHIT_DESC", "<style=cIsUtility>Impair</style> enemies on hit for <style=cIsUtility>-60% movement speed</style>, <style=cIsDamage>-30% damage</style>, and <style=cIsDamage>-30 armor</style> for <style=cIsUtility>2s</style> <style=cStack>(+2s per stack)</style>.");
        }
    }
}
