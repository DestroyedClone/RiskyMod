﻿using EntityStates;
using EntityStates.RiskyMod.Bandit2;
using EntityStates.RiskyMod.Bandit2.Primary;
using EntityStates.RiskyMod.Bandit2.Revolver;
using R2API;
using RiskyMod.Survivors.Bandit2.Components;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace RiskyMod.Survivors.Bandit2
{
    public class Bandit2Core
    {
        public static BuffDef SpecialDebuff;
        public static DamageAPI.ModdedDamageType SpecialDamage;
        public static DamageAPI.ModdedDamageType RackEmUpDamage;
        public static DamageAPI.ModdedDamageType AlwaysBackstab;
        public static bool enabled = true;

        public static bool enablePassiveSkillChanges = true;
        public static bool enablePrimarySkillChanges = true;
        public static bool enableSecondarySkillChanges = true;
        public static bool enableUtilitySkillChanges = true;
        public static bool enableSpecialSkillChanges = true;

        public static BodyIndex Bandit2Index;

        public Bandit2Core()
        {
            if (!enabled) return;

            AlwaysBackstab = DamageAPI.ReserveDamageType();
            SpecialDamage = DamageAPI.ReserveDamageType();

            new BanditSpecialGracePeriod();
            ModifySkills(RoR2Content.Survivors.Bandit2.bodyPrefab.GetComponent<SkillLocator>());

            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                Bandit2Index = BodyCatalog.FindBodyIndex("Bandit2Body");
            };
        }
        private void ModifySkills(SkillLocator sk)
        {
            //How far to go with changing Bandit?
            //Stay near vanilla, or try to reach a halfway with BanditReloaded?

            //Wanted Quickdraw/Backstab as selectable passives, but Quickdraw (isnta reload on skill use) on its own doesn't feel as fun as Backstab.
            //Would like to combine them, but Bandit's earlygame DPS is already through the roof, even with nerfed backstab multiplier.
                //Maybe only knives can backstab?
                    //But being able to dump your shotgun into the back of an enemy is a big part of the fun.
            //But even with Bandit's high earlygame DPS, his lategame DPS is lackluster and he doesn't stand out much.
                //Minimum reload entry duration certainly doesn't help there, though I want to keep it
                //because I think you shouldn't be able to simply machinegun reloadable skills without letting go of the trigger.
            //Enemies sometimes just see you through invis and refuse to let you backstab them. True Invis + Longer Cloak?
            ModifyPassives(sk);

            ModifyPrimaries(sk);
            ModifyUtilities(sk);
            ModifySpecials(sk);

            //Secondaries:
            //Boost damage 33% to compensate for lower backstab crit mult.
            //Default knife gets bigger hitbox so you don't whiff so much, lunge forward like Loader's alt shift but with less forcce.
            //Throwing knife gains damage over distance.
            //Merge Dynamite into the mod?
                //Should Dynamite backstab? Was considering making backstab not apply to explosives/AOE.
            //Does Hemorrhage even help much lategame?

            //Specials
            //10% HP Execute?
            //Add Rack Em Up from BanditReloaded
            //Select between Reset Cooldowns and Desperado as a passive.
        }

        private void ModifyPassives(SkillLocator sk)
        {
            if (!enablePassiveSkillChanges) return;
            new BackstabRework();
            sk.passiveSkill.skillDescriptionToken = "BANDIT2_PASSIVE_DESCRIPTION_RISKYMOD";
        }

        private void ModifyPrimaries(SkillLocator sk)
        {
            if (!enablePrimarySkillChanges) return;

            LoadoutAPI.AddSkill(typeof(EnterReload));
            LoadoutAPI.AddSkill(typeof(Reload));

            LoadoutAPI.AddSkill(typeof(FirePrimaryShotgun));
            ReloadSkillDef shotgunDef = ReloadSkillDef.CreateInstance<ReloadSkillDef>();
            shotgunDef.activationState = new SerializableEntityStateType(typeof(FirePrimaryShotgun));
            shotgunDef.activationStateMachineName = "Weapon";
            shotgunDef.baseMaxStock = 4;
            shotgunDef.baseRechargeInterval = 0f;
            shotgunDef.beginSkillCooldownOnSkillEnd = false;
            shotgunDef.canceledFromSprinting = false;
            shotgunDef.dontAllowPastMaxStocks = true;
            shotgunDef.forceSprintDuringState = false;
            shotgunDef.fullRestockOnAssign = true;
            shotgunDef.icon = sk.primary._skillFamily.variants[0].skillDef.icon;
            shotgunDef.interruptPriority = InterruptPriority.Skill;
            shotgunDef.isCombatSkill = true;
            shotgunDef.keywordTokens = new string[] { };
            shotgunDef.mustKeyPress = false;
            shotgunDef.cancelSprintingOnActivation = true;
            shotgunDef.rechargeStock = 0;
            shotgunDef.requiredStock = 1;
            shotgunDef.skillName = "FireShotgun";
            shotgunDef.skillNameToken = "BANDIT2_PRIMARY_NAME";
            shotgunDef.skillDescriptionToken = "BANDIT2_PRIMARY_DESC_RISKYMOD";
            shotgunDef.stockToConsume = 1;
            shotgunDef.graceDuration = 0.4f;
            shotgunDef.reloadState = new SerializableEntityStateType(typeof(EnterReload));
            shotgunDef.reloadInterruptPriority = InterruptPriority.Any;
            LoadoutAPI.AddSkillDef(shotgunDef);
            Skills.Burst = shotgunDef;
            sk.primary._skillFamily.variants[0].skillDef = Skills.Burst;

            LoadoutAPI.AddSkill(typeof(FirePrimaryRifle));
            ReloadSkillDef rifleDef = ReloadSkillDef.CreateInstance<ReloadSkillDef>();
            rifleDef.activationState = new SerializableEntityStateType(typeof(FirePrimaryRifle));
            rifleDef.activationStateMachineName = "Weapon";
            rifleDef.baseMaxStock = 4;
            rifleDef.baseRechargeInterval = 0f;
            rifleDef.beginSkillCooldownOnSkillEnd = false;
            rifleDef.canceledFromSprinting = false;
            rifleDef.dontAllowPastMaxStocks = true;
            rifleDef.forceSprintDuringState = false;
            rifleDef.fullRestockOnAssign = true;
            rifleDef.icon = sk.primary._skillFamily.variants[1].skillDef.icon;
            rifleDef.interruptPriority = InterruptPriority.Skill;
            rifleDef.isCombatSkill = true;
            rifleDef.keywordTokens = new string[] { };
            rifleDef.mustKeyPress = false;
            rifleDef.cancelSprintingOnActivation = true;
            rifleDef.rechargeStock = 0;
            rifleDef.requiredStock = 1;
            rifleDef.skillName = "FireRifle";
            rifleDef.skillNameToken = "BANDIT2_PRIMARY_ALT_NAME";
            rifleDef.skillDescriptionToken = "BANDIT2_PRIMARY_ALT_DESC_RISKYMOD";
            rifleDef.stockToConsume = 1;
            rifleDef.graceDuration = 0.4f;
            rifleDef.reloadState = new SerializableEntityStateType(typeof(EnterReload));
            rifleDef.reloadInterruptPriority = InterruptPriority.Any;
            LoadoutAPI.AddSkillDef(rifleDef);
            Skills.Blast = rifleDef;
            sk.primary._skillFamily.variants[1].skillDef = Skills.Blast;
        }
        private void ModifyUtilities(SkillLocator sk)
        {
            if (!enableUtilitySkillChanges) return;

            LoadoutAPI.AddSkill(typeof(ThrowSmokebomb));
            LoadoutAPI.AddSkill(typeof(StealthMode));
            SkillDef stealthDef = SkillDef.CreateInstance<SkillDef>();
            stealthDef.activationState = new SerializableEntityStateType(typeof(ThrowSmokebomb));
            stealthDef.activationStateMachineName = "Stealth";
            stealthDef.baseMaxStock = 1;
            stealthDef.baseRechargeInterval = 6f;
            stealthDef.beginSkillCooldownOnSkillEnd = false;
            stealthDef.canceledFromSprinting = false;
            stealthDef.forceSprintDuringState = true;
            stealthDef.dontAllowPastMaxStocks = true;
            stealthDef.fullRestockOnAssign = true;
            stealthDef.icon = sk.utility._skillFamily.variants[0].skillDef.icon;
            stealthDef.interruptPriority = InterruptPriority.Skill;
            stealthDef.isCombatSkill = false;
            stealthDef.keywordTokens = new string[] { "KEYWORD_STUNNING" };
            stealthDef.mustKeyPress = false;
            stealthDef.cancelSprintingOnActivation = false;
            stealthDef.rechargeStock = 1;
            stealthDef.requiredStock = 1;
            stealthDef.skillName = "Stealth";
            stealthDef.skillNameToken = "BANDIT2_UTILITY_NAME";
            stealthDef.skillDescriptionToken = "BANDIT2_UTILITY_DESCRIPTION";
            stealthDef.stockToConsume = 1;
            LoadoutAPI.AddSkillDef(stealthDef);
            Skills.Smokebomb = stealthDef;

            sk.utility._skillFamily.variants[0].skillDef = Skills.Smokebomb;
        }

        private void ModifySpecials(SkillLocator sk)
        {
            if (!enableSpecialSkillChanges) return;
            SpecialDamageType(sk);
            SpecialDebuff = BuildSpecialDebuff();
            new SpecialDamageTweaks();

            LoadoutAPI.AddSkill(typeof(BaseSidearmState));
            LoadoutAPI.AddSkill(typeof(ExitSidearm));

            SkillDef lightsOutDef = SkillDef.CreateInstance<SkillDef>();
            LoadoutAPI.AddSkill(typeof(PrepLightsOut));
            LoadoutAPI.AddSkill(typeof(FireLightsOut));
            lightsOutDef.activationState = new SerializableEntityStateType(typeof(PrepLightsOut));
            lightsOutDef.activationStateMachineName = "Weapon";
            lightsOutDef.baseMaxStock = 1;
            lightsOutDef.baseRechargeInterval = 4f;
            lightsOutDef.beginSkillCooldownOnSkillEnd = false;
            lightsOutDef.canceledFromSprinting = false;
            lightsOutDef.forceSprintDuringState = false;
            lightsOutDef.dontAllowPastMaxStocks = true;
            lightsOutDef.fullRestockOnAssign = true;
            lightsOutDef.icon = sk.special._skillFamily.variants[0].skillDef.icon;
            lightsOutDef.interruptPriority = InterruptPriority.Skill;
            lightsOutDef.isCombatSkill = true;
            lightsOutDef.keywordTokens = new string[] { "KEYWORD_SLAYER" };
            lightsOutDef.mustKeyPress = false;
            lightsOutDef.cancelSprintingOnActivation = true;
            lightsOutDef.rechargeStock = 1;
            lightsOutDef.requiredStock = 1;
            lightsOutDef.skillName = "LightsOut";
            lightsOutDef.skillNameToken = "BANDIT2_SPECIAL_NAME";
            lightsOutDef.skillDescriptionToken = SpecialDamageTweaks.specialExecute? "BANDIT2_SPECIAL_DESCRIPTION_RISKYMOD" : "BANDIT2_SPECIAL_DESCRIPTION_NOEXECUTE_RISKYMOD";
            lightsOutDef.stockToConsume = 1;
            LoadoutAPI.AddSkillDef(lightsOutDef);
            Skills.LightsOut = lightsOutDef;
            sk.special._skillFamily.variants[0].skillDef = Skills.LightsOut;

            SkillDef reuDef = SkillDef.CreateInstance<SkillDef>();
            LoadoutAPI.AddSkill(typeof(PrepRackEmUp));
            LoadoutAPI.AddSkill(typeof(FireRackEmUp));
            reuDef.activationState = new SerializableEntityStateType(typeof(PrepRackEmUp));
            reuDef.activationStateMachineName = "Weapon";
            reuDef.baseMaxStock = 1;
            reuDef.baseRechargeInterval = 4f;
            reuDef.beginSkillCooldownOnSkillEnd = false;
            reuDef.canceledFromSprinting = false;
            reuDef.forceSprintDuringState = false;
            reuDef.dontAllowPastMaxStocks = true;
            reuDef.fullRestockOnAssign = true;
            reuDef.icon = sk.special._skillFamily.variants[1].skillDef.icon;
            reuDef.interruptPriority = InterruptPriority.Skill;
            reuDef.isCombatSkill = true;
            reuDef.keywordTokens = new string[] { "KEYWORD_SLAYER" };
            reuDef.mustKeyPress = false;
            reuDef.cancelSprintingOnActivation = true;
            reuDef.rechargeStock = 1;
            reuDef.requiredStock = 1;
            reuDef.skillName = "RackEmUp";
            reuDef.skillNameToken = "BANDIT2_SPECIAL_ALT_NAME_RISKYMOD";
            reuDef.skillDescriptionToken = SpecialDamageTweaks.specialExecute ? "BANDIT2_SPECIAL_ALT_DESCRIPTION_RISKYMOD" : "BANDIT2_SPECIAL_ALT_DESCRIPTION_NOEXECUTE_RISKYMOD";
            reuDef.stockToConsume = 1;
            LoadoutAPI.AddSkillDef(reuDef);
            Skills.RackEmUp = reuDef;
            sk.special._skillFamily.variants[1].skillDef = Skills.RackEmUp;
        }

        private BuffDef BuildSpecialDebuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(0.8039216f, 0.482352942f, 0.843137264f);
            buff.canStack = true;
            buff.isDebuff = false;
            buff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffBanditSkullIcon");
            buff.name = "RiskyModBanditRevolver";
            BuffAPI.Add(new CustomBuff(buff));
            return buff;
        }

        private void SpecialDamageType(SkillLocator sk)
        {
            RoR2Content.Survivors.Bandit2.bodyPrefab.AddComponent<SpecialDamageController>();
            GenericSkill passive = RoR2Content.Survivors.Bandit2.bodyPrefab.AddComponent<GenericSkill>();

            SkillDef gunslingerDef = ScriptableObject.CreateInstance<SkillDef>();
            gunslingerDef.activationState = new SerializableEntityStateType(typeof(BaseState));
            gunslingerDef.activationStateMachineName = "Weapon";
            gunslingerDef.skillDescriptionToken = "BANDIT2_REVOLVER_DESCRIPTION_RISKYMOD";
            gunslingerDef.skillName = "Gunslinger";
            gunslingerDef.skillNameToken = "BANDIT2_REVOLVER_NAME_RISKYMOD";
            gunslingerDef.icon = sk.passiveSkill.icon;  //TODO: ICON
            Skills.Gunslinger = gunslingerDef;
            LoadoutAPI.AddSkillDef(Skills.Gunslinger);

            SkillDef desperado = ScriptableObject.CreateInstance<SkillDef>();
            desperado.activationState = new SerializableEntityStateType(typeof(BaseState));
            desperado.activationStateMachineName = "Weapon";
            desperado.skillDescriptionToken = "BANDIT2_REVOLVER_ALT_DESCRIPTION_RISKYMOD";
            desperado.skillName = "Desperado";
            desperado.skillNameToken = "BANDIT2_SPECIAL_ALT_NAME";
            desperado.icon = sk.passiveSkill.icon;  //TODO: ICON
            Skills.Desperado = desperado;
            LoadoutAPI.AddSkillDef(Skills.Desperado);

            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[2];
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = Skills.Gunslinger,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(Skills.Gunslinger.skillName, false, null)
            };
            skillFamily.variants[1] = new SkillFamily.Variant
            {
                skillDef = Skills.Desperado,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(Skills.Desperado.skillName, false, null)
            };
            LoadoutAPI.AddSkillFamily(skillFamily);
            passive._skillFamily = skillFamily;
        }
    }

    public class Skills
    {
        public static ReloadSkillDef Blast;
        public static ReloadSkillDef Burst;

        public static SkillDef Knife;
        public static SkillDef ThrowKnife;
        public static SkillDef Dynamite;

        public static SkillDef Smokebomb;

        public static SkillDef LightsOut;
        public static SkillDef RackEmUp;

        public static SkillDef Gunslinger;
        public static SkillDef Desperado;
    }
}
