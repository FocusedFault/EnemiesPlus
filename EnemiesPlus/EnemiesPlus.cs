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
  [BepInPlugin("com.Nuxlar.EnemiesPlus", "EnemiesPlus", "0.8.2")]

  public class EnemiesPlus : BaseUnityPlugin
  {
    private GameObject golem = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/GolemBody.prefab").WaitForCompletion();
    private GameObject bellMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellMaster.prefab").WaitForCompletion();
    private GameObject bell = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellBody.prefab").WaitForCompletion();
    private GameObject beetleGuard = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardBody.prefab").WaitForCompletion();
    private GameObject beetleGuardMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardMaster.prefab").WaitForCompletion();
    private BuffDef teamWarcryBuff = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/TeamWarCry/bdTeamWarCry.asset").WaitForCompletion();
    private GameObject frenzyAura = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamWarCry/TeamWarCryAura.prefab").WaitForCompletion(), "BGFrenzyAura");
    private static Material frenzyMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerBuffBillboard.mat").WaitForCompletion();

    public static BuffDef frenzyBuff;

    public void Awake()
    {
      SetupBuff();
      BuffBeamSetup();
      frenzyAura.GetComponent<DestroyOnTimer>().duration = 8f;
      frenzyAura.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sharedMaterial = frenzyMat;
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
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
      On.EntityStates.Bell.BellWeapon.BuffBeam.OnEnter += PreventBellBellBuff;
      On.EntityStates.Bell.BellWeapon.BuffBeam.GetMinimumInterruptPriority += BellPriority;
      On.EntityStates.BeetleMonster.HeadbuttState.OnEnter += UncloakBeetle;
      On.RoR2.HealthComponent.TakeDamage += UncloakBeetle2;
      On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += AddFrenzyVFX;
      RecalculateStatsAPI.GetStatCoefficients += AddFrenzyBehavior;
    }

    private void AddFrenzyVFX(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
    {
      orig(self);
      self.UpdateSingleTemporaryVisualEffect(ref self.teamWarCryEffectInstance, frenzyAura, self.bestFitRadius, self.HasBuff(frenzyBuff), "HeadCenter");
    }

    private void AddFrenzyBehavior(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
      if (sender && sender.HasBuff(frenzyBuff))
      {
        args.attackSpeedMultAdd += 0.3f;
        args.moveSpeedMultAdd += 0.3f;
      }
    }

    private void PreventBellBellBuff(On.EntityStates.Bell.BellWeapon.BuffBeam.orig_OnEnter orig, BuffBeam self)
    {
      IntPtr ptr = typeof(BaseState).GetMethod(nameof(BaseState.OnEnter)).MethodHandle.GetFunctionPointer();
      Action baseOnEnter = (Action)Activator.CreateInstance(typeof(Action), self, ptr);
      baseOnEnter();

      int num = (int)Util.PlaySound(BuffBeam.playBeamSoundString, self.gameObject);
      Ray aimRay = self.GetAimRay();
      BullseyeSearch bullseyeSearch = new BullseyeSearch();
      bullseyeSearch.teamMaskFilter = TeamMask.none;
      if ((bool)self.teamComponent)
        bullseyeSearch.teamMaskFilter.AddTeam(self.teamComponent.teamIndex);
      bullseyeSearch.filterByLoS = false;
      bullseyeSearch.maxDistanceFilter = 50f;
      bullseyeSearch.maxAngleFilter = 360f;
      bullseyeSearch.searchOrigin = aimRay.origin;
      bullseyeSearch.searchDirection = aimRay.direction;
      bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
      bullseyeSearch.RefreshCandidates();
      bullseyeSearch.FilterOutGameObject(self.gameObject);
      self.target = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
      Debug.LogFormat("Buffing target {0}", (object)self.target);
      if ((bool)self.target && self.target.healthComponent.body.name != "BellBody(Clone)")
      {
        self.targetBody = self.target.healthComponent.body;
        self.targetBody.AddBuff(RoR2Content.Buffs.Immune.buffIndex);
      }
      string childName = "Muzzle";
      Transform modelTransform = self.GetModelTransform();
      if (!(bool)modelTransform)
        return;
      ChildLocator component1 = modelTransform.GetComponent<ChildLocator>();
      if (!(bool)component1)
        return;
      self.muzzleTransform = component1.FindChild(childName);
      self.buffBeamInstance = GameObject.Instantiate<GameObject>(BuffBeam.buffBeamPrefab);
      ChildLocator component2 = self.buffBeamInstance.GetComponent<ChildLocator>();
      if ((bool)component2)
        self.beamTipTransform = component2.FindChild("BeamTip");
      self.healBeamCurve = self.buffBeamInstance.GetComponentInChildren<BezierCurveLine>();
    }

    private void UncloakBeetle2(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
    {
      if (self.body && self.body.name == "BeetleBody(Clone)")
      {
        if (self.body.HasBuff(RoR2Content.Buffs.AffixHauntedRecipient) && self.body.gameObject.GetComponent<ReflectiveShell>())
        {
          GameObject.Destroy(self.body.gameObject.GetComponent<ReflectiveShell>());
          self.body.skinIndex = 1;
          self.body.modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>().ApplySkin(1);
          self.body.RemoveBuff(RoR2Content.Buffs.AffixHauntedRecipient);
        }
      }
      orig(self, damageInfo);
    }
    private void UncloakBeetle(On.EntityStates.BeetleMonster.HeadbuttState.orig_OnEnter orig, HeadbuttState self)
    {
      if (self.characterBody.HasBuff(RoR2Content.Buffs.AffixHauntedRecipient) && self.characterBody.gameObject.GetComponent<ReflectiveShell>())
      {
        GameObject.Destroy(self.characterBody.gameObject.GetComponent<ReflectiveShell>());
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
      if (body.name == "BeetleBody(Clone)" && UnityEngine.Random.value < 0.5f)
      {
        body.gameObject.AddComponent<ReflectiveShell>();
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
    private void SetupBuff()
    {
      frenzyBuff = ScriptableObject.CreateInstance<BuffDef>();
      frenzyBuff.name = "BGFrenzy";
      frenzyBuff.canStack = false;
      frenzyBuff.isCooldown = false;
      frenzyBuff.isDebuff = false;
      frenzyBuff.buffColor = Color.yellow;
      frenzyBuff.iconSprite = teamWarcryBuff.iconSprite;
      (frenzyBuff as UnityEngine.Object).name = frenzyBuff.name;

      ContentAddition.AddBuffDef(frenzyBuff);
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
            Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 15f, EntityStates.LemurianBruiserMonster.FireMegaFireball.projectilePrefab, targetHurtbox);
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
            Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 15f, EntityStates.LemurianMonster.FireFireball.projectilePrefab, targetHurtbox);
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