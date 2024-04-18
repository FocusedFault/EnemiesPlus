using RoR2;
using UnityEngine;

namespace EnemiesPlus
{
    public class NuxHelfireEffectController : MonoBehaviour
    {
        private CharacterBody victimBody;
        private ModelLocator modelLocator;
        private BurnEffectController helfireEffectController;
        private GameObject helfireEffectPrefab = EnemiesPlus.helfireIgniteEffect;

        private void Start()
        {
            this.victimBody = GetComponent<CharacterBody>();
            this.modelLocator = GetComponent<ModelLocator>();

            AddHelfireEffect();
        }

        private void OnDestroy()
        {
            RemoveHelfireEffect();
        }

        private void AddHelfireEffect()
        {
            this.victimBody = (bool)this.victimBody ? this.victimBody : GetComponent<CharacterBody>();
            if ((bool)this.victimBody)
            {
                EffectManager.SpawnEffect(helfireEffectPrefab, new EffectData()
                {
                    origin = this.victimBody.corePosition
                }, true);
            }

            this.modelLocator = (bool)this.modelLocator ? this.modelLocator : GetComponent<ModelLocator>();
            if ((bool)this.modelLocator && (bool)this.modelLocator.modelTransform)
            {
                this.helfireEffectController = this.gameObject.AddComponent<BurnEffectController>();
                this.helfireEffectController.effectType = BurnEffectController.helfireEffect;
                this.helfireEffectController.target = this.modelLocator.modelTransform.gameObject;
            }
        }

        private void RemoveHelfireEffect()
        {
            if ((bool)this.helfireEffectController)
                Destroy(this.helfireEffectController);
        }
    }
}