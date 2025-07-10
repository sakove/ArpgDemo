using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseEventSO<>))]
//根据类型，重写下面的内容
public class BaseEventSOEditor<T> : Editor
{
    //先找到当前事件
    private BaseEventSO<T> baseEventSO;

    private void OnEnable()
    {
        if (baseEventSO == null)
            baseEventSO = target as BaseEventSO<T>;
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUILayout.LabelField("订阅数量"+GetListeners().Count);

        foreach (var listener in GetListeners())
        {
            EditorGUILayout.LabelField(listener.ToString());//显示监视器的名称
            
        }
    }
    
    //获得所有注册监听的MonoBehaviour
    private List<MonoBehaviour> GetListeners()
    {
        List<MonoBehaviour> listeners = new();

        if (baseEventSO==null||baseEventSO.OnEventRaised == null)
        {
            return listeners;
        }
         
        var subscribers =baseEventSO.OnEventRaised.GetInvocationList();

        foreach (var subscriber in subscribers)
        {
            var obj= subscriber.Target as MonoBehaviour;
            if (!listeners.Contains(obj))
            {
                listeners.Add(obj);
            }
        }
        return listeners;
    }
    
}