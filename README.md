# DragonBloodContract

一个类似于死亡细胞和星界战士的2D动作平台游戏项目。

## 项目结构

```
Assets/
  ├── Scripts/
  │   ├── Player/           # 玩家相关脚本
  │   ├── Enemies/          # 敌人相关脚本
  │   ├── Managers/         # 游戏管理器脚本
  │   ├── UI/               # UI相关脚本
  │   └── Utils/            # 工具类脚本
  ├── Scenes/
  │   ├── Persistent/       # 持久化场景
  │   └── Levels/           # 关卡场景
  ├── Prefabs/              # 预制体
  ├── UI/                   # UI Toolkit资源
  │   ├── Icons/            # UI图标
  │   ├── MainUI.uxml       # 主游戏UI
  │   ├── PauseUI.uxml      # 暂停菜单UI
  │   ├── GameOverUI.uxml   # 游戏结束UI
  │   └── GameStyles.uss    # 共享样式表
  ├── Art/                  # 美术资源
  ├── Animations/           # 动画资源
  └── Audio/                # 音频资源
```

## 核心脚本说明

### 管理器脚本

- **GameManager**: 游戏主管理器，负责场景加载和游戏流程控制
- **AddressablesManager**: 资源管理器，基于Addressables系统加载和管理资源
- **UIToolkitManager**: UI管理器，基于UI Toolkit处理所有UI相关功能
- **ObjectPooler**: 对象池管理器，用于优化游戏性能

### 玩家脚本

- **PlayerManager**: 玩家状态管理，处理生命值、能量等数据
- **PlayerController**: 玩家控制器，处理输入和移动

### 敌人脚本

- **EnemyBase**: 敌人基类，提供基础AI行为

## 技术栈

本项目使用以下Unity技术：

1. **UI Toolkit**: 用于构建现代化、响应式的用户界面
   - 使用UXML定义UI结构
   - 使用USS定义UI样式
   - 支持事件系统和数据绑定

2. **Addressables**: 用于资源管理和异步加载
   - 优化内存使用和加载时间
   - 支持远程资源加载和更新
   - 管理资源依赖关系

3. **新输入系统**: 用于处理玩家输入
   - 支持多平台输入映射
   - 可配置的输入动作和绑定
   - 事件驱动的输入处理

## 场景结构

本项目使用两种类型的场景:

1. **持久化场景 (PersistentScene)**
   - 包含不会被销毁的管理器对象
   - 包含玩家角色
   - 负责在关卡间保持状态

2. **关卡场景 (Level1, Level2, ...)**
   - 包含特定关卡的环境和敌人
   - 可以独立设计和加载
   - 通过Addressables异步加载

## 使用说明

1. 打开 `Assets/Scenes/Persistent/PersistentScene.unity` 作为主场景
2. 在Unity编辑器中按Play运行游戏
3. 游戏将自动加载第一个关卡

## 开发指南

### 添加新敌人

1. 创建一个继承自 `EnemyBase` 的新脚本
2. 重写必要的方法来自定义行为
3. 将脚本附加到敌人预制体上

### 添加新关卡

1. 复制 `Assets/Scenes/Levels/Level1.unity` 并重命名
2. 在新场景中设计关卡
3. 确保场景中有一个带有 "PlayerSpawnPoint" 标签的游戏对象
4. 将场景添加到Addressables系统中
5. 在 `GameManager` 中添加关卡引用

### 添加新UI界面

1. 创建新的UXML文件定义界面结构
2. 使用GameStyles.uss中的样式类
3. 在UIToolkitManager中添加加载和管理代码

## 输入系统

本项目使用Unity新输入系统，主要操作包括:

- **WASD/方向键**: 移动
- **空格键**: 跳跃
- **鼠标左键/J键**: 攻击
- **E键**: 互动
- **Shift键**: 冲刺

## 注意事项

- 玩家脚本放在持久化场景中，确保在关卡间保持状态
- 使用对象池来优化频繁创建/销毁的对象(如子弹、特效等)
- 使用Addressables管理所有资源，避免直接引用
- 所有UI元素应通过UIToolkitManager进行管理 