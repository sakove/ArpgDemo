# 技能动画系统使用指南

本文档介绍如何在 Animator 中设置动画状态，以配合技能系统的三种动画控制方式。

## 动画控制方式

技能系统支持三种动画控制方式：

1. **触发器控制 (Trigger)**：使用命名的触发器来激活特定动画
2. **参数控制 (Parameter)**：使用技能ID参数和通用触发器来激活动画
3. **动画覆盖 (AnimationOverride)**：动态替换动画片段

## Animator 设置指南

### 1. 基本设置

在 Animator 中，需要添加以下参数：

- `Attack` (Trigger)：基础攻击触发器
- `UseSkill` (Trigger)：通用技能触发器
- `SkillID` (Int)：技能ID参数
- `IsAttacking` (Bool)：标记是否正在攻击

### 2. 状态机结构

建议的状态机结构如下：

```
Base Layer
├── Idle
├── Run
├── Jump
├── Fall
├── Attack
│   ├── Attack1
│   ├── Attack2
│   └── Attack3
└── Skills
    ├── Skill_Generic (由SkillID参数控制内部过渡)
    │   ├── Fireball (SkillID = 1)
    │   ├── IceSpear (SkillID = 2)
    │   └── LightningBolt (SkillID = 3)
    └── Skill_Override (使用动画覆盖)
```

### 3. 过渡设置

#### 从任何状态到攻击状态的过渡：
- 条件：`Attack` 触发器
- 可中断：根据游戏设计决定

#### 从任何状态到技能状态的过渡：
- 条件：`UseSkill` 触发器
- 可中断：根据游戏设计决定

#### 技能子状态机内部过渡：
- 从入口到具体技能状态的条件：`SkillID` 等于特定值
- 例如：到 Fireball 状态的条件是 `SkillID = 1`

### 4. 动画覆盖设置

对于使用动画覆盖的技能：

1. 创建一个 AnimatorOverrideController 资源
2. 将其基础控制器设置为角色的 AnimatorController
3. 在覆盖列表中，添加一个名为 "Skill" 的动画片段
4. 在运行时，技能系统会自动替换这个动画片段

## 使用示例

### 基础攻击（触发器方式）：
```csharp
// BasicAttackSkill 已配置为使用 "Attack" 触发器
```

### 火球技能（参数方式）：
```csharp
// FireballSkill 已配置为使用参数方式，SkillID = 1
```

### 冲斩技能（动画覆盖方式）：
```csharp
// DashSlashSkill 已配置为使用动画覆盖方式
// 需要在Inspector中为其分配skillAnimation
```

## 注意事项

1. 确保所有技能的动画持续时间与技能的 `skillDuration` 参数匹配
2. 对于使用动画覆盖的技能，必须在Inspector中分配 `skillAnimation`
3. 如果添加新的技能ID，记得在Animator中添加相应的状态和过渡条件 