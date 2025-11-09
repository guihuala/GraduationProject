using UnityEngine;
using Dialog;
using GuiFramework;

public class NPCDemo : MonoBehaviour
{
    [SerializeField] private Transform npcTransform; // 在Inspector中指定NPC位置
    
    private void Start()
    {
        // 创建UI工厂并注册对话框
        var factory = gameObject.GetComponent<RuntimeDialogUIFactory>();
        factory.CreateAndRegister();
        
        // 注册事件监听
        RegisterEventListeners();
        
        Debug.Log("NPC对话系统已初始化，按E键开始对话");
    }
    
    private void RegisterEventListeners()
    {
        // 注册有参事件 - 使用MsgRecAction委托
        MsgCenter.RegisterMsg(MsgConst.ON_DIALOG_START, OnDialogStart);
        MsgCenter.RegisterMsg(MsgConst.ON_DIALOG_NODE_ENTER, OnDialogNodeEnter);
        MsgCenter.RegisterMsg(MsgConst.ON_DIALOG_CHOICE, OnDialogChoice);
        MsgCenter.RegisterMsg(MsgConst.ON_DIALOG_END, OnDialogEnd);
    }
    
    private void UnregisterEventListeners()
    {
        // 注销有参事件
        MsgCenter.UnregisterMsg(MsgConst.ON_DIALOG_START, OnDialogStart);
        MsgCenter.UnregisterMsg(MsgConst.ON_DIALOG_NODE_ENTER, OnDialogNodeEnter);
        MsgCenter.UnregisterMsg(MsgConst.ON_DIALOG_CHOICE, OnDialogChoice);
        MsgCenter.UnregisterMsg(MsgConst.ON_DIALOG_END, OnDialogEnd);
    }
    
    private void Update()
    {
        // 按E键开始对话
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 检查玩家是否在NPC附近（简单距离检测）
            if (IsPlayerNearNPC())
            {
                CreateDemoDialog();
            }
        }
    }
    
    private bool IsPlayerNearNPC()
    {
        // 简单的距离检查 - 如果有Player标签的对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && npcTransform != null)
        {
            return Vector3.Distance(player.transform.position, npcTransform.position) < 3f;
        }
        return true; // 如果没有Player对象，默认返回true进行测试
    }
    
    private void CreateDemoDialog()
    {
        // 创建对话数据
        DialogData dialogData = CreateDemoDialogData();
        
        // 启动对话 - 将target设为null让对话框固定在屏幕位置
        int sessionId = DialogManager.Instance.StartDialog(
            dialogData, 
            null, // 固定位置
            () => Debug.Log("对话结束回调被调用！")
        );
        
        Debug.Log($"对话已启动，会话ID: {sessionId}");
    }
    
    private DialogData CreateDemoDialogData()
    {
        // 创建DialogData实例
        var dialogData = ScriptableObject.CreateInstance<DialogData>();
        dialogData.dialogId = 1;
        dialogData.startNodeId = 0;
        
        // 创建对话节点列表
        var nodes = new System.Collections.Generic.List<DialogNode>();
        
        // 节点0：问候
        var node0 = new DialogNode
        {
            id = 0,
            speakerName = "村民",
            dialogText = "你好，旅行者！欢迎来到我们的村庄。",
            choices = new System.Collections.Generic.List<DialogChoice>()
        };
        
        // 节点1：提供选择
        var node1 = new DialogNode
        {
            id = 1,
            speakerName = "村民", 
            dialogText = "有什么我可以帮你的吗？",
            choices = new System.Collections.Generic.List<DialogChoice>
            {
                new DialogChoice { choiceText = "购买物品", nextNodeId = 2 },
                new DialogChoice { choiceText = "了解村庄", nextNodeId = 3 },
                new DialogChoice { choiceText = "再见", nextNodeId = 4 }
            }
        };
        
        // 节点2：购买选项
        var node2 = new DialogNode
        {
            id = 2,
            speakerName = "村民",
            dialogText = "我这里有些药水和武器，你需要什么？",
            choices = new System.Collections.Generic.List<DialogChoice>
            {
                new DialogChoice { choiceText = "购买治疗药水", nextNodeId = 5 },
                new DialogChoice { choiceText = "购买铁剑", nextNodeId = 6 },
                new DialogChoice { choiceText = "返回", nextNodeId = 1 }
            }
        };
        
        // 节点3：教程选项
        var node3 = new DialogNode
        {
            id = 3,
            speakerName = "村民",
            dialogText = "想要了解基本的操作教程吗？",
            choices = new System.Collections.Generic.List<DialogChoice>
            {
                new DialogChoice { choiceText = "开始教程", nextNodeId = 7 },
                new DialogChoice { choiceText = "不用了", nextNodeId = 1 }
            }
        };
        
        // 节点4：结束对话
        var node4 = new DialogNode
        {
            id = 4,
            speakerName = "村民",
            dialogText = "再见，祝你好运！",
            nextNodeId = -1 // 结束对话
        };
        
        // 节点5：购买药水（触发购买逻辑）
        var node5 = new DialogNode
        {
            id = 5,
            speakerName = "村民",
            dialogText = "治疗药水已添加到你的背包！",
            nextNodeId = 1
        };
        
        // 节点6：购买武器（触发购买逻辑）
        var node6 = new DialogNode
        {
            id = 6, 
            speakerName = "村民",
            dialogText = "铁剑已装备！",
            nextNodeId = 1
        };
        
        // 节点7：教程开始（触发教程逻辑）
        var node7 = new DialogNode
        {
            id = 7,
            speakerName = "村民",
            dialogText = "教程开始！使用WASD移动，空格键跳跃。",
            nextNodeId = 1
        };
        
        // 添加所有节点
        nodes.AddRange(new[] { node0, node1, node2, node3, node4, node5, node6, node7 });
        
        // 使用反射设置nodes（实际项目中DialogData应该有公开的nodes列表）
        var field = typeof(DialogData).GetField("nodes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(dialogData, nodes);
        }
        else
        {
            Debug.LogError("无法设置DialogData的nodes字段！");
        }
        
        return dialogData;
    }
    
    // ========== 事件处理函数 ==========
    
    private void OnDialogStart(params object[] args)
    {
        if (args.Length >= 3)
        {
            int dialogId = (int)args[0];
            int sessionId = (int)args[1];
            int targetInstanceId = (int)args[2];
            
            Debug.Log($"对话开始 - 对话ID: {dialogId}, 会话ID: {sessionId}, 目标ID: {targetInstanceId}");
        }
    }
    
    private void OnDialogNodeEnter(params object[] args)
    {
        if (args.Length >= 2)
        {
            int sessionId = (int)args[0];
            int nodeId = (int)args[1];
            
            Debug.Log($"进入对话节点 - 会话ID: {sessionId}, 节点ID: {nodeId}");
        }
    }
    
    private void OnDialogChoice(params object[] args)
    {
        if (args.Length >= 3)
        {
            int sessionId = (int)args[0];
            int nodeId = (int)args[1];
            int choiceIndex = (int)args[2];
            
            Debug.Log($"对话选择 - 会话ID: {sessionId}, 节点ID: {nodeId}, 选择索引: {choiceIndex}");
            
            // 根据选择执行游戏逻辑
            HandleChoiceLogic(nodeId, choiceIndex);
        }
    }
    
    private void OnDialogEnd(params object[] args)
    {
        if (args.Length >= 2)
        {
            int sessionId = (int)args[0];
            int dialogId = (int)args[1];
            
            Debug.Log($"对话结束 - 会话ID: {sessionId}, 对话ID: {dialogId}");
        }
    }
    
    private void HandleChoiceLogic(int nodeId, int choiceIndex)
    {
        // 根据节点ID和选择索引执行具体游戏逻辑
        switch (nodeId)
        {
            case 2: // 购买节点
                if (choiceIndex == 0) // 购买药水
                {
                    Debug.Log("执行：获得治疗药水！");
                    // 调用物品系统：PlayerInventory.AddItem("HealthPotion");
                }
                else if (choiceIndex == 1) // 购买武器  
                {
                    Debug.Log("执行：获得铁剑！");
                    // 调用装备系统：PlayerEquipment.EquipWeapon("IronSword");
                }
                break;
                
            case 3: // 教程节点
                if (choiceIndex == 0) // 开始教程
                {
                    Debug.Log("执行：开始新手教程！");
                    // 调用教程系统：TutorialManager.StartTutorial("BasicControls");
                }
                break;
                
            case 1: // 主选择节点
                Debug.Log($"在主菜单选择了选项 {choiceIndex}");
                break;
        }
    }
    
    private void OnDestroy()
    {
        // 清理事件监听
        UnregisterEventListeners();
    }
    
    // 用于测试的简单方法
    [ContextMenu("立即测试对话")]
    public void TestDialogImmediately()
    {
        CreateDemoDialog();
    }
}