# 分层状态机设置指南

本文档提供了如何为玩家角色设置分层状态机的详细步骤。

## 分层结构概述

我们将创建以下层级结构：

1. **Base Layer (权重: 1)**
   - 负责基本移动动画（闲置、移动、跳跃、下落）
   - 不使用遮罩，影响整个角色

2. **Combat Layer (权重: 1)**
   - 负责战斗相关动画（各种攻击、技能施放）
   - 根据当前移动状态播放不同的攻击动画
   - 默认为空状态，不影响角色动画

3. **Interaction Layer (权重: 0 -> 1 -> 0)**
   - 负责特殊交互动画（睡觉、交互等）
   - 不使用遮罩，完全覆盖其他层
   - 默认权重为0，需要时通过代码设置为1

## 设置步骤

### 1. 创建新的Animator Controller

1. 在Project窗口中，右键点击`Assets/Animations`文件夹
2. 选择`Create > Animator Controller`
3. 命名为`PlayerLayered`

### 2. 设置Base Layer

1. 打开`PlayerLayered`控制器
2. 默认已有一个名为"Base Layer"的层
3. 添加以下状态：
   - **Idle**: 默认状态，播放角色闲置动画
   - **Moving**: 播放角色移动动画
   - **Jumping**: 播放角色跳跃动画
   - **Falling**: 播放角色下落动画
4. 设置状态转换条件：
   - Idle -> Moving: `Speed > 0.1`
   - Moving -> Idle: `Speed < 0.1`
   - Idle/Moving -> Jumping: `Jump`
   - Jumping -> Falling: `VerticalSpeed < 0`
   - Falling -> Idle: `IsGrounded && Speed < 0.1`
   - Falling -> Moving: `IsGrounded && Speed > 0.1`

### 3. 添加Combat Layer

1. 在Animator窗口左上角，点击"Layers"
2. 点击"+"按钮添加新层
3. 命名为"Combat"
4. 设置权重为1
5. 添加以下状态：
   - **Empty**: 默认状态，不包含任何动画（空状态）
   - **IdleAttack**: 站立时的攻击动画
   - **RunAttack**: 跑步时的攻击动画
   - **JumpAttack**: 跳跃时的攻击动画
   - **FallAttack**: 下落时的攻击动画
   - **Skill**: 技能动画（将被动态覆盖）
6. 设置状态转换条件：
   - Empty -> IdleAttack: `Attack` (Trigger) and `CurrentState == 0`
   - Empty -> RunAttack: `Attack` (Trigger) and `CurrentState == 1`
   - Empty -> JumpAttack: `Attack` (Trigger) and `CurrentState == 2`
   - Empty -> FallAttack: `Attack` (Trigger) and `CurrentState == 3`
   - Empty -> Skill: `UseSkill`
   - IdleAttack/RunAttack/JumpAttack/FallAttack/Skill -> Empty: `Exit Time` (设置为动画结束时)

### 4. 添加Interaction Layer

1. 再次点击"+"按钮添加新层
2. 命名为"Interaction"
3. 设置**默认权重为0**（重要！这样默认情况下不会影响其他层）
4. 添加以下状态：
   - **Empty**: 默认状态，不包含任何动画
   - **Sleeping**: 播放角色睡觉动画
   - **Special**: 通用特殊动画状态（将被动态覆盖）
5. 设置状态转换条件：
   - Empty -> Sleeping: `Sleep`
   - Empty -> Special: `SpecialAction`
   - Sleeping/Special -> Empty: `Exit Time` (设置为动画结束时)

### 5. 设置参数

在Parameters选项卡中添加以下参数：

1. **Speed** (Float): 角色移动速度
2. **IsGrounded** (Bool): 角色是否在地面上
3. **VerticalSpeed** (Float): 角色垂直速度
4. **Jump** (Trigger): 跳跃触发器
5. **Attack** (Trigger): 攻击触发器
6. **UseSkill** (Trigger): 技能使用触发器
7. **Sleep** (Trigger): 睡觉触发器
8. **SpecialAction** (Trigger): 特殊动作触发器
9. **CurrentState** (Int): 当前移动状态的整数表示（由AnimationStateHelper设置）

### 6. 添加AnimationStateHelper组件

1. 在玩家预制体上添加`AnimationStateHelper`组件
2. 确保`Animator`字段指向玩家的Animator组件
3. 保持`CurrentState`参数名为默认值

## 注意事项

1. **组合动画**: 由于我们使用的是完整的精灵图片动画，我们需要为每种组合状态（如跑步攻击、跳跃攻击）准备单独的动画片段。

2. **动画覆盖**: 我们将继续使用`AnimatorOverrideController`来动态替换技能动画，但现在它只会影响Combat Layer的"Skill"状态或Interaction Layer的"Special"状态。

3. **层权重控制**: 在代码中，我们需要动态控制Interaction Layer的权重。例如：
   ```csharp
   // 开始特殊交互动画
   animator.SetLayerWeight(2, 1f); // 2是Interaction Layer的索引
   animator.SetTrigger("Sleep");
   
   // 结束时（通过动画事件或计时器）
   animator.SetLayerWeight(2, 0f);
   ```

4. **攻击动画选择**: 我们使用`AnimationStateHelper`组件来判断角色当前的移动状态，并触发相应的攻击动画。 