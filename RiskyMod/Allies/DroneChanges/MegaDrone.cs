﻿using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RiskyMod.Allies.DroneChanges
{
    public class MegaDrone
    {
		public static bool allowRepair = true;
        public MegaDrone()
		{
			GameObject megaDroneMasterObject = LegacyResourcesAPI.Load<GameObject>("prefabs/charactermasters/MegaDroneMaster");

			AISkillDriver[] aiDrivers = megaDroneMasterObject.GetComponentsInChildren<AISkillDriver>();
            for (int i = 0; i < aiDrivers.Length; i++)
            {
                if (aiDrivers[i].customName == "StopTooCloseTarget")
                {
                    aiDrivers[i].movementType = AISkillDriver.MovementType.StrafeMovetarget;
					aiDrivers[i].noRepeat = true;
					aiDrivers[i].resetCurrentEnemyOnNextDriverSelection = true; //Does this actually improve it?
                    break;
                }
            }

			GameObject megaDroneBrokenObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/MegaDroneBroken.prefab").WaitForCompletion();
			PurchaseInteraction pi = megaDroneBrokenObject.GetComponent<PurchaseInteraction>();
			pi.cost = 300;	//Vanilla is 350

			GameObject megaDroneBodyObject = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/MegaDroneBody");
			if (allowRepair)
            {
				CharacterDeathBehavior cdb = megaDroneBodyObject.GetComponent<CharacterDeathBehavior>();
				cdb.deathState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.RiskyMod.MegaDrone.MegaDroneDeathState));
				Content.Content.entityStates.Add(typeof(EntityStates.RiskyMod.MegaDrone.MegaDroneDeathState));
			}

			//This doesn't really do anything useful.
			/*CharacterBody megaDroneBody = megaDroneBodyObject.GetComponent<CharacterBody>();
			megaDroneBody.acceleration = 20f;   //Vanilla is 20f
			megaDroneBody.baseMoveSpeed = 24f;   //Vanilla is 20f*/
		}
    }
}

namespace EntityStates.RiskyMod.MegaDrone
{
	public class MegaDroneDeathState : GenericCharacterDeath
	{
		public override void OnEnter()
		{
			if (NetworkServer.active)
			{
				//Based on ZetTweaks https://github.com/William758/ZetTweaks/blob/main/GameplayModule.cs
				if (base.transform)
				{
					DirectorPlacementRule placementRule = new DirectorPlacementRule
					{
						placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
						position = base.transform.position,
					};

					GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, new Xoroshiro128Plus(0UL)));
					if (gameObject)
					{
						PurchaseInteraction purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
						if (purchaseInteraction && purchaseInteraction.costType == CostTypeIndex.Money)
						{
							purchaseInteraction.Networkcost = Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost);
						}

						gameObject.transform.rotation = base.transform.rotation;
					}
				}

				EntityState.Destroy(base.gameObject);
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			Util.PlaySound(MegaDroneDeathState.initialSoundString, base.gameObject);
			Transform modelTransform = base.GetModelTransform();
			if (modelTransform && NetworkServer.active)
			{
				ChildLocator component = modelTransform.GetComponent<ChildLocator>();
				if (component)
				{
					component.FindChild("LeftJet").gameObject.SetActive(false);
					component.FindChild("RightJet").gameObject.SetActive(false);
					if (MegaDroneDeathState.initialEffect)
					{
						EffectManager.SpawnEffect(MegaDroneDeathState.initialEffect, new EffectData
						{
							origin = base.transform.position,
							scale = MegaDroneDeathState.initialEffectScale
						}, true);
					}
				}
			}

			//Disable gibs
			Rigidbody component2 = base.GetComponent<Rigidbody>();
			RagdollController component3 = modelTransform.GetComponent<RagdollController>();
			if (component3 && component2)
			{
				component3.BeginRagdoll(component2.velocity * MegaDroneDeathState.velocityMagnitude);
			}
			ExplodeRigidbodiesOnStart component4 = modelTransform.GetComponent<ExplodeRigidbodiesOnStart>();
			if (component4)
			{
				component4.force = MegaDroneDeathState.explosionForce;
				component4.enabled = true;
			}
		}

		public static SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscBrokenMegaDrone");
		public static string initialSoundString = "Play_drone_deathpt1";
		public static GameObject initialEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXDroneDeath.prefab").WaitForCompletion();
		public static float initialEffectScale = 15f;
		public static float velocityMagnitude = 1f;
		public static float explosionForce = 60000f;
	}
}
