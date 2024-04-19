using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using EntityStates.BeetleGuardMonster;
using System.Linq;
using System.Collections.Generic;

namespace EnemiesPlus
{
    public class RallyCry : BaseState
    {
        public float baseDuration = 3.5f;
        public float buffDuration = 8f;
        private Animator modelAnimator;
        private List<HurtBox> nearbyAllies;
        private float duration;
        private bool hasCastBuff;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            Ray aimRay = this.GetAimRay();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.teamMaskFilter = TeamMask.none;
            if ((bool)this.teamComponent)
                bullseyeSearch.teamMaskFilter.AddTeam(this.teamComponent.teamIndex);
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.maxDistanceFilter = 16f;
            bullseyeSearch.maxAngleFilter = 360f;
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.RefreshCandidates();
            bullseyeSearch.FilterOutGameObject(this.gameObject);
            this.nearbyAllies = bullseyeSearch.GetResults().ToList<HurtBox>();
            this.modelAnimator = this.GetModelAnimator();
            if (!(bool)this.modelAnimator)
                return;
            Util.PlayAttackSpeedSound("Play_beetle_queen_attack1", this.gameObject, 5);
            this.PlayCrossfade("Body", nameof(DefenseUp), "DefenseUp.playbackRate", this.duration, 0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((bool)this.modelAnimator && (double)this.modelAnimator.GetFloat("DefenseUp.activate") > 0.5 && !this.hasCastBuff)
            {
                ScaleParticleSystemDuration component = Object.Instantiate<GameObject>(DefenseUp.defenseUpPrefab, this.transform.position, Quaternion.identity, this.transform).GetComponent<ScaleParticleSystemDuration>();
                if ((bool)component)
                    component.newDuration = DefenseUp.buffDuration;
                this.hasCastBuff = true;
                if (NetworkServer.active)
                {
                    this.characterBody.AddTimedBuff(RoR2Content.Buffs.SmallArmorBoost, DefenseUp.buffDuration);
                    foreach (HurtBox nearbyAlly in this.nearbyAllies)
                    {
                        if (nearbyAlly.healthComponent.body && !nearbyAlly.healthComponent.body.HasBuff(EnemiesPlus.frenzyBuff))
                            nearbyAlly.healthComponent.body.AddTimedBuff(EnemiesPlus.frenzyBuff, this.buffDuration);
                    }
                }
            }
            if ((double)this.fixedAge < (double)this.duration || !this.isAuthority)
                return;
            this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}