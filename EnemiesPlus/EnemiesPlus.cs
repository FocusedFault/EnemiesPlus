using BepInEx;
using R2API;
using RoR2.Projectile;
using RoR2;
using RoR2.Skills;
using RoR2.CharacterAI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates;
using EntityStates.BeetleMonster;
using EntityStates.Bell.BellWeapon;

namespace EnemiesPlus
{
  [BepInPlugin("com.Nuxlar.EnemiesPlus", "EnemiesPlus", "0.8.1")]

  public class EnemiesPlus : BaseUnityPlugin
  {
    private GameObject golem = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/GolemBody.prefab").WaitForCompletion();
    private GameObject bellMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellMaster.prefab").WaitForCompletion();
    private GameObject bell = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellBody.prefab").WaitForCompletion();
    private GameObject beetleGuard = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardBody.prefab").WaitForCompletion();
    private GameObject beetleGuardMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardMaster.prefab").WaitForCompletion();

    public void Awake()
    {
      BuffBeamSetup();
      ContentAddition.AddEntityState<RallyCry>(out _);
      AISkillDriver rallyCryDriver = beetleGuardMaster.AddComponent<AISkillDriver>();
      rallyCryDriver.customName = "RallyCry";
      rallyCryDriver.skillSlot = SkillSlot.Utility;
      rallyCryDriver.requireSkillReady = true;
      rallyCryDriver.movementType = AISkillDriver.MovementType.Stop;
      golem.GetComponent<CharacterBody>().baseRegen = 6f;
      beetleGuard.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef.activationState = new SerializableEntityStateType(typeof(RallyCry));
      IL.EntityStates.LemurianMonster.FireFireball.OnEnter += PredictiveFireball;
      IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate += PredictiveMegaFireball;
      On.EntityStates.Bell.BellWeapon.BuffBeam.OnEnter += SwitchBuffBeamBuff;
      On.EntityStates.Bell.BellWeapon.BuffBeam.OnExit += SwitchBuffBeamBuff2;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
      On.EntityStates.Bell.BellWeapon.BuffBeam.GetMinimumInterruptPriority += BellPriority;
      On.EntityStates.BeetleMonster.HeadbuttState.OnEnter += UncloakBeetle;
      On.RoR2.HealthComponent.TakeDamage += UncloakBeetle2;
    }
    private void SwitchBuffBeamBuff(On.EntityStates.Bell.BellWeapon.BuffBeam.orig_OnEnter orig, BuffBeam self)
    {
      orig(self);
      if ((bool)self.target)
        self.targetBody.AddBuff(RoR2Content.Buffs.Immune.buffIndex);
    }
    private void SwitchBuffBeamBuff2(On.EntityStates.Bell.BellWeapon.BuffBeam.orig_OnExit orig, BuffBeam self)
    {
      if ((bool)self.targetBody)
        self.targetBody.RemoveBuff(RoR2Content.Buffs.Immune.buffIndex);
      orig(self);
    }
    private void UncloakBeetle2(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
    {
      if (self.body && self.body.name == "BeetleBody(Clone)")
      {
        if (self.body.HasBuff(RoR2Content.Buffs.AffixHauntedRecipient))
        {
          self.body.skinIndex = 1;
          self.body.modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>().ApplySkin(1);
          self.body.RemoveBuff(RoR2Content.Buffs.AffixHauntedRecipient);
        }
      }
      orig(self, damageInfo);
    }
    private void UncloakBeetle(On.EntityStates.BeetleMonster.HeadbuttState.orig_OnEnter orig, HeadbuttState self)
    {
      if (self.characterBody.HasBuff(RoR2Content.Buffs.AffixHauntedRecipient))
      {
        self.characterBody.skinIndex = 1;
        self.modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>().ApplySkin(1);
        self.characterBody.RemoveBuff(RoR2Content.Buffs.AffixHauntedRecipient);
      }
      orig(self);
    }

    private InterruptPriority BellPriority(On.EntityStates.Bell.BellWeapon.BuffBeam.orig_GetMinimumInterruptPriority orig, EntityStates.Bell.BellWeapon.BuffBeam self)
    {
      return InterruptPriority.Death;
    }
    private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
    {
      orig(self, body);
      if (body.name == "BeetleBody(Clone)")
      {
        body.AddBuff(RoR2Content.Buffs.AffixHauntedRecipient);
      }
      if (body.name == "BeetleGuardBody(Clone)")
      {
        int desiredPosition = 2;
        AISkillDriver[] arr = self.gameObject.GetComponent<BaseAI>().skillDrivers;

        if (desiredPosition >= 0 && desiredPosition < arr.Length)
        {
          // Swap the last element with the element at the desired position
          AISkillDriver temp = arr[desiredPosition];
          arr[desiredPosition] = arr[arr.Length - 1];
          arr[arr.Length - 1] = temp;
        }
        self.gameObject.GetComponent<BaseAI>().skillDrivers = arr;
      }
      if (body.name == "BellBody(Clone)")
      {
        int desiredPosition = 2;
        AISkillDriver[] arr = self.gameObject.GetComponent<BaseAI>().skillDrivers;

        if (desiredPosition >= 0 && desiredPosition < arr.Length)
        {
          // Swap the last element with the element at the desired position
          AISkillDriver temp = arr[desiredPosition];
          arr[desiredPosition] = arr[arr.Length - 1];
          arr[arr.Length - 1] = temp;
        }
        self.gameObject.GetComponent<BaseAI>().skillDrivers = arr;
      }
    }
    private void BuffBeamSetup()
    {
      SkillDef buffBeam = ScriptableObject.CreateInstance<SkillDef>();

      buffBeam.skillName = "BuffBeam";
      (buffBeam as ScriptableObject).name = "BuffBeam";
      buffBeam.skillNameToken = "BuffBeam";
      buffBeam.skillDescriptionToken = "Creates a beam to a nearby ally and makes them invincible";
      // buffBeam.icon = ;

      buffBeam.activationState = new SerializableEntityStateType(typeof(EntityStates.Bell.BellWeapon.BuffBeam));
      buffBeam.activationStateMachineName = "Weapon";
      buffBeam.interruptPriority = InterruptPriority.Death;

      buffBeam.baseMaxStock = 1;
      buffBeam.baseRechargeInterval = 15;

      buffBeam.rechargeStock = 1;
      buffBeam.requiredStock = 1;
      buffBeam.stockToConsume = 1;

      buffBeam.dontAllowPastMaxStocks = true;
      buffBeam.beginSkillCooldownOnSkillEnd = true;
      buffBeam.canceledFromSprinting = false;
      buffBeam.forceSprintDuringState = false;
      buffBeam.fullRestockOnAssign = true;
      buffBeam.resetCooldownTimerOnUse = false;
      buffBeam.isCombatSkill = true;
      buffBeam.mustKeyPress = false;
      buffBeam.cancelSprintingOnActivation = false;

      ContentAddition.AddSkillDef(buffBeam);

      GenericSkill skill = bell.AddComponent<GenericSkill>();
      skill.skillName = "BuffBeam";

      SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
      (newFamily as ScriptableObject).name = "BellBuffBeamFamily";
      newFamily.variants = new SkillFamily.Variant[] { new SkillFamily.Variant() { skillDef = buffBeam } };

      skill._skillFamily = newFamily;
      ContentAddition.AddSkillFamily(newFamily);
      bell.GetComponent<SkillLocator>().secondary = skill;
      AISkillDriver buffBeamDriver = bellMaster.AddComponent<AISkillDriver>();
      buffBeamDriver.customName = "BuffBeam";
      buffBeamDriver.skillSlot = SkillSlot.Secondary;
      buffBeamDriver.requireSkillReady = true;
      buffBeamDriver.requireEquipmentReady = false;
      buffBeamDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
    }

    private void PredictiveMegaFireball(ILContext il)
    {
      ILCursor c = new ILCursor(il);
      if (c.TryGotoNext(MoveType.After,
           x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
          ))
      {
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Ray, EntityStates.LemurianBruiserMonster.FireMegaFireball, Ray>>((aimRay, self) =>
        {
          if (self.characterBody && !self.characterBody.isPlayerControlled)
          {
            HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
            Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 30f, EntityStates.LemurianBruiserMonster.FireMegaFireball.projectilePrefab, targetHurtbox);
            return newAimRay;
          }
          return aimRay;
        });
      }
      else
        Debug.LogError("Entropy: PredictiveMegaFireball IL Hook failed");
    }

    private void PredictiveFireball(ILContext il)
    {
      ILCursor c = new ILCursor(il);
      if (c.TryGotoNext(MoveType.After,
           x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
          ))
      {
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Ray, EntityStates.LemurianMonster.FireFireball, Ray>>((aimRay, self) =>
        {
          if (self.characterBody && !self.characterBody.isPlayerControlled)
          {
            HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
            Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 30f, EntityStates.LemurianMonster.FireFireball.projectilePrefab, targetHurtbox);
            return newAimRay;
          }
          return aimRay;
        });
      }
      else
        Debug.LogError("Entropy: PredictiveFireball IL Hook failed");
    }

    public static HurtBox GetMasterAITargetHurtbox(CharacterMaster cm)
    {
      if (cm && cm.aiComponents.Length > 0)
      {
        foreach (BaseAI ai in cm.aiComponents)
        {
          if (ai.currentEnemy != null && ai.currentEnemy.bestHurtBox != null)
            return ai.currentEnemy.bestHurtBox;
        }
      }
      return null;
    }

    public static HurtBox AcquireTarget(Ray aimRay, TeamIndex attackerTeam, float maxTargetAngle)
    {
      BullseyeSearch search = new BullseyeSearch();

      search.teamMaskFilter = TeamMask.allButNeutral;
      search.teamMaskFilter.RemoveTeam(attackerTeam);

      search.filterByLoS = true;
      search.searchOrigin = aimRay.origin;
      search.sortMode = BullseyeSearch.SortMode.Angle;
      search.maxDistanceFilter = 200f;
      search.maxAngleFilter = maxTargetAngle;
      search.searchDirection = aimRay.direction;
      search.RefreshCandidates();

      HurtBox targetHurtBox = search.GetResults().FirstOrDefault<HurtBox>();

      return targetHurtBox;
    }

    public static Ray PredictAimray(Ray aimRay, TeamIndex attackerTeam, float maxTargetAngle, float projectileSpeed, HurtBox targetHurtBox)
    {
      bool hasHurtbox;
      if (targetHurtBox == null)
        targetHurtBox = AcquireTarget(aimRay, attackerTeam, maxTargetAngle);
      hasHurtbox = targetHurtBox && targetHurtBox.healthComponent && targetHurtBox.healthComponent.body && targetHurtBox.healthComponent.body.characterMotor;

      if (hasHurtbox && projectileSpeed > 0f)
      {
        CharacterBody targetBody = targetHurtBox.healthComponent.body;
        Vector3 targetPosition = targetHurtBox.transform.position;
        Vector3 targetVelocity = targetBody.characterMotor.velocity;

        if (targetVelocity.sqrMagnitude > 0f && !(targetBody && targetBody.hasCloakBuff))   //Dont bother predicting stationary targets
        {
          //A very simplified way of estimating, won't be 100% accurate.
          Vector3 currentDistance = targetPosition - aimRay.origin;
          float timeToImpact = currentDistance.magnitude / projectileSpeed;

          //Vertical movenent isn't predicted well by self, so just use the target's current Y
          Vector3 lateralVelocity = new Vector3(targetVelocity.x, 0f, targetVelocity.z);
          Vector3 futurePosition = targetPosition + lateralVelocity * timeToImpact;

          //Only attempt prediction if player is jumping upwards.
          //Predicting downwards movement leads to groundshots.
          if (targetBody.characterMotor && !targetBody.characterMotor.isGrounded && targetVelocity.y > 0f)
          {
            //point + vt + 0.5at^2
            float futureY = targetPosition.y + targetVelocity.y * timeToImpact;
            futureY += 0.5f * Physics.gravity.y * timeToImpact * timeToImpact;
            futurePosition.y = futureY;
          }

          Ray newAimray = new Ray
          {
            origin = aimRay.origin,
            direction = (futurePosition - aimRay.origin).normalized
          };

          float angleBetweenVectors = Vector3.Angle(aimRay.direction, newAimray.direction);
          if (angleBetweenVectors <= maxTargetAngle)
          {
            return newAimray;
          }
        }
      }
      return aimRay;
    }

    public static Ray PredictAimrayPS(Ray aimRay, TeamIndex attackerTeam, float maxTargetAngle, GameObject projectilePrefab, HurtBox targetHurtBox)
    {
      float speed = -1f;
      if (projectilePrefab)
      {
        ProjectileSimple ps = projectilePrefab.GetComponent<ProjectileSimple>();
        if (ps)
          speed = ps.desiredForwardSpeed;
      }

      if (speed <= 0f)
      {
        Debug.LogError("Entropy: Could not get speed of ProjectileSimple.");
        return aimRay;
      }

      return speed > 0f ? PredictAimray(aimRay, attackerTeam, maxTargetAngle, speed, targetHurtBox) : aimRay;
    }
  }
}