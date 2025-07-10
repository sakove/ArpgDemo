using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// 输入管理器
/// 将Unity的新输入系统与事件系统集成
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("输入事件")]
    [SerializeField] private InputEventSO _attackEvent;
    [SerializeField] private InputEventSO _jumpEvent;
    [SerializeField] private InputEventSO _dashEvent;
    [SerializeField] private InputEventSO _interactEvent;
    [SerializeField] private InputEventSO _skillEvent;
    
    [Header("调试设置")]
    [SerializeField] private bool _debugMode = false;
    
    // 输入动作引用
    private InputSystem_Actions _inputActions;
    
    // 输入状态
    private Dictionary<string, bool> _inputStates = new Dictionary<string, bool>();
    
    private void Awake()
    {
        // 初始化输入系统
        _inputActions = new InputSystem_Actions();
    }
    
    private void OnEnable()
    {
        // 启用输入动作
        _inputActions.Enable();
        
        // 注册输入回调
        RegisterInputCallbacks();
    }
    
    private void OnDisable()
    {
        // 禁用输入动作
        _inputActions.Disable();
        
        // 取消注册输入回调
        UnregisterInputCallbacks();
    }
    
    private void RegisterInputCallbacks()
    {
        // 攻击输入
        _inputActions.Player.Attack.performed += ctx => OnAttackInput(ctx);
        _inputActions.Player.Attack.canceled += ctx => OnAttackInputReleased(ctx);
        
        // 跳跃输入
        _inputActions.Player.Jump.performed += ctx => OnJumpInput(ctx);
        _inputActions.Player.Jump.canceled += ctx => OnJumpInputReleased(ctx);
        
        // 冲刺输入
        _inputActions.Player.Sprint.performed += ctx => OnDashInput(ctx);
        _inputActions.Player.Sprint.canceled += ctx => OnDashInputReleased(ctx);
        
        // 交互输入
        _inputActions.Player.Interact.performed += ctx => OnInteractInput(ctx);
        _inputActions.Player.Interact.canceled += ctx => OnInteractInputReleased(ctx);
        
        // 技能输入
        //_inputActions.Player.Skill.performed += ctx => OnSkillInput(ctx);
       // _inputActions.Player.Skill.canceled += ctx => OnSkillInputReleased(ctx);
    }
    
    private void UnregisterInputCallbacks()
    {
        // 攻击输入
        _inputActions.Player.Attack.performed -= ctx => OnAttackInput(ctx);
        _inputActions.Player.Attack.canceled -= ctx => OnAttackInputReleased(ctx);
        
        // 跳跃输入
        _inputActions.Player.Jump.performed -= ctx => OnJumpInput(ctx);
        _inputActions.Player.Jump.canceled -= ctx => OnJumpInputReleased(ctx);
        
        // 冲刺输入
        _inputActions.Player.Sprint.performed -= ctx => OnDashInput(ctx);
        _inputActions.Player.Sprint.canceled -= ctx => OnDashInputReleased(ctx);
        
        // 交互输入
        _inputActions.Player.Interact.performed -= ctx => OnInteractInput(ctx);
        _inputActions.Player.Interact.canceled -= ctx => OnInteractInputReleased(ctx);
        
        // 技能输入
        //_inputActions.Player.Skill.performed -= ctx => OnSkillInput(ctx);
        //_inputActions.Player.Skill.canceled -= ctx => OnSkillInputReleased(ctx);
    }
    
    // 攻击输入处理
    private void OnAttackInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("攻击按下");
        _inputStates["Attack"] = true;
        _attackEvent?.TriggerInput("pressed", this);
    }
    
    private void OnAttackInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("攻击释放");
        _inputStates["Attack"] = false;
        _attackEvent?.TriggerInput("released", this);
    }
    
    // 跳跃输入处理
    private void OnJumpInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("跳跃按下");
        _inputStates["Jump"] = true;
        _jumpEvent?.TriggerInput("pressed", this);
    }
    
    private void OnJumpInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("跳跃释放");
        _inputStates["Jump"] = false;
        _jumpEvent?.TriggerInput("released", this);
    }
    
    // 冲刺输入处理
    private void OnDashInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("冲刺按下");
        _inputStates["Dash"] = true;
        _dashEvent?.TriggerInput("pressed", this);
    }
    
    private void OnDashInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("冲刺释放");
        _inputStates["Dash"] = false;
        _dashEvent?.TriggerInput("released", this);
    }
    
    // 交互输入处理
    private void OnInteractInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("交互按下");
        _inputStates["Interact"] = true;
        _interactEvent?.TriggerInput("pressed", this);
    }
    
    private void OnInteractInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("交互释放");
        _inputStates["Interact"] = false;
        _interactEvent?.TriggerInput("released", this);
    }
    
    // 技能输入处理
    private void OnSkillInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能按下");
        _inputStates["Skill"] = true;
        _skillEvent?.TriggerInput("pressed", this);
    }
    
    private void OnSkillInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能释放");
        _inputStates["Skill"] = false;
        _skillEvent?.TriggerInput("released", this);
    }
    
    // 检查输入状态
    public bool IsInputActive(string inputName)
    {
        if (_inputStates.TryGetValue(inputName, out bool state))
            return state;
        return false;
    }
    
    // 动态切换输入映射
    public void SwitchInputMap(string mapName)
    {
        switch (mapName.ToLower())
        {
            case "player":
                _inputActions.Player.Enable();
                _inputActions.UI.Disable();
                break;
            case "ui":
                _inputActions.Player.Disable();
                _inputActions.UI.Enable();
                break;
            case "none":
                _inputActions.Player.Disable();
                _inputActions.UI.Disable();
                break;
            default:
                Debug.LogWarning($"未知的输入映射: {mapName}");
                break;
        }
    }
} 