using UnityEngine;

namespace GrimmchildCoopMod
{
    public class GrimmchildHurtbox : MonoBehaviour
    {
        private Player2Controller controller;

        private void Awake()
        {
            controller =
                GetComponentInParent<Player2Controller>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryReceiveDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryReceiveDamage(other);
        }

        private void TryReceiveDamage(Collider2D other)
        {
            if (controller == null ||
                controller.IsDead ||
                other == null)
            {
                return;
            }

            DamageHero damageHero =
                other.GetComponent<DamageHero>();

            if (damageHero == null)
            {
                damageHero =
                    other.GetComponentInParent<DamageHero>();
            }

            if (damageHero == null)
                return;

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild recibió daño de: " +
                other.gameObject.name);

            controller.Kill();
        }
    }

}