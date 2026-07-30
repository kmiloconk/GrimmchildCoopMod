using InControl;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace GrimmchildCoopMod
{
    public class GrimmchildCoopMod : Mod
    {
        private bool grimmPrepared;

        public override string GetVersion() => "1.0.0";

        public override void Initialize()
        {
            ModHooks.GetPlayerIntHook += OnGetPlayerInt;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
        }

        private int OnGetPlayerInt(string name, int originalValue)
        { 
            if (name == "charmCost_40")
                return 0;

            return originalValue;
        }

        private void OnHeroUpdate()
        {
            LockKnightController();
            PlayerData.instance.SetBool("gotCharm_40", true);
            PlayerData.instance.SetBool("equippedCharm_40", true);
            PlayerData.instance.SetInt("grimmChildLevel", 4);

            if (HeroController.instance == null ||
                PlayerData.instance == null)
            {
                return;
            }

           



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
                grimm.AddComponent<Player2Controller>();

            grimmPrepared = true;

            Log("Grimmchild preparado para Jugador 2.");
        }

        private void LockKnightController()
        {
            if (!InputManager.AssignDevices())
                return;

            if (InputHandler.Instance == null)
                return;

            if (InputHandler.Instance.inputActions == null)
                return;

            InputDevice player1 = InputManager.GetPlayer1Device();

            if (player1 == null)
                return;

            InputHandler.Instance.inputActions.Device = player1;
        }
    }
}