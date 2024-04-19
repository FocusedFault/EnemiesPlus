using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EnemiesPlus
{
    public class BeetleSpit : BaseState
    {
        public static float baseDuration = 1.25f;
        public static float damageCoefficient;
        public static string attackSoundString = "Play_beetle_worker_attack";
        private GameObject projectilePrefab = EnemiesPlus.beetleSpit;
        private bool hasFired;
        private float stopwatch;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.stopwatch = 0.0f;
            this.duration = baseDuration / this.attackSpeedStat;
            this.GetModelTransform();
            Util.PlayAttackSpeedSound(attackSoundString, this.gameObject, 1.5f);
            this.PlayCrossfade("Body", "EmoteSurprise", "Headbutt.playbackRate", this.duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.deltaTime;
            if (!this.hasFired && (double)this.stopwatch >= 1)
            {
                this.hasFired = true;
                Ray aimRay = this.GetAimRay();
                ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.damageStat * 1, 0.0f, Util.CheckRoll(this.critStat, this.characterBody.master));
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
