using UnityEngine;
using System.Collections.Generic;

namespace Dialog
{
    [CreateAssetMenu(fileName = "DialogData", menuName = "Dialog/DialogData")]
    public class DialogData : ScriptableObject
    {
        public int dialogId; // optional identifier
        public int startNodeId;
        public List<DialogNode> dialogNodes = new List<DialogNode>();

        public DialogNode GetNodeById(int id)
        {
            return dialogNodes.Find(node => node.id == id);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // Ensure node ids are unique; if not, log a warning
            if (dialogNodes != null)
            {
                var idSet = new HashSet<int>();
                foreach (var node in dialogNodes)
                {
                    if (node == null) continue;
                    if (idSet.Contains(node.id))
                    {
                        Debug.LogWarning($"DialogData ({name}) contains duplicate node id: {node.id}");
                    }
                    else
                    {
                        idSet.Add(node.id);
                    }
                }

                // Ensure startNodeId exists
                if (dialogNodes.Find(n => n.id == startNodeId) == null && dialogNodes.Count > 0)
                {
                    Debug.LogWarning($"DialogData ({name}) startNodeId ({startNodeId}) not found. Consider setting startNodeId to first node id.");
                }
            }
#endif
        }
    }
}