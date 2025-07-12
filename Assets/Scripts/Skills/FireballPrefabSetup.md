# 火球预制体设置指南

本文档介绍如何创建一个与 `FireballSkill` 和 `Projectile` 脚本兼容的火球预制体。

## 基本设置

1. 创建一个新的空游戏对象，命名为 "Fireball"
2. 添加以下组件:
   - `SpriteRenderer`: 用于火球的视觉表现
   - `CircleCollider2D`: 设置为触发器 (Is Trigger = true)
   - `Rigidbody2D`: 设置为动态 (Dynamic)，并禁用重力 (Gravity Scale = 0)
   - `TrailRenderer`: 为火球添加拖尾效果
   - `Projectile`: 我们刚刚创建的脚本

## 详细配置

### SpriteRenderer
- 分配一个适合的火球精灵
- 调整 Order in Layer 确保火球在适当的渲染层级
- 可以添加材质以实现发光效果

### CircleCollider2D
- 设置为触发器 (Is Trigger = true)
- 调整半径以匹配视觉大小 (通常为 0.3-0.5)

### Rigidbody2D
- Body Type: Dynamic
- Gravity Scale: 0
- Collision Detection: Continuous
- Interpolate: Interpolate
- Constraints: 根据需要冻结 Z 轴旋转

### TrailRenderer
- 设置适当的宽度 (通常为 0.1-0.3)
- 设置时间 (Time = 0.5)
- 添加渐变颜色 (从橙色/红色到透明)
- 材质: 使用 "Particles/Additive" 或自定义材质

### Projectile 脚本配置
- **基本属性**:
  - Damage: 20 (或根据游戏平衡调整)
  - Speed: 15
  - Lifetime: 3
  - Direction: Vector2.right (会被 FireballSkill 覆盖)

- **物理设置**:
  - Affected By Gravity: false
  - Destroy On Hit: true
  - Max Penetration: 0 (无穿透)

- **视觉效果**:
  - Hit Effect Prefab: 分配一个爆炸特效预制体
  - Destroy Effect Prefab: 可以使用相同的爆炸特效
  - Trail Renderer: 拖放场景中的 TrailRenderer 组件

- **音效**:
  - Launch Sound: 火球发射音效
  - Hit Sound: 爆炸音效
  - Destroy Sound: 可以使用相同的爆炸音效

## 粒子效果

为了增强视觉效果，可以添加粒子系统:

1. 添加一个子对象，命名为 "FireParticles"
2. 添加 `ParticleSystem` 组件
3. 配置粒子系统:
   - Start Lifetime: 0.5-1.0
   - Start Speed: 1-2
   - Start Size: 0.1-0.3
   - Start Color: 橙色/红色带透明度
   - Shape: Cone 或 Sphere
   - Emission Rate: 20-30
   - Renderer: 使用 "Particles/Additive" 材质

## 预制体保存

完成配置后，将整个游戏对象拖到 Project 窗口中以创建预制体。

## 使用方法

1. 在 `FireballSkill` ScriptableObject 中，将创建的火球预制体分配给 `fireballPrefab` 字段
2. 调整 `castDistance` 和其他参数以匹配您的游戏需求
3. 将 `FireballSkill` 分配给 `CombatController` 的技能槽之一

## 注意事项

- 确保火球预制体的层级设置正确，以便它能与敌人和环境正确交互
- 如果火球不应该与某些对象碰撞，请适当设置物理层级
- 考虑为火球添加光源组件以增强视觉效果
- 可以调整 `Projectile` 脚本中的参数来创建不同类型的火球变体 