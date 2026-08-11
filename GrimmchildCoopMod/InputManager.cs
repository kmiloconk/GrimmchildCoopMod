using InControl;
using UnityEngine;


namespace GrimmchildCoopMod
{
    public static class InputManager
    {
        private const float DeadZone = 0.2f;

        private static InputDevice player1Device;
        private static InputDevice player2Device;
        private static bool previousAttackPressed;
        private static InputDevice previousAttackDevice;

        private static bool devicesAssigned;

        public static bool AssignDevices()
        {
            if (devicesAssigned)
                return true;

            if (InControl.InputManager.Devices.Count < 2)
                return false;

            int grimmchildIndex =
                GrimmchildCoopMod.Settings.GrimmchildController;

            // Por seguridad.
            if (grimmchildIndex < 0 || grimmchildIndex > 1)
                grimmchildIndex = 1;

            int knightIndex =
                grimmchildIndex == 0 ? 1 : 0;

            player1Device =
                InControl.InputManager.Devices[knightIndex];

            player2Device =
                InControl.InputManager.Devices[grimmchildIndex];

            if (player1Device == null ||
                player2Device == null)
            {
                return false;
            }

            devicesAssigned = true;

            Modding.Logger.Log("[GrimmchildCoopMod] Knight Controller: " +(knightIndex + 1) +" - " +player1Device.Name);

            Modding.Logger.Log("[GrimmchildCoopMod] Grimmchild Controller: " +(grimmchildIndex + 1) +" - " + player2Device.Name);

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

            if (device == null)
            {
                previousAttackPressed = false;
                previousAttackDevice = null;
                return false;
            }

            /*
             * Si cambió el mando asignado a Grimmchild,
             * reiniciamos el estado del botón.
             */
            if (!object.ReferenceEquals(
                previousAttackDevice,
                device))
            {
                previousAttackDevice = device;
                previousAttackPressed =
                    device.Action3.IsPressed;

                return false;
            }

            bool currentlyPressed =
                device.Action3.IsPressed;

            bool wasPressed =
                currentlyPressed &&
                !previousAttackPressed;

            previousAttackPressed =
                currentlyPressed;

            return wasPressed;
        }
        public static void ResetDevices()
        {
            player1Device = null;
            player2Device = null;
            devicesAssigned = false;

            previousAttackPressed = false;
            previousAttackDevice = null;
        }
    }
}
