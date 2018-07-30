using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;

// ReSharper disable UnusedMember.Global

namespace infinitegrimm
{
    // ReSharper disable once InconsistentNaming
    internal static class PlayMakerFSMExtensions
    {
        private static readonly FieldInfo FSM_STRING_PARAMS = typeof(ActionData).GetField("fsmStringParams", BindingFlags.NonPublic | BindingFlags.Instance);

        public static List<FsmString> getStringParams(this ActionData self)
        {
            return (List<FsmString>)FSM_STRING_PARAMS.GetValue(self);
        }

        public static FsmState getState(this PlayMakerFSM self, string name)
        {
            return self.FsmStates.FirstOrDefault(state => state.Name == name);
        }

        public static void removeActionsOfType<T>(this FsmState self)
        {
            self.Actions = self.Actions.Where(action => !(action is T)).ToArray();
        }

        public static T[] getActionsOfType<T>(this FsmState self) where T : FsmStateAction
        {
            return self.Actions.OfType<T>().ToArray();
        }

        public static void clearTransitions(this FsmState self)
        {
            self.Transitions = new FsmTransition[0];
        }

        public static void addTransition(this FsmState self, string eventName, string toState)
        {
            List<FsmTransition> transitions = self.Transitions.ToList();

            FsmTransition trans = new FsmTransition
            {
                ToState = toState,
                FsmEvent = FsmEvent.EventListContains(eventName)
                    ? FsmEvent.GetFsmEvent(eventName)
                    : new FsmEvent(eventName)
            };


            transitions.Add(trans);

            self.Transitions = transitions.ToArray();
        }

        public static void addAction(this FsmState self, FsmStateAction action)
        {
            List<FsmStateAction> actions = self.Actions.ToList();
            actions.Add(action);
            self.Actions = actions.ToArray();
        }
    }
}
