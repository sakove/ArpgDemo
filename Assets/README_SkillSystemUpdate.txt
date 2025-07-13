# 技能系统更新：从3个技能槽扩展到7个

## 概述

我们已经将技能系统从3个技能槽扩展到7个，以支持更多的技能选择。这个更新涉及多个文件的修改，包括输入系统、战斗控制器和技能事件类型的更改。

## 主要更改

### 1. 输入事件类型更新 (InputEventSO.cs)

- 将原有的 `Skill` 类型细分为 `Skill1` 到 `Skill7` 七种类型
- 这样可以更精确地区分不同技能的输入事件

### 2. 输入管理器更新 (InputManager.cs)

- 添加了 `_skill4Event` 到 `_skill7Event` 的序列化字段
- 添加了技能4-7的输入处理方法
- 更新了 `ValidateInputEvents` 方法，确保每个技能事件的类型正确
- 添加了技能4-7的输入回调注册和取消注册代码（需要在重新生成InputSystem_Actions.cs后取消注释）

### 3. 战斗控制器更新 (CombatController.cs)

- 将 `equippedSkills` 数组大小从3扩展到7
- 将 `skillCooldowns` 数组大小从3扩展到7
- 添加了 `skill4Event` 到 `skill7Event` 的序列化字段
- 添加了技能4-7的输入回调方法
- 更新了事件订阅逻辑，支持技能4-7的事件处理

### 4. 输入系统动作更新 (InputSystem_Actions.inputactions)

- 添加了Skill4-Skill7的输入动作定义
- 为新增的技能设置了默认按键绑定（数字键4-7）
- 需要在Unity编辑器中重新生成InputSystem_Actions.cs文件

## 后续步骤

1. 在Unity编辑器中重新生成InputSystem_Actions.cs文件（参见 README_InputSystem.txt）
2. 取消注释InputManager.cs中的技能4-7输入回调注册和取消注册代码
3. 在Inspector中为新增的技能事件字段分配相应的InputEventSO资产
4. 在技能槽中分配更多的技能

## 技能使用方式

玩家现在可以使用以下按键触发技能：
- 技能1: 键盘数字键 1
- 技能2: 键盘数字键 2
- 技能3: 键盘数字键 3
- 技能4: 键盘数字键 4
- 技能5: 键盘数字键 5
- 技能6: 键盘数字键 6
- 技能7: 键盘数字键 7 