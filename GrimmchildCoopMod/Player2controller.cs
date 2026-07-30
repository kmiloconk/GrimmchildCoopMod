using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



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

        

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            controlFSM = GrimmSprite.GetControlFSM(gameObject);
            animator = GetComponent<tk2dSpriteAnimator>();

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

            if (resting)
                return;

            UpdateAttack();
            UpdateTeleport();
        }

        private void FixedUpdate()
        {
            UpdateMovement();
        }

        private void UpdateMovement()
        {
            if (body == null ||
                teleporting ||
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

            if (currentlyAtBench && !previousBenchState)
            {
                StartResting();
            }
            else if (!currentlyAtBench && previousBenchState)
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

            // Esperamos dos frames para que las acciones del FSM terminen.
            yield return null;
            yield return null;

            resting = false;

            if (animator != null && !animator.IsPlaying("Fly 4"))
                animator.Play("Fly 4");

            RestartFlyingAudio();

            
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
            attackCooldownTimer = 0f;

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

            if (body != null)
            {
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
    }
}