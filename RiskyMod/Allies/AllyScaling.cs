﻿using RoR2;
using RoR2.CharacterAI;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RiskyMod.Allies
{
    public class AllyScaling
    {
        public static bool normalizeDroneDamage = true;

        public static bool noVoidDeath = true;
        public static bool noOverheat = true;

        public delegate void ChangeAllyScaling(AllyInfo ally, CharacterBody allyBody);  //allyBody is guaranteed to be non-null
        public static ChangeAllyScaling ChangeAllyScalingActions;

        public AllyScaling()
        {
            AlliesCore.ModifyAlliesActions += ModifyAllies;
            ChangeAllyScalingActions += MegaDrone_Scaling;
            ChangeAllyScalingActions += FlameDrone_Scaling;
        }

        private void ModifyAllies(List<AllyInfo> allies)
        {
            foreach (AllyInfo ally in allies)
            {
                ChangeScaling(ally);
            }
        }

        private void ChangeScaling(AllyInfo ally)
        {
            GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(ally.bodyIndex);
            CharacterBody allyBody = null;
            if (bodyPrefab)
            {
                allyBody = bodyPrefab.GetComponent<CharacterBody>();
            }
            if (!bodyPrefab || !allyBody) return;

            if (noVoidDeath) allyBody.bodyFlags |= CharacterBody.BodyFlags.ImmuneToVoidDeath;
            if (noOverheat) allyBody.bodyFlags |= CharacterBody.BodyFlags.OverheatImmune;

            bool ignoreScaling = (ally.tags & AllyTag.DontModifyScaling) == AllyTag.DontModifyScaling;
            if (!ignoreScaling)
            {
                if ((ally.tags & AllyTag.Drone) == AllyTag.Drone)
                {
                    //Don't like how normalization is split between AllyScaling and AlliesCore
                    if (normalizeDroneDamage)
                    {
                        allyBody.baseDamage = 12f;
                    }
                }

                if ((ally.tags & AllyTag.Turret) == AllyTag.Turret)
                {
                    allyBody.bodyFlags |= CharacterBody.BodyFlags.ResistantToAOE;
                }

                if ((ally.tags & AllyTag.UseShield) == AllyTag.UseShield)
                {
                    allyBody.baseMaxShield += allyBody.baseMaxHealth * 0.08f;
                }

                //Drones always regen to full in 40s
                if ((ally.tags & AllyTag.DontModifyRegen) != AllyTag.DontModifyRegen)
                {
                    allyBody.baseRegen = allyBody.baseMaxHealth / 40f;
                    allyBody.levelRegen = allyBody.baseRegen * 0.2f;
                }

                //Set Level Stats
                allyBody.levelDamage = allyBody.baseDamage * 0.3f;
                allyBody.levelMaxHealth = allyBody.baseMaxHealth * 0.2f;
                allyBody.levelMaxShield = allyBody.baseMaxShield * 0.2f;
                allyBody.autoCalculateLevelStats = false;
            }

            //Can be used by external mods who want to do their own thing with custom allies?
            if (ChangeAllyScalingActions != null) ChangeAllyScalingActions.Invoke(ally, allyBody);
        }

        private void MegaDrone_Scaling(AllyInfo ally, CharacterBody allyBody)
        {
            if (ally.bodyName == "MegaDroneBody")
            {
                allyBody.baseArmor = 20f;

                if ((ally.tags & AllyTag.DontModifyRegen) != AllyTag.DontModifyRegen)
                {
                    allyBody.baseRegen = allyBody.baseMaxHealth / 30f;
                    allyBody.levelRegen = allyBody.baseRegen * 0.2f;
                }
            }
        }

        private void FlameDrone_Scaling(AllyInfo ally, CharacterBody allyBody)
        {
            if (ally.bodyName == "FlameDroneBody")
            {
                allyBody.baseArmor = 20f;

                if ((ally.tags & AllyTag.DontModifyRegen) != AllyTag.DontModifyRegen)
                {
                    allyBody.baseRegen = allyBody.baseMaxHealth / 20f;
                    allyBody.levelRegen = allyBody.baseRegen * 0.2f;
                }
            }
        }
    }
}
