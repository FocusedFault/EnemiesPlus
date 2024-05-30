using RoR2;
using System.Linq;
using EntityStates;
using EntityStates.Bell.BellWeapon;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace EnemiesPlus
{
    public class BuffBeamPlus : BaseState
    {
        public static float duration;
        public static GameObject buffBeamPrefab;
        public static AnimationCurve beamWidthCurve;
        public static string playBeamSoundString;
        public static string stopBeamSoundString;
        public HurtBox target;
        private float healTimer;
        private float healInterval;
        private float healChunk;
        private CharacterBody targetBody;
        private GameObject buffBeamInstance;
        private BezierCurveLine healBeamCurve;
        private Transform muzzleTransform;
        private Transform beamTipTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(BuffBeam.playBeamSoundString, this.gameObject);
            Ray aimRay = this.GetAimRay();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.teamMaskFilter = TeamMask.none;
            if ((bool)this.teamComponent)
                bullseyeSearch.teamMaskFilter.AddTeam(this.teamComponent.teamIndex);
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.maxDistanceFilter = 50f;
            bullseyeSearch.maxAngleFilter = 360f;
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.RefreshCandidates();
            bullseyeSearch.FilterOutGameObject(this.gameObject);
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
                            this.target = hurtBox;
                            this.targetBody = targetBody;
                            targetBody.AddBuff(RoR2Content.Buffs.Immune.buffIndex);
                            break;
                        }
                    }
                }
            }

            if (!this.target && !this.targetBody)
            {
                this.outer.SetNextStateToMain();
                return;
            }

            string childName = "Muzzle";
            Transform modelTransform = this.GetModelTransform();
            if (!(bool)modelTransform)
                return;
            ChildLocator component1 = modelTransform.GetComponent<ChildLocator>();
            if (!(bool)component1)
                return;
            this.muzzleTransform = component1.FindChild(childName);
            this.buffBeamInstance = GameObject.Instantiate<GameObject>(EntityStates.Bell.BellWeapon.BuffBeam.buffBeamPrefab);
            ChildLocator component2 = this.buffBeamInstance.GetComponent<ChildLocator>();
            if ((bool)component2)
                this.beamTipTransform = component2.FindChild("BeamTip");
            this.healBeamCurve = this.buffBeamInstance.GetComponentInChildren<BezierCurveLine>();
        }

        private void UpdateHealBeamVisuals()
        {
            this.healBeamCurve.lineRenderer.widthMultiplier = BuffBeam.beamWidthCurve.Evaluate(this.age / BuffBeam.duration);
            this.healBeamCurve.v0 = this.muzzleTransform.forward * 3f;
            this.healBeamCurve.transform.position = this.muzzleTransform.position;
            if (!(bool)this.target)
                return;
            this.beamTipTransform.position = this.targetBody.mainHurtBox.transform.position;
        }

        public override void Update()
        {
            base.Update();
            this.UpdateHealBeamVisuals();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((double)this.fixedAge < (double)BuffBeam.duration && !(this.target == null) || !this.isAuthority)
                return;
            this.outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            Util.PlaySound(BuffBeam.stopBeamSoundString, this.gameObject);
            EntityState.Destroy(this.buffBeamInstance);
            if ((bool)this.targetBody)
                this.targetBody.RemoveBuff(RoR2Content.Buffs.Immune.buffIndex);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            HurtBoxReference.FromHurtBox(this.target).Write(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            HurtBoxReference hurtBoxReference = new HurtBoxReference();
            hurtBoxReference.Read(reader);
            this.target = hurtBoxReference.ResolveGameObject()?.GetComponent<HurtBox>();
        }
    }
}
