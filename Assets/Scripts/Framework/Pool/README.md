# 对象池 + 状态机 + 调度系统

## 一、概述
本框架旨在为 Unity 项目提供 **高性能、可扩展、零外部依赖** 的底层功能，包含三大核心模块：

1. **对象池管理系统（PoolManager）**
   - 支持 GameObject 与普通对象两类池；
   - 自动创建与复用对象池；
   - 可手动清理指定或全部池。

2. **有限状态机系统（StateMachine）**
   - 通用可复用状态机；
   - 状态可放回对象池以复用；
   - 自动管理 Update / LateUpdate / FixedUpdate 注册。

3. **Mono 调度系统与扩展方法（MonoManager + KidGameExtension）**
   - 允许非 MonoBehaviour 类使用 Update、协程等功能；
   - 提供丰富的扩展方法与便捷调用。

---

## 二、项目结构
```
Framework/
 ├── GameExtension.cs        // 扩展方法（协程 / Update / 对象池）
 ├── GameObjectPoolData.cs   // GameObject 对象池数据结构
 ├── ObjectPoolData.cs       // 普通对象池数据结构
 ├── PoolManager.cs          // 对象池管理器（单例）
 ├── BaseState.cs            // 状态基类
 ├── StateMachine.cs         // 状态机核心
 └── MonoManager.cs          // 全局 Mono 调度器
```

---

## 三、模块详解

### ① 对象池系统（PoolManager）

**功能特点：**
- 支持 GameObject 与普通对象两类池；
- 自动创建与复用对象；
- 提供预热数量（initSpawnAmount）；
- 支持按名称或类型清空池。

**常用方法：**
| 方法 | 说明 |
|------|------|
| `GetGameObject(name, prefab, parent)` | 从池中取出对象（自动创建） |
| `PushGameObject(obj)` | 回收 GameObject |
| `GetObject<T>()` | 获取普通对象 |
| `PushObject(object)` | 回收普通对象 |
| `Clear()` | 清理池（可指定类型） |

**使用示例：**
```csharp
// 获取一个敌人对象
var enemy = PoolManager.Instance.GetGameObject("Enemy", enemyPrefab);

// 用完回收
enemy.GameObjectPushPool();

// 获取普通对象
var bulletData = PoolManager.Instance.GetObject<BulletData>();
PoolManager.Instance.PushObject(bulletData);
```

---

### ② 状态机系统（StateMachine）

**组成：**
- `StateBase` 状态基类；
- `StateMachine` 状态机核心；
- `IStateMachineOwner` 状态拥有者接口。

**生命周期方法：**
| 方法名 | 说明 |
|---------|------|
| `Init()` | 状态初始化 |
| `Enter()` | 进入状态 |
| `Update()` | 每帧更新 |
| `Exit()` | 离开状态 |
| `UnInit()` | 状态反初始化（回收） |

**状态机主要方法：**
| 方法 | 说明 |
|------|------|
| `Init(owner)` | 初始化状态机 |
| `ChangeState<T>(int type)` | 切换状态 |
| `Stop()` | 停止状态机 |
| `Destroy()` | 销毁状态机（兼容旧接口 `Destory`） |

**示例：**
```csharp
public class PlayerController : MonoBehaviour, IStateMachineOwner
{
    private StateMachine stateMachine;

    void Start()
    {
        stateMachine = new StateMachine();
        stateMachine.Init(this);
        stateMachine.ChangeState<IdleState>(0);
    }
}

public class IdleState : StateBase
{
    public override void Enter() => Debug.Log("进入待机状态");

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            stateMachine.ChangeState<AttackState>(1);
    }
}
```

---

### ③ Mono 调度系统与扩展方法

**MonoManager：**
- 全局 Update / LateUpdate / FixedUpdate 调度；
- 可注册和移除回调；
- 提供封装的 Instantiate 方法。

| 方法 | 说明 |
|------|------|
| `AddUpdateListener(Action)` | 注册 Update |
| `AddLateUpdateListener(Action)` | 注册 LateUpdate |
| `AddFixedUpdateListener(Action)` | 注册 FixedUpdate |
| `RemoveUpdateListener(Action)` | 移除 Update |
| `InstantiateGameObject(obj)` | 快捷实例化 |

**KidGameExtension：**
- 扩展方法让普通类也能使用 Mono 功能。

```csharp
// 注册 Update 回调
this.OnUpdate(MyUpdate);

// 启动协程
this.StartCoroutine(MyCoroutine());

// 回收对象
obj.GameObjectPushPool();
data.ObjectPushPool();
```

---

## 四、使用流程

1. 在场景中创建一个空物体 `GameRoot`；
2. 挂载 `MonoManager` 与 `PoolManager`；
3. 所有对象池与状态机逻辑统一通过 `PoolManager.Instance` 管理；
4. 状态对象从对象池中获取；
5. 通过扩展方法注册更新与协程。

---

## 五、常见问题（FAQ）

**Q1：对象池会重复添加吗？**  
不会，系统内部会检查 key，确保唯一性。

**Q2：Destroy 与 Destory 有何区别？**  
`Destory` 为旧拼写（已通过 `[Obsolete]` 兼容保留），建议使用 `Destroy()`。

**Q3：GameObject 回收后仍显示？**  
请确保未手动修改 Active 状态，系统会自动 `SetActive(false)`。

**Q4：非 Mono 类如何使用协程？**  
通过 `this.StartCoroutine()` 自动委托给 `MonoManager`。