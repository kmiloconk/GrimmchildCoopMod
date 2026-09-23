using GlobalEnums;
using GrimmchildCoopMod;
using Modding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrimmchildCoop
{
    public static class GrimmchildHUD
    {
        private static GameObject hudRoot;
        private static CanvasGroup canvasGroup;
        private static GrimmchildHUDController controller;

        private static Image grimmchildIcon;

        private static Sprite grimmLevel4Sprite;
        private static Sprite grimmBasicSprite;

        private static Sprite flameOnSprite;
        private static Sprite flameTransitionSprite;
        private static Sprite flameOffSprite;


        private static readonly List<Image> flameOnImages =
            new List<Image>();

        private static readonly List<Image> flameTransitionImages =
            new List<Image>();

        private static readonly List<Image> flameOffImages =
            new List<Image>();

        private static int currentMaxHealth = -1;
        private static int currentHealth = -1;

        private static readonly Vector2 IconPosition =
            new Vector2(-110f, -130f);

        private static readonly Vector2 FirstFlamePosition =
            new Vector2(-195f, -130f);

        private static readonly Vector2 IconSize =
            new Vector2(90f, 90f);

        private static readonly Vector2 FlameSize =
            new Vector2(65f, 65f);

        private const float FlameSpacing = 55f;


        public static bool IsCreated
        {
            get { return hudRoot != null; }
        }

        public static void Create()
        {
            if (hudRoot != null)
                return;

            grimmLevel4Sprite =
                FindSprite("charm_grimmkin_04");

            grimmBasicSprite =
                FindSprite("charm_grimmkin_01");

            flameOffSprite =
                FindSprite("Grimm_charm_flame_backboard");

            flameTransitionSprite =
                FindSprite("Grimm_charm_flame");

            flameOnSprite =
                FindSprite("Grimm_charm_flame_front");

            if (grimmLevel4Sprite == null ||
                grimmBasicSprite == null ||
                flameOffSprite == null ||
                flameTransitionSprite == null ||
                flameOnSprite == null)
            {
                Modding.Logger.Log(
                    "[GrimmchildCoopMod] HUD could not be created: missing sprites.");

                return;
            }

            hudRoot =
                new GameObject("Grimmchild HUD");

            Object.DontDestroyOnLoad(hudRoot);

            Canvas canvas =
                hudRoot.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 1000;

            CanvasScaler scaler =
                hudRoot.AddComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            scaler.matchWidthOrHeight = 1f;

            canvasGroup =
                hudRoot.AddComponent<CanvasGroup>();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1f;

            controller =
                hudRoot.AddComponent<GrimmchildHUDController>();

            grimmchildIcon =
                CreateImage(
                    "Grimmchild Icon",
                    hudRoot.transform,
                    grimmLevel4Sprite,
                    IconPosition,
                    IconSize);

            CreateHealthFlames();

            UpdateHealth();

            controller.Initialize(
                hudRoot.transform,
                canvasGroup);

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Grimmchild HUD created.");
        }

        private static void CreateHealthFlames()
        {
            ClearFlames();

            int maxHealth =
                GetGrimmchildMaxHealth();

            currentMaxHealth = maxHealth;

            for (int i = 0; i < maxHealth; i++)
            {
                Vector2 position =
                    new Vector2(
                        FirstFlamePosition.x -
                        (i * FlameSpacing),
                        FirstFlamePosition.y);

                Image flameOff =
                    CreateImage(
                        "Flame " + (i + 1) + " Off",
                        hudRoot.transform,
                        flameOffSprite,
                        position,
                        FlameSize);

                Image flameTransition =
                    CreateImage(
                        "Flame " + (i + 1) + " Transition",
                        hudRoot.transform,
                        flameTransitionSprite,
                        position,
                        FlameSize);

                Image flameOn =
                    CreateImage(
                        "Flame " + (i + 1) + " On",
                        hudRoot.transform,
                        flameOnSprite,
                        position,
                        FlameSize);

                flameOffImages.Add(flameOff);
                flameTransitionImages.Add(flameTransition);
                flameOnImages.Add(flameOn);

                // Transition layer is only visible while animating.
                flameTransition.enabled = false;
            }
        }

        private static Image CreateImage(
            string objectName,
            Transform parent,
            Sprite sprite,
            Vector2 position,
            Vector2 size)
        {
            GameObject imageObject =
                new GameObject(objectName);

            imageObject.transform.SetParent(
                parent,
                false);

            Image image =
                imageObject.AddComponent<Image>();

            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect =
                image.rectTransform;

            rect.anchorMin =
                new Vector2(1f, 1f);

            rect.anchorMax =
                new Vector2(1f, 1f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                position;

            rect.sizeDelta =
                size;

            return image;
        }

        private static Sprite FindSprite(
            string spriteName)
        {
            Sprite[] sprites =
                Resources.FindObjectsOfTypeAll<Sprite>();

            foreach (Sprite sprite in sprites)
            {
                if (sprite != null &&
                    sprite.name == spriteName)
                {
                    Modding.Logger.Log(
                        "[GrimmchildCoopMod] HUD sprite found: " +
                        spriteName);

                    return sprite;
                }
            }

            Modding.Logger.Log(
                "[GrimmchildCoopMod] HUD sprite not found: " +
                spriteName);

            return null;
        }

        public static int GetGrimmchildMaxHealth()
        {
            if (PlayerData.instance == null)
                return 1;

            int knightPermanentMasks =
                PlayerData.instance.GetInt(
                    "maxHealthBase");

            return Mathf.Clamp(
                1 + (knightPermanentMasks - 5),
                1,
                5);
        }

        public static void SetHealth(
            int health)
        {
            int maxHealth =
                GetGrimmchildMaxHealth();

            int oldHealth =
                currentHealth;

            currentHealth =
                Mathf.Clamp(
                    health,
                    0,
                    maxHealth);

            /*
             * First initialization:
             * do not animate the HUD.
             */
            if (oldHealth < 0 ||
                controller == null)
            {
                RefreshImmediate();
                return;
            }

            if (currentHealth < oldHealth)
            {
                controller.AnimateHealthLoss(
                    oldHealth,
                    currentHealth);
            }
            else if (currentHealth > oldHealth)
            {
                controller.AnimateHealthGain(
                    oldHealth,
                    currentHealth);
            }
            else
            {
                RefreshImmediate();
            }

            UpdateIcon();
        }

        public static void SetFullHealth()
        {
            SetHealth(
                GetGrimmchildMaxHealth());
        }

        public static void UpdateHealth()
        {
            if (hudRoot == null)
                return;

            int maxHealth =
                GetGrimmchildMaxHealth();

            if (maxHealth != currentMaxHealth)
            {
                int oldMaxHealth =
                    currentMaxHealth;

                CreateHealthFlames();

                if (currentHealth < 0)
                {
                    currentHealth =
                        maxHealth;
                }
                else if (oldMaxHealth >= 0 &&
                         maxHealth > oldMaxHealth)
                {
                    /*
                     * Player2Controller controls whether the new
                     * maximum HP should also fill current HP.
                     * Here we only rebuild the visual slots.
                     */
                    currentHealth =
                        Mathf.Clamp(
                            currentHealth,
                            0,
                            maxHealth);
                }

                RefreshImmediate();
                return;
            }

            if (currentHealth < 0)
            {
                currentHealth =
                    maxHealth;

                RefreshImmediate();
            }
        }

        private static void RefreshImmediate()
        {
            if (hudRoot == null)
                return;

            int maxHealth =
                flameOnImages.Count;

            int health =
                Mathf.Clamp(
                    currentHealth,
                    0,
                    maxHealth);

            for (int i = 0;
                 i < flameOnImages.Count;
                 i++)
            {
                if (flameOffImages[i] != null)
                {
                    flameOffImages[i].enabled =
                        true;
                }

                if (flameTransitionImages[i] != null)
                {
                    flameTransitionImages[i].enabled =
                        false;
                }

                if (flameOnImages[i] != null)
                {
                    flameOnImages[i].enabled =
                        i < health;
                }
            }

            UpdateIcon();
        }

        private static void UpdateIcon()
        {
            if (grimmchildIcon == null)
                return;

            grimmchildIcon.sprite =
                currentHealth > 0
                    ? grimmLevel4Sprite
                    : grimmBasicSprite;
        }

        /*
         * These methods are used by GrimmchildHUDController.
         */

        public static int FlameCount
        {
            get { return flameOnImages.Count; }
        }

        public static Image GetFlameOn(
            int index)
        {
            if (index < 0 ||
                index >= flameOnImages.Count)
            {
                return null;
            }

            return flameOnImages[index];
        }

        public static Image GetFlameTransition(
            int index)
        {
            if (index < 0 ||
                index >= flameTransitionImages.Count)
            {
                return null;
            }

            return flameTransitionImages[index];
        }

        public static Image GetFlameOff(
            int index)
        {
            if (index < 0 ||
                index >= flameOffImages.Count)
            {
                return null;
            }

            return flameOffImages[index];
        }

        public static void SetVisible(
            bool visible)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha =
                visible ? 1f : 0f;
        }

        private static void ClearFlames()
        {
            foreach (Image image in flameOnImages)
            {
                if (image != null)
                    Object.Destroy(image.gameObject);
            }

            foreach (Image image in flameTransitionImages)
            {
                if (image != null)
                    Object.Destroy(image.gameObject);
            }

            foreach (Image image in flameOffImages)
            {
                if (image != null)
                    Object.Destroy(image.gameObject);
            }

            flameOnImages.Clear();
            flameTransitionImages.Clear();
            flameOffImages.Clear();
        }

        public static void Destroy()
        {
            if (hudRoot != null)
            {
                Object.Destroy(hudRoot);
            }

            hudRoot = null;
            canvasGroup = null;
            controller = null;
            grimmchildIcon = null;

            grimmLevel4Sprite = null;
            grimmBasicSprite = null;

            flameOnSprite = null;
            flameTransitionSprite = null;
            flameOffSprite = null;

            flameOnImages.Clear();
            flameTransitionImages.Clear();
            flameOffImages.Clear();

            currentMaxHealth = -1;
            currentHealth = -1;
        }

        public static void OnSceneTransitionStart()
        {
            if (controller != null)
            {
                controller.OnSceneTransitionStart();
            }
        }

        
    }

    /*
     * This component owns every HUD animation.
     *
     * GrimmchildHUD keeps the health data and sprites.
     * This class only controls visual transitions.
     */
    public class GrimmchildHUDController :
    MonoBehaviour
    {
        /*
         * ===== PARAMETERS TO TUNE =====
         */

        // Duration of the intermediate flame frame.
        private const float FlameTransitionTime = 0.10f;

        // Delay between flames when restoring several HP.
        private const float FlameRestoreDelay = 0.07f;

        // Scene fade speed.
        private const float SceneFadeSpeed = 2f;

        private const float SceneFadeInDelay = 0.8f;

        private const float InterfaceFadeSpeed = 12f;

        private bool sceneTransition;
        private bool waitingForNewScene;
        private bool waitingForFadeInDelay;

        private float sceneFadeInTimer;
        
        private const float PausedAlpha = 0.40f;

        private Transform hudTransform;
        private CanvasGroup canvasGroup;

        private Vector3 normalScale =
            Vector3.one;

        private bool initialized;
        private bool wasPaused;

        

        private Coroutine healthAnimation;

        public void Initialize(
            Transform root,
            CanvasGroup group)
        {
            hudTransform = root;
            canvasGroup = group;

            normalScale =
                Vector3.one;

            hudTransform.localScale =
                normalScale;

            initialized = true;
        }

        private void Update()
        {
            if (!initialized ||
                canvasGroup == null ||
                hudTransform == null)
            {
                return;
            }

            UpdateGlobalHUDState();
        }

        private void UpdateGlobalHUDState()
        {

            GameManager gm =
                GameManager.instance;

            UIManager ui =
                UIManager.instance;

            if (gm == null ||
                ui == null)
            {
                canvasGroup.alpha = 0f;
                return;
            }

            bool gameplayScene =
                gm.IsGameplayScene();



            if (sceneTransition)
            {
                /*
                 * Step 1:
                 * Fade out before leaving the current scene.
                 */
                if (waitingForNewScene)
                {
                    canvasGroup.alpha =
                        Mathf.MoveTowards(
                            canvasGroup.alpha,
                            0f,
                            SceneFadeSpeed *
                            Time.unscaledDeltaTime);

                    if (canvasGroup.alpha <= 0.01f)
                    {
                        canvasGroup.alpha = 0f;
                        waitingForNewScene = false;
                        waitingForFadeInDelay = true;
                        sceneFadeInTimer = 0f;
                    }

                    return;
                }

                /*
                 * Step 2:
                 * The new gameplay scene is ready.
                 * Keep the HUD hidden for a short delay.
                 */
                if (waitingForFadeInDelay)
                {
                    canvasGroup.alpha = 0f;

                    if (gameplayScene &&
                        ui.uiState == UIState.PLAYING)
                    {
                        sceneFadeInTimer +=
                            Time.unscaledDeltaTime;

                        if (sceneFadeInTimer >=
                            SceneFadeInDelay)
                        {
                            waitingForFadeInDelay = false;
                        }
                    }

                    return;
                }

                /*
                 * Step 3:
                 * Fade back in.
                 */
                if (gameplayScene &&
                    ui.uiState == UIState.PLAYING)
                {
                    canvasGroup.alpha =
                        Mathf.MoveTowards(
                            canvasGroup.alpha,
                            1f,
                            SceneFadeSpeed *
                            Time.unscaledDeltaTime);

                    if (canvasGroup.alpha >= 0.99f)
                    {
                        canvasGroup.alpha = 1f;
                        sceneTransition = false;
                    }
                }

                return;
            }

            /*
             * ==================================================
             * MAIN MENU
             * ==================================================
             */

            if (!gameplayScene ||
                gm.IsMenuScene() ||
                ui.uiState == UIState.MAIN_MENU_HOME)
            {
                canvasGroup.alpha = 0f;

                hudTransform.localScale =
                    normalScale;

                wasPaused = false;

                return;
            }

            /*
             * ==================================================
             * LOADING / INACTIVE
             * ==================================================
             *
             * This remains as a secondary safety check.
             */

            if (ui.uiState == UIState.LOADING ||
                ui.uiState == UIState.INACTIVE)
            {
                canvasGroup.alpha =
                    Mathf.MoveTowards(
                        canvasGroup.alpha,
                        0f,
                        SceneFadeSpeed *
                        Time.unscaledDeltaTime);

                return;
            }

            /*
             * ==================================================
             * PAUSE
             * ==================================================
             */

            if (ui.uiState == UIState.PAUSED)
            {
                canvasGroup.alpha =
                    Mathf.MoveTowards(
                        canvasGroup.alpha,
                        PausedAlpha,
                        SceneFadeSpeed *
                        Time.unscaledDeltaTime);

                hudTransform.localScale =
                    normalScale;

                wasPaused = true;

                return;
            }

            /*
             * Leaving pause.
             */

            if (wasPaused)
            {
                wasPaused = false;
            }
            /*
 * ==================================================
 * QUICK MAP
 * ==================================================
 *
 * The native HUD shrinks while the quick map is open.
 * Grimmchild's HUD remains visible, but darkened,
 * just like when the game is paused.
 */

            if (InputManager.KnightQuickMapIsPressed())
            {
                canvasGroup.alpha =
                    Mathf.MoveTowards(
                        canvasGroup.alpha,
                        PausedAlpha,
                        InterfaceFadeSpeed *
                        Time.unscaledDeltaTime);

                return;
            }
            /*
             * ==================================================
             * NORMAL GAMEPLAY
             * ==================================================
             */

            if (ui.uiState == UIState.PLAYING ||ui.uiState == UIState.CUTSCENE)
            {
                SyncWithNativeHUD();
            }

            /*
     * ==================================================
     * TEMPORARY SCALE TEST
     * ==================================================
     */

            if (Input.GetKey(KeyCode.F8))
            {
                Modding.Logger.Log("[GrimmchildCoopMod] HUD TEST: F8 pressed - scaling down");
                hudTransform.localScale =
                    new Vector3(0.5f, 0.5f, 1f);
            }

        }

        /*
         * ======================================================
         * HEALTH LOSS
         * ======================================================
         */



        public void AnimateHealthLoss(
            int oldHealth,
            int newHealth)
        {
            if (healthAnimation != null)
            {
                StopCoroutine(
                    healthAnimation);
            }

            healthAnimation =
                StartCoroutine(
                    AnimateHealthLossRoutine(
                        oldHealth,
                        newHealth));
        }

        private IEnumerator AnimateHealthLossRoutine(
            int oldHealth,
            int newHealth)
        {
            /*
             * Index 0 is the flame closest to Grimmchild.
             *
             * [3] [2] [1] [0] [Grimm]
             *
             * 4 -> 3 removes index 3.
             */

            for (int health = oldHealth;
                 health > newHealth;
                 health--)
            {
                int index =
                    health - 1;

                Image flameOn =
                    GrimmchildHUD.GetFlameOn(index);

                Image transition =
                    GrimmchildHUD.GetFlameTransition(index);

                Image flameOff =
                    GrimmchildHUD.GetFlameOff(index);

                if (flameOff != null)
                {
                    flameOff.enabled = true;
                }

                if (flameOn != null)
                {
                    flameOn.enabled = false;
                }

                if (transition != null)
                {
                    transition.enabled = true;
                }

                yield return new WaitForSecondsRealtime(
                    FlameTransitionTime);

                if (transition != null)
                {
                    transition.enabled = false;
                }
            }

            healthAnimation = null;
        }

        /*
         * ======================================================
         * HEALTH GAIN
         * ======================================================
         */

        public void AnimateHealthGain(
            int oldHealth,
            int newHealth)
        {
            if (healthAnimation != null)
            {
                StopCoroutine(
                    healthAnimation);
            }

            healthAnimation =
                StartCoroutine(
                    AnimateHealthGainRoutine(
                        oldHealth,
                        newHealth));
        }

        private IEnumerator AnimateHealthGainRoutine(
            int oldHealth,
            int newHealth)
        {
            /*
             * Restore from Grimmchild towards the left.
             *
             * 0 -> index 0
             * 1 -> index 1
             * etc.
             */

            for (int health = oldHealth;
                 health < newHealth;
                 health++)
            {
                int index =
                    health;

                Image flameOn =
                    GrimmchildHUD.GetFlameOn(index);

                Image transition =
                    GrimmchildHUD.GetFlameTransition(index);

                Image flameOff =
                    GrimmchildHUD.GetFlameOff(index);

                if (flameOff != null)
                {
                    flameOff.enabled = true;
                }

                if (flameOn != null)
                {
                    flameOn.enabled = false;
                }

                if (transition != null)
                {
                    transition.enabled = true;
                }

                yield return new WaitForSecondsRealtime(
                    FlameTransitionTime);

                if (transition != null)
                {
                    transition.enabled = false;
                }

                if (flameOn != null)
                {
                    flameOn.enabled = true;
                }

                if (health + 1 < newHealth)
                {
                    yield return new WaitForSecondsRealtime(
                        FlameRestoreDelay);
                }
            }

            healthAnimation = null;
        }

        /*
         * ======================================================
         * SCENE TRANSITION
         * ======================================================
         */

        public void OnSceneTransitionStart()
        {
            sceneTransition = true;
            waitingForNewScene = true;
            waitingForFadeInDelay = false;
            sceneFadeInTimer = 0f;
        }

        private void SyncWithNativeHUD()
        {
            GameCameras cameras =
                GameCameras.instance;

            if (cameras == null ||
                cameras.hudCanvas == null)
            {
                return;
            }

            float nativeScale =
                cameras.hudCanvas.transform.localScale.x;

            bool nativeHudNormal =
                nativeScale >= 0.99f;

            float targetAlpha =
                nativeHudNormal ? 1f : 0f;

            canvasGroup.alpha =
                Mathf.MoveTowards(
                    canvasGroup.alpha,
                    targetAlpha,
                    InterfaceFadeSpeed *
                    Time.unscaledDeltaTime);
        }
    }
}