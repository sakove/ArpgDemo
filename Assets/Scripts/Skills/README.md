# 技能动画系统使用指南 (AnimationOverride 方案)

本文档介绍如何在 Animator 中进行设置，以配合当前项目统一使用的、基于 `AnimationOverrideController` 的技能动画系统。

## 核心思想：动态覆盖，告别繁杂

本项目的技能系统**只使用一种动画控制方式：动画覆盖 (AnimationOverride)**。

这种方式的核心是，在 Animator 中我们不为每一个具体技能（如火球、冲斩）创建单独的动画状态，而是只有一个名为 `"Skill"` 的**通用占位状态**。在代码中，当一个技能被激活时，我们会动态地将这个技能对应的动画片段（`AnimationClip`）“塞”到这个占位状态里去播放。

**优点:**
- **高度可扩展**: 添加新技能无需修改 Animator，避免其变得越来越臃肿、混乱。
- **职责分离**: 动画师和设计师可以独立创建动画和技能，无需开发者介入修改状态机。
- **维护简单**: Animator 结构保持极简，一目了然。

## Animator 设置指南

为了让该系统正常工作，你的 Animator Controller (例如 `Player.controller`) 中需要进行以下简单设置。

### 1. 所需参数 (Parameters)

在 Animator 窗口的 "Parameters" 标签页，请确保有以下参数：

- `UseSkill` (Trigger): 一个通用的触发器，用于从任何状态转换到我们的技能占位状态。
- `IsAttacking` (Bool): 用于标记角色是否处于攻击或技能状态，以控制状态转换。

> 注意：不再需要 `SkillID` 这个整数参数来控制动画了。

### 2. 所需状态 (States)

在 Animator 的基础层 (Base Layer) 中，你需要创建一个动画状态，并将其**精确命名为 `Skill`**。

- **`Motion`**: 为这个状态随便指定一个动画片段作为**占位符**。这个动画本身不会被播放，它仅仅是作为被覆盖的目标。
- **`Transitions`**:
    - **进入过渡**: 创建一个从 `Any State` 到 `Skill` 状态的过渡。
        - **Conditions**: `UseSkill`
    - **退出过渡**: 创建一个从 `Skill` 到你的人物默认状态（如 `Idle` 或 `Movement` 混合树）的过渡。
        - **Has Exit Time**: **勾选** (设置为 true)。
        - **Exit Time**: 通常设置为 1，表示动画播放完毕后自动退出。

**简化的状态机结构示例:**
```
Base Layer
├── Movement (Blend Tree)
├── Jump
├── Fall
└── Skill  <-- 我们的通用技能占位状态
```
(从 `Any State` 连线到 `Skill`, 条件是 `UseSkill` 触发器)
(从 `Skill` 连线到 `Movement`, 条件是 `Exit Time`)

## 如何添加一个带动画的新技能

遵循这个系统，添加新技能的流程非常简单：

1.  **创建动画片段**: 制作你的技能动画，保存为 `.anim` 文件。
2.  **创建技能资产**: 在 Project 窗口中，右键 `Create -> Skills -> [你的技能类型]` 来创建一个新的技能资产文件（`.asset`）。
3.  **关联动画**: 选中你新创建的技能资产，在 Inspector 窗口中，找到 `Skill Animation` 字段，将你的 `.anim` 文件拖拽进去。
4.  **完成!** 将这个技能资产分配到 `CombatController` 的技能槽中即可使用。完全不需要打开或修改 Animator Controller。

## 代码工作流（幕后英雄）

简单来说，代码层面会自动完成以下工作：

1.  玩家输入技能，`PlayerStateMachine` 进入 `PlayerUsingSkillState`。
2.  `PlayerUsingSkillState` 调用当前技能的 `Activate()` 方法。
3.  `skill.Activate()` 内部调用 `PlayAnimation()`。
4.  `PlayAnimation()` 会：
    a. 获取或创建 `AnimatorOverrideController`。
    b. 将当前技能的 `skillAnimation` 字段里的动画片段，覆盖到 Animator 中名为 `"Skill"` 的动画片段上。
    c. 触发 `UseSkill` 扳机。
5.  Animator 响应触发器，进入 `Skill` 状态，此时播放的已经是被我们动态替换后的新动画了。 