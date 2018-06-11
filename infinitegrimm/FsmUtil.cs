﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
// ReSharper disable UnusedMember.Global

namespace infinitegrimm
{
    // ReSharper disable once InconsistentNaming
    internal static class FsmUtil
    {
        private static void removeAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return;
        }

        private static readonly FieldInfo FSM_STRING_PARAMS_FIELD;
        static FsmUtil()
        {
            FieldInfo[] fieldInfo = typeof(ActionData).
                GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo t in fieldInfo)
            {
                if (t.Name != "fsmStringParams") continue;
                FSM_STRING_PARAMS_FIELD = t;
                break;
            }
        }

        public static void removeAction(PlayMakerFSM fsm, string stateName, int index)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                Modding.Logger.Log("[FSM UTIL] " + actions[0].GetType());

                actions.removeAt(index);

                t.Actions = actions;
            }
        }

        public static FsmState getState(PlayMakerFSM fsm, string stateName)
        {
            return (from t in fsm.FsmStates where t.Name == stateName let actions = t.Actions select t).FirstOrDefault();
        }

        public static FsmStateAction getAction(PlayMakerFSM fsm, string stateName, int index)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                Modding.Logger.Log("[FSM UTIL] " + actions[index].GetType());

                return actions[index];
            }

            return null;
        }


        public static void addAction(PlayMakerFSM fsm, string stateName, FsmStateAction action)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                actions[actions.Length - 1] = action;

                t.Actions = actions;
            }
        }

        public static void changeTransition(PlayMakerFSM fsm, string stateName, string eventName, string toState)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                foreach (FsmTransition trans in t.Transitions)
                {
                    if (trans.EventName == eventName)
                    {
                        trans.ToState = toState;
                    }
                }
            }
        }

        public static void removeTransitions(PlayMakerFSM fsm, List<string> states, List<string> transitions)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (!states.Contains(t.Name)) continue;
                List<FsmTransition> transList = new List<FsmTransition>();
                foreach (FsmTransition trans in t.Transitions)
                {
                    if (!transitions.Contains(trans.ToState))
                    {
                        transList.Add(trans);
                    }
                    else
                    {
                        Modding.Logger.Log($"[FSM UTIL] Removing {trans.ToState} transition from {t.Name}");
                    }
                }
                t.Transitions = transList.ToArray();
            }
        }

        public static void replaceStringVariable(PlayMakerFSM fsm, List<string> states, Dictionary<string, string> dict)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (!states.Contains(t.Name)) continue;
                foreach (FsmString str in (List<FsmString>)FSM_STRING_PARAMS_FIELD.GetValue(t.ActionData))
                {
                    List<FsmString> val = new List<FsmString>();
                    if (dict.ContainsKey(str.Value))
                    {
                        val.Add(dict[str.Value]);
                        found = true;
                    }
                    else
                    {
                        val.Add(str);
                    }

                    if (val.Count > 0)
                    {
                        FSM_STRING_PARAMS_FIELD.SetValue(t.ActionData, val);
                    }
                }
                if (found)
                {
                    t.LoadActions();
                }
            }
        }

        public static void replaceStringVariable(PlayMakerFSM fsm, string state, Dictionary<string, string> dict)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (t.Name != state && state != "") continue;
                foreach (FsmString str in (List<FsmString>)FSM_STRING_PARAMS_FIELD.GetValue(t.ActionData))
                {
                    List<FsmString> val = new List<FsmString>();
                    if (dict.ContainsKey(str.Value))
                    {
                        val.Add(dict[str.Value]);
                        found = true;
                    }
                    else
                    {
                        val.Add(str);
                    }

                    if (val.Count > 0)
                    {
                        FSM_STRING_PARAMS_FIELD.SetValue(t.ActionData, val);
                    }
                }
                if (found)
                {
                    t.LoadActions();
                }
            }
        }

        public static void replaceStringVariable(PlayMakerFSM fsm, string state, string src, string dst)
        {
            Modding.Logger.Log("[FSM UTIL] Replacing FSM Strings");
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (t.Name != state && state != "") continue;
                Modding.Logger.Log($"[FSM UTIL] Found FsmState with name \"{t.Name}\" ");
                foreach (FsmString str in (List<FsmString>)FSM_STRING_PARAMS_FIELD.GetValue(t.ActionData))
                {
                    List<FsmString> val = new List<FsmString>();
                    Modding.Logger.Log($"[FSM UTIL] Found FsmString with value \"{str}\" ");
                    if (str.Value.Contains(src))
                    {
                        val.Add(dst);
                        found = true;
                        Modding.Logger.Log($"[FSM UTIL] Found FsmString with value \"{str}\", changing to \"{dst}\" ");
                    }
                    else
                    {
                        val.Add(str);
                    }

                    if (val.Count > 0)
                    {
                        FSM_STRING_PARAMS_FIELD.SetValue(t.ActionData, val);
                    }
                }
                if (found)
                {
                    t.LoadActions();
                }
            }
        }

    }
}