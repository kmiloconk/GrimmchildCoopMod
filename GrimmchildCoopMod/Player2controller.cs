using HutongGames.PlayMaker;
using System.Collections;
using UnityEngine;
using HutongGames.PlayMaker.Actions;



namespace GrimmchildCoopMod
{
    public class Player2Controller : MonoBehaviour
    {
        private const float MoveSpeed = 8f;
        private const float AttackCooldown = 0.6f;


        private const float TeleportDistance = 15f;

        private bool resting;
        private bool previousBenchState;

        private Rigidbody2D body;
        private PlayMakerFSM controlFSM;
        private tk2dSpriteAnimator animator;
        private float teleportTimer;

        private float attackCooldownTimer;
        private bool teleporting;
        private bool dead;
        private bool reviving;

        private MeshRenderer[] meshRenderers;
        private Collider2D[] grimmColliders;

        public bool IsDead
        {
            get { return dead; }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            controlFSM = GrimmSprite.GetControlFSM(gameObject);
            animator = GetComponent<tk2dSpriteAnimator>();

            CreateHurtbox();

            meshRenderers =
                GetComponentsInChildren<MeshRenderer>(true);

            grimmColliders =
                GetComponentsInChildren<Collider2D>(true);

            dead = GrimmchildCoopMod.GrimmchildIsDead;

            if (dead)
            {
                ApplyDeadStateImmediately();
            }

            if (body == null)
            {
                Modding.Logger.LogError(
                    "[GrimmchildCoopMod] Grimmchild no tiene Rigidbody2D.");
            }

            if (controlFSM == null)
            {
                Modding.Logger.LogError(
                    "[GrimmchildCoopMod] No se encontró la FSM Control.");
            }

            if (animator == null)
            {
                Modding.Logger.LogError(
                    "[GrimmchildCoopMod] No se encontró tk2dSpriteAnimator.");
            }
        }

        private void Update()
        {
            UpdateBenchState();

            if (dead || reviving || resting)
                return;

            UpdateAttack();
            UpdateTeleport();
        }

        private void FixedUpdate()
        {
            if (dead || reviving)
                return;

            UpdateMovement();
        }

        private void UpdateMovement()
        {
            if (body == null ||
                teleporting ||
                resting ||
                dead ||
                reviving)
            {
                return;
            }

            Vector2 input = InputManager.GetMovement();

            body.velocity = input * MoveSpeed;

            if (Mathf.Abs(input.x) > 0.05f)
            {
                UpdateFacing(input.x);
            }

            UpdateFlyingAnimation();
        }

        private void UpdateBenchState()
        {
            bool currentlyAtBench = IsKnightResting();

            if (currentlyAtBench && !previousBenchState)
            {
                if (dead && !reviving)
                {
                    StartCoroutine(ReviveRoutine());
                }
                else if (!dead)
                {
                    StartResting();
                }
            }
            else if (!currentlyAtBench &&
                     previousBenchState &&
                     resting)
            {
                StopResting();
            }

            previousBenchState = currentlyAtBench;
        }

        private bool IsKnightResting()
        {
            if (PlayerData.instance == null)
                return false;

            return PlayerData.instance.GetBool("atBench");
        }

        private void StartResting()
        {
            if (resting || controlFSM == null)
                return;

            StopAllCoroutines();

            teleporting = false;
            resting = true;

            if (body != null)
            {
                body.velocity = Vector2.zero;
            }

            controlFSM.SetState("Rest Pause");
        }

        private void StopResting()
        {
            if (!resting || controlFSM == null)
                return;

            StartCoroutine(WakeUpRoutine());
        }

        private IEnumerator WaitForWake()
        {
            float timeout = 3f;

            while (controlFSM != null && timeout > 0f)
            {
                if (controlFSM.ActiveStateName == "Follow")
                    break;

                timeout -= UnityEngine.Time.deltaTime;
                yield return null;
            }

            yield return null;
            yield return null;

            resting = false;

            if (animator != null && !animator.IsPlaying("Fly 4"))
                animator.Play("Fly 4");

            RestartFlyingAudio();

        }

        private void ApplyDeadStateImmediately()
        {
            dead = true;
            reviving = false;
            resting = false;
            teleporting = false;

            StopFlyingAudio();
            SetGrimmchildVisible(false);

            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.simulated = false;
            }
        }

        private void RestartFlyingAudio()
        {
            UnityEngine.AudioSource[] audioSources =
                GetComponentsInChildren<UnityEngine.AudioSource>(true);

            foreach (UnityEngine.AudioSource source in audioSources)
            {
                if (source == null || source.clip == null)
                    continue;

                if (source.clip.name != "grimmchild_fly_loop")
                    continue;

                source.enabled = true;
                source.loop = true;


                source.Stop();
                source.time = 0f;
                source.Play();


                return;
            }

            Modding.Logger.Log(
                "[GrimmchildCoopMod] No se encontró grimmchild_fly_loop.");
        }

        private void UpdateFlyingAnimation()
        {
            if (animator == null || teleporting || IsAttacking())
                return;

            if (!animator.IsPlaying("Fly 4"))
            {
                animator.Play("Fly 4");
            }
        }

        private void UpdateFacing(float horizontal)
        {
            Vector3 scale = transform.localScale;
            float absoluteX = Mathf.Abs(scale.x);

            scale.x = horizontal > 0f
                ? absoluteX
                : -absoluteX;

            transform.localScale = scale;
        }

        private void UpdateAttack()
        {
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= Time.deltaTime;
            }

            if (!InputManager.AttackWasPressed())
                return;

            if (attackCooldownTimer > 0f)
                return;

            if (StartAttack())
            {
                attackCooldownTimer = AttackCooldown;
            }
        }

        private bool StartAttack()
        {
            if (controlFSM == null || IsAttacking())
                return false;

            UpdateGrimmchildDamage();

            controlFSM.SetState("Check For Target");

            return true;
        }


        private bool IsAttacking()
        {
            if (controlFSM == null)
                return false;

            string state = controlFSM.ActiveStateName;

            return state == "Check For Target" ||
                   state == "Antic" ||
                   state == "Shoot";
        }

        private void UpdateTeleport()
        {
            if (controlFSM == null ||
                HeroController.instance == null)
            {
                return;
            }

            if (teleporting)
            {
                teleportTimer += Time.deltaTime;

                if (teleportTimer < 4f)
                    return;



                StopAllCoroutines();

                teleporting = false;
                teleportTimer = 0f;

                controlFSM.SetState("Follow");

                if (animator != null)
                {
                    animator.Play("Fly 4");
                }
            }

            if (IsAttacking())
                return;

            float distance = Vector2.Distance(
                transform.position,
                HeroController.instance.transform.position
            );

            if (distance >= TeleportDistance)
            {
                StartCoroutine(OriginalTeleportRoutine());
            }
        }

        private IEnumerator OriginalTeleportRoutine()
        {
            teleporting = true;

            if (body != null)
            {
                body.velocity = Vector2.zero;
            }



            if (controlFSM == null)
            {
                teleporting = false;
                yield break;
            }

            controlFSM.SetState("Tele Start");

            float timeout = 3f;

            yield return null;

            while (timeout > 0f)
            {
                if (controlFSM == null)
                    break;

                if (controlFSM.ActiveStateName == "Follow")
                    break;

                timeout -= Time.deltaTime;
                yield return null;
            }

            if (controlFSM != null &&
                controlFSM.ActiveStateName != "Follow")
            {


                controlFSM.SetState("Follow");
            }

            teleporting = false;

            if (animator != null)
            {
                animator.Play("Fly 4");
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();

            teleporting = false;
            teleportTimer = 0f;
            attackCooldownTimer = 0f;
            reviving = false;

            if (body != null)
            {
                body.velocity = Vector2.zero;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(ResetAfterSceneChange());
        }

        private IEnumerator ResetAfterSceneChange()
        {
            yield return null;
            yield return null;

            body = GetComponent<Rigidbody2D>();
            controlFSM = GrimmSprite.GetControlFSM(gameObject);
            animator = GetComponent<tk2dSpriteAnimator>();

            teleporting = false;
            teleportTimer = 0f;
            attackCooldownTimer = 0f;
            reviving = false;

            dead = GrimmchildCoopMod.GrimmchildIsDead;

            if (dead)
            {
                ApplyDeadStateImmediately();

                Modding.Logger.Log(
                    "[GrimmchildCoopMod] Grimmchild continúa muerto tras cambiar de escena.");

                yield break;
            }

            SetGrimmchildVisible(true);

            if (body != null)
            {
                body.simulated = true;
                body.velocity = Vector2.zero;
            }

            if (controlFSM != null)
            {
                controlFSM.SetState("Follow");
            }

            if (animator != null)
            {
                animator.Play("Fly 4");
            }

            RestartFlyingAudio();

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild reiniciado después del cambio de escena.");
        }

        private IEnumerator WakeUpRoutine()
        {
            float waitForSleepTimeout = 1f;


            while (controlFSM != null &&
                   controlFSM.ActiveStateName != "Rest Start" &&
                   waitForSleepTimeout > 0f)
            {
                waitForSleepTimeout -= Time.deltaTime;
                yield return null;
            }

            if (controlFSM == null)
            {
                resting = false;
                yield break;
            }

            if (controlFSM.ActiveStateName == "Rest Start")
            {

                controlFSM.SendEvent("BENCHREST END");
            }
            else
            {

                controlFSM.SetState("Wake");
            }

            yield return StartCoroutine(WaitForWake());
        }

        private void UpdateGrimmchildDamage()
        {
            if (controlFSM == null)
                return;

            FsmState shootState = controlFSM.Fsm.GetState("Shoot");

            if (shootState == null)
            {
                Modding.Logger.Log(
                    "[GrimmchildCoopMod] No se encontró el estado Shoot.");

                return;
            }

            foreach (FsmStateAction action in shootState.Actions)
            {
                SetFsmInt setFsmInt = action as SetFsmInt;

                if (setFsmInt == null)
                    continue;

                string fsmName = setFsmInt.fsmName != null
                    ? setFsmInt.fsmName.Value
                    : string.Empty;

                string variableName = setFsmInt.variableName != null
                    ? setFsmInt.variableName.Value
                    : string.Empty;

                if (fsmName != "Attack" ||
                    variableName != "Damage")
                {
                    continue;
                }

                int damage = 11;

                if (GrimmchildCoopMod.Settings.ScaleDamageWithNail)
                {
                    damage = GetKnightNailDamage();
                }

                if (damage <= 0)
                    damage = 11;

                setFsmInt.setValue.Value = damage;

                Modding.Logger.Log(
                    "[GrimmchildCoopMod] Daño de Grimmchild configurado: " +
                    damage);

                return;
            }

            Modding.Logger.Log(
                "[GrimmchildCoopMod] No se encontró la acción de daño en Shoot.");
        }

        private int GetKnightNailDamage()
        {
            if (PlayerData.instance == null)
                return 5;

            int nailDamage =
                PlayerData.instance.GetInt("nailDamage");

            return nailDamage > 0
                ? nailDamage
                : 5;
        }

        private void CreateHurtbox()
        {
            Transform existing =
                transform.Find("Player2 Hurtbox");

            if (existing != null)
                return;

            GameObject hurtboxObject =
                new GameObject("Player2 Hurtbox");

            hurtboxObject.transform.SetParent(transform);
            hurtboxObject.transform.localPosition = Vector3.zero;
            hurtboxObject.transform.localRotation = Quaternion.identity;
            hurtboxObject.transform.localScale = Vector3.one;

            CircleCollider2D hurtbox =
                hurtboxObject.AddComponent<CircleCollider2D>();

            hurtbox.isTrigger = true;
            hurtbox.radius = 0.65f;

            hurtboxObject.AddComponent<GrimmchildHurtbox>();

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Hurtbox de Grimmchild creada.");
        }

        public void Kill()
        {
            if (dead || reviving)
                return;

            StopAllCoroutines();

            dead = true;
            resting = false;
            teleporting = false;
            teleportTimer = 0f;
            attackCooldownTimer = 0f;

            GrimmchildCoopMod.SetGrimmchildDead(true);

            if (body != null)
            {
                body.velocity = Vector2.zero;
            }

            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild ha muerto.");

            if (body != null)
            {
                body.velocity = Vector2.zero;
            }

            /*
             * La FSM original reproduce tanto Tele Out 4
             * como el sonido de salida.
             */
            if (controlFSM != null)
            {
                controlFSM.SetState("Tele Start");
            }
            else if (animator != null)
            {
                animator.Play("Tele Out 4");
            }

            yield return new WaitForSeconds(0.25f);

            SetGrimmchildVisible(false);
            StopFlyingAudio();

            if (controlFSM != null)
            {
                /*
                 * Evita que la secuencia automática continúe y
                 * teletransporte nuevamente al personaje.
                 */
                controlFSM.SetState("Follow");
            }

            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.simulated = false;
            }

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild oculto hasta descansar en una banca.");
        }

        private IEnumerator ReviveRoutine()
        {
            if (!dead || reviving)
                yield break;

            reviving = true;

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Reviviendo a Grimmchild en la banca.");

            if (HeroController.instance != null)
            {
                transform.position =
                    HeroController.instance.transform.position;
            }

            if (body != null)
            {
                body.simulated = true;
                body.velocity = Vector2.zero;
                body.position = transform.position;
            }

            SetGrimmchildVisible(true);

            /*
             * Tele reproduce el sonido original de aparición y
             * vuelve a habilitar el renderer.
             */
            if (controlFSM != null)
            {
                controlFSM.SetState("Tele");
            }
            else if (animator != null)
            {
                animator.Play("Tele In 4");
            }

            yield return new WaitForSeconds(0.3f);

            dead = false;
            reviving = false;
            resting = true;

            GrimmchildCoopMod.SetGrimmchildDead(false);

            if (controlFSM != null)
            {
                controlFSM.SetState("Rest Pause");
            }

            RestartFlyingAudio();

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild revivido.");
        }

        private void SetGrimmchildVisible(bool visible)
        {
            if (meshRenderers != null)
            {
                foreach (MeshRenderer renderer in meshRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = visible;
                    }
                }
            }

            if (grimmColliders != null)
            {
                foreach (Collider2D collider in grimmColliders)
                {
                    if (collider != null)
                    {
                        collider.enabled = visible;
                    }
                }
            }
        }

        private void StopFlyingAudio()
        {
            AudioSource[] audioSources =
                GetComponentsInChildren<AudioSource>(true);

            foreach (AudioSource source in audioSources)
            {
                if (source == null || source.clip == null)
                    continue;

                if (source.clip.name != "grimmchild_fly_loop")
                    continue;

                source.Stop();
                return;
            }
        }
    }
}
