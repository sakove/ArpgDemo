# 输入系统设置指南

## 概述

本项目使用 Unity 的新输入系统 (New Input System)。所有的输入动作（如移动、跳跃、技能）都在 `Assets/InputSystem_Actions.inputactions` 这个资产文件中定义。

系统会根据这个 `.inputactions` 文件自动生成一个对应的 C# 脚本 (`InputSystem_Actions.cs`)，代码通过这个脚本来响应输入。

## **重要**: 何时以及如何更新输入脚本

如果你修改了 `InputSystem_Actions.inputactions` 文件（例如，添加了一个新的动作，或者修改了一个按键绑定），你 **必须** 手动重新生成 C# 脚本才能让改动生效。

**重新生成 `InputSystem_Actions.cs` 的步骤：**

1.  在 Unity 编辑器中，找到并选中 `Assets/InputSystem_Actions.inputactions` 文件。
2.  在 Inspector 窗口中，确认 "Generate C# Class" 选项是**勾选状态**。
3.  点击 Inspector 窗口右下角的 **"Apply"** 按钮。
4.  Unity 会自动更新 `InputSystem_Actions.cs` 文件。

![Inspector aasets](https://i.imgur.com/gA9Iu6j.png)

## 当前按键绑定参考

-   **移动**: WASD / 左摇杆
-   **跳跃**: 空格键 / Gamepad South
-   **攻击**: 鼠标左键 / J 键 / Gamepad West
-   **冲刺**: 左 Shift / K 键 / Gamepad East
-   **技能 1-7**: 键盘数字键 1-7 