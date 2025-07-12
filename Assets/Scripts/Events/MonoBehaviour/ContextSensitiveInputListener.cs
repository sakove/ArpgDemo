using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 上下文敏感的输入监听器
/// 展示如何根据游戏环境动态改变按键功能
/// 例如：靠近敌人时按E是攻击，靠近宝箱时按E是打开
/// </summary>
public class ContextSensitiveInputListener : InputEventListener
{
    [Header("交互设置")]
    [SerializeField] private float _interactRange = 2f;
    [SerializeField] private LayerMask _interactableLayers;
    
    [Header("交互类型")]
    [SerializeField] private bool _canInteractWithEnemies = true;
    [SerializeField] private bool _canInteractWithItems = true;
    [SerializeField] private bool _canInteractWithNPCs = true;
    [SerializeField] private bool _canInteractWithObjects = true;
    
    // 交互优先级（数字越大优先级越高）
    [Header("交互优先级")]
    [SerializeField] private int _enemyPriority = 10;
    [SerializeField] private int _itemPriority = 20;
    [SerializeField] private int _npcPriority = 30;
    [SerializeField] private int _objectPriority = 5;
    
    // 当前可交互对象
    private GameObject _currentInteractable;
    private InteractableType _currentInteractableType;
    
    // 交互对象类型
    private enum InteractableType
    {
        None,
        Enemy,
        Item,
        NPC,
        Object
    }
    
    private void Update()
    {
        // 检测周围的可交互对象
        DetectInteractables();
        
        // 根据当前可交互对象更新优先级
        UpdatePriority();
        
        // 更新UI提示（如果需要）
        UpdateInteractionPrompt();
    }
    
    private void DetectInteractables()
    {
        // 重置当前交互对象
        _currentInteractable = null;
        _currentInteractableType = InteractableType.None;
        
        // 检测周围的碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _interactRange, _interactableLayers);
        
        // 如果没有检测到任何物体，直接返回
        if (colliders.Length == 0)
            return;
            
        // 按距离排序
        System.Array.Sort(colliders, (a, b) => 
            Vector2.Distance(transform.position, a.transform.position)
            .CompareTo(Vector2.Distance(transform.position, b.transform.position)));
            
        // 确定最近的可交互对象及其类型
        foreach (Collider2D collider in colliders)
        {
            // 检查是否是敌人
            if (_canInteractWithEnemies && collider.CompareTag("Enemy"))
            {
                _currentInteractable = collider.gameObject;
                _currentInteractableType = InteractableType.Enemy;
                break;
            }
            // 检查是否是物品
            else if (_canInteractWithItems && collider.CompareTag("Item"))
            {
                _currentInteractable = collider.gameObject;
                _currentInteractableType = InteractableType.Item;
                break;
            }
            // 检查是否是NPC
            else if (_canInteractWithNPCs && collider.CompareTag("NPC"))
            {
                _currentInteractable = collider.gameObject;
                _currentInteractableType = InteractableType.NPC;
                break;
            }
            // 检查是否是可交互物体
            else if (_canInteractWithObjects && collider.CompareTag("Interactable"))
            {
                _currentInteractable = collider.gameObject;
                _currentInteractableType = InteractableType.Object;
                break;
            }
        }
    }
    
    private void UpdatePriority()
    {
        // 根据当前交互对象类型设置优先级
        switch (_currentInteractableType)
        {
            case InteractableType.Enemy:
                SetPriority(_enemyPriority);
                break;
            case InteractableType.Item:
                SetPriority(_itemPriority);
                break;
            case InteractableType.NPC:
                SetPriority(_npcPriority);
                break;
            case InteractableType.Object:
                SetPriority(_objectPriority);
                break;
            default:
                SetPriority(0); // 默认优先级
                break;
        }
    }
    
    private void UpdateInteractionPrompt()
    {
        // 这里可以更新UI提示，例如显示"按E攻击"、"按E拾取"等
        // 实际实现需要根据你的UI系统
        
        if (_currentInteractable != null)
        {
            string promptText = "按E ";
            
            switch (_currentInteractableType)
            {
                case InteractableType.Enemy:
                    promptText += "攻击";
                    break;
                case InteractableType.Item:
                    promptText += "拾取";
                    break;
                case InteractableType.NPC:
                    promptText += "对话";
                    break;
                case InteractableType.Object:
                    promptText += "交互";
                    break;
            }
            
            // 显示提示
            // UIManager.Instance.ShowInteractionPrompt(promptText);
        }
        else
        {
            // 隐藏提示
            // UIManager.Instance.HideInteractionPrompt();
        }
    }
    
    protected override bool CanProcessEvent(InputEventData data)
    {
        // 首先检查基础条件
        if (!base.CanProcessEvent(data))
            return false;
            
        // 只响应按下事件
        if (data.InputState != InputState.Pressed)
            return false;
            
        // 只有当有可交互对象时才能交互
        return _currentInteractable != null;
    }
    
    public override void OnEventRaised(InputEventData data)
    {
        // 如果条件不满足，则不响应事件
        if (_useCondition && !CanProcessEvent(data))
            return;
            
        // 只处理按下事件
        if (data.InputState == InputState.Pressed)
        {
            // 根据交互对象类型执行不同的交互逻辑
            switch (_currentInteractableType)
            {
                case InteractableType.Enemy:
                    InteractWithEnemy();
                    break;
                case InteractableType.Item:
                    InteractWithItem();
                    break;
                case InteractableType.NPC:
                    InteractWithNPC();
                    break;
                case InteractableType.Object:
                    InteractWithObject();
                    break;
            }
        }
        
        // 调用基类方法，触发通用响应
        base.OnEventRaised(data);
    }
    
    private void InteractWithEnemy()
    {
        Debug.Log("与敌人交互: " + _currentInteractable.name);
        // 这里可以添加攻击敌人的逻辑
    }
    
    private void InteractWithItem()
    {
        Debug.Log("与物品交互: " + _currentInteractable.name);
        // 这里可以添加拾取物品的逻辑
    }
    
    private void InteractWithNPC()
    {
        Debug.Log("与NPC交互: " + _currentInteractable.name);
        // 这里可以添加对话系统的逻辑
    }
    
    private void InteractWithObject()
    {
        Debug.Log("与物体交互: " + _currentInteractable.name);
        // 这里可以添加与物体交互的逻辑，如开门、开宝箱等
    }
    
    // 绘制交互范围的可视化（仅在编辑器中）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactRange);
    }
} 