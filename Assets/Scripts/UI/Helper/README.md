# SimpleUITips（独立版）
无需第三方依赖（仅 UnityEngine.UI）。将 `UIHelperStandalone` 挂到常驻 Canvas，按 `Inspector` 指定 5 个 Prefab 即可。

## 文件结构
```
SimpleUITips/
  Scripts/
    SimpleUITips.cs
```

## 快速开始
1. 新建 Canvas（Screen Space - Overlay）。
2. 给 Canvas 添加空物体 `SimpleUITips`，挂 `UIHelperStandalone`。
3. 准备 5 个 Prefab：
   - `BubblePrefab`：含 `UIBubbleItem` + 3 个 Text（`ContentText`、`KeyText`、`ItemNameText`）。
   - `TipPrefab`：含 `UITipItem` + 1 个 Text（`ContentText`）。
   - `SignPrefab`：含 `UISignItem` + 1 个 Image（`IconImg`）。
   - `CircleProgressPrefab`：含 `UICommonProgressItem` + 1 个 Image（`ProgressBarImage`，`Image.type=Filled`）。
   - `FixedPosTextPrefab`：含 `UIFixedPosTextItem` + 1 个 Text（`ContentText`）。

> 以上 Prefab 为普通 UGUI 物体即可，不含任何第三方组件。

## 典型用法
```csharp
UIHelperStandalone.Instance.AddBubble(
    new BubbleInfo(new List<string>{"E"}, obj, player, "交互：打开", "门"));

UIHelperStandalone.Instance.ShowTip(new TipInfo("+10", someWorldPos, 0.75f));

UIHelperStandalone.Instance.ShowSign(new SignInfo("Icons/Warning", obj, 1.0f));

UIHelperStandalone.Instance.ShowFixedText(FixedUIPosType.Top, "获得成就", 1.5f);

UIHelperStandalone.Instance.ShowCircleProgress("craft-1", CircleProgressType.Auto, obj, 2.0f);
UIHelperStandalone.Instance.ShowCircleProgress("charge", CircleProgressType.Manual, obj);
UIHelperStandalone.Instance.SetCircleProgress01("charge", 0.5f);
UIHelperStandalone.Instance.DestroyCircleProgress("charge");
```
