using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EnemiesPlus
{
    public class BeetleSpit : BaseState
    {
        public static float baseDuration = 1.5f;
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
            // Util.PlaySound(attackSoundString, this.gameObject);
            this.PlayCrossfade("Body", "EmoteSurprise", "Headbutt.playbackRate", this.duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.deltaTime;
            if (!this.hasFired && (double)this.stopwatch >= 1.25)
            {
                this.hasFired = true;
                Ray aimRay = this.GetAimRay();
                float projectileHspeed = 50;
                LayerIndex layerIndex = LayerIndex.world;
                int mask1 = (int)layerIndex.mask;
                layerIndex = LayerIndex.entityPrecise;
                int mask2 = (int)layerIndex.mask;
                LayerMask layerMask = (LayerMask)(mask1 | mask2);
                RaycastHit hitInfo;
                if (Util.CharacterRaycast(this.gameObject, aimRay, out hitInfo, float.PositiveInfinity, layerMask, QueryTriggerInteraction.Ignore))
                {
                    float num = projectileHspeed;
                    Vector3 vector3_1 = hitInfo.point - aimRay.origin;
                    Vector2 vector2 = new Vector2(vector3_1.x, vector3_1.z);
                    float magnitude1 = vector2.magnitude;
                    float initialYspeed = Trajectory.CalculateInitialYSpeed(magnitude1 / num, vector3_1.y);
                    Vector3 vector3_2 = new Vector3(vector2.x / magnitude1 * num, initialYspeed, vector2.y / magnitude1 * num);
                    float magnitude2 = vector3_2.magnitude;
                    aimRay.direction = vector3_2 / magnitude2;
                }
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
