using UnityEngine;
using UnityEngine.Events;
using System.Linq;

//泛型类匹配泛型Event事件
public class BaseEventListener<T> : MonoBehaviour
{
    [Header("事件配置")]
    public BaseEventSO<T> eventSO;
    public UnityEvent<T> Response;
    
    [Header("监听器设置")]
    [SerializeField] protected bool _isEnabled = true;
    [SerializeField] protected bool _startEnabled = true;
    [SerializeField] protected int _priority = 0;
    [Tooltip("如果为true，则只有条件满足时才会响应事件")]
    [SerializeField] protected bool _useCondition = false;
    
    protected virtual void Start()
    {
        if (!_startEnabled)
            UnregisterListener();
    }
    
    //注册事件
    protected virtual void OnEnable()
    {
        RegisterListener();
    }

    //注销事件
    protected virtual void OnDisable()
    {
        UnregisterListener();
    }

    public void RegisterListener()
    {
        if (eventSO != null && !IsRegistered())
        {
            // 使用优先级注册
            eventSO.AddListener(OnEventRaised, _priority, this, "OnEventRaised");
        }
    }

    public void UnregisterListener()
    {
        if (eventSO != null)
        {
            eventSO.RemoveListener(OnEventRaised);
        }
    }

    public bool IsRegistered()
    {
        if (eventSO == null || eventSO.OnEventRaised == null)
            return false;
            
        return eventSO.OnEventRaised.GetInvocationList()
            .Any(d => d.Target == this && d.Method.Name == "OnEventRaised");
    }

    // 启用/禁用监听器
    public virtual void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }
    
    // 条件判断，子类可以重写此方法添加自定义条件
    protected virtual bool CanProcessEvent(T value)
    {
        return _isEnabled;
    }

    // 事件响应
    public virtual void OnEventRaised(T value)
    {
        // 如果使用条件判断且条件不满足，则不响应事件
        if (_useCondition && !CanProcessEvent(value))
            return;
            
        Response?.Invoke(value);
    }
}