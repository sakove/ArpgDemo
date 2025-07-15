# 龙血契约 (Dragon Blood Contract) - 脚本架构

## 项目架构

本项目采用基于组件和状态机的架构，以实现高度可扩展和可维护的2D横版动作游戏。

### 核心系统

1.  **状态机系统 (`StateMachine/`)**
    *   `PlayerStateMachine`: 管理玩家的各种状态（移动、跳跃、攻击、使用技能等）和状态转换。
    *   `PlayerState`: 所有具体状态的基类，定义了 `Enter`, `Exit`, `LogicUpdate` 等核心方法。
    *   具体状态存放于 `States/` 目录。

2.  **技能系统 (`Skills/`)**
    *   采用基于 `ScriptableObject` 的高度可配置化设计。每个技能都是一个独立的 `.asset` 文件。
    *   动画实现：统一使用 `AnimatorOverrideController` 动态替换动画，极大简化了新技能的添加流程，无需修改 Animator。
    *   `Skill.cs`: 所有技能的基类。

3.  **战斗系统 (`Combat/`)**
    *   `CombatController`: 处理玩家的战斗相关逻辑，如连击、技能使用和冷却。
    *   `IDamageable`: 可受伤害物体的统一接口。

4.  **效果系统 (`Effects/`)**
    *   `TimeManager`: 控制游戏时间，用于实现**命中停顿 (Hit Stop)**等增强打击感的效果。
    *   `CameraShake`: 相机震动效果，增强关键动作的视觉反馈。

### 文件夹结构

-   `StateMachine/`: 状态机相关脚本
    -   `States/`: 具体的状态类
-   `Skills/`: 技能 ScriptableObject 和相关逻辑
-   `Combat/`: 战斗系统相关脚本
-   `Effects/`: 游戏效果相关脚本
-   `Player/`: 玩家控制器和核心逻辑
-   `Enemies/`: 敌人相关脚本
-   `Managers/`: 游戏核心管理器
-   `UI/`: 用户界面
-   `Utils/`: 工具类

## 游戏手感优化 (Game Feel)

为了实现优秀的游戏手感，本项目实现了以下关键机制：

1.  **输入缓冲 (Input Buffering)**
    *   允许玩家在某个动作（如攻击）结束前提前输入下一个指令，指令会在当前动作结束后立即执行，使操作更连贯。

2.  **土狼时间 (Coyote Time)**
    *   允许玩家在离开平台边缘后的极短时间内仍然可以起跳，让跳跃手感更宽容，不易失误。

3.  **短跳/长跳 (Variable Jump Height)**
    *   通过检测跳跃键的按键时长来控制跳跃高度，长按跳得更高，短按则小跳。

4.  **命中停顿 (Hit Stop)**
    *   攻击命中敌人时，在极短时间内（几帧）冻结攻击者和受击者，极大地增强了打击的力量感。

5.  **相机震动 (Camera Shake)**
    *   在关键动作（如重击、受击）时添加相机震动，增强视觉反馈。

## 开发指南

### 添加新状态

1.  在 `StateMachine/States/` 目录下创建一个继承自 `PlayerState` 的新类。
2.  实现必要的方法：`Enter()`, `Exit()`, `LogicUpdate()` 等。
3.  在 `PlayerStateMachine.cs` 中实例化并注册这个新状态。
4.  在需要转换到此状态的地方，调用 `stateMachine.ChangeState()`。

### 添加新技能

得益于现在的动画系统，添加新技能非常简单：
1.  创建一个继承自 `Skill.cs` 的新技能逻辑类（如果需要特殊逻辑的话）。
2.  在Unity编辑器中，右键 `Create -> Skills -> Basic Skill` (或你自定义的技能类型) 来创建一个新的技能资产 (`.asset` 文件)。
3.  **配置技能参数**：设置冷却时间、持续时间等。
4.  **关联动画**：将该技能的动画片段 (`.anim`) 拖到技能资产的 `Skill Animation` 字段上。
5.  将配置好的技能资产分配给 `CombatController` 的 `Equipped Skills` 数组中。

### 添加新敌人

1.  创建一个继承自 `EnemyBase.cs` 的新类。
2.  实现敌人的AI和战斗逻辑。
3.  配置敌人的属性和行为。

## 输入系统

本项目使用Unity新输入系统 (Input System)，主要操作如下：

-   移动：WASD或方向键
-   跳跃：空格键
-   攻击：鼠标左键或J键
-   冲刺：左Shift或K键
-   技能1-7：键盘数字键 1-7 