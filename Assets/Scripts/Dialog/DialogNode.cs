using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialog
{
    [Serializable]
    public class DialogChoice
    {
        public string choiceText;
        public int nextNodeId;
    }

    [Serializable]
    public class DialogNode
    {
        public int id;
        public string speakerName;
        [TextArea(2, 6)]
        public string dialogText;
        public List<DialogChoice> choices = new List<DialogChoice>();
        public int nextNodeId = -1; // -1 means end of dialog

        public bool IsEndNode => nextNodeId == -1 && (choices == null || choices.Count == 0);
    }
}
