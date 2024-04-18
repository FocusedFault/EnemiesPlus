using EntityStates;
using EntityStates.ImpMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EnemiesPlus
{
    public class SpikeSlash : BaseState
    {
        public static float baseDuration = 3.5f;
        public static float damageCoefficient = 4f;
        public static float procCoefficient;
        public static float selfForce;
        public static float forceMagnitude = 16f;
        public static GameObject hitEffectPrefab;
        public static GameObject swipeEffectPrefab;
        public static string enterSoundString;
        public static string slashSoundString;
        public static float walkSpeedPenaltyCoefficient;
        public static GameObject projectilePrefab = EnemiesPlus.voidSpike;
        private Animator modelAnimator;
        private float duration;
        private int slashCount;
        private Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = DoubleSlash.baseDuration / this.attackSpeedStat;
            this.modelAnimator = this.GetModelAnimator();
            this.modelTransform = this.GetModelTransform();
            this.characterMotor.walkSpeedPenaltyCoefficient = DoubleSlash.walkSpeedPenaltyCoefficient;
            int num = (int)Util.PlayAttackSpeedSound(DoubleSlash.enterSoundString, this.gameObject, this.attackSpeedStat);
            if ((bool)(Object)this.modelAnimator)
            {
                this.PlayAnimation("Gesture, Additive", "DoubleSlash", "DoubleSlash.playbackRate", this.duration);
                this.PlayAnimation("Gesture, Override", "DoubleSlash", "DoubleSlash.playbackRate", this.duration);
            }
            if (!(bool)(Object)this.characterBody)
                return;
            this.characterBody.SetAimTimer(this.duration + 2f);
        }

        public override void OnExit()
        {
            this.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            base.OnExit();
        }

        private void HandleSlash(string animatorParamName, string muzzleName, string hitBoxGroupName)
        {
            if ((double)this.modelAnimator.GetFloat(animatorParamName) <= 0.100000001490116)
                return;
            Util.PlaySound(DoubleSlash.slashSoundString, this.gameObject);
            EffectManager.SimpleMuzzleFlash(DoubleSlash.swipeEffectPrefab, this.gameObject, muzzleName, true);
            ++this.slashCount;
            Ray aimRay = this.GetAimRay();
            ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.damageStat * 1, 0.0f, Util.CheckRoll(this.critStat, this.characterBody.master));
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && (bool)(Object)this.modelAnimator)
            {
                switch (this.slashCount)
                {
                    case 0:
                        this.HandleSlash("HandR.hitBoxActive", "SwipeRight", "HandR");
                        break;
                    case 1:
                        this.HandleSlash("HandL.hitBoxActive", "SwipeLeft", "HandL");
                        break;
                }
            }
            if ((double)this.fixedAge < (double)this.duration || !this.isAuthority)
                return;
            this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
