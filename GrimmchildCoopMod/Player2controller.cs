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
        private bool receivingHit;

        private Rigidbody2D body;
        private PlayMakerFSM controlFSM;
        private tk2dSpriteAnimator animator;
        private float teleportTimer;
        private bool animatorWasEnabled;
        private bool controlFsmWasEnabled;

        private bool handlingKnightRespawn;

        private bool respawnSleeping;

        private float attackCooldownTimer;
        private bool teleporting;
        private bool dead;
        private bool reviving;
        private bool sceneResetComplete = true;

        private MeshRenderer[] meshRenderers;
        private Collider2D[] grimmColliders;
        private const float HitStopDuration = 0.08f;



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

            InputManager.UpdateAttackState();

            if (!sceneResetComplete)
                return;

            UpdateBenchState();

            if (dead ||
                reviving ||
                resting ||
                receivingHit)
            {
                return;
            }

            UpdateAttack();
            UpdateTeleport();
        }

        private void FixedUpdate()
        {
            if (dead ||
                reviving ||
                resting ||
                receivingHit)
            {
                return;
            }

            UpdateMovement();
        }

        private void UpdateMovement()
        {
            if (body == null ||
                teleporting ||
                receivingHit ||
                resting)
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

            /*
             * Estado especial después de que el Caballero
             * haya reaparecido en una banca.
             *
             * Grimmchild ya está colocado dormido en el suelo.
             * Solo esperamos a que el Caballero se levante.
             */
            if (respawnSleeping)
            {
                if (!currentlyAtBench &&
                    previousBenchState)
                {
                    StartCoroutine(
                        WakeFromRespawnRoutine());
                }

                previousBenchState =
                    currentlyAtBench;

                return;
            }

            if (GrimmchildCoopMod.ReviveAfterKnightDeathPending)
            {
                previousBenchState =
                    currentlyAtBench;

                return;
            }

            if (currentlyAtBench &&
                !previousBenchState)
            {
                if (dead && !reviving)
                {
                    StartCoroutine(
                        ReviveRoutine());
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

            previousBenchState =
                currentlyAtBench;
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

            
            controlFSM.SetState("Rest Start");

            
            if (animator != null)
            {
                animator.Play("Fly 4");
            }
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

        public void HandleKnightRespawn()
        {
            if (handlingKnightRespawn ||
                dead ||
                reviving)
            {
                return;
            }

            StartCoroutine(
                HandleKnightRespawnRoutine());
        }

        private IEnumerator HandleKnightRespawnRoutine()
        {
            handlingKnightRespawn = true;

            /*
             * Esperamos a que el Caballero haya reaparecido
             * realmente sentado en la banca.
             */
            while (HeroController.instance == null ||
                   PlayerData.instance == null ||
                   !PlayerData.instance.GetBool("atBench"))
            {
                yield return null;
            }

            /*
             * Damos dos frames para que el juego termine
             * de posicionar al Caballero.
             */
            yield return null;
            yield return null;

            EnterKnightRespawnSleep();

            handlingKnightRespawn = false;

            GrimmchildCoopMod.CompleteKnightRespawn();

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Respawn del Caballero completado con Grimmchild dormido.");
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

            if (animator != null)
            {
                animator.enabled = true;
            }

            if (controlFSM != null)
            {
                controlFSM.enabled = true;
            }

            teleporting = false;
            teleportTimer = 0f;
            attackCooldownTimer = 0f;

            reviving = false;
            resting = false;
            receivingHit = false;
            respawnSleeping = false;
            handlingKnightRespawn = false;

            previousBenchState = false;

            if (body != null)
            {
                body.velocity = Vector2.zero;
            }
        }

        private void OnEnable()
        {
            sceneResetComplete = false;
            StartCoroutine(ResetAfterSceneChange());
        }

        private IEnumerator ResetAfterSceneChange()
        {
            sceneResetComplete = false;

            yield return null;
            yield return null;

            body = GetComponent<Rigidbody2D>();
            controlFSM = GrimmSprite.GetControlFSM(gameObject);
            animator = GetComponent<tk2dSpriteAnimator>();

            resting = false;
            receivingHit = false;
            reviving = false;
            previousBenchState = false;
            respawnSleeping = false;
            handlingKnightRespawn = false;

            if (animator != null)
            {
                animator.enabled = true;
            }

            if (controlFSM != null)
            {
                controlFSM.enabled = true;
            }

            teleporting = false;
            teleportTimer = 0f;
            attackCooldownTimer = 0f;

            dead = GrimmchildCoopMod.GrimmchildIsDead;

            if (dead)
            {
                ApplyDeadStateImmediately();

                sceneResetComplete = true;

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

            RestoreControlState();

            RestartFlyingAudio();

            sceneResetComplete = true;

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

            /*
             * Usamos la misma capa que HeroBox para que ataques,
             * proyectiles y peligros interactúen con esta hurtbox
             * igual que con el Caballero.
             */
            hurtboxObject.layer = GetHeroDamageLayer();

            CircleCollider2D hurtbox =
                hurtboxObject.AddComponent<CircleCollider2D>();

            hurtbox.isTrigger = true;
            hurtbox.radius = 0.65f;

            hurtboxObject.AddComponent<GrimmchildHurtbox>();

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Hurtbox creada en capa: " +
                LayerMask.LayerToName(hurtboxObject.layer));
        }

        public void ReviveAfterKnightDeath()
        {
            if (!dead || reviving)
                return;

            StartCoroutine(
                ReviveAfterKnightDeathRoutine());
        }

        private IEnumerator ReviveAfterKnightDeathRoutine()
        {
            reviving = true;

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Esperando respawn del Caballero con Grimmchild muerto...");

            while (!sceneResetComplete)
            {
                yield return null;
            }

            while (HeroController.instance == null ||
                   PlayerData.instance == null ||
                   !PlayerData.instance.GetBool("atBench"))
            {
                yield return null;
            }

            yield return null;
            yield return null;

            /*
             * Revivimos lógicamente a Grimmchild.
             */
            dead = false;
            reviving = false;

            GrimmchildCoopMod.SetGrimmchildDead(false);

            /*
             * Lo colocamos directamente dormido.
             *
             * Sin Tele.
             * Sin Rest Start.
             * Sin Fly 4.
             */
            EnterKnightRespawnSleep();

            GrimmchildHurtbox hurtbox =
                GetComponentInChildren<GrimmchildHurtbox>(true);

            if (hurtbox != null)
            {
                hurtbox.ResetHit();
            }

            /*
             * MUY IMPORTANTE:
             * hay dos flags pendientes cuando mueren ambos.
             * Tenemos que limpiar LAS DOS para que no se
             * ejecute HandleKnightRespawn después.
             */
            GrimmchildCoopMod.CompleteKnightDeathRevive();
            GrimmchildCoopMod.CompleteKnightRespawn();

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild revivido directamente dormido junto al Caballero.");
        }

        private Vector3 GetKnightRespawnSleepPosition()
        {
            if (HeroController.instance == null)
                return transform.position;

            Vector3 heroPosition =
                HeroController.instance.transform.position;

            /*
             * Buscamos suelo un poco al lado del Caballero.
             */
            float targetX =
                heroPosition.x + 1.25f;

            Vector2 rayOrigin =
                new Vector2(
                    targetX,
                    heroPosition.y + 2f);

            RaycastHit2D[] hits =
                Physics2D.RaycastAll(
                    rayOrigin,
                    Vector2.down,
                    6f);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (hit.collider.isTrigger)
                    continue;

                /*
                 * Ignoramos colliders de Grimmchild.
                 */
                if (hit.collider.transform == transform ||
                    hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                /*
                 * Ignoramos colliders del Caballero.
                 */
                if (HeroController.instance != null &&
                    (hit.collider.transform ==
                        HeroController.instance.transform ||
                     hit.collider.transform.IsChildOf(
                        HeroController.instance.transform)))
                {
                    continue;
                }

                /*
                 * Ponemos el centro de Grimmchild ligeramente
                 * por encima del punto del suelo.
                 */
                return new Vector3(
                    targetX,
                    hit.point.y + 0.45f,
                    transform.position.z);
            }

            /*
             * Fallback por si no encontramos suelo.
             */
            return new Vector3(
                heroPosition.x + 1.25f,
                heroPosition.y - 0.8f,
                transform.position.z);
        }


        private void EnterKnightRespawnSleep()
        {
            if (HeroController.instance == null)
                return;

            Vector3 sleepPosition =
                GetKnightRespawnSleepPosition();

            transform.position =
                sleepPosition;

            if (body != null)
            {
                body.simulated = true;
                body.velocity = Vector2.zero;
                body.position = sleepPosition;
            }

            SetGrimmchildVisible(true);

            teleporting = false;
            teleportTimer = 0f;
            attackCooldownTimer = 0f;
            receivingHit = false;

            resting = true;
            respawnSleeping = true;
            previousBenchState = true;

            /*
             * NO Rest Start.
             * NO Fly 4.
             * NO Tele.
             *
             * Aparece directamente en el estado
             * de estar dormido.
             */
            if (controlFSM != null)
            {
                controlFSM.enabled = true;
                controlFSM.SetState("Rest Pause");
            }

            if (animator != null)
            {
                animator.enabled = true;
            }

            StopFlyingAudio();

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild colocado dormido después del respawn.");
        }

        private IEnumerator WakeFromRespawnRoutine()
        {
            if (!respawnSleeping)
                yield break;

            respawnSleeping = false;

            /*
             * Ya está dormido en Rest Pause.
             * No necesitamos esperar a Rest Start como hace
             * WakeUpRoutine().
             */
            if (controlFSM != null)
            {
                controlFSM.enabled = true;
                controlFSM.SetState("Wake");
            }

            yield return StartCoroutine(
                WaitForWake());
        }

        private int GetHeroDamageLayer()
        {
            if (HeroController.instance == null)
                return gameObject.layer;

            Collider2D[] heroColliders =
                HeroController.instance.GetComponentsInChildren<Collider2D>(true);

            foreach (Collider2D collider in heroColliders)
            {
                if (collider != null &&
                    collider.gameObject.name == "HeroBox")
                {
                    return collider.gameObject.layer;
                }
            }

            return HeroController.instance.gameObject.layer;
        }
        public void Kill()
        {
            if (dead || reviving || receivingHit)
                return;

            StartCoroutine(HitRoutine());
        }

        private IEnumerator HitRoutine()
        {
            receivingHit = true;

            if (body != null)
            {
                body.velocity = Vector2.zero;
            }

            // Feedback inmediato.
            PlayHitFlash();
            PlayHitVibration();

            // Guardamos el estado actual antes de congelar.
            if (animator != null)
            {
                animatorWasEnabled = animator.enabled;
                animator.enabled = false;
            }

            if (controlFSM != null)
            {
                controlFsmWasEnabled = controlFSM.enabled;
                controlFSM.enabled = false;
            }

            /*
             * Durante este tiempo:
             * - no se mueve
             * - no cambia de frame
             * - la FSM no avanza
             *
             * El resto del juego continúa normalmente.
             */
            yield return new WaitForSecondsRealtime(HitStopDuration);

            // Restauramos antes de iniciar el teletransporte.
            if (animator != null)
            {
                animator.enabled = animatorWasEnabled;
            }

            if (controlFSM != null)
            {
                controlFSM.enabled = controlFsmWasEnabled;
            }

            receivingHit = false;

            dead = true;
            resting = false;
            teleporting = false;
            teleportTimer = 0f;
            attackCooldownTimer = 0f;

            GrimmchildCoopMod.SetGrimmchildDead(true);

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

        private void PlayHitFlash()
        {
            SpriteFlash[] flashes = GetComponents<SpriteFlash>();

            foreach (SpriteFlash flash in flashes)
            {
                if (flash != null)
                {
                    flash.FlashGrimmHit();
                }
            }
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

            GrimmchildHurtbox hurtbox = GetComponentInChildren<GrimmchildHurtbox>(true);

            if (hurtbox != null)
            {
                hurtbox.ResetHit();
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

        private void PlayHitVibration()
        {
            InControl.InputDevice device =
                InputManager.GetPlayer2Device();

            if (device == null)
                return;

            StartCoroutine(HitVibrationRoutine(device));
        }

        private IEnumerator HitVibrationRoutine(
            InControl.InputDevice device)
        {
            device.Vibrate(0.7f, 0.9f);

            yield return new WaitForSecondsRealtime(0.12f);

            device.StopVibration();
        }

        private void RestoreControlState()
        {
            if (controlFSM != null)
            {
                controlFSM.enabled = true;

                if (!dead)
                {
                    controlFSM.SetState("Follow");
                }
            }

            if (animator != null)
            {
                animator.enabled = true;

                if (!dead)
                {
                    animator.Play("Fly 4");
                }
            }

            receivingHit = false;
            teleporting = false;
            teleportTimer = 0f;
            attackCooldownTimer = 0f;
        }

        private void LateUpdate()
        {
            InControl.InputDevice knightDevice =
                InputManager.GetPlayer1Device();

            if (knightDevice != null)
            {
                knightDevice.RequestActivation();
            }
        }
    }
}
