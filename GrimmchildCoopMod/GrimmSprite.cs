using HutongGames.PlayMaker;
using Modding;
using System.Collections.Generic;
using UnityEngine;


namespace GrimmchildCoopMod
{
    public static class GrimmSprite
    {
        public static GameObject GetGrimmchild()
        {
            foreach (PlayMakerFSM fsm
                in UnityEngine.Object.FindObjectsOfType<PlayMakerFSM>())
            {
                if (fsm.gameObject.name == "Grimmchild(Clone)" &&
                    fsm.FsmName == "Control")
                {
                    return fsm.gameObject;
                }
            }

            return null;
        }

        public static PlayMakerFSM GetControlFSM(GameObject grimm)
        {
            if (grimm == null)
                return null;

            foreach (PlayMakerFSM fsm
                in grimm.GetComponents<PlayMakerFSM>())
            {
                if (fsm.FsmName == "Control")
                    return fsm;
            }

            return null;
        }

        public static void DisableAI(GameObject grimm)
        {
            PlayMakerFSM control = GetControlFSM(grimm);

            if (control == null)
            {
                Modding.Logger.Log(
                    "[GrimmchildCoopMod] No se encontró la FSM Control.");

                return;
            }


            FsmState follow = FindState(control, "Follow");

            if (follow == null)
            {
                Modding.Logger.Log(
                    "[GrimmchildCoopMod] No se encontró Follow.");

                return;
            }

            RemoveMovementActions(follow);
            RemoveAutomaticTransitions(follow);

            Modding.Logger.Log(
                "[GrimmchildCoopMod] IA de Grimmchild modificada.");
        }



        private static FsmState FindState(
            PlayMakerFSM fsm,
            string stateName)
        {
            if (fsm == null)
                return null;

            foreach (FsmState state in fsm.FsmStates)
            {
                if (state.Name == stateName)
                    return state;
            }

            return null;
        }

        private static void RemoveMovementActions(FsmState follow)
        {
            List<FsmStateAction> newActions =
                new List<FsmStateAction>();

            foreach (FsmStateAction action in follow.Actions)
            {
                if (action == null)
                    continue;

                string actionName = action.GetType().Name;

                if (actionName == "DistanceFlySmooth" ||
                    actionName == "GrimmChildFly")
                {
                    Modding.Logger.Log(
                        "[GrimmchildCoopMod] Acción eliminada: " +
                        actionName);

                    continue;
                }

                newActions.Add(action);
            }

            follow.Actions = newActions.ToArray();
        }

        private static void RemoveAutomaticTransitions(FsmState follow)
        {
            List<FsmTransition> newTransitions =
                new List<FsmTransition>();

            foreach (FsmTransition transition in follow.Transitions)
            {
                if (transition == null)
                    continue;

                string destination = transition.ToState;

                if (destination == "Check For Target" ||
                    destination == "Antic" ||
                    destination == "Shoot")
                {
                    Modding.Logger.Log(
                        "[GrimmchildCoopMod] Ataque automático bloqueado: " +
                        follow.Name + " -> " + destination);

                    continue;
                }

                if (ContainsTeleportName(destination))
                {
                    Modding.Logger.Log(
                        "[GrimmchildCoopMod] Teletransporte automático bloqueado: " +
                        follow.Name + " -> " + destination);

                    continue;
                }

                newTransitions.Add(transition);
            }

            follow.Transitions = newTransitions.ToArray();
        }

        private static bool ContainsTeleportName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            string lowerValue = value.ToLowerInvariant();

            return lowerValue.Contains("tele") ||
                   lowerValue.Contains("warp");
        }



    }
}
