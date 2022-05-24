﻿using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.RiskyMod.Mage
{
	public class SpecialLightning : BaseState
	{
		public virtual void LoadStats()
		{
			loadBaseTickCount = SpecialLightning.baseTickCount;
			loadBaseTickFrequency = SpecialLightning.baseTickFrequency;
			loadMaxDistance = SpecialLightning.maxDistance;
		}

		public virtual void ModifyAttack(LightningOrb lo) { }

		public override void OnEnter()
		{
			base.OnEnter();
			this.stopwatch = 0f;

			LoadStats();

			this.entryDuration = SpecialLightning.baseEntryDuration / this.attackSpeedStat;

			this.tickFrequency = loadBaseTickFrequency * this.attackSpeedStat;
			//totalTickCount = Mathf.FloorToInt(loadBaseTickCount * this.attackSpeedStat);

			this.flamethrowerDuration = SpecialLightning.baseAttackDuration;

			//effectResetStopwatch = 0f;

			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(this.entryDuration + this.flamethrowerDuration + 1f);
			}
			Transform modelTransform = base.GetModelTransform();
			if (modelTransform)
			{
				this.childLocator = modelTransform.GetComponent<ChildLocator>();
				this.leftMuzzleTransform = this.childLocator.FindChild("MuzzleLeft");
				this.rightMuzzleTransform = this.childLocator.FindChild("MuzzleRight");
			}

			if (base.isAuthority && base.characterBody)
			{
				this.isCrit = Util.CheckRoll(this.critStat, base.characterBody.master);
			}
			base.PlayAnimation("Gesture, Additive", "PrepFlamethrower", "Flamethrower.playbackRate", this.entryDuration);
		}

		public override void OnExit()
		{
			Util.PlaySound(SpecialLightning.endAttackSoundString, base.gameObject);
			base.PlayCrossfade("Gesture, Additive", "ExitFlamethrower", 0.1f);
			base.OnExit();
		}

		private void FireGauntlet(string muzzleString)
		{
			//totalTickCount--;
			Ray aimRay = base.GetAimRay();

			Vector3 lightningOrigin = aimRay.origin;
			if (this.leftMuzzleTransform && this.rightMuzzleTransform)
			{
				if (rightMuzzle)
				{
					lightningOrigin = this.rightMuzzleTransform.position;
				}
				else
				{
					lightningOrigin = this.leftMuzzleTransform.position;
				}

				rightMuzzle = !rightMuzzle;
			}

			if (NetworkServer.active)
			{
				List<HealthComponent> bouncedObjects = new List<HealthComponent>();
				LightningOrb lo = new LightningOrb
				{
					bouncedObjects = bouncedObjects,
					attacker = base.gameObject,
					inflictor = base.gameObject,
					damageValue = base.damageStat * SpecialLightning.tickDamageCoefficient,
					procCoefficient = SpecialLightning.procCoefficientPerTick,
					teamIndex = base.GetTeam(),
					isCrit = this.isCrit,
					procChainMask = default,
					lightningType = LightningOrb.LightningType.MageLightning,
					damageColorIndex = DamageColorIndex.Default,
					bouncesRemaining = 2,
					targetsToFindPerBounce = 3,
					range = loadMaxDistance * 0.375f,
					origin = lightningOrigin,
					damageType = DamageType.Generic,
					speed = 120f,
					//damageCoefficientPerBounce = 0.8f
				};
				ModifyAttack(lo);

				BullseyeSearch search = new BullseyeSearch();
				search.teamMaskFilter = TeamMask.allButNeutral;
				search.teamMaskFilter.RemoveTeam(characterBody.teamComponent.teamIndex);
				search.filterByLoS = false;
				search.searchOrigin = aimRay.origin;
				search.sortMode = BullseyeSearch.SortMode.Angle;
				search.maxDistanceFilter = loadMaxDistance;
				search.maxAngleFilter = 45f;
				search.searchDirection = aimRay.direction;
				search.RefreshCandidates();

				GameObject effect = SpecialLightning.gauntletMissEffectPrefab;

				HurtBox target = search.GetResults().FirstOrDefault();
				if (target)
				{
					lo.target = target;
					OrbManager.instance.AddOrb(lo);
					lo.bouncedObjects.Add(target.healthComponent);
					effect = SpecialLightning.gauntletEffectPrefab;
				}

				EffectManager.SimpleMuzzleFlash(effect, base.gameObject, "MuzzleLeft", true);
				EffectManager.SimpleMuzzleFlash(effect, base.gameObject, "MuzzleRight", true);

				/*int i = 0;
				List<HurtBox> targets = search.GetResults().ToList();
				if (targets.Count > 0)
				{
					foreach (HurtBox hb in targets)
					{
						if (!lo.bouncedObjects.Contains(hb.healthComponent))
						{
							lo.target = hb;
							OrbManager.instance.AddOrb(lo);
							lo.bouncedObjects.Add(hb.healthComponent);

							i++;
							if (i >= 3) break;
						}
					}
				}*/
			}

			if (base.characterMotor)
			{
				base.characterMotor.ApplyForce(aimRay.direction * -SpecialLightning.recoilForce, false, false);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatch += Time.fixedDeltaTime;

			if (this.stopwatch >= this.entryDuration && !this.hasBegunAttack)
			{
				this.hasBegunAttack = true;
				Util.PlaySound(SpecialLightning.startAttackSoundString, base.gameObject);
				base.PlayAnimation("Gesture, Additive", "Flamethrower", "Flamethrower.playbackRate", this.flamethrowerDuration);
				this.FireGauntlet("MuzzleCenter");
			}
			if (this.hasBegunAttack)
			{
				//effectResetStopwatch += Time.fixedDeltaTime;
				this.flamethrowerStopwatch += Time.deltaTime;
				if (this.flamethrowerStopwatch > 1f / tickFrequency)
				{
					this.flamethrowerStopwatch -= 1f / tickFrequency;
					this.FireGauntlet("MuzzleCenter");
				}
			}

			/*if (effectResetStopwatch > SpecialLightning.effectResetDuration)
			{
				effectResetStopwatch = 0f;
				if (totalTickCount * Mathf.Max(1f / tickFrequency, Time.fixedDeltaTime) > 0.6f)
				{
					ResetFlamethrowerEffect();
				}
			}*/

			if (base.fixedAge > (SpecialLightning.baseEntryDuration + SpecialLightning.baseAttackDuration) && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		bool rightMuzzle = true; //Keep track of which gauntlet the lightning visually comes out from

		public static GameObject gauntletEffectPrefab;
		public static GameObject gauntletMissEffectPrefab;
		public static float maxDistance = 40f;
		public static float baseEntryDuration = 0.6f;
		public static float baseAttackDuration = 3f;
		public static float tickDamageCoefficient = 0.8f;
		public static float procCoefficientPerTick = 1f;
		public static float baseTickFrequency = 5f;
		public static float force = 100f;

		public static string startAttackSoundString = "Play_mage_m2_shoot";//"Play_mage_m2_charge";
		public static string endAttackSoundString = "";//"Play_mage_r_lightningblast";
		//public static string attackSoundString = "Play_item_proc_chain_lightning";//"Play_mage_r_lightningblast"; //"Play_mage_m1_cast_lightning";

		public static float recoilForce = 0f;

		//public static float effectResetDuration = 2.5f;
		//private float effectResetStopwatch;

		private static int baseTickCount = 20;
		//private int totalTickCount;
		private float tickFrequency;

		public int loadBaseTickCount;
		public float loadBaseTickFrequency;
		public float loadMaxDistance;

		public static GameObject laserEffectPrefab;

		private float flamethrowerStopwatch;
		private float stopwatch;
		private float entryDuration;
		private float flamethrowerDuration;
		private bool hasBegunAttack;
		private ChildLocator childLocator;
		private Transform leftMuzzleTransform;
		private Transform rightMuzzleTransform;
		private bool isCrit;
		private const float flamethrowerEffectBaseDistance = 16f;
	}
}