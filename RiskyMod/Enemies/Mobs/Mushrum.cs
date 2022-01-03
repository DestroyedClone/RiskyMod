﻿using UnityEngine;
using RoR2;

namespace RiskyMod.Enemies.Mobs
{
    public class Mushrum
    {
        public static bool enabled = true;
        public Mushrum()
        {
            if (!enabled) return;
            DisableRegen();
        }

        private void DisableRegen()
        {
            GameObject enemyObject = Resources.Load<GameObject>("prefabs/characterbodies/minimushroombody");
            CharacterBody cb = enemyObject.GetComponent<CharacterBody>();
            cb.baseRegen = 0f;
            cb.levelRegen = 0f;
        }
    }
}