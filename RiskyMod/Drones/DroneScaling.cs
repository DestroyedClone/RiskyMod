﻿using RoR2;
using RoR2.CharacterAI;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RiskyMod.Drones
{
    public class DroneScaling
    {
        public static bool enabled = true;
        public DroneScaling()
        {
            if (!enabled) return;

            //Backup
            ChangeScaling(LoadBody("BackupDroneBody"));

            //T1 drones
            ChangeScaling(LoadBody("Drone1Body"));
            ChangeScaling(LoadBody("Drone2Body"));
            ChangeScaling(LoadBody("Turret1Body"));

            //T2 drones
            ChangeScaling(LoadBody("MissileDroneBody"));
            ChangeScaling(LoadBody("FlameDroneBody"));
            ChangeScaling(LoadBody("EquipmentDroneBody"));
            ChangeScaling(LoadBody("EmergencyDroneBody"));

            //T3 drones
            ChangeScaling(LoadBody("MegaDroneBody"));

            //Squids
            ChangeScaling(LoadBody("SquidTurretBody"));

            //Beetle Allies
            ChangeScaling(LoadBody("BeetleGuardAllyBody"));
        }

        private void ChangeScaling(GameObject go)
        {
            CharacterBody cb = go.GetComponent<CharacterBody>();

            cb.baseRegen = cb.baseMaxHealth / 25f;  //Drones take 20s to regen to full

            bool noShield = false;

            //Specific changes
            switch (cb.name)
            {
                case "MegaDroneBody": //If I'm gonna pay the price of a legendary chest to buy a drone, it better be worth it.
                    cb.bodyFlags |= CharacterBody.BodyFlags.OverheatImmune;
                    cb.baseMaxHealth *= 1.5f;
                    break;
                case "SquidTurretBody":
                    cb.baseMaxHealth = 720f;
                    break;
                case "Turret1Body": //Shield seems to be enough to put them in a good spot.
                    cb.baseMaxHealth *= 1.5f;
                    break;
                /*case "FlameDroneBody": //These seem to die faster than other drones?
                    //cb.baseRegen = cb.baseMaxHealth / 20f;
                    cb.baseMaxHealth *= 1.5f;
                    break;*/
                case "BeetleGuardAllyBody":
                    cb.levelArmor -= 1f;    //Queens Gland Guards get no armor bonus.
                    noShield = true;
                    break;
                default:
                    break;
            }

            //This makes their performance stay the same on every stage. (Everything's HP increases 30% per level, so damage and regen increase matches that.)
            if (!noShield)
            {
                cb.baseMaxShield += cb.baseMaxHealth * 0.1f;
                cb.levelMaxShield = cb.baseMaxShield * 0.3f;
            }
            cb.levelRegen = cb.baseRegen * 0.3f;
            cb.levelDamage = cb.baseDamage * 0.3f;
            cb.levelArmor += 1f;    //Drones need bonus armor because of increasing enemycounts and elite counts, otherwise they end up dying really quickly.
            cb.levelMaxHealth = cb.baseMaxHealth * 0.3f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameObject LoadBody(string bodyname)
        {
            return Resources.Load<GameObject>("prefabs/characterbodies/" + bodyname);
        }
    }
}
