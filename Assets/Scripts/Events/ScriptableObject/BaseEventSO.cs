using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class BaseEventSO<T> : ScriptableObject
{
    public string description;
    
    // 使用委托列表替代单一委托，以支持优先级
    private List<EventListenerInfo<T>> _listeners = new List<EventListenerInfo<T>>();
    
    // 保留原有的委托用于兼容
    public UnityAction<T> OnEventRaised
    {
        get { return InvokeListeners; }
        set
        {
            if (value != null)
            {
                // 当外部使用+=添加监听器时，使用默认优先级0添加到列表
                _listeners.Add(new EventListenerInfo<T>(value.Target, value.Method.Name, 0, value));
            }
            else
            {
                // 当设置为null时清空所有监听器
                _listeners.Clear();
            }
        }
    }

    public string lastSender;
    
    [SerializeField] private bool _debugMode = false;

    // 添加带优先级的监听器
    public void AddListener(UnityAction<T> listener, int priority = 0, object target = null, string methodName = null)
    {
        if (listener != null)
        {
            // 如果未提供target和methodName，则从委托中获取
            if (target == null) target = listener.Target;
            if (string.IsNullOrEmpty(methodName)) methodName = listener.Method.Name;
            
            // 检查是否已存在相同的监听器
            if (!_listeners.Any(l => l.Target == target && l.MethodName == methodName))
            {
                _listeners.Add(new EventListenerInfo<T>(target, methodName, priority, listener));
                // 按优先级排序，高优先级的监听器先执行
                _listeners = _listeners.OrderByDescending(l => l.Priority).ToList();
            }
        }
    }

    // 移除监听器
    public void RemoveListener(UnityAction<T> listener)
    {
        if (listener != null)
        {
            _listeners.RemoveAll(l => l.Target == listener.Target && l.MethodName == listener.Method.Name);
        }
    }

    // 移除特定目标的所有监听器
    public void RemoveAllListenersFromTarget(object target)
    {
        if (target != null)
        {
            _listeners.RemoveAll(l => l.Target == target);
        }
    }

    // 触发事件
    public void RaiseEvent(T value, object sender)
    {
        InvokeListeners(value);

        if (_debugMode)
        {
            Debug.Log($"事件 {name} 被 {sender} 触发，值: {value}");
            lastSender = sender.ToString(); 
        }
    }

    // 按优先级顺序调用所有监听器
    private void InvokeListeners(T value)
    {
        // 创建副本以防止在遍历过程中修改列表
        var listenersCopy = new List<EventListenerInfo<T>>(_listeners);
        foreach (var listener in listenersCopy)
        {
            try
            {
                listener.Action?.Invoke(value);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"事件监听器执行错误: {e.Message}");
            }
        }
    }
}

// 存储监听器信息的辅助类
public class EventListenerInfo<T>
{
    public object Target { get; private set; }
    public string MethodName { get; private set; }
    public int Priority { get; private set; }
    public UnityAction<T> Action { get; private set; }

    public EventListenerInfo(object target, string methodName, int priority, UnityAction<T> action)
    {
        Target = target;
        MethodName = methodName;
        Priority = priority;
        Action = action;
    }
}