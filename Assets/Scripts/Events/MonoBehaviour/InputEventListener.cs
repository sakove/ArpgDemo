using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 输入事件监听器，专门用于处理游戏输入事件
/// 支持优先级系统和条件判断，适用于2D动作游戏的复杂输入处理
/// </summary>
public class InputEventListener : BaseEventListener<string>
{
    [Header("输入设置")]
    [Tooltip("输入冷却时间(秒)")]  
    [SerializeField] private float _cooldownTime = 0.1f;
    [SerializeField] private bool _ignoreCooldown = false;
    
    [Header("状态限制")]
    [SerializeField] private bool _requireGrounded = false;
    [SerializeField] private bool _requireAirborne = false;
    [SerializeField] private bool _canInterruptAttack = false;
    [SerializeField] private bool _canInterruptDash = false;
    
    [Header("资源消耗")]
    [SerializeField] private bool _requireResource = false;
    [SerializeField] private float _resourceCost = 0;
    
    // 引用其他组件
    [Header("组件引用")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerManager _playerManager;
    
    // 内部状态
    private float _lastUsedTime = -10f;
    
    protected void Awake()
    {
        // 如果没有手动指定，则尝试自动获取组件
        if (_playerController == null)
            _playerController = GetComponentInParent<PlayerController>();
            
        if (_playerManager == null)
            _playerManager = GetComponentInParent<PlayerManager>();
    }
    
    protected override bool CanProcessEvent(string inputValue)
    {
        // 基础启用检查
        if (!base.CanProcessEvent(inputValue))
            return false;
            
        // 冷却检查
        if (!_ignoreCooldown && Time.time < _lastUsedTime + _cooldownTime)
            return false;

        /*   
        // 状态检查
        if (_playerController != null)
        {
            // 地面/空中检查
            if (_requireGrounded && !_playerController.IsGrounded)
                return false;
                
            if (_requireAirborne && _playerController.IsGrounded)
                return false;
                
            // 动作打断检查
            if (_playerController.isAttacking && !_canInterruptAttack)
                return false;
                
            if (_playerController.isDashing && !_canInterruptDash)
                return false;
        }
        
        // 资源检查
        if (_requireResource && _playerManager != null)
        {
            if (_playerManager.CurrentResource < _resourceCost)
                return false;
        }
        */
        return true;
    }
    
    public override void OnEventRaised(string inputValue)
    {
        // 如果条件不满足，则不响应事件
        if (_useCondition && !CanProcessEvent(inputValue))
            return;
            
        // 更新最后使用时间
        _lastUsedTime = Time.time;
        /*
        // 消耗资源
        if (_requireResource && _playerManager != null)
        {
            _playerManager.ConsumeResource(_resourceCost);
        }
        */
        // 调用响应
        base.OnEventRaised(inputValue);
    }
    
    // 辅助方法：设置优先级
    public void SetPriority(int priority)
    {
        if (_priority != priority)
        {
            // 如果已注册，需要先取消注册再重新注册
            bool wasRegistered =IsRegistered();
            if (wasRegistered)
                UnregisterListener();
                
            _priority = priority;
            
            if (wasRegistered)
                RegisterListener();
        }
    }
} 