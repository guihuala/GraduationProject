using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Dialog.Editor
{
    public class DialogGraphEditor : EditorWindow
    {
        private DialogData currentDialogData;
        private Vector2 graphOffset;
        private Vector2 graphDrag;
        private float zoom = 1f;
        
        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;
        private GUIStyle nodeTitleStyle;
        
        private DialogNode selectedNode;
        private DialogNode connectingFromNode;
        private bool isMakingChoiceConnection = false;
        private int connectingChoiceIndex = -1;
        
        private Vector2 scrollPosition;
        private Rect graphViewRect;
        
        private Dictionary<int, Rect> nodeRects = new Dictionary<int, Rect>();
        private const float NODE_WIDTH = 200f;
        private const float NODE_HEADER_HEIGHT = 30f;
        private const float NODE_FIELD_HEIGHT = 20f;
        private const float NODE_FIELD_SPACING = 5f;
        private const float CHOICE_BUTTON_HEIGHT = 25f;

        [MenuItem("Tools/Dialog Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<DialogGraphEditor>("Dialog Graph");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            CreateStyles();
        }

        private void CreateStyles()
        {
            // Node style
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = MakeTex(600, 600, new Color(0.3f, 0.3f, 0.3f, 0.95f));
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
            nodeStyle.padding = new RectOffset(10, 10, 10, 10);

            // Selected node style
            selectedNodeStyle = new GUIStyle();
            selectedNodeStyle.normal.background = MakeTex(600, 600, new Color(0.4f, 0.4f, 0.6f, 0.95f));
            selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
            selectedNodeStyle.padding = new RectOffset(10, 10, 10, 10);

            // Node title style
            nodeTitleStyle = new GUIStyle(EditorStyles.boldLabel);
            nodeTitleStyle.alignment = TextAnchor.MiddleCenter;
            nodeTitleStyle.normal.textColor = Color.white;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void OnGUI()
        {
            DrawToolbar();
            
            if (currentDialogData == null)
            {
                DrawWelcomeScreen();
                return;
            }

            DrawGraph();
            DrawSidePanel();
            
            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);
            
            if (GUI.changed)
                Repaint();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Dialog data selection
            EditorGUI.BeginChangeCheck();
            currentDialogData = (DialogData)EditorGUILayout.ObjectField(currentDialogData, typeof(DialogData), false);
            if (EditorGUI.EndChangeCheck() && currentDialogData != null)
            {
                RefreshNodeRects();
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("New Node", EditorStyles.toolbarButton))
            {
                CreateNode();
            }
            
            if (GUILayout.Button("Center View", EditorStyles.toolbarButton))
            {
                CenterView();
            }
            
            if (GUILayout.Button("Validate", EditorStyles.toolbarButton))
            {
                ValidateGraph();
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawWelcomeScreen()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            
            GUILayout.Label("Dialog Graph Editor", EditorStyles.largeLabel);
            GUILayout.Space(20);
            GUILayout.Label("Select a DialogData asset or create a new one", EditorStyles.centeredGreyMiniLabel);
            
            if (GUILayout.Button("Create New Dialog Data", GUILayout.Height(40)))
            {
                CreateNewDialogData();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void CreateNewDialogData()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Dialog Data", "NewDialogData", "asset", "Save dialog data");
            if (!string.IsNullOrEmpty(path))
            {
                var dialogData = CreateInstance<DialogData>();
                AssetDatabase.CreateAsset(dialogData, path);
                AssetDatabase.SaveAssets();
                currentDialogData = dialogData;
                Selection.activeObject = currentDialogData;
            }
        }

        private void DrawGraph()
        {
            // Graph background
            graphViewRect = new Rect(300, 0, position.width - 300, position.height);
            GUI.Box(graphViewRect, "", EditorStyles.helpBox);
            
            // Draw grid
            DrawGrid(graphViewRect, 20f, 0.2f, Color.gray);
            DrawGrid(graphViewRect, 100f, 0.4f, Color.gray);
            
            // Begin zoomable area
            Matrix4x4 originalMatrix = GUI.matrix;
            Rect scaledRect = new Rect(graphViewRect);
            scaledRect.position = scrollPosition;
            
            GUI.BeginGroup(graphViewRect);
            {
                Matrix4x4 translation = Matrix4x4.TRS(graphViewRect.position + graphDrag, Quaternion.identity, Vector3.one);
                Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoom, zoom, 1.0f));
                GUI.matrix = translation * scale;
                
                // Draw connections first (behind nodes)
                DrawConnections();
                
                // Draw nodes
                BeginWindows();
                if (currentDialogData != null && currentDialogData.dialogNodes != null)
                {
                    for (int i = 0; i < currentDialogData.dialogNodes.Count; i++)
                    {
                        var node = currentDialogData.dialogNodes[i];
                        if (node != null)
                        {
                            if (!nodeRects.ContainsKey(node.id))
                            {
                                nodeRects[node.id] = new Rect(50 + i * 250, 50, NODE_WIDTH, 200);
                            }
                            
                            Rect nodeRect = nodeRects[node.id];
                            nodeRect = GUILayout.Window(node.id, nodeRect, DrawNodeWindow, $"Node {node.id}");
                            nodeRects[node.id] = nodeRect;
                        }
                    }
                }
                EndWindows();
                
                // Draw connection line if in progress
                if (connectingFromNode != null)
                {
                    Vector2 startPos = GetNodeConnectionPoint(connectingFromNode, connectingChoiceIndex);
                    Vector2 endPos = Event.current.mousePosition;
                    DrawConnectionCurve(startPos, endPos, Color.yellow);
                    GUI.changed = true;
                }
            }
            GUI.EndGroup();
            GUI.matrix = originalMatrix;
            
            // Draw scrollbars
            DrawScrollBars();
        }

        private void DrawGrid(Rect rect, float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(rect.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(rect.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            // Vertical lines
            for (int i = 0; i <= widthDivs; i++)
            {
                Handles.DrawLine(
                    new Vector3(rect.x + i * gridSpacing, rect.y, 0),
                    new Vector3(rect.x + i * gridSpacing, rect.y + rect.height, 0f));
            }

            // Horizontal lines
            for (int j = 0; j <= heightDivs; j++)
            {
                Handles.DrawLine(
                    new Vector3(rect.x, rect.y + j * gridSpacing, 0),
                    new Vector3(rect.x + rect.width, rect.y + j * gridSpacing, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawScrollBars()
        {
            // Horizontal scrollbar
            scrollPosition.x = GUI.HorizontalScrollbar(
                new Rect(graphViewRect.x, position.height - 15, graphViewRect.width, 15),
                scrollPosition.x, graphViewRect.width, 0, graphViewRect.width * 2);
            
            // Vertical scrollbar
            scrollPosition.y = GUI.VerticalScrollbar(
                new Rect(position.width - 15, graphViewRect.y, 15, graphViewRect.height),
                scrollPosition.y, graphViewRect.height, 0, graphViewRect.height * 2);
        }

        private void DrawNodeWindow(int id)
        {
            var node = currentDialogData.dialogNodes.Find(n => n.id == id);
            if (node == null) return;

            bool isSelected = selectedNode == node;
            
            // Node header
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Node {node.id}", nodeTitleStyle, GUILayout.Height(NODE_HEADER_HEIGHT));
            
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                DeleteNode(node);
                return;
            }
            GUILayout.EndHorizontal();
            
            // Node content
            GUILayout.Space(5);
            
            // Speaker name
            EditorGUILayout.LabelField("Speaker");
            node.speakerName = EditorGUILayout.TextField(node.speakerName, GUILayout.Height(NODE_FIELD_HEIGHT));
            
            GUILayout.Space(NODE_FIELD_SPACING);
            
            // Dialog text
            EditorGUILayout.LabelField("Dialog Text");
            node.dialogText = EditorGUILayout.TextArea(node.dialogText, GUILayout.Height(40));
            
            GUILayout.Space(NODE_FIELD_SPACING);
            
            // Next node ID
            EditorGUILayout.LabelField("Next Node ID");
            node.nextNodeId = EditorGUILayout.IntField(node.nextNodeId, GUILayout.Height(NODE_FIELD_HEIGHT));
            
            GUILayout.Space(NODE_FIELD_SPACING);
            
            // Choices section
            EditorGUILayout.LabelField("Choices");
            if (node.choices == null)
                node.choices = new List<DialogChoice>();
            
            for (int i = 0; i < node.choices.Count; i++)
            {
                GUILayout.BeginVertical("box");
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Choice {i + 1}", GUILayout.Width(60));
                
                if (GUILayout.Button("Connect", GUILayout.Width(50)))
                {
                    connectingFromNode = node;
                    connectingChoiceIndex = i;
                    isMakingChoiceConnection = true;
                }
                
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    node.choices.RemoveAt(i);
                    i--;
                    GUI.changed = true;
                    continue;
                }
                GUILayout.EndHorizontal();
                
                node.choices[i].choiceText = EditorGUILayout.TextField(node.choices[i].choiceText);
                node.choices[i].nextNodeId = EditorGUILayout.IntField("Next Node", node.choices[i].nextNodeId);
                
                GUILayout.EndVertical();
            }
            
            if (GUILayout.Button("Add Choice"))
            {
                node.choices.Add(new DialogChoice());
            }
            
            // Connection buttons
            GUILayout.Space(10);
            if (GUILayout.Button("Connect to Next Node"))
            {
                connectingFromNode = node;
                connectingChoiceIndex = -1;
                isMakingChoiceConnection = false;
            }
            
            if (isSelected && GUILayout.Button("Set as Start Node"))
            {
                currentDialogData.startNodeId = node.id;
                EditorUtility.SetDirty(currentDialogData);
            }
            
            // Select node on click
            if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                selectedNode = node;
                // DialogNode is not a UnityEngine.Object; select the DialogData asset instead so inspector shows asset
                Selection.activeObject = currentDialogData;
                EditorGUIUtility.PingObject(currentDialogData);
                Event.current.Use();
            }
            
            GUI.DragWindow();
        }

        private void DrawConnections()
        {
            if (currentDialogData?.dialogNodes == null) return;

            foreach (var node in currentDialogData.dialogNodes)
            {
                if (node == null) continue;

                // Draw connection to next node
                if (node.nextNodeId >= 0 && !node.IsEndNode)
                {
                    var targetNode = currentDialogData.dialogNodes.Find(n => n.id == node.nextNodeId);
                    if (targetNode != null && nodeRects.ContainsKey(node.id) && nodeRects.ContainsKey(targetNode.id))
                    {
                        Vector2 startPos = GetNodeOutputPoint(node);
                        Vector2 endPos = GetNodeInputPoint(targetNode);
                        DrawConnectionCurve(startPos, endPos, Color.white);
                    }
                }

                // Draw choice connections
                if (node.choices != null)
                {
                    for (int i = 0; i < node.choices.Count; i++)
                    {
                        var choice = node.choices[i];
                        if (choice.nextNodeId >= 0)
                        {
                            var targetNode = currentDialogData.dialogNodes.Find(n => n.id == choice.nextNodeId);
                            if (targetNode != null && nodeRects.ContainsKey(node.id) && nodeRects.ContainsKey(targetNode.id))
                            {
                                Vector2 startPos = GetChoiceConnectionPoint(node, i);
                                Vector2 endPos = GetNodeInputPoint(targetNode);
                                DrawConnectionCurve(startPos, endPos, Color.green);
                            }
                        }
                    }
                }
            }
        }

        private Vector2 GetNodeOutputPoint(DialogNode node)
        {
            if (nodeRects.ContainsKey(node.id))
            {
                Rect rect = nodeRects[node.id];
                return new Vector2(rect.x + rect.width, rect.y + rect.height * 0.3f);
            }
            return Vector2.zero;
        }

        private Vector2 GetNodeInputPoint(DialogNode node)
        {
            if (nodeRects.ContainsKey(node.id))
            {
                Rect rect = nodeRects[node.id];
                return new Vector2(rect.x, rect.y + rect.height * 0.3f);
            }
            return Vector2.zero;
        }

        private Vector2 GetChoiceConnectionPoint(DialogNode node, int choiceIndex)
        {
            if (nodeRects.ContainsKey(node.id))
            {
                Rect rect = nodeRects[node.id];
                float yOffset = NODE_HEADER_HEIGHT + 120 + (choiceIndex * 80); // Approximate choice button position
                return new Vector2(rect.x + rect.width, rect.y + yOffset);
            }
            return Vector2.zero;
        }

        private Vector2 GetNodeConnectionPoint(DialogNode node, int choiceIndex)
        {
            if (choiceIndex >= 0)
                return GetChoiceConnectionPoint(node, choiceIndex);
            else
                return GetNodeOutputPoint(node);
        }

        private void DrawConnectionCurve(Vector2 start, Vector2 end, Color color)
        {
            Handles.BeginGUI();
            Handles.color = color;
            
            Vector3 startPos = new Vector3(start.x, start.y, 0);
            Vector3 endPos = new Vector3(end.x, end.y, 0);
            Vector3 startTan = startPos + Vector3.right * 50;
            Vector3 endTan = endPos + Vector3.left * 50;
            
            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 3f);
            
            // Draw arrow
            Vector2 direction = (end - start).normalized;
            Vector2 right = Quaternion.Euler(0, 0, 30) * -direction * 10f;
            Vector2 left = Quaternion.Euler(0, 0, -30) * -direction * 10f;
            
            Handles.DrawAAConvexPolygon(end, end + right, end + left);
            
            Handles.EndGUI();
        }

        private void DrawSidePanel()
        {
            GUILayout.BeginArea(new Rect(0, 0, 300, position.height), EditorStyles.helpBox);
            
            GUILayout.Label("Dialog Graph Editor", EditorStyles.largeLabel);
            GUILayout.Space(10);
            
            if (currentDialogData != null)
            {
                // Dialog data properties
                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.LabelField("Dialog Data Properties", EditorStyles.boldLabel);
                currentDialogData.dialogId = EditorGUILayout.IntField("Dialog ID", currentDialogData.dialogId);
                currentDialogData.startNodeId = EditorGUILayout.IntField("Start Node ID", currentDialogData.startNodeId);
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(currentDialogData);
                }
                
                GUILayout.Space(20);
                
                // Node list
                EditorGUILayout.LabelField("Nodes", EditorStyles.boldLabel);
                if (currentDialogData.dialogNodes != null)
                {
                    foreach (var node in currentDialogData.dialogNodes)
                    {
                        if (node == null) continue;
                        
                        GUILayout.BeginHorizontal();
                        bool isSelected = selectedNode == node;
                        if (GUILayout.Toggle(isSelected, $"Node {node.id}: {node.speakerName}", "Button"))
                        {
                            if (!isSelected)
                            {
                                selectedNode = node;
                                Selection.activeObject = currentDialogData;
                                EditorGUIUtility.PingObject(currentDialogData);
                            }
                        }
                        
                        if (GUILayout.Button("GoTo", GUILayout.Width(40)))
                        {
                            FocusOnNode(node);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                
                GUILayout.Space(10);
                
                // Selected node properties
                if (selectedNode != null)
                {
                    EditorGUILayout.LabelField("Selected Node", EditorStyles.boldLabel);
                    EditorGUI.BeginChangeCheck();
                    
                    selectedNode.id = EditorGUILayout.IntField("Node ID", selectedNode.id);
                    selectedNode.speakerName = EditorGUILayout.TextField("Speaker", selectedNode.speakerName);
                    selectedNode.dialogText = EditorGUILayout.TextArea("Dialog Text", selectedNode.dialogText, GUILayout.Height(60));
                    selectedNode.nextNodeId = EditorGUILayout.IntField("Next Node ID", selectedNode.nextNodeId);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(currentDialogData);
                    }
                }
            }
            
            GUILayout.EndArea();
        }

        private void ProcessEvents(Event e)
        {
            if (!graphViewRect.Contains(e.mousePosition)) return;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1) // Right click
                    {
                        ShowContextMenu(e.mousePosition);
                        e.Use();
                    }
                    else if (e.button == 0 && isMakingChoiceConnection && connectingFromNode != null)
                    {
                        // Check if clicked on a node to complete connection
                        var targetNode = GetNodeAtPosition(e.mousePosition);
                        if (targetNode != null && targetNode != connectingFromNode)
                        {
                            if (connectingChoiceIndex >= 0)
                            {
                                // Connecting a choice
                                connectingFromNode.choices[connectingChoiceIndex].nextNodeId = targetNode.id;
                            }
                            else
                            {
                                // Connecting main next node
                                connectingFromNode.nextNodeId = targetNode.id;
                            }
                            EditorUtility.SetDirty(currentDialogData);
                        }
                        
                        connectingFromNode = null;
                        connectingChoiceIndex = -1;
                        isMakingChoiceConnection = false;
                        e.Use();
                    }
                    break;
                    
                case EventType.MouseDrag:
                    if (e.button == 2 || (e.button == 0 && e.alt)) // Middle mouse or Alt+Left drag
                    {
                        graphDrag += e.delta;
                        e.Use();
                    }
                    break;
                    
                case EventType.ScrollWheel:
                    zoom -= e.delta.y * 0.01f;
                    zoom = Mathf.Clamp(zoom, 0.3f, 2f);
                    e.Use();
                    break;
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if (currentDialogData?.dialogNodes == null) return;

            // Process node events here if needed
        }

        private void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Create Node"), false, () => CreateNodeAtPosition(mousePosition));
            
            var clickedNode = GetNodeAtPosition(mousePosition);
            if (clickedNode != null)
            {
                menu.AddItem(new GUIContent("Delete Node"), false, () => DeleteNode(clickedNode));
                menu.AddItem(new GUIContent("Set as Start Node"), false, () => 
                {
                    currentDialogData.startNodeId = clickedNode.id;
                    EditorUtility.SetDirty(currentDialogData);
                });
            }
            
            menu.ShowAsContext();
        }

        private DialogNode GetNodeAtPosition(Vector2 mousePosition)
        {
            if (currentDialogData?.dialogNodes == null) return null;

            foreach (var node in currentDialogData.dialogNodes)
            {
                if (node != null && nodeRects.ContainsKey(node.id))
                {
                    // Convert screen position to graph position
                    Vector2 graphPos = (mousePosition - graphViewRect.position - graphDrag) / zoom;
                    if (nodeRects[node.id].Contains(graphPos))
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        private void CreateNode()
        {
            if (currentDialogData == null) return;

            var newNode = new DialogNode();
            newNode.id = GetNextAvailableNodeId();
            newNode.speakerName = "New Speaker";
            newNode.dialogText = "Enter dialog text here...";
            
            if (currentDialogData.dialogNodes == null)
                currentDialogData.dialogNodes = new List<DialogNode>();
            
            currentDialogData.dialogNodes.Add(newNode);
            
            // Position new node
            nodeRects[newNode.id] = new Rect(50 + currentDialogData.dialogNodes.Count * 250, 50, NODE_WIDTH, 200);
            
            EditorUtility.SetDirty(currentDialogData);
        }

        private void CreateNodeAtPosition(Vector2 clickPosition)
        {
            CreateNode();
            
            // Position the new node at the clicked position
            Vector2 graphPos = (clickPosition - graphViewRect.position - graphDrag) / zoom;
            if (currentDialogData.dialogNodes.Count > 0)
            {
                var lastNode = currentDialogData.dialogNodes[currentDialogData.dialogNodes.Count - 1];
                nodeRects[lastNode.id] = new Rect(graphPos.x, graphPos.y, NODE_WIDTH, 200);
            }
        }

        private void DeleteNode(DialogNode node)
        {
            if (currentDialogData?.dialogNodes == null) return;

            // Remove connections to this node
            foreach (var otherNode in currentDialogData.dialogNodes)
            {
                if (otherNode == null || otherNode == node) continue;
                
                if (otherNode.nextNodeId == node.id)
                    otherNode.nextNodeId = -1;
                
                if (otherNode.choices != null)
                {
                    foreach (var choice in otherNode.choices)
                    {
                        if (choice.nextNodeId == node.id)
                            choice.nextNodeId = -1;
                    }
                }
            }
            
            // Remove from start node if needed
            if (currentDialogData.startNodeId == node.id && currentDialogData.dialogNodes.Count > 1)
            {
                var otherNode = currentDialogData.dialogNodes.Find(n => n != node);
                if (otherNode != null)
                    currentDialogData.startNodeId = otherNode.id;
            }
            
            currentDialogData.dialogNodes.Remove(node);
            nodeRects.Remove(node.id);
            
            if (selectedNode == node)
                selectedNode = null;
            
            EditorUtility.SetDirty(currentDialogData);
        }

        private int GetNextAvailableNodeId()
        {
            if (currentDialogData?.dialogNodes == null || currentDialogData.dialogNodes.Count == 0)
                return 1;
            
            return currentDialogData.dialogNodes.Max(n => n.id) + 1;
        }

        private void RefreshNodeRects()
        {
            nodeRects.Clear();
            if (currentDialogData?.dialogNodes == null) return;

            for (int i = 0; i < currentDialogData.dialogNodes.Count; i++)
            {
                var node = currentDialogData.dialogNodes[i];
                if (node != null && !nodeRects.ContainsKey(node.id))
                {
                    nodeRects[node.id] = new Rect(50 + i * 250, 50, NODE_WIDTH, 200);
                }
            }
        }

        private void CenterView()
        {
            if (currentDialogData?.dialogNodes == null || currentDialogData.dialogNodes.Count == 0) return;

            // Calculate bounds of all nodes
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            
            foreach (var rect in nodeRects.Values)
            {
                minX = Mathf.Min(minX, rect.x);
                minY = Mathf.Min(minY, rect.y);
                maxX = Mathf.Max(maxX, rect.x + rect.width);
                maxY = Mathf.Max(maxY, rect.y + rect.height);
            }
            
            // Center the view
            Vector2 center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            graphDrag = -center + graphViewRect.size * 0.5f / zoom;
        }

        private void FocusOnNode(DialogNode node)
        {
            if (node == null || !nodeRects.ContainsKey(node.id)) return;

            Rect rect = nodeRects[node.id];
            Vector2 center = rect.center;
            graphDrag = -center + graphViewRect.size * 0.5f / zoom;
        }

        private void ValidateGraph()
        {
            if (currentDialogData == null) return;

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            // Check for duplicate node IDs
            var idSet = new HashSet<int>();
            foreach (var node in currentDialogData.dialogNodes)
            {
                if (node == null) continue;
                
                if (idSet.Contains(node.id))
                    errors.Add($"Duplicate node ID: {node.id}");
                else
                    idSet.Add(node.id);
            }

            // Check start node exists
            if (currentDialogData.dialogNodes.Find(n => n.id == currentDialogData.startNodeId) == null)
                errors.Add($"Start node ID {currentDialogData.startNodeId} not found");

            // Check for broken connections
            foreach (var node in currentDialogData.dialogNodes)
            {
                if (node == null) continue;
                
                if (node.nextNodeId >= 0 && currentDialogData.dialogNodes.Find(n => n.id == node.nextNodeId) == null)
                    warnings.Add($"Node {node.id} has broken connection to node {node.nextNodeId}");
                
                if (node.choices != null)
                {
                    foreach (var choice in node.choices)
                    {
                        if (choice.nextNodeId >= 0 && currentDialogData.dialogNodes.Find(n => n.id == choice.nextNodeId) == null)
                            warnings.Add($"Node {node.id} choice has broken connection to node {choice.nextNodeId}");
                    }
                }
            }

            // Show results
            if (errors.Count == 0 && warnings.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation", "No issues found!", "OK");
            }
            else
            {
                string message = "";
                if (errors.Count > 0)
                {
                    message += "Errors:\n" + string.Join("\n", errors) + "\n\n";
                }
                if (warnings.Count > 0)
                {
                    message += "Warnings:\n" + string.Join("\n", warnings);
                }
                EditorUtility.DisplayDialog("Validation Results", message, "OK");
            }
        }
    }
}

