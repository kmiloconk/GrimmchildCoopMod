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
        
        private bool grimmPrepared;

        public static GrimmchildSettings Settings =
            new GrimmchildSettings();

        public override string GetVersion()
        {
            return "1.1.0";
        }

        public override void Initialize()
        {
            ModHooks.GetPlayerIntHook += OnGetPlayerInt;
            ModHooks.HeroUpdateHook += OnHeroUpdate;

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

            GameObject grimm = GrimmSprite.GetGrimmchild();

            if (grimm == null)
            {
                grimmPrepared = false;
                return;
            }

            if (grimmPrepared)
                return;

            GrimmSprite.DisableAI(grimm);

            if (grimm.GetComponent<Player2Controller>() == null)
            {
                grimm.AddComponent<Player2Controller>();
            }

            grimmPrepared = true;

            Log("Grimmchild preparado para Jugador 2.");
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

            menu.Add(
                new IMenuMod.MenuEntry
                {
                    Name = "Grimmchild Damage",

                    Description =
                        "Choose between Grimmchild's original damage " +
                        "or scaling with the Knight's current nail damage.",

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

            return menu;
        }
    }
}