# DragonBloodContract

一个基于Unity引擎开发的2D动作平台游戏，灵感来源于《死亡细胞》和《星界战士》。

## 核心技术栈

- **引擎**: Unity
- **渲染**: Universal Render Pipeline (URP)
- **UI**: UI Toolkit
- **资源管理**: Addressables
- **输入系统**: New Input System
- **动画**: Animator, DOTween

## 项目结构概览

本项目的目录结构经过精心设计，旨在实现高内聚、低耦合的目标。

```
Assets/
├── AddressableAssetsData/ # Addressables 系统配置文件
├── Art/                   # 美术资源 (模型、贴图、特效等)
├── Audio/                 # 音频资源 (音乐、音效)
├── Game Data/             # ScriptableObject 游戏数据
│   ├── Events/            # 事件系统数据 (事件频道)
│   └── Skills/            # 技能 ScriptableObject 资产
├── Prefabs/               # 预制体 (玩家、敌人、场景道具)
├── Scenes/                # 游戏场景
│   ├── Persistent/        # 持久化场景
│   └── Levels/            # 关卡场景
├── Scripts/               # C# 脚本
│   ├── Combat/            # 战斗系统 (连击、技能使用)
│   ├── Effects/           # 特效控制 (相机抖动、时间暂停)
│   ├── Enemies/           # 敌人AI和行为
│   ├── Events/            # 事件系统核心逻辑
│   ├── Managers/          # 核心管理器
│   ├── Player/            # 玩家控制和核心组件
│   ├── Skills/            # 技能 ScriptableObject 基类
│   └── StateMachine/      # 状态机 (玩家、敌人)
└── UI Toolkit/            # UI Toolkit 资源 (UXML, USS)
```

## 核心系统详解

### 1. 事件系统 (`Scripts/Events`)

采用基于 `ScriptableObject` 的事件系统，实现各模块间的解耦通信。事件的监听和触发都通过配置 `ScriptableObject` 资产完成，无需在代码中硬编码引用。

### 2. 状态机 (`Scripts/StateMachine`)

一个通用的状态机实现，目前主要用于玩家 (`PlayerStateMachine`)。它负责管理玩家的各种状态（如移动、跳跃、攻击、使用技能等）及其之间的转换逻辑。

### 3. 技能系统 (`Scripts/Skills` & `Game Data/Skills`)

一个高度可扩展的技能系统，其核心是 `ScriptableObject`。

- **技能定义**: 每个技能都是一个 `Skill.cs` 的派生类资产，可以在 Inspector 中独立配置其冷却、伤害、音效等属性。
- **动画实现**: 采用 `AnimatorOverrideController` 动态管理技能动画。只需在技能资产中指定动画片段，无需修改 Animator Controller，极大地简化了新技能的添加流程。
- **战斗集成**: `CombatController.cs` 负责处理技能的激活、连击逻辑和冷却计时。

### 4. 场景管理

项目采用分离的场景加载策略，以提高性能和灵活性。
- **`Persistent.unity`**: 持久化场景，包含`GameManager`、`InputManager`等在游戏运行期间不应被销毁的核心管理器。
- **关卡加载**: 所有关卡场景都由 `GameManager` 通过 `Addressables` 系统异步加载和卸载。

## 开发指南

### 如何添加新技能

1.  在 `Assets/Scripts/Skills` 目录下，创建一个新的C#脚本并继承自 `Skill.cs` (仅当需要独特的技能逻辑时)。
2.  在Unity编辑器中，右键 `Create -> Skills -> Basic Skill` (或你的新技能类型) 来创建一个新的技能资产。
3.  在Inspector中配置技能参数，并将对应的**动画片段**拖入 `Skill Animation` 字段。
4.  将配置好的技能资产添加到 `Player` 预制体上的 `CombatController` 的技能槽中。

### 如何添加新状态

1.  在 `Assets/Scripts/StateMachine/States` 目录下，创建一个新的C#脚本并继承自 `PlayerState.cs`。
2.  实现状态的 `Enter`, `Exit`, `LogicUpdate`, `PhysicsUpdate` 方法。
3.  在 `PlayerStateMachine.cs` 中实例化并注册这个新状态。
4.  在需要进行状态转换的地方，调用 `stateMachine.ChangeState()` 方法。

## 项目启动

1.  确保已在 `Package Manager` 中安装 `Input System`, `Addressables`, 和 `UI Toolkit` 包。
2.  游戏的启动入口是 `Persistent` 场景。`GameManager` 会自动处理后续主菜单和关卡的加载流程。 