using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialog
{
    [Serializable]
    public class DialogSession
    {
        // use Unity-serialized field naming (camelCase where needed), keep analyzer happy by exposing PascalCase for some
        public int sessionId;
        public DialogData dialogData;
        public int currentNodeId;
        public Transform target;
        public Action OnComplete;

        // runtime variable context (simple key-value store)
        public Dictionary<string, object> Variables = new Dictionary<string, object>();

        public DialogSession(int id, DialogData data, int startNodeId, Transform targetTransform, Action onCompleteCallback)
        {
            sessionId = id;
            dialogData = data;
            currentNodeId = startNodeId;
            target = targetTransform;
            OnComplete = onCompleteCallback;
        }

        public DialogNode GetCurrentNode()
        {
            return dialogData?.GetNodeById(currentNodeId);
        }
    }
}
