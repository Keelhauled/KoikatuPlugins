﻿using UnityEngine.Events;

namespace DefaultParamEditor
{
    public static class UnityExtensions
    {
        public static void ActuallyRemoveAllListeners(this UnityEventBase evt)
        {
            evt.RemoveAllListeners();
            for(var i = 0; i < evt.GetPersistentEventCount(); i++)
                evt.SetPersistentListenerState(i, UnityEventCallState.Off);
        }
    }
}
