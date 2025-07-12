using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 输入事件监听器，用于响应输入事件
/// </summary>
public class InputEventListener : BaseEventListener<InputEventData>
{
    [System.Serializable]
    public class InputUnityEvent : UnityEvent<InputEventData> { }
    
    [Header("输入事件响应")]
    [SerializeField] private InputUnityEvent _onPressed;
    [SerializeField] private InputUnityEvent _onReleased;
    [SerializeField] private InputUnityEvent _onValueChanged;
    
    public override void OnEventRaised(InputEventData data)
    {
        // 首先执行基类的事件响应，确保通用的Response能够被调用
        base.OnEventRaised(data);
        
        // 然后根据输入状态分发到特定的事件响应
        switch (data.InputState)
        {
            case InputState.Pressed:
                _onPressed?.Invoke(data);
                break;
                
            case InputState.Released:
                _onReleased?.Invoke(data);
                break;
                
            case InputState.Performed:
                _onValueChanged?.Invoke(data);
                break;
        }
    }
    
    // 简化的访问器，让外部代码可以直接添加监听器
    public void AddPressedListener(UnityAction<InputEventData> action)
    {
        _onPressed.AddListener(action);
    }
    
    public void AddReleasedListener(UnityAction<InputEventData> action)
    {
        _onReleased.AddListener(action);
    }
    
    public void AddValueChangedListener(UnityAction<InputEventData> action)
    {
        _onValueChanged.AddListener(action);
    }
} 