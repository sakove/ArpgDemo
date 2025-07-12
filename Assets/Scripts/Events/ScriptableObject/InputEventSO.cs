using UnityEngine;

/// <summary>
/// 输入事件SO，专门用于处理游戏输入事件
/// </summary>
[CreateAssetMenu(fileName = "InputEventSO", menuName = "Events/InputEventSO")]
public class InputEventSO : BaseEventSO<InputEventData>
{
    [Header("输入信息")]
    [Tooltip("输入的描述，如'攻击'、'跳跃'等")]
    public string inputDescription;
    [Tooltip("默认的输入按键")]
    public string defaultInputKey;
    [Tooltip("输入的类型")]
    public InputEventType inputType;
    
    /// <summary>
    /// 触发按下输入事件
    /// </summary>
    /// <param name="sender">触发事件的对象</param>
    public void TriggerPressed(object sender)
    {
        InputEventData data = new InputEventData
        {
            InputState = InputState.Pressed,
            InputType = inputType,
            Value = 1.0f
        };
        RaiseEvent(data, sender);
    }

    /// <summary>
    /// 触发释放输入事件
    /// </summary>
    /// <param name="sender">触发事件的对象</param>
    public void TriggerReleased(object sender)
    {
        InputEventData data = new InputEventData
        {
            InputState = InputState.Released,
            InputType = inputType,
            Value = 0.0f
        };
        RaiseEvent(data, sender);
    }
    
    /// <summary>
    /// 触发持续输入事件（用于模拟量输入如摇杆）
    /// </summary>
    /// <param name="value">输入值，通常是一个向量的分量</param>
    /// <param name="sender">触发事件的对象</param>
    public void TriggerValue(float value, object sender)
    {
        InputEventData data = new InputEventData
        {
            InputState = InputState.Performed,
            InputType = inputType,
            Value = value
        };
        RaiseEvent(data, sender);
    }
    
    /// <summary>
    /// 触发矢量输入事件（用于移动等）
    /// </summary>
    /// <param name="vector">输入向量</param>
    /// <param name="sender">触发事件的对象</param>
    public void TriggerVector(Vector2 vector, object sender)
    {
        InputEventData data = new InputEventData
        {
            InputState = InputState.Performed,
            InputType = inputType,
            Vector = vector
        };
        RaiseEvent(data, sender);
    }
}

/// <summary>
/// 输入事件数据，包含输入的详细信息
/// </summary>
[System.Serializable]
public class InputEventData
{
    public InputState InputState;
    public InputEventType InputType;
    public float Value;
    public Vector2 Vector;
    
    public override string ToString()
    {
        return $"[{InputType}] {InputState}: Value={Value}, Vector=({Vector.x}, {Vector.y})";
    }
}

/// <summary>
/// 输入状态枚举
/// </summary>
public enum InputState
{
    Pressed,    // 按下
    Released,   // 释放
    Performed   // 持续执行（用于模拟量输入）
}

/// <summary>
/// 输入事件类型枚举
/// </summary>
public enum InputEventType
{
    Move,       // 移动
    Jump,       // 跳跃
    Sprint,     // 冲刺
    Attack,     // 攻击
    Interact,   // 交互
    Skill,      // 技能
    Menu        // 菜单
} 