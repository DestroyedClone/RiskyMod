﻿using EntityStates.RiskyMod.Captain;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace RiskyMod.Survivors.Captain
{
    public class CaptainCore
    {
        public static bool enabled = true;
        public static bool enablePrimarySkillChanges = true;
        public static GameObject bodyPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CaptainBody");

        public static bool modifyTaser = true;
        public static bool nukeChanges = true;

        public static bool beaconRework = true;

        public static BodyIndex CaptainIndex;

        public CaptainCore()
        {
            if (!enabled) return;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.Admiral"))
            {
                Debug.LogError("RiskyMod: Loading CaptainCore while Admiral is installed, there may be conflicts. Check your RiskyMod/Admiral config files and adjust accordingly.");
            }

            new Microbots();
            new CaptainOrbitalHiddenRealms();
            ModifySkills(bodyPrefab.GetComponent<SkillLocator>());

            On.RoR2.EntityStateCatalog.InitializeStateFields += (orig, self) =>
            {
                orig(self);
                ChargeShotgun.chargeupVfxPrefab = EntityStates.Captain.Weapon.ChargeCaptainShotgun.chargeupVfxPrefab;
                ChargeShotgun.holdChargeVfxPrefab = EntityStates.Captain.Weapon.ChargeCaptainShotgun.holdChargeVfxPrefab;
            };

            On.RoR2.BodyCatalog.Init += (orig) =>
            {
                orig();
                CaptainIndex = BodyCatalog.FindBodyIndex("CaptainBody");
            };

            if (nukeChanges || beaconRework)
            {
                On.RoR2.GenericSkill.ApplyAmmoPack += (orig, self) =>
                {
                    //Probably don't need to actually check for BeaconRework since vanilla beacons don't benefit from ammopacks.
                    if (CaptainCore.beaconRework && (self.skillName == "SupplyDrop1" || self.skillName == "SupplyDrop2"))
                    {
                        return;
                    }
                    else if (CaptainCore.nukeChanges && self.activationState.stateType == typeof(EntityStates.Captain.Weapon.SetupAirstrikeAlt))
                    {
                        if (self.stock < self.maxStock)
                        {
                            self.rechargeStopwatch += 0.5f * self.finalRechargeInterval;
                        }
                    }
                    else
                    {
                        orig(self);
                    }
                };
            }
        }

        private void ModifySkills(SkillLocator sk)
        {
            ModifyPrimaries(sk);
            ModifySecondaries(sk);
            //ModifyUtilities(sk);
            ModifyBeacons(sk);
        }

        private void ModifyPrimaries(SkillLocator sk)
        {
            if (!enablePrimarySkillChanges) return;

            Content.Content.entityStates.Add(typeof(ChargeShotgun));
            Content.Content.entityStates.Add(typeof(FireShotgun));

            SkillDef shotgunDef = SkillDef.CreateInstance<SkillDef>();
            shotgunDef.activationState = new SerializableEntityStateType(typeof(ChargeShotgun));
            shotgunDef.activationStateMachineName = "Weapon";
            shotgunDef.baseMaxStock = 1;
            shotgunDef.baseRechargeInterval = 0f;
            shotgunDef.beginSkillCooldownOnSkillEnd = false;
            shotgunDef.canceledFromSprinting = false;
            shotgunDef.dontAllowPastMaxStocks = true;
            shotgunDef.forceSprintDuringState = false;
            shotgunDef.fullRestockOnAssign = true;
            shotgunDef.icon = sk.primary._skillFamily.variants[0].skillDef.icon;
            shotgunDef.interruptPriority = InterruptPriority.Any;
            shotgunDef.isCombatSkill = true;
            shotgunDef.keywordTokens = new string[] { };
            shotgunDef.mustKeyPress = false;
            shotgunDef.cancelSprintingOnActivation = true;
            shotgunDef.rechargeStock = 1;
            shotgunDef.requiredStock = 1;
            shotgunDef.skillName = "ChargeShotgun";
            shotgunDef.skillNameToken = "CAPTAIN_PRIMARY_NAME";
            shotgunDef.skillDescriptionToken = "CAPTAIN_PRIMARY_DESCRIPTION_RISKYMOD";
            shotgunDef.stockToConsume = 1;
            SneedUtils.SneedUtils.FixSkillName(shotgunDef);
            Content.Content.skillDefs.Add(shotgunDef);
            Skills.Shotgun = shotgunDef;
            sk.primary._skillFamily.variants[0].skillDef = Skills.Shotgun;
        }

        private void ModifySecondaries(SkillLocator sk)
        {
            if (modifyTaser)
            {
                new TaserRework();
                sk.secondary.skillFamily.variants[0].skillDef.skillDescriptionToken = "CAPTAIN_SECONDARY_DESCRIPTION_RISKYMOD";
                if (Tweaks.CharacterMechanics.Shock.enabled) sk.secondary.skillFamily.variants[0].skillDef.keywordTokens = new string[] { "KEYWORD_SHOCKING_RISKYMOD" };
            }
        }

        private void ModifyBeacons(SkillLocator sk)
        {
            if (beaconRework)
            {
                new BeaconRework(sk);
            }
        }
    }

    public class Skills
    {
        public static SkillDef Shotgun;
    }
}
