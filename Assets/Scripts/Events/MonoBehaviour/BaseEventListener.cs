using UnityEngine;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// 泛型基础事件监听器，用于监听ScriptableObject事件
/// </summary>
public class BaseEventListener<T> : MonoBehaviour
{
    [Header("事件配置")]
    [Tooltip("要监听的事件SO")]
    public BaseEventSO<T> eventSO;
    
    [Tooltip("当事件触发时的响应")]
    public UnityEvent<T> Response;
    
    [Header("监听器设置")]
    [Tooltip("是否启用此监听器")]
    [SerializeField] protected bool _isEnabled = true;
    
    [Tooltip("是否在Start时启用")]
    [SerializeField] protected bool _startEnabled = true;
    
    [Tooltip("监听优先级，较高的值先触发")]
    [SerializeField] protected int _priority = 0;
    
    [Tooltip("如果为true，则只有条件满足时才会响应事件")]
    [SerializeField] protected bool _useCondition = false;

    // 当前注册状态的缓存
    private bool _isCurrentlyRegistered = false;
    
    protected virtual void Start()
    {
        if (!_startEnabled)
            UnregisterListener();
    }
    
    /// <summary>
    /// 注册事件监听
    /// </summary>
    protected virtual void OnEnable()
    {
        RegisterListener();
    }

    /// <summary>
    /// 注销事件监听
    /// </summary>
    protected virtual void OnDisable()
    {
        UnregisterListener();
    }

    /// <summary>
    /// 向事件SO注册监听器
    /// </summary>
    public void RegisterListener()
    {
        if (eventSO != null && !_isCurrentlyRegistered)
        {
            // 使用优先级注册
            eventSO.AddListener(OnEventRaised, _priority, this, "OnEventRaised");
            _isCurrentlyRegistered = true;
        }
    }

    /// <summary>
    /// 从事件SO取消注册监听器
    /// </summary>
    public void UnregisterListener()
    {
        if (eventSO != null && _isCurrentlyRegistered)
        {
            eventSO.RemoveListener(OnEventRaised);
            _isCurrentlyRegistered = false;
        }
    }

    /// <summary>
    /// 检查此监听器是否已注册到事件
    /// </summary>
    public bool IsRegistered()
    {
        // 优先使用缓存的状态，提高性能
        if (_isCurrentlyRegistered) 
            return true;
            
        // 如果缓存显示未注册，进行实际检查
        if (eventSO == null || eventSO.OnEventRaised == null)
            return false;
            
        // 在事件的调用列表中查找此监听器
        bool isRegistered = eventSO.OnEventRaised.GetInvocationList()
            .Any(d => d.Target == this && d.Method.Name == "OnEventRaised");
            
        // 更新缓存状态
        _isCurrentlyRegistered = isRegistered;
        return isRegistered;
    }

    /// <summary>
    /// 启用/禁用监听器
    /// </summary>
    public virtual void SetEnabled(bool enabled)
    {
        if (_isEnabled == enabled) return; // 避免不必要的处理
        
        _isEnabled = enabled;
        
        // 如果禁用且已注册，则取消注册
        if (!_isEnabled && _isCurrentlyRegistered)
        {
            UnregisterListener();
        }
        // 如果启用且未注册，则注册
        else if (_isEnabled && !_isCurrentlyRegistered)
        {
            RegisterListener();
        }
    }
    
    /// <summary>
    /// 条件判断，子类可以重写此方法添加自定义条件
    /// </summary>
    protected virtual bool CanProcessEvent(T value)
    {
        return _isEnabled;
    }

    /// <summary>
    /// 事件响应
    /// </summary>
    public virtual void OnEventRaised(T value)
    {
        // 如果使用条件判断且条件不满足，则不响应事件
        if (_useCondition && !CanProcessEvent(value))
            return;
            
        Response?.Invoke(value);
    }
    
    /// <summary>
    /// 设置监听器优先级
    /// </summary>
    public void SetPriority(int priority)
    {
        if (_priority == priority) return;
        
        bool wasRegistered = _isCurrentlyRegistered;
        if (wasRegistered)
            UnregisterListener();
            
        _priority = priority;
        
        if (wasRegistered)
            RegisterListener();
    }
}