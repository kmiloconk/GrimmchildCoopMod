using System;
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision != null)
            {
                TryReceiveDamage(collision.collider);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision != null)
            {
                TryReceiveDamage(collision.collider);
            }
        }

        private void TryReceiveDamage(Collider2D other)
        {
            if (controller == null ||
                controller.IsDead ||
                other == null)
            {
                return;
            }

            // Vulnerability disabled in the mod settings.
            if (!GrimmchildCoopMod.Settings.GrimmchildVulnerable)
                return;

            if (other.transform == transform ||
                other.transform.IsChildOf(controller.transform))
            {
                return;
            }

            DamageHero damageHero =
                FindDamageHero(other);

            bool isHazard =
                IsHazardObject(other);

            if (damageHero == null && !isHazard)
                return;

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild recibió daño de: " +
                other.gameObject.name +
                " | Capa: " +
                LayerMask.LayerToName(other.gameObject.layer) +
                " | Tipo: " +
                (damageHero != null ? "DamageHero" : "Hazard"));

            controller.Kill();
        }

        private DamageHero FindDamageHero(Collider2D other)
        {
            /*
             * 1. En el mismo objeto del collider.
             */
            DamageHero damageHero =
                other.GetComponent<DamageHero>();

            if (damageHero != null)
                return damageHero;

            /*
             * 2. En alguno de sus padres.
             */
            damageHero =
                other.GetComponentInParent<DamageHero>();

            if (damageHero != null)
                return damageHero;

            /*
             * 3. En el objeto que contiene el Rigidbody2D.
             * Muchos proyectiles tienen el collider y DamageHero
             * repartidos entre hijos diferentes.
             */
            Rigidbody2D attachedBody =
                other.attachedRigidbody;

            if (attachedBody != null)
            {
                damageHero =
                    attachedBody.GetComponent<DamageHero>();

                if (damageHero != null)
                    return damageHero;

                damageHero =
                    attachedBody.GetComponentInChildren<DamageHero>(true);

                if (damageHero != null)
                    return damageHero;
            }

            /*
             * 4. Busca en el árbol principal del objeto.
             */
            Transform root = other.transform.root;

            if (root != null)
            {
                damageHero =
                    root.GetComponentInChildren<DamageHero>(true);
            }

            return damageHero;
        }

        private bool IsHazardObject(Collider2D other)
        {
            Transform current = other.transform;

            /*
             * Algunos pinchos y peligros utilizan componentes
             * HazardRespawn en vez de DamageHero.
             */
            while (current != null)
            {
                Component[] components =
                    current.GetComponents<Component>();

                foreach (Component component in components)
                {
                    if (component == null)
                        continue;

                    string typeName =
                        component.GetType().Name;

                    if (typeName.IndexOf(
                            "HazardRespawn",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                current = current.parent;
            }

            string layerName =
                LayerMask.LayerToName(other.gameObject.layer);

            return !string.IsNullOrEmpty(layerName) &&
                   layerName.IndexOf(
                       "Hazard",
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}