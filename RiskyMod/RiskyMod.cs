﻿using R2API;
using BepInEx;
using RoR2;
using R2API.Utils;
using RiskyMod.Items.Uncommon;
using RiskyMod.Items.Common;
using RiskyMod.SharedHooks;
using RiskyMod.Items.Boss;
using RiskyMod.Items.Lunar;
using RiskyMod.Items.Legendary;
using UnityEngine;
using RiskyMod.Tweaks;
using RiskyMod.Fixes;
using RiskyMod.Items;
using RiskyMod.Drones;
using RiskyMod.Items.Equipment;
using UnityEngine.Networking;
using RiskyMod.MonoBehaviours;
using RiskyMod.MoonRework;

namespace RiskyMod
{
    [BepInDependency("com.PlasmaCore.StickyStunter", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInDependency("com.Moffein.MercExposeFix", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.FixPlayercount", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.NoLevelupHeal", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.AI_Blacklist", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("dev.wildbook.multitudes", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.TPDespair.ZetArtifacts", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.ArtifactReliquaryHealingFix", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.RaiseMonsterLevelCap", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.RiskyLives.RiskyMod", "RiskyMod Alpha", "0.0.6")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(LanguageAPI), nameof(RecalculateStatsAPI), nameof(PrefabAPI),
        nameof(ProjectileAPI), nameof(EffectAPI), nameof(DamageAPI), nameof(BuffAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class RiskyMod : BaseUnityPlugin
    {
        public static bool disableProcChains = true;

        public static ItemDef emptyItemDef = null;
        public static BuffDef emptyBuffDef = null;

        public static AssistManager assistManager = null;

        public void Awake()
        {
            CheckDependencies();
            ReadConfig();
            RunFixes();
            RunTweaks();
            new ItemsCore();
            new DronesCore();
            new MoonReworkCore();
            SetupAssists();
            AddHooks();
        }

        private void ReadConfig()
        {

        }

        private void CheckDependencies()
        {
            NoLevelupHeal.enabled = !BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Moffein.NoLevelupHeal");
            RemoveLevelCap.enabled = !BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Moffein.RaiseMonsterLevelCap");
            FixMercExpose.enabled = !BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Moffein.MercExposeFix");

            FixPlayercount.enabled = !BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Moffein.FixPlayercount");
            FixPlayercount.MultitudesLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dev.wildbook.multitudes");
            FixPlayercount.ZetArtifactsLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TPDespair.ZetArtifacts");
            FixVengeanceLeveling.enabled = !BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Moffein.AI_Blacklist");
            PreventArtifactHeal.enabled = !BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rob.ArtifactReliquaryHealingFix");
        }

        private void RunTweaks()
        {
            new RunScaling();
            new TrueOSP();
            new ShieldGating();
            new SceneDirectorMonsterRewards();
            new BanditSpecialGracePeriod();
            new Shock();
            new NoLevelupHeal();
            new RemoveLevelCap();
        }

        private void RunFixes()
        {
            new FixMercExpose();
            new FixPlayercount();
            new FixVengeanceLeveling();
            new FixDamageTypeOverwrite();
            new FixFocusCrystalSelfDamage();
            new PreventArtifactHeal();
        }
        
        private void AddHooks()
        {
            //A hook needs to be used at least once to be added
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEnemy.GlobalEventManager_OnHitEnemy;
            RecalculateStatsAPI.GetStatCoefficients += GetStatsCoefficient.RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.RecalculateStats += RecalculateStats.CharacterBody_RecalculateStats;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage.HealthComponent_TakeDamage;
            On.RoR2.GlobalEventManager.OnCharacterDeath += OnCharacterDeath.GlobalEventManager_OnCharacterDeath;
            new ModifyFinalDamage();
            new StealBuffVFX();
        }

        private void SetupAssists()
        {
            AssistManager.initialized = true;
            On.RoR2.Run.Start += (orig, self) =>
            {
                orig(self);
                if (NetworkServer.active)
                {
                    RiskyMod.assistManager = self.gameObject.GetComponent<AssistManager>();
                    if (!RiskyMod.assistManager)
                    {
                        RiskyMod.assistManager = self.gameObject.AddComponent<AssistManager>();
                    }
                }
            };
        }
    }
}
