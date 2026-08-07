using InControl;
using Modding;
using System.Collections.Generic;
using UnityEngine;


namespace GrimmchildCoopMod
{
    public class GrimmchildCoopMod :
        Mod,
        IGlobalSettings<GrimmchildSettings>,
        IMenuMod
    {
        private static bool reviveGrimmchildAfterKnightDeath;
        public static bool GrimmchildIsDead { get; private set; }

        public static void SetGrimmchildDead(bool value)
        {
            GrimmchildIsDead = value;
        }
        private static GrimmchildCoopMod instance;

        private int preparedGrimmInstanceId;


        public static GrimmchildSettings Settings =
            new GrimmchildSettings();

        public override string GetVersion()
        {
            return "1.2.0";
        }

        public override void Initialize()
        {
            instance = this;
            ModHooks.GetPlayerIntHook += OnGetPlayerInt;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.AfterPlayerDeadHook += OnKnightDead;

            Log("GrimmchildCoopMod inicializado.");
        }


        private int OnGetPlayerInt(
            string name,
            int originalValue)
        {
            if (name == "charmCost_40")
                return 0;

            return originalValue;
        }

        private void OnHeroUpdate()
        {
            LockKnightController();

            if (PlayerData.instance == null)
                return;

            PlayerData.instance.SetBool("gotCharm_40", true);
            PlayerData.instance.SetBool("equippedCharm_40", true);
            PlayerData.instance.SetInt("grimmChildLevel", 4);

            if (HeroController.instance == null)
                return;

            GameObject grimm =
                GrimmSprite.GetGrimmchild();

            if (grimm == null)
                return;

            int currentInstanceId =
                grimm.GetInstanceID();

            Player2Controller controller =
                grimm.GetComponent<Player2Controller>();

            if (preparedGrimmInstanceId != currentInstanceId)
            {
                GrimmSprite.DisableAI(grimm);

                if (controller == null)
                {
                    controller =
                        grimm.AddComponent<Player2Controller>();
                }

                preparedGrimmInstanceId =
                    currentInstanceId;

                Log("Grimmchild preparado para Jugador 2.");
            }

            
            if (reviveGrimmchildAfterKnightDeath &&
                controller != null)
            {
                reviveGrimmchildAfterKnightDeath = false;

                controller.ReviveAfterKnightDeath();
            }
        }


        private void LockKnightController()
        {
            if (!InputManager.AssignDevices())
                return;

            if (InputHandler.Instance == null ||
                InputHandler.Instance.inputActions == null)
            {
                return;
            }

            InputDevice player1 =
                InputManager.GetPlayer1Device();

            if (player1 == null)
                return;

            InputHandler.Instance.inputActions.Device = player1;
        }


        private void OnKnightDead()
        {
            if (!GrimmchildIsDead)
                return;

            reviveGrimmchildAfterKnightDeath = true;

            Log("El Caballero murió con Grimmchild muerto. Revivirá al reaparecer.");
        }





        public void OnLoadGlobal(
            GrimmchildSettings settings)
        {
            Settings = settings ??
                       new GrimmchildSettings();
        }

        public GrimmchildSettings OnSaveGlobal()
        {
            return Settings;
        }

        public bool ToggleButtonInsideMenu
        {
            get { return false; }
        }

        public List<IMenuMod.MenuEntry> GetMenuData(
            IMenuMod.MenuEntry? toggleButtonEntry)
        {
            List<IMenuMod.MenuEntry> menu =
                new List<IMenuMod.MenuEntry>();

            menu.Add(new IMenuMod.MenuEntry
                {
                    Name = "Grimmchild Damage",

                    Description =
                        "Original damage or scaling with the Knight's current nail damage.",

                    Values = new[]
                {
                    "Original",
                    "Scale with Nail"
                },

                    Saver = option =>
                    {
                        Settings.ScaleDamageWithNail =
                            option == 1;
                    },

                    Loader = () =>
                    {
                        return Settings.ScaleDamageWithNail
                            ? 1
                            : 0;
                    }
                });

            menu.Add( new IMenuMod.MenuEntry
            {
                Name = "Grimmchild Vulnerability",

                Description =
                "Grimmchild can receive damage",

                Values = new[]
            {
                "Off",
                "On"
            },

                Saver = option =>
            {
                Settings.GrimmchildVulnerable =
                    option == 1;
            },

            Loader = () =>
            {
                return Settings.GrimmchildVulnerable
                    ? 1
                    : 0;
            }
        });

            return menu;
        }
    }
}