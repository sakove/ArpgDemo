using UnityEngine;

/// <summary>
/// 输入事件SO，专门用于处理游戏输入事件
/// </summary>
[CreateAssetMenu(fileName = "InputEventSO", menuName = "Events/InputEventSO")]
public class InputEventSO : BaseEventSO<string>
{
    [Header("输入信息")]
    [Tooltip("输入的描述，如'攻击'、'跳跃'等")]
    public string inputDescription;
    [Tooltip("默认的输入按键")]
    public string defaultInputKey;
    
    // 可以在此添加更多输入相关的功能，如：
    // - 输入映射
    // - 输入配置
    // - 输入提示等
    
    /// <summary>
    /// 触发输入事件
    /// </summary>
    /// <param name="inputContext">输入上下文，可以是按键名称、输入状态等</param>
    /// <param name="sender">触发事件的对象</param>
    public void TriggerInput(string inputContext, object sender)
    {
        RaiseEvent(inputContext, sender);
    }
} 