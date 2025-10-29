# 修改器与策略系统使用说明

本文档介绍了修改器与策略系统的使用方法，以及如何通过不同的修改器类型来灵活地修改数值。该系统基于可扩展的策略模式，支持序列化和反序列化操作，适用于动态数据修改场景。

## 目录
1. [系统概述](#系统概述)
2. [核心概念](#核心概念)
    - [修改器类型](#修改器类型)
    - [修改器策略](#修改器策略)
    - [序列化的修改器](#序列化的修改器)
3. [如何使用](#如何使用)
    - [创建修改器](#创建修改器)
    - [应用修改器](#应用修改器)
4. [扩展和自定义](#扩展和自定义)

---

## 系统概述

该系统通过 **修改器** 和 **修改器策略** 来提供灵活的数值修改功能。每个修改器可以包含不同的修改值，并且可以通过不同的策略来应用这些修改。系统支持序列化，允许修改器的保存和加载，方便在游戏或应用中进行数据的持久化操作。

## 核心概念

### 修改器类型

修改器的类型通过 `ModifierType` 枚举定义，常见的类型包括：

- **Add**: 加法修改器。
- **Mul**: 乘法修改器。
- **PriorityAdd**: 优先级加法修改器。
- **PriorityMul**: 优先级乘法修改器。
- **Override**: 覆盖修改器（通过优先级决定最终值）。
- **Clamp**: 限制范围修改器。

这些修改器类型用于决定如何处理数据，并通过不同策略对数值进行操作。

### 修改器策略

修改器策略通过 `IModifierStrategy` 接口定义，策略的作用是对目标值进行修改。每种策略实现了具体的修改规则，以下是常见策略的介绍：

1. **AddModifierStrategy**: 实现加法操作，将多个修改器的值加到目标值。
2. **AfterAddModifierStrategy**: 该策略类似于 `AddModifierStrategy`，但它在加法操作后应用修改。
3. **ClampModifierStrategy**: 对目标值应用夹紧操作，确保数值在指定范围内。
4. **MulModifierStrategy**: 对目标值应用乘法操作，将多个修改器的值相乘。
5. **OverrideModifierStrategy**: 根据优先级选择合适的修改器，直接覆盖目标值。
6. **PriorityAddModifierStrategy**: 根据修改器的优先级决定加法操作的应用。
7. **PriorityMulModifierStrategy**: 根据修改器的优先级决定乘法操作的应用。

### 序列化的修改器

`SerializableModifier` 类提供了修改器的可序列化表示。通过它，可以将修改器保存到文件或数据库中，后续可以从存储中加载并恢复修改器的状态。

`SerializableModifier` 类包含以下字段：
- `Type`: 修改器类型。
- `Priority`: 修改器的优先级。
- `FloatValue`: 浮动值的修改。
- `RangeValue`: 范围值，用于带范围的修改器。
- `IsRangeModifier`: 是否为范围修改器。

---

## 如何使用

### 创建修改器

使用 `IModifier` 或其子类（如 `FloatModifier` 和 `RangeModifier`）创建修改器。每个修改器应包含值和优先级。

```csharp
IModifier addModifier = new FloatModifier { Value = 10f, Priority = 1 };
IModifier mulModifier = new RangeModifier { Value = new Vector2(1f, 2f), Priority = 2 };
