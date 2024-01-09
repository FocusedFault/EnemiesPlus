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
        public static float baseDuration = 3.5f;
        public static float buffDuration = 8f;
        public static GameObject defenseUpPrefab;
        private Animator modelAnimator;
        private List<HurtBox> nearbyAllies;
        private float duration;
        private bool hasCastBuff;

        public override void OnEnter()
        {
            base.OnEnter();
            Debug.LogWarning(DefenseUp.buffDuration);
            this.duration = DefenseUp.baseDuration / this.attackSpeedStat;
            Ray aimRay = this.GetAimRay();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.teamMaskFilter = TeamMask.none;
            if ((bool)(Object)this.teamComponent)
                bullseyeSearch.teamMaskFilter.AddTeam(this.teamComponent.teamIndex);
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.maxDistanceFilter = 13f;
            bullseyeSearch.maxAngleFilter = 360f;
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.RefreshCandidates();
            bullseyeSearch.FilterOutGameObject(this.gameObject);
            this.nearbyAllies = bullseyeSearch.GetResults().ToList<HurtBox>();
            this.modelAnimator = this.GetModelAnimator();
            if (!(bool)(Object)this.modelAnimator)
                return;
            this.PlayCrossfade("Body", nameof(DefenseUp), "DefenseUp.playbackRate", this.duration, 0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((bool)(Object)this.modelAnimator && (double)this.modelAnimator.GetFloat("DefenseUp.activate") > 0.5 && !this.hasCastBuff)
            {
                ScaleParticleSystemDuration component = Object.Instantiate<GameObject>(DefenseUp.defenseUpPrefab, this.transform.position, Quaternion.identity, this.transform).GetComponent<ScaleParticleSystemDuration>();
                if ((bool)(Object)component)
                    component.newDuration = DefenseUp.buffDuration;
                this.hasCastBuff = true;
                if (NetworkServer.active)
                {
                    Util.PlaySound("Play_beetle_guard_death", this.gameObject);
                    this.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, DefenseUp.buffDuration);
                    foreach (HurtBox nearbyAlly in this.nearbyAllies)
                    {
                        if (nearbyAlly.healthComponent.body)
                            nearbyAlly.healthComponent.body.AddTimedBuff(EnemiesPlus.frenzyBuff, DefenseUp.buffDuration);
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