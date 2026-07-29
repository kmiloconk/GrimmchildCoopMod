using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InControl;
using UnityEngine;


namespace GrimmchildCoopMod
{
    public static class InputManager
    {
        private const float DeadZone = 0.2f;

        private static InputDevice player1Device;
        private static InputDevice player2Device;

        private static bool devicesAssigned;

        public static bool AssignDevices()
        {
            if (devicesAssigned)
                return true;

            if (InControl.InputManager.Devices.Count < 2)
                return false;

            player1Device = InControl.InputManager.Devices[0];
            player2Device = InControl.InputManager.Devices[1];

            if (player1Device == null || player2Device == null)
                return false;

            devicesAssigned = true;

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Jugador 1: " + player1Device.Name);

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Jugador 2: " + player2Device.Name);

            return true;
        }

        public static InputDevice GetPlayer1Device()
        {
            AssignDevices();
            return player1Device;
        }

        public static InputDevice GetPlayer2Device()
        {
            AssignDevices();
            return player2Device;
        }

        public static Vector2 GetMovement()
        {
            InputDevice device = GetPlayer2Device();

            if (device == null)
                return Vector2.zero;

            float x = device.LeftStickX.Value;
            float y = device.LeftStickY.Value;

            if (Mathf.Abs(x) < DeadZone)
                x = 0f;

            if (Mathf.Abs(y) < DeadZone)
                y = 0f;

            return new Vector2(x, y);
        }

        public static bool AttackWasPressed()
        {
            InputDevice device = GetPlayer2Device();

            return device != null && device.Action3.WasPressed;
        }

        public static void ResetDevices()
        {
            player1Device = null;
            player2Device = null;
            devicesAssigned = false;

            Modding.Logger.Log(
                "[GrimmchildCoopMod] Asignación de mandos reiniciada.");
        }
    }
}