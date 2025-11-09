using UnityEngine;
using System;
using System.Collections.Generic;
using GuiFramework;

namespace Dialog
{
    public class DialogManager : Singleton<DialogManager>
    {
        [SerializeField] private string dialogPanelName = "DialogPanel"; // panel name configured in UIDatas (optional)
        
        private Dictionary<int, DialogSession> _sessions = new Dictionary<int, DialogSession>();
        private Dictionary<int, DialogPanel> _activePanels = new Dictionary<int, DialogPanel>(); // key by sessionId
        private int _nextSessionId = 1;

        private struct PanelInfo
        {
            public DialogPanel Panel;
            public bool FromUIManager;
            public string PanelName; // if from UIManager, store name used
        }

        private Dictionary<int, PanelInfo> _panelInfos = new Dictionary<int, PanelInfo>();

        /// <summary>
        /// Start a dialog session. Returns sessionId.
        /// </summary>
        public int StartDialog(DialogData dialogData, Transform target, Action onComplete = null)
        {
            if (dialogData == null)
            {
                Debug.LogError("StartDialog: dialogData is null");
                return -1;
            }

            int sessionId = _nextSessionId++;
            int startNode = dialogData.startNodeId;

            var session = new DialogSession(sessionId, dialogData, startNode, target, onComplete);
            _sessions[sessionId] = session;

            // Broadcast dialog start
            MsgCenter.SendMsg(MsgConst.ON_DIALOG_START, dialogData.dialogId, sessionId, target ? target.GetInstanceID() : -1);

            ShowCurrentNode(sessionId);
            return sessionId;
        }

        public void StopDialog(int sessionId)
        {
            if (_sessions.ContainsKey(sessionId))
            {
                EndSession(sessionId);
            }
        }

        private void ShowCurrentNode(int sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session)) return;

            var node = session.GetCurrentNode();
            if (node == null)
            {
                EndSession(sessionId);
                return;
            }

            // Broadcast node enter
            MsgCenter.SendMsg(MsgConst.ON_DIALOG_NODE_ENTER, session.sessionId, node.id);

            // Create or reuse panel
            if (!_activePanels.TryGetValue(sessionId, out var panel) || panel == null)
            {
                var parent = UIManager.Instance != null ? UIManager.Instance.UIRoot : this.transform;

                // Try to get a pooled DialogPanel
                panel = DialogPanel.GetPooledPanel(parent);

                // If no pooled panel, try to open via UIManager if a panel name is configured
                if (panel == null && UIManager.Instance != null && !string.IsNullOrEmpty(dialogPanelName))
                {
                    var opened = UIManager.Instance.OpenPanel(dialogPanelName);
                    var openedDialogPanel = opened as DialogPanel;
                    if (openedDialogPanel != null)
                    {
                        panel = openedDialogPanel;
                    }
                }
                
                if (panel == null)
                {
                    Debug.LogError("DialogManager: Unable to create DialogPanel instance. Ensure dialogPanelName exists in UIDatas or fallback prefab is set.");
                    return;
                }

                _activePanels[sessionId] = panel;

                // record panel info
                PanelInfo info = new PanelInfo();
                info.Panel = panel;
                info.FromUIManager = false;
                info.PanelName = null;

                // If we used UIManager.OpenPanel above, mark as fromUIManager
                if (UIManager.Instance != null && !string.IsNullOrEmpty(dialogPanelName))
                {
                    var maybe = UIManager.Instance.GetPanel<DialogPanel>(dialogPanelName);
                    if (maybe == panel)
                    {
                        info.FromUIManager = true;
                        info.PanelName = dialogPanelName;
                    }
                }

                _panelInfos[sessionId] = info;

                panel.SetTarget(session.target);

                // If this panel wasn't opened by UIManager, call OpenPanel to play open animation
                if (!_panelInfos[sessionId].FromUIManager)
                {
                    panel.OpenPanel($"Dialog_{sessionId}");
                }
            }

            panel.ShowDialog(node.speakerName, node.dialogText);
            panel.ClearChoices();

            if (node.choices != null && node.choices.Count > 0)
            {
                for (int i = 0; i < node.choices.Count; i++)
                {
                    var choice = node.choices[i];
                    int choiceIndex = i;
                    int nextId = choice.nextNodeId;
                    string text = choice.choiceText;
                    panel.AddChoice(text, () => OnChoiceSelected(sessionId, node.id, choiceIndex, nextId));
                }
            }
            else if (!node.IsEndNode)
            {
                int nextId = node.nextNodeId;
                panel.AddChoice("继续", () => OnChoiceSelected(sessionId, node.id, -1, nextId));
            }
            else
            {
                EndSession(sessionId);
            }
        }

        private void OnChoiceSelected(int sessionId, int nodeId, int choiceIndex, int nextNodeId)
        {
            // Broadcast choice
            MsgCenter.SendMsg(MsgConst.ON_DIALOG_CHOICE, sessionId, nodeId, choiceIndex);

            if (!_sessions.TryGetValue(sessionId, out var session)) return;

            if (nextNodeId >= 0)
            {
                session.currentNodeId = nextNodeId;
                ShowCurrentNode(sessionId);
            }
            else
            {
                EndSession(sessionId);
            }
        }

        private void EndSession(int sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                // Clean up bubble
                if (_activePanels.TryGetValue(sessionId, out var panel) && panel != null)
                {
                    // check panel origin
                    if (_panelInfos.TryGetValue(sessionId, out var info))
                    {
                        if (info.FromUIManager)
                        {
                            // if opened via UIManager, close via UIManager to maintain stack
                            if (UIManager.Instance != null && !string.IsNullOrEmpty(info.PanelName))
                            {
                                UIManager.Instance.ClosePanel(info.PanelName);
                            }
                            else
                            {
                                info.Panel.ClosePanel();
                            }
                        }
                        else
                        {
                            // pooled or fallback: return to pool
                            DialogPanel.ReleasePanelToPool(info.Panel);
                        }
                        _activePanels.Remove(sessionId);
                        _panelInfos.Remove(sessionId);
                    }
                    else
                    {
                        // unknown origin: just destroy
                        panel.Hide();
                        Destroy(panel.gameObject);
                        _activePanels.Remove(sessionId);
                    }
                }

                // Broadcast end
                MsgCenter.SendMsg(MsgConst.ON_DIALOG_END, session.sessionId, session.dialogData != null ? session.dialogData.dialogId : -1);

                // Invoke callback
                session.OnComplete?.Invoke();

                _sessions.Remove(sessionId);
            }
        }
    }
}
