# 分层状态机系统使用指南

本文档提供了如何使用新的分层状态机系统的详细说明。

## 系统概述

我们的分层状态机系统由以下几个部分组成：

1. **分层Animator Controller**：使用Unity的分层动画系统，将动画分为基础层、战斗层和交互层。
2. **PlayerController扩展**：提供了控制动画层权重和触发动画的方法。
3. **PlayerStateMachine扩展**：增加了对特殊动画状态的支持。
4. **PlayerSpecialAnimationState**：专门用于处理特殊动画（如睡觉、重生等）。
5. **AnimationStateHelper**：辅助判断角色当前的移动状态，用于选择合适的攻击动画。

## 动画层的使用

### 基础层 (Base Layer)

- **用途**：控制角色的基本移动动画（闲置、移动、跳跃、下落）。
- **权重**：始终为1，作为基础动画层。
- **参数**：
  - `Speed`：角色移动速度
  - `IsGrounded`：角色是否在地面上
  - `VerticalSpeed`：角色垂直速度
  - `Jump`：跳跃触发器

### 战斗层 (Combat Layer)

- **用途**：控制角色的战斗动画（攻击、技能）。
- **权重**：默认为1，可以通过`playerController.SetCombatLayerWeight()`方法调整。
- **参数**：
  - `Attack`：攻击触发器
  - `UseSkill`：技能使用触发器
  - `CurrentState`：当前移动状态（由AnimationStateHelper设置）

### 交互层 (Interaction Layer)

- **用途**：控制角色的特殊交互动画（睡觉、重生等）。
- **权重**：默认为0，需要时通过`playerController.SetInteractionLayerWeight()`方法设置为1。
- **参数**：
  - `Sleep`：睡觉触发器
  - `SpecialAction`：通用特殊动作触发器

## 组合动画系统

由于我们使用的是完整的精灵图片动画，我们需要为每种组合状态准备单独的动画片段：

1. **基础攻击动画**：
   - `IdleAttack`：站立时的攻击动画
   - `RunAttack`：跑步时的攻击动画
   - `JumpAttack`：跳跃时的攻击动画
   - `FallAttack`：下落时的攻击动画

2. **AnimationStateHelper**：
   - 这个组件会持续监测角色的移动状态（闲置、移动、跳跃、下落）。
   - 当玩家按下攻击键时，`PlayerAttackingState`会根据当前的移动状态触发相应的攻击动画。

3. **设置步骤**：
   - 为每种组合状态创建动画片段。
   - 在Combat Layer中创建对应的状态。
   - 设置从Empty到各个攻击状态的转换，使用相应的触发器。

## 如何添加新的技能动画

1. **创建技能动画**：
   - 创建一个新的动画片段（AnimationClip）。
   - 设置动画的关键帧和持续时间。

2. **配置技能ScriptableObject**：
   - 将动画片段分配给技能的`skillAnimation`字段。
   - 设置`isSpecialAnimation`属性：
     - `false`：普通技能，使用战斗层。
     - `true`：特殊动画，使用交互层。

3. **使用技能**：
   - 通过`stateMachine.ChangeState(stateMachine.UsingSkillState, skill)`切换到技能使用状态。
   - 系统会根据`isSpecialAnimation`属性自动选择合适的动画层。

## 如何添加新的特殊动画

1. **创建特殊动画**：
   - 创建一个新的动画片段（AnimationClip）。
   - 设置动画的关键帧和持续时间。

2. **在Animator Controller中添加触发器**：
   - 在交互层中添加一个新的状态，使用新的动画片段。
   - 添加一个从Empty到新状态的转换，使用新的触发器。
   - 添加一个从新状态回到Empty的转换，使用Exit Time。

3. **使用特殊动画**：
   - 通过`stateMachine.ChangeToSpecialAnimationState("触发器名称", 持续时间)`切换到特殊动画状态。
   - 系统会自动设置交互层权重为1，并在动画结束后恢复。

## 示例：添加睡觉功能

```csharp
// 创建一个睡觉点
public class SleepingSpot : MonoBehaviour
{
    [SerializeField] private float sleepDuration = 3f;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStateMachine stateMachine = other.GetComponent<PlayerStateMachine>();
            if (stateMachine != null)
            {
                // 切换到睡觉状态
                stateMachine.ChangeToSpecialAnimationState("Sleep", sleepDuration);
            }
        }
    }
}
```

## 注意事项

1. **组合动画**：由于使用完整的精灵图片，我们需要为每种组合状态（如跑步攻击、跳跃攻击）准备单独的动画片段。

2. **动画覆盖**：我们继续使用`AnimatorOverrideController`来动态替换技能动画，但现在它会根据技能类型影响不同的动画层。

3. **层权重控制**：
   - 战斗层权重通常保持为1，除非在特定情况下需要禁用战斗动画。
   - 交互层权重默认为0，只有在播放特殊动画时才设置为1。

4. **状态转换**：
   - 普通状态转换使用`stateMachine.ChangeState(newState)`。
   - 技能状态转换使用`stateMachine.ChangeState(stateMachine.UsingSkillState, skill)`。
   - 特殊动画状态转换使用`stateMachine.ChangeToSpecialAnimationState(trigger, duration)`。 