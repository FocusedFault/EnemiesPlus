using BepInEx;
using EntityStates;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using RoR2.Projectile;
using RoR2.Skills;
using HG;

namespace EnemiesPlus
{
  [BepInPlugin("com.Nuxlar.EnemiesPlus", "EnemiesPlus", "1.0.3")]

  public class EnemiesPlus : BaseUnityPlugin
  {
    private GameObject beetleMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleMaster.prefab").WaitForCompletion();
    private GameObject beetleBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleBody.prefab").WaitForCompletion();
    private GameObject lemMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/LemurianMaster.prefab").WaitForCompletion();
    private GameObject lemBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/LemurianBody.prefab").WaitForCompletion();
    private GameObject impMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/ImpMaster.prefab").WaitForCompletion();
    private GameObject impBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/ImpBody.prefab").WaitForCompletion();
    private GameObject beetleQueenSpit = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpit.prefab").WaitForCompletion();
    private GameObject beetleQueenAcid = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenAcid.prefab").WaitForCompletion();
    public static GameObject beetleSpit = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpit.prefab").WaitForCompletion(), "BeetleSpitProjectileNux");
    public static GameObject beetleSpitGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpitGhost.prefab").WaitForCompletion(), "BeetleSpitProjectileGhostNux");
    public static GameObject beetleSpitExplosion = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleSpitExplosion.prefab").WaitForCompletion(), "BeetleSpitExplosionNux");
    public static GameObject voidSpike = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpVoidspikeProjectile.prefab").WaitForCompletion(), "VoidSpikeNux");
    private SpawnCard greaterWispCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/GreaterWisp/cscGreaterWisp.asset").WaitForCompletion();
    private SpawnCard bellCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Bell/cscBell.asset").WaitForCompletion();
    private BuffDef beetleJuice = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Beetle/bdBeetleJuice.asset").WaitForCompletion();
    private GameObject beetleGuard = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardBody.prefab").WaitForCompletion();
    private GameObject beetleGuardMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardMaster.prefab").WaitForCompletion();
    private BuffDef teamWarcryBuff = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/TeamWarCry/bdTeamWarCry.asset").WaitForCompletion();
    private GameObject frenzyAura = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamWarCry/TeamWarCryAura.prefab").WaitForCompletion(), "BGFrenzyAura");
    private static Material frenzyMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerBuffBillboard.mat").WaitForCompletion();
    private GameObject bellMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellMaster.prefab").WaitForCompletion();
    private GameObject bell = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellBody.prefab").WaitForCompletion();
    private Sprite fireBuffSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffOnFireIcon.tif").WaitForCompletion();
    private GameObject lunarGolemMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemMaster.prefab").WaitForCompletion();
    private GameObject lunarGolem = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemBody.prefab").WaitForCompletion();
    private GameObject lunarWisp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispBody.prefab").WaitForCompletion();
    private GameObject lunarWispMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab").WaitForCompletion();
    private GameObject lunarWispTrackingBomb = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion(), "LunarWispOrbNux");
    private GameObject magmaWorm = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaWormBody.prefab").WaitForCompletion();
    private GameObject electricWorm = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricWormBody.prefab").WaitForCompletion();
    private GameObject magmaWormMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaWormMaster.prefab").WaitForCompletion();
    private GameObject electricWormMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricWormMaster.prefab").WaitForCompletion();

    public static GameObject helfireIgniteEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BurnNearby/HelfireIgniteEffect.prefab").WaitForCompletion();
    public static BuffDef frenzyBuff;
    public static BuffDef helfireBuff;
    private DamageAPI.ModdedDamageType beetleJuiceDT;
    private DamageAPI.ModdedDamageType helfireDT;
    private DotController.DotIndex helfireDotIdx;

    public void Awake()
    {
      beetleJuice.canStack = true;
      beetleJuiceDT = DamageAPI.ReserveDamageType();

      DamageAPI.ModdedDamageTypeHolderComponent mdc = beetleSpit.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
      mdc.Add(beetleJuiceDT);
      DamageAPI.ModdedDamageTypeHolderComponent mdc2 = beetleQueenSpit.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
      mdc2.Add(beetleJuiceDT);
      DamageAPI.ModdedDamageTypeHolderComponent mdc3 = beetleQueenAcid.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
      mdc3.Add(beetleJuiceDT);

      helfireDT = DamageAPI.ReserveDamageType();
      DamageAPI.ModdedDamageTypeHolderComponent mdc4 = lunarWispTrackingBomb.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
      mdc4.Add(helfireDT);

      greaterWispCard.directorCreditCost = 120;

      ContentAddition.AddEntityState<BeetleSpit>(out _);
      ContentAddition.AddEntityState<SpikeSlash>(out _);
      ContentAddition.AddEntityState<RallyCry>(out _);

      beetleQueenSpit.GetComponent<ProjectileImpactExplosion>().destroyOnEnemy = true;

      beetleSpit.transform.localScale /= 2;
      beetleSpit.GetComponent<Rigidbody>().useGravity = false;
      beetleSpit.GetComponent<ProjectileController>().ghostPrefab = beetleSpitGhost;
      ProjectileImpactExplosion pie = beetleSpit.GetComponent<ProjectileImpactExplosion>();
      pie.impactEffect = beetleSpitExplosion;
      pie.childrenProjectilePrefab = null;
      pie.destroyOnEnemy = true;
      pie.fireChildren = false;
      pie.blastRadius = 2f;

      beetleSpitGhost.transform.localScale /= 2;
      beetleSpitGhost.transform.GetChild(1).localScale /= 2;
      beetleSpitGhost.transform.GetChild(0).GetChild(0).localScale /= 2;
      beetleSpitGhost.transform.GetChild(1).GetChild(0).localScale /= 2;
      beetleSpitGhost.transform.GetChild(1).GetChild(1).localScale /= 2;

      beetleSpitExplosion.transform.GetChild(0).localScale /= 2;
      foreach (Transform child in beetleSpitExplosion.transform.GetChild(0))
      {
        child.localScale /= 2;
      }

      ContentAddition.AddEffect(beetleSpitExplosion);

      SetupBGBuff();
      frenzyAura.GetComponent<DestroyOnTimer>().duration = 8f;
      frenzyAura.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sharedMaterial = frenzyMat;
      ContentAddition.AddEntityState<RallyCry>(out _);
      AISkillDriver rallyCryDriver = beetleGuardMaster.AddComponent<AISkillDriver>();
      rallyCryDriver.customName = "RallyCry";
      rallyCryDriver.skillSlot = SkillSlot.Utility;
      rallyCryDriver.requireSkillReady = true;
      // rallyCryDriver.maxUserHealthFraction = 0.8f;
      rallyCryDriver.movementType = AISkillDriver.MovementType.Stop;
      beetleGuard.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef.activationState = new SerializableEntityStateType(typeof(RallyCry));

      SetupHelfireBuff();

      BuffBeamSetup();

      SkillDef impSlash = impBody.GetComponent<SkillLocator>().primary.skillFamily.variants[0].skillDef;
      impSlash.activationState = new SerializableEntityStateType(typeof(SpikeSlash));
      impSlash.baseRechargeInterval = 5f;

      lemBody.GetComponent<SkillLocator>().secondary.skillFamily.variants[0].skillDef.baseRechargeInterval = 1f;

      ProjectileImpactExplosion pie2 = voidSpike.GetComponent<ProjectileImpactExplosion>();
      pie2.destroyOnWorld = true;
      // lem hitbox orig 4.0121 11.522 7.3294
      SkillLocator skillLocator = beetleBody.GetComponent<SkillLocator>();
      skillLocator.primary.skillFamily.variants[0].skillDef.activationState = new SerializableEntityStateType(typeof(BeetleSpit));
      skillLocator.primary.skillFamily.variants[0].skillDef.baseRechargeInterval = 3f;
      skillLocator.secondary.skillFamily.variants[0].skillDef.baseMaxStock = 0;

      ParticleSystem[] magmaWormPS = magmaWorm.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem ps in magmaWormPS)
        ps.startSize *= 2;

      SkillDef magmaWormUtilityDef = magmaWorm.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef;
      magmaWormUtilityDef.activationState = new SerializableEntityStateType(typeof(EntityStates.MagmaWorm.Leap));
      magmaWormUtilityDef.baseRechargeInterval = 60f;
      magmaWormUtilityDef.activationStateMachineName = "Weapon";

      foreach (AISkillDriver driver in magmaWormMaster.GetComponents<AISkillDriver>())
      {
        switch (driver.customName)
        {
          case "Blink":
            driver.shouldSprint = true;
            driver.minDistance = 0f;
            driver.aimType = AISkillDriver.AimType.AtMoveTarget;
            break;
          default:
            driver.skillSlot = SkillSlot.None;
            break;
        }
      }

      ParticleSystem[] electricWormPS = electricWorm.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem ps in electricWormPS)
        ps.startSize *= 2;

      SkillDef electicWormUtilityDef = electricWorm.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef;
      electicWormUtilityDef.activationState = new SerializableEntityStateType(typeof(EntityStates.MagmaWorm.Leap));
      electicWormUtilityDef.baseRechargeInterval = 60f;
      electicWormUtilityDef.activationStateMachineName = "Weapon";

      foreach (AISkillDriver driver in electricWormMaster.GetComponents<AISkillDriver>())
      {
        switch (driver.customName)
        {
          case "Blink":
            driver.shouldSprint = true;
            driver.minDistance = 0f;
            driver.aimType = AISkillDriver.AimType.AtMoveTarget;
            break;
          default:
            driver.skillSlot = SkillSlot.None;
            break;
        }
      }

      foreach (AISkillDriver driver in beetleMaster.GetComponents<AISkillDriver>())
      {
        switch (driver.customName)
        {
          case "HeadbuttOffNodegraph":
            driver.minDistance = 5f;
            driver.maxDistance = 20f;
            break;
          case "ChaseOffNodegraph":
            driver.maxDistance = 5f;
            driver.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            break;
          case "FollowNodeGraphToTarget":
            driver.minDistance = 15f;
            break;
        }
      }

      foreach (AISkillDriver driver in impMaster.GetComponents<AISkillDriver>())
      {
        switch (driver.customName)
        {
          case "Slash":
            driver.minDistance = 15f;
            driver.maxDistance = 45f;
            break;
          case "LeaveNodegraph":
            driver.minDistance = 0f;
            driver.maxDistance = 15f;
            driver.shouldSprint = true;
            driver.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            break;
          case "StrafeBecausePrimaryIsntReady":
            driver.minDistance = 15f;
            break;
          case "BlinkBecauseClose":
            driver.minDistance = 25f;
            driver.maxDistance = 45f;
            break;
          case "PathToTarget":
            driver.minDistance = 15f;
            break;
        }
      }

      foreach (AISkillDriver driver in lunarWispMaster.GetComponents<AISkillDriver>())
      {
        switch (driver.customName)
        {
          case "Back Up":
            driver.maxDistance = 15f;
            break;
          case "Minigun":
            driver.minDistance = 15f;
            driver.maxDistance = 75f;
            break;
          case "Chase":
            driver.minDistance = 30f;
            driver.shouldSprint = true;
            break;
        }
      }

      CharacterBody lunarWispBody = lunarWisp.GetComponent<CharacterBody>();
      lunarWispBody.baseMoveSpeed = 20f;
      lunarWispBody.baseAcceleration = 20f;

      lunarGolem.GetComponent<SetStateOnHurt>().canBeHitStunned = false;

      lunarGolem.GetComponent<SkillLocator>().secondary.skillFamily.variants[0].skillDef.interruptPriority = InterruptPriority.Death;
      lunarGolemMaster.GetComponents<AISkillDriver>().Where(driver => driver.customName == "StrafeAndShoot").First().requireSkillReady = true;

      AISkillDriver lunarShellDriver = lunarGolemMaster.AddComponent<AISkillDriver>();
      lunarShellDriver.customName = "LunarShell";
      lunarShellDriver.skillSlot = SkillSlot.Secondary;
      lunarShellDriver.requireSkillReady = true;
      lunarShellDriver.requireEquipmentReady = false;
      lunarShellDriver.driverUpdateTimerOverride = 3f;
      // lunarShellDriver.maxUserHealthFraction = 0.75f;
      lunarShellDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
      lunarShellDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;

      // On.EntityStates.AI.BaseAIState.AimAt += PredictiveAiming;
      On.RoR2.CharacterMaster.OnBodyStart += RearrangeSkillDrivers;
      On.RoR2.CharacterBody.OnBuffFirstStackGained += AddHelfireEffect;
      On.RoR2.CharacterBody.OnBuffFinalStackLost += RemoveHelfireEffect;

      On.EntityStates.LemurianMonster.Bite.OnEnter += BiteLeap;
      On.EntityStates.Wisp1Monster.FireEmbers.OnEnter += IncreaseWispEmbers;
      On.EntityStates.GolemMonster.ChargeLaser.Update += TweakGolemLaserAim;
      On.EntityStates.Bell.BellWeapon.BuffBeam.OnEnter += PreventBellBellBuff;
      On.EntityStates.Bell.BellWeapon.BuffBeam.OnExit += RemoveInvincibility;
      On.EntityStates.Bell.BellWeapon.BuffBeam.GetMinimumInterruptPriority += BellPriority;
      On.EntityStates.LunarWisp.SeekingBomb.OnEnter += ReplaceSeekingBombPrefab;
      On.RoR2.WormBodyPositionsDriver.FixedUpdateServer += RemoveRandomTurns;

      On.RoR2.GlobalEventManager.OnHitEnemy += AddBeetleJuiceStack;
      On.RoR2.GlobalEventManager.OnHitEnemy += ApplyHelfire;
      On.RoR2.HealthComponent.Heal += PreventHelfireHeal;
      On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += AddFrenzyVFX;

      DotAPI.CustomDotBehaviour customHelfireDotBehavior = AddPercentHelfireDamage;
      helfireDotIdx = DotAPI.RegisterDotDef(0.2f, 0f, DamageColorIndex.DeathMark, helfireBuff, customHelfireDotBehavior);

      RecalculateStatsAPI.GetStatCoefficients += AddFrenzyBehavior;
      RecalculateStatsAPI.GetStatCoefficients += AddLunarShellBehavior;
    }

    private void RemoveRandomTurns(On.RoR2.WormBodyPositionsDriver.orig_FixedUpdateServer orig, RoR2.WormBodyPositionsDriver self)
    {
      CharacterBody body = self.gameObject.GetComponent<CharacterBody>();
      Vector3 targetPosition = self.referenceTransform.position;

      if (body && body.master)
      {
        if (self.gameObject.GetComponents<EntityStateMachine>().Where(machine => machine.customName == "Weapon").First().state.GetType() != typeof(EntityStates.MagmaWorm.Leap))
        {
          BaseAI baseAI = body.masterObject.GetComponent<BaseAI>();
          if (baseAI && baseAI.currentEnemy != null && baseAI.currentEnemy.characterBody != null)
            targetPosition = baseAI.currentEnemy.characterBody.corePosition;
        }
      }

      float speedMultiplier = self.wormBodyPositions.speedMultiplier;
      Vector3 normalized = (targetPosition - self.chaserPosition).normalized;
      float num1 = (float)((self.chaserIsUnderground ? (double)self.maxTurnSpeed : (double)self.maxTurnSpeed * (double)self.turnRateCoefficientAboveGround) * (Math.PI / 180.0));
      Vector3 vector3 = Vector3.RotateTowards(new Vector3(self.chaserVelocity.x, 0.0f, self.chaserVelocity.z), new Vector3(normalized.x, 0.0f, normalized.z) * speedMultiplier, num1 * Time.fixedDeltaTime, float.PositiveInfinity);
      vector3 = vector3.normalized * speedMultiplier;
      float num2 = targetPosition.y - self.chaserPosition.y;
      float num3 = -self.chaserVelocity.y * self.yDamperConstant;
      float num4 = num2 * self.ySpringConstant;
      if (self.allowShoving && (double)Mathf.Abs(self.chaserVelocity.y) < (double)self.yShoveVelocityThreshold && (double)num2 > (double)self.yShovePositionThreshold)
        self.chaserVelocity = self.chaserVelocity.XAZ(self.chaserVelocity.y + self.yShoveForce * Time.fixedDeltaTime);
      if (!self.chaserIsUnderground)
      {
        num4 *= self.wormForceCoefficientAboveGround;
        num3 *= self.wormForceCoefficientAboveGround;
      }
      self.chaserVelocity = self.chaserVelocity.XAZ(self.chaserVelocity.y + (num4 + num3) * Time.fixedDeltaTime);
      self.chaserVelocity += Physics.gravity * Time.fixedDeltaTime;
      self.chaserVelocity = new Vector3(vector3.x, self.chaserVelocity.y, vector3.z);
      self.chaserPosition += self.chaserVelocity * Time.fixedDeltaTime;
      self.chasePositionVisualizer.position = self.chaserPosition;
      self.chaserIsUnderground = -(double)num2 < (double)self.wormBodyPositions.undergroundTestYOffset;
      self.keyFrameGenerationTimer -= Time.deltaTime;
      if ((double)self.keyFrameGenerationTimer > 0.0)
        return;
      self.keyFrameGenerationTimer = self.keyFrameGenerationInterval;
      self.wormBodyPositions.AttemptToGenerateKeyFrame(self.wormBodyPositions.GetSynchronizedTimeStamp() + self.wormBodyPositions.followDelay, self.chaserPosition, self.chaserVelocity);
    }

    private void ReplaceSeekingBombPrefab(On.EntityStates.LunarWisp.SeekingBomb.orig_OnEnter orig, EntityStates.LunarWisp.SeekingBomb self)
    {
      EntityStates.LunarWisp.SeekingBomb.projectilePrefab = lunarWispTrackingBomb;
      orig(self);
    }

    private void AddHelfireEffect(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
    {
      orig(self, buffDef);
      if (buffDef == helfireBuff)
        self.gameObject.AddComponent<NuxHelfireEffectController>();
    }

    private void RemoveHelfireEffect(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
    {
      orig(self, buffDef);
      if (buffDef == helfireBuff)
        GameObject.Destroy(self.gameObject.GetComponent<NuxHelfireEffectController>());
    }

    private void AddPercentHelfireDamage(DotController self, DotController.DotStack dotStack)
    {
      if (dotStack.dotIndex == helfireDotIdx)
      {
        if (self.victimBody && self.victimBody.healthComponent)
        {
          dotStack.damageType |= DamageType.NonLethal;
          dotStack.damage = self.victimBody.healthComponent.fullHealth * 0.1f / 10f * 0.2f; ;
        }
      }
    }

    private void AddLunarShellBehavior(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
      if (sender && sender.HasBuff(RoR2Content.Buffs.LunarShell))
      {
        args.armorAdd += 200f;
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
      buffBeam.baseRechargeInterval = 30;

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
      buffBeamDriver.maxUserHealthFraction = 0.8f;
      buffBeamDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
    }

    private void TweakGolemLaserAim(On.EntityStates.GolemMonster.ChargeLaser.orig_Update orig, EntityStates.GolemMonster.ChargeLaser self)
    {
      IntPtr ptr = typeof(BaseState).GetMethod(nameof(BaseState.Update)).MethodHandle.GetFunctionPointer();
      Action baseUpdate = (Action)Activator.CreateInstance(typeof(Action), self, ptr);
      baseUpdate();

      if (!(bool)self.laserEffect || !(bool)self.laserLineComponent)
        return;
      float distance = 1000f;
      Ray aimRay = self.GetAimRay();
      Vector3 position = self.laserEffect.transform.parent.position;
      Vector3 point = aimRay.GetPoint(distance);
      self.laserDirection = point - position;
      Ray ray = aimRay;
      RaycastHit raycastHit;
      double maxDistance = (double)distance;
      if (Physics.Raycast(ray, out raycastHit, (float)maxDistance, LayerIndex.CommonMasks.bullet))
        point = raycastHit.point;
      self.laserLineComponent.SetPosition(0, position);
      self.laserLineComponent.SetPosition(1, point);
      float num1;
      if ((double)self.duration - (double)self.age > 0.5)
      {
        num1 = self.age / self.duration;
      }
      else
      {
        self.flashTimer -= Time.deltaTime;
        if ((double)self.flashTimer <= 0.0)
        {
          self.laserOn = !self.laserOn;
          self.flashTimer = 0.0333333351f;
        }
        num1 = self.laserOn ? 1f : 0.0f;
      }
      float num2 = num1 * EntityStates.GolemMonster.ChargeLaser.laserMaxWidth;
      self.laserLineComponent.startWidth = num2;
      self.laserLineComponent.endWidth = num2;
    }

    private void IncreaseWispEmbers(On.EntityStates.Wisp1Monster.FireEmbers.orig_OnEnter orig, EntityStates.Wisp1Monster.FireEmbers self)
    {
      EntityStates.Wisp1Monster.FireEmbers.damageCoefficient = 0.75f; // 1.5
      EntityStates.Wisp1Monster.FireEmbers.bulletCount = 6; // 3
      orig(self);
    }

    private void PreventBellBellBuff(On.EntityStates.Bell.BellWeapon.BuffBeam.orig_OnEnter orig, EntityStates.Bell.BellWeapon.BuffBeam self)
    {
      IntPtr ptr = typeof(BaseState).GetMethod(nameof(BaseState.OnEnter)).MethodHandle.GetFunctionPointer();
      Action baseOnEnter = (Action)Activator.CreateInstance(typeof(Action), self, ptr);
      baseOnEnter();

      Util.PlaySound(EntityStates.Bell.BellWeapon.BuffBeam.playBeamSoundString, self.gameObject);
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
      bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
      bullseyeSearch.RefreshCandidates();
      bullseyeSearch.FilterOutGameObject(self.gameObject);
      List<HurtBox> hurtBoxes = bullseyeSearch.GetResults().ToList();

      if (hurtBoxes.Count > 0)
      {
        foreach (HurtBox hurtBox in hurtBoxes)
        {
          if (hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.alive)
          {
            CharacterBody targetBody = hurtBox.healthComponent.body;
            if (targetBody && targetBody.name != "BellBody(Clone)" && (targetBody.hullClassification == HullClassification.Golem || targetBody.hullClassification == HullClassification.BeetleQueen))
            {
              self.target = hurtBox;
              self.targetBody = targetBody;
              targetBody.AddBuff(RoR2Content.Buffs.Immune.buffIndex);
              break;
            }
          }
        }
      }

      if (!self.target && !self.targetBody)
      {
        self.outer.SetNextStateToMain();
        return;
      }

      string childName = "Muzzle";
      Transform modelTransform = self.GetModelTransform();
      if (!(bool)modelTransform)
        return;
      ChildLocator component1 = modelTransform.GetComponent<ChildLocator>();
      if (!(bool)component1)
        return;
      self.muzzleTransform = component1.FindChild(childName);
      self.buffBeamInstance = GameObject.Instantiate<GameObject>(EntityStates.Bell.BellWeapon.BuffBeam.buffBeamPrefab);
      ChildLocator component2 = self.buffBeamInstance.GetComponent<ChildLocator>();
      if ((bool)component2)
        self.beamTipTransform = component2.FindChild("BeamTip");
      self.healBeamCurve = self.buffBeamInstance.GetComponentInChildren<BezierCurveLine>();
    }

    private void RemoveInvincibility(On.EntityStates.Bell.BellWeapon.BuffBeam.orig_OnExit orig, EntityStates.Bell.BellWeapon.BuffBeam self)
    {
      orig(self);
      if ((bool)self.targetBody)
        self.targetBody.RemoveBuff(RoR2Content.Buffs.Immune.buffIndex);
    }

    private InterruptPriority BellPriority(On.EntityStates.Bell.BellWeapon.BuffBeam.orig_GetMinimumInterruptPriority orig, EntityStates.Bell.BellWeapon.BuffBeam self)
    {
      return InterruptPriority.Death;
    }

    private void SetupBGBuff()
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

    private void SetupHelfireBuff()
    {
      helfireBuff = ScriptableObject.CreateInstance<BuffDef>();
      helfireBuff.name = "Helfire";
      helfireBuff.canStack = true;
      helfireBuff.isCooldown = false;
      helfireBuff.isDebuff = true;
      helfireBuff.buffColor = Color.cyan;
      helfireBuff.iconSprite = fireBuffSprite;
      (helfireBuff as UnityEngine.Object).name = helfireBuff.name;

      ContentAddition.AddBuffDef(helfireBuff);
    }

    private void RearrangeSkillDrivers(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
    {
      orig(self, body);
      if (body.name == "BeetleGuardBody(Clone)")
      {
        int desiredPosition = 2;
        AISkillDriver[] arr = self.gameObject.GetComponent<BaseAI>().skillDrivers;

        if (desiredPosition >= 0 && desiredPosition < arr.Length)
        {
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
          AISkillDriver temp = arr[desiredPosition];
          arr[desiredPosition] = arr[arr.Length - 1];
          arr[arr.Length - 1] = temp;
        }
        self.gameObject.GetComponent<BaseAI>().skillDrivers = arr;
      }
      if (body.name == "LunarGolemBody(Clone)")
      {
        int desiredPosition = 0;
        AISkillDriver[] arr = self.gameObject.GetComponent<BaseAI>().skillDrivers;

        if (desiredPosition >= 0 && desiredPosition < arr.Length)
        {
          List<AISkillDriver> list = new();
          list.Add(arr[arr.Length - 1]);
          for (int i = 0; i < arr.Length - 1; i++)
          {
            if (i == arr.Length - 1)
              continue;
            list.Add(arr[i]);
          }
          arr = list.ToArray();
        }
        self.gameObject.GetComponent<BaseAI>().skillDrivers = arr;
      }
      if (body.name == "MagmaWormBody(Clone)")
      {
        int desiredPosition = 0;
        AISkillDriver[] arr = self.gameObject.GetComponent<BaseAI>().skillDrivers;

        if (desiredPosition >= 0 && desiredPosition < arr.Length)
        {
          List<AISkillDriver> list = new();
          list.Add(arr[1]);
          for (int i = 0; i < arr.Length - 1; i++)
          {
            if (i == arr.Length - 1)
              continue;
            list.Add(arr[i]);
          }
          arr = list.ToArray();
        }
        self.gameObject.GetComponent<BaseAI>().skillDrivers = arr;
      }
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
        args.attackSpeedMultAdd += 0.25f;
        args.moveSpeedMultAdd += 0.25f;
      }
    }

    private float PreventHelfireHeal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
    {
      if (self.body && self.body.HasBuff(helfireBuff))
        return 0f;
      else
        return orig(self, amount, procChainMask, nonRegen);
    }

    private void ApplyHelfire(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
    {
      orig(self, damageInfo, victim);
      if (damageInfo.attacker && victim && !damageInfo.rejected)
      {
        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
        if (attackerBody && victimBody)
        {
          if (attackerBody.HasBuff(RoR2Content.Buffs.LunarShell) || DamageAPI.HasModdedDamageType(damageInfo, helfireDT))
            DotController.InflictDot(victim, damageInfo.attacker, helfireDotIdx, 10f, 1f);
        }
      }
    }

    private void AddBeetleJuiceStack(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
    {
      orig(self, damageInfo, victim);
      if (damageInfo.HasModdedDamageType(beetleJuiceDT) && victim && !damageInfo.rejected)
      {
        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
        if (victimBody)
        {
          victimBody.AddTimedBuff(beetleJuice, 5f);
          foreach (CharacterBody.TimedBuff buff in victimBody.timedBuffs)
          {
            if (buff.buffIndex == RoR2Content.Buffs.BeetleJuice.buffIndex)
            {
              buff.timer = 5f;
            }
          }
        }
      }
    }

    private void BiteLeap(On.EntityStates.LemurianMonster.Bite.orig_OnEnter orig, EntityStates.LemurianMonster.Bite self)
    {
      orig(self);
      Vector3 leapDirection = self.GetAimRay().direction;
      leapDirection.y = 0.0f;
      float magnitude = leapDirection.magnitude;
      if ((double)magnitude > 0.0)
        leapDirection /= magnitude;
      self.characterMotor.velocity = (leapDirection * self.characterBody.moveSpeed * 2f) with
      {
        y = self.characterBody.jumpPower * 0.25f
      };
      self.characterMotor.Motor.ForceUnground();
    }
  }
}