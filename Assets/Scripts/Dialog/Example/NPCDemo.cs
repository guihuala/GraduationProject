using System.Collections.Generic;
using UnityEngine;
using Dialog;
using GuiFramework;
using SimpleUITips;

public class NPCDemo : MonoBehaviour
{
    [Header("NPC配置")]
    [SerializeField] private Transform npcTransform; // 在Inspector中指定NPC位置
    
    [Header("对话配置")]
    [SerializeField] private DialogData dialogData; // 在Inspector中分配配置好的DialogData
    
    private void Start()
    {
        UIHelper.Instance.AddBubble(
            new BubbleInfo(new List<string>{"E"}, gameObject, gameObject, "交互：打开", "门"));
        RegisterEventListeners();
        
        if (npcTransform == null)
        {
            npcTransform = this.transform; // fallback to this GameObject
            Debug.LogWarning("NPCDemo: npcTransform 未设置，已回退为脚本所在物体的 Transform");
        }
        
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
                StartDialog();
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
    
    private void StartDialog()
    {
        if (dialogData == null)
        {
            Debug.LogError("DialogData未分配！请在Inspector中分配对话数据。");
            return;
        }
        
        if (DialogManager.Instance == null)
        {
            Debug.LogError("DialogManager实例为空！");
            return;
        }

        // log check: ensure dialog has nodes
        if (dialogData.dialogNodes == null || dialogData.dialogNodes.Count == 0)
        {
            Debug.LogError($"DialogData (id={dialogData.dialogId}) 没有包含任何节点，无法开始对话。");
            return;
        }
        
        var target = npcTransform != null ? npcTransform : this.transform;
        Debug.Log($"开始对话，DialogId={dialogData.dialogId}, startNodeId={dialogData.startNodeId}, target={(target!=null?target.name:"null")}");
        
        // 启动对话 - 将target设为npcTransform以让对话框跟随NPC
        int sessionId = DialogManager.Instance.StartDialog(
            dialogData, 
            target,
            () => Debug.Log("对话结束回调被调用！")
        );
        
        Debug.Log($"对话已启动，会话ID: {sessionId}");
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
        StartDialog();
    }
    
    // 强制开始对话（忽略距离检查）
    [ContextMenu("强制开始对话")]
    public void ForceStartDialog()
    {
        StartDialog();
    }
}