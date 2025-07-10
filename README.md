# DragonBloodContract

一个基于Unity引擎开发的2D动作平台游戏，灵感来源于《死亡细胞》和《星界战士》。

## 核心技术栈

- **渲染**: Universal Render Pipeline (URP)
- **UI**: UI Toolkit
- **资源管理**: Addressables
- **输入系统**: New Input System
- **动画**: DOTween

## 项目结构概览

本项目的目录结构经过精心设计，旨在实现高内聚、低耦合的目标。

```
Assets/
├── AddressableAssetsData/ # Addressables 系统配置文件
├── Art/                   # 美术资源 (模型、贴图、特效等)
├── Audio/                 # 音频资源 (音乐、音效)
├── Game Data/             # ScriptableObject 游戏数据
│   └── Events/            # 事件系统数据 (事件频道)
├── Prefabs/               # 预制体 (玩家、敌人、场景道具)
├── Scenes/                # 游戏场景
│   ├── Persistent/        # 持久化场景
│   └── Levels/            # 关卡场景
├── Scripts/               # C# 脚本
│   ├── Combat/            # 战斗系统 (连击、伤害计算)
│   ├── Effects/           # 特效控制 (相机抖动、时间暂停)
│   ├── Enemies/           # 敌人AI和行为
│   ├── Events/            # 事件系统核心逻辑
│   ├── Managers/          # 核心管理器
│   ├── Player/            # 玩家控制和状态
│   ├── Skills/            # 技能系统
│   └── StateMachine/      # 状态机 (玩家、敌人)
└── UI Toolkit/            # UI Toolkit 资源 (UXML, USS)
```

## 核心系统详解

### 1. 事件系统 (`Scripts/Events`)

采用基于 `ScriptableObject` 的事件系统，实现各模块间的解耦通信。

- **`ScriptableObject/`**: 定义事件频道 (Event Channel)，如 `IntEventSO`, `VoidEventSO`。
- **`MonoBehaviour/`**: 提供事件监听器 (EventListener)，如 `IntEventListener`，可方便地在Inspector中绑定事件和响应。
- **`Editor/`**: 提供自定义编辑器扩展，用于在Inspector中调试和查看事件的订阅情况。

### 2. 状态机 (`Scripts/StateMachine`)

一个通用的状态机实现，目前主要用于玩家 (`PlayerStateMachine`)。

- **`PlayerState.cs`**: 所有玩家状态的基类。
- **`PlayerStateMachine.cs`**: 状态机核心，负责状态的切换和管理。
- **`States/`**: 包含具体的状态实现，如 `PlayerIdleState`, `PlayerMovingState`, `PlayerAttackingState` 等。

### 3. 技能系统 (`Scripts/Skills`)

一个可扩展的技能系统，允许通过 `ScriptableObject` 定义不同的技能。

- **`Skill.cs`**: 所有技能的基类，定义了技能的通用属性（如冷却时间、消耗）。
- **`BasicAttackSkill.cs`**: 一个具体的攻击技能实现，展示了如何定义伤害、攻击范围和连击。
- **`CombatController.cs`**: 负责处理技能的激活、连击逻辑和冷却计时。

### 4. 场景管理

项目采用分离的场景加载策略，以提高性能和灵活性。

- **`Persistent.unity`**: 持久化场景，包含`GameManager`、`InputManager`等在游戏运行期间不应被销毁的核心管理器。
- **`Levels/`**: 包含所有可玩关卡。这些关卡由 `GameManager` 通过 `Addressables` 系统异步加载和卸载。

## 开发指南

### 如何添加新技能

1. 在 `Assets/Scripts/Skills` 目录下，创建一个新的脚本并继承自 `Skill.cs`。
2. 重写 `Activate(GameObject user)` 方法来实现技能逻辑。
3. 在Unity编辑器中，右键 `Create -> Skills -> [你的技能]` 来创建一个新的技能实例。
4. 在 `PlayerController` 或 `CombatController` 中引用并使用这个新的技能资源。

### 如何添加新状态

1. 在 `Assets/Scripts/StateMachine/States` 目录下，创建一个新的脚本并继承自 `PlayerState.cs`。
2. 实现状态的 `Enter`, `Exit`, `LogicUpdate`, `PhysicsUpdate` 方法。
3. 在 `PlayerStateMachine.cs` 中实例化并注册这个新状态。
4. 在需要进行状态转换的地方，调用 `stateMachine.ChangeState()` 方法。

## 使用说明

1. 确保已在 `Package Manager` 中安装 `Input System`, `Addressables`, 和 `UI Toolkit` 包。
2. 首次运行时，从 `Assets/Scenes/Persistent/Persistent.unity` 场景启动。
3. 游戏将自动加载 `MainMenu` 或第一个关卡。 