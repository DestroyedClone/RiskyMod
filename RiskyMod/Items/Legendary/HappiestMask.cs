﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RiskyMod.SharedHooks;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskyMod.Items.Legendary
{
    public class HappiestMask
    {
        public static bool enabled = true;
        public static BuffDef GhostCooldown;
        public static BuffDef GhostReady;

        public HappiestMask()
        {
            if (!enabled) return;
            HG.ArrayUtils.ArrayAppend(ref ItemsCore.changedItemPickups, RoR2Content.Items.GhostOnKill);
            HG.ArrayUtils.ArrayAppend(ref ItemsCore.changedItemDescs, RoR2Content.Items.GhostOnKill);

            //Remove vanilla effect
            IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Items), "GhostOnKill")
                    );
                c.Remove();
                c.Emit<RiskyMod>(OpCodes.Ldsfld, nameof(RiskyMod.emptyItemDef));
            };

            GhostCooldown = ScriptableObject.CreateInstance<BuffDef>();
            GhostCooldown.buffColor = new Color(88f/255f, 91f/255f, 98f/255f);
            GhostCooldown.canStack = true;
            GhostCooldown.isDebuff = true;
            GhostCooldown.name = "RiskyMod_GhostCooldownDebuff";
            GhostCooldown.iconSprite = RoR2Content.Buffs.BanditSkull.iconSprite;
            BuffAPI.Add(new CustomBuff(GhostCooldown));

            GhostReady = ScriptableObject.CreateInstance<BuffDef>();
            GhostReady.buffColor = new Color(0.9f, 0.9f, 0.9f);
            GhostReady.canStack = false;
            GhostReady.isDebuff = false;
            GhostReady.name = "RiskyMod_GhostReadyBuff";
            GhostReady.iconSprite = RoR2Content.Buffs.BanditSkull.iconSprite;
            BuffAPI.Add(new CustomBuff(GhostReady));

            OnCharacterDeath.OnCharacterDeathInventoryActions += SpawnGhost;
            On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
            {
                orig(self);
                if (NetworkServer.active)
                {
                    self.AddItemBehavior<GhostOnKillBehavior>(self.inventory.GetItemCount(RoR2Content.Items.GhostOnKill));
                }
            };
        }

        private static void SpawnGhost(GlobalEventManager self, DamageReport damageReport, CharacterBody attackerBody, Inventory attackerInventory, CharacterBody victimBody)
        {
            int itemCount = attackerInventory.GetItemCount(RoR2Content.Items.GhostOnKill);
            if (itemCount > 0)
            {
                if (attackerBody.HasBuff(GhostReady.buffIndex))
                {
                    GhostOnKillBehavior gokb = attackerBody.GetComponent<GhostOnKillBehavior>();
                    if (gokb && gokb.CanSpawnGhost())
                    {
                        gokb.AddGhost(SpawnMaskGhost(victimBody, attackerBody, itemCount * 30));
                        for (int i = 1; i <= 20; i++)
                        {
                            attackerBody.AddTimedBuff(GhostCooldown.buffIndex, i);
                        }
                    }
                }
            }
        }
        public static CharacterBody SpawnMaskGhost(CharacterBody targetBody, CharacterBody ownerBody, int duration)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'RoR2.CharacterBody RoR2.Util::TryToCreateGhost(RoR2.CharacterBody, RoR2.CharacterBody, int)' called on client");
                return null;
            }
            if (!targetBody)
            {
                return null;
            }
            GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(targetBody);
            if (!bodyPrefab)
			{
                return null;
            }
            CharacterMaster characterMaster = MasterCatalog.allAiMasters.FirstOrDefault((CharacterMaster master) => master.bodyPrefab == bodyPrefab);
            if (!characterMaster)
            {
                return null;
            }
            MasterSummon masterSummon = new MasterSummon();
            masterSummon.masterPrefab = characterMaster.gameObject;
            masterSummon.ignoreTeamMemberLimit = true;
            masterSummon.position = targetBody.footPosition;
            CharacterDirection component = targetBody.GetComponent<CharacterDirection>();
            masterSummon.rotation = (component ? Quaternion.Euler(0f, component.yaw, 0f) : targetBody.transform.rotation);
            masterSummon.summonerBodyObject = (ownerBody ? ownerBody.gameObject : null);
            masterSummon.inventoryToCopy = targetBody.inventory;
            masterSummon.useAmbientLevel = null;
            CharacterMaster characterMaster2 = masterSummon.Perform();
            if (!characterMaster2)
            {
                return null;
            }
            else
            {
                Inventory inventory = characterMaster2.inventory;
                if (inventory)
                {
                    inventory.GiveItem(RoR2Content.Items.Ghost.itemIndex);
                    inventory.GiveItem(RoR2Content.Items.UseAmbientLevel.itemIndex);
                    inventory.GiveItem(RoR2Content.Items.HealthDecay.itemIndex, duration);
                    if (ownerBody && ownerBody.teamComponent && ownerBody.teamComponent.teamIndex == TeamIndex.Player)
                    {
                        inventory.GiveItem(RoR2Content.Items.BoostDamage.itemIndex, 150);
                    }
                }
            }
            CharacterBody body = characterMaster2.GetBody();
            if (body)
            {
                foreach (EntityStateMachine entityStateMachine in body.GetComponents<EntityStateMachine>())
                {
                    entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                }
            }
            return body;
        }

        public class GhostOnKillBehavior : CharacterBody.ItemBehavior
        {
            private List<CharacterBody> activeGhosts;

            private void Awake()
            {
                activeGhosts = new List<CharacterBody>();
            }

            private void FixedUpdate()
            {
                bool flag = this.body.HasBuff(GhostCooldown.buffIndex);
                bool flag2 = this.body.HasBuff(GhostReady.buffIndex);
                if (!flag && !flag2)
                {
                    this.body.AddBuff(GhostReady.buffIndex);
                }
                if (flag2 && flag)
                {
                    this.body.RemoveBuff(GhostReady.buffIndex);
                }

                UpdateGhosts();
            }

            private void UpdateGhosts()
            {
                List<CharacterBody> toRemove = new List<CharacterBody>();
                foreach(CharacterBody cb in activeGhosts)
                {
                    if (!(cb && cb.healthComponent && cb.healthComponent.alive))
                    {
                        toRemove.Add(cb);
                    }
                }

                foreach(CharacterBody cb in toRemove)
                {
                    activeGhosts.Remove(cb);
                }
            }

            public bool CanSpawnGhost()
            {
                int itemCount = base.body.inventory.GetItemCount(RoR2Content.Items.GhostOnKill.itemIndex);
                return activeGhosts.Count < 3 + (itemCount - 1);
            }

            public void AddGhost(CharacterBody cb)
            {
                activeGhosts.Add(cb);
            }
        }
    }
}