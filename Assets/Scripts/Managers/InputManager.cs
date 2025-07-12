using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// 输入管理器
/// 将Unity的新输入系统与事件系统集成
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("动作事件")]
    [SerializeField] private InputEventSO _moveEvent;
    [SerializeField] private InputEventSO _jumpEvent;
    [SerializeField] private InputEventSO _sprintEvent;
    [SerializeField] private InputEventSO _attackEvent;
    [SerializeField] private InputEventSO _interactEvent;
    
    [Header("调试设置")]
    [SerializeField] private bool _debugMode = false;
    
    // 输入动作引用
    private InputSystem_Actions _inputActions;

    // 确保输入管理器是单例
    private static InputManager _instance;
    public static InputManager Instance { get { return _instance; } }

    
    private void Awake()
    {
        // 单例模式设置
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // 保证在场景转换时不被销毁
        DontDestroyOnLoad(gameObject);
        
        // 初始化输入系统
        _inputActions = new InputSystem_Actions();
        
        // 确保所有InputEventSO的InputType都被正确设置
        ValidateInputEvents();
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
    
    private void ValidateInputEvents()
    {
        // 确保每个事件的InputType与其用途一致
        if (_moveEvent != null && _moveEvent.inputType != InputEventType.Move)
            _moveEvent.inputType = InputEventType.Move;
            
        if (_jumpEvent != null && _jumpEvent.inputType != InputEventType.Jump)
            _jumpEvent.inputType = InputEventType.Jump;
            
        if (_sprintEvent != null && _sprintEvent.inputType != InputEventType.Sprint)
            _sprintEvent.inputType = InputEventType.Sprint;
            
        if (_attackEvent != null && _attackEvent.inputType != InputEventType.Attack)
            _attackEvent.inputType = InputEventType.Attack;
            
        if (_interactEvent != null && _interactEvent.inputType != InputEventType.Interact)
            _interactEvent.inputType = InputEventType.Interact;
    }
    
    private void RegisterInputCallbacks()
    {
        // 移动输入 - 需要连续监听
        if (_moveEvent != null)
        {
            _inputActions.Player.Move.performed += OnMoveInput;
            _inputActions.Player.Move.canceled += OnMoveInputCanceled;
        }
        
        // 攻击输入
        if (_attackEvent != null)
        {
            _inputActions.Player.Attack.performed += OnAttackInput;
            _inputActions.Player.Attack.canceled += OnAttackInputReleased;
        }
        
        // 跳跃输入
        if (_jumpEvent != null)
        {
            _inputActions.Player.Jump.performed += OnJumpInput;
            _inputActions.Player.Jump.canceled += OnJumpInputReleased;
        }
        
        // 冲刺输入
        if (_sprintEvent != null)
        {
            _inputActions.Player.Sprint.performed += OnSprintInput;
            _inputActions.Player.Sprint.canceled += OnSprintInputReleased;
        }
        
        // 交互输入
        if (_interactEvent != null)
        {
            _inputActions.Player.Interact.performed += OnInteractInput;
            _inputActions.Player.Interact.canceled += OnInteractInputReleased;
        }
    }
    
    private void UnregisterInputCallbacks()
    {
        // 移动输入
        if (_moveEvent != null)
        {
            _inputActions.Player.Move.performed -= OnMoveInput;
            _inputActions.Player.Move.canceled -= OnMoveInputCanceled;
        }
        
        // 攻击输入
        if (_attackEvent != null)
        {
            _inputActions.Player.Attack.performed -= OnAttackInput;
            _inputActions.Player.Attack.canceled -= OnAttackInputReleased;
        }
        
        // 跳跃输入
        if (_jumpEvent != null)
        {
            _inputActions.Player.Jump.performed -= OnJumpInput;
            _inputActions.Player.Jump.canceled -= OnJumpInputReleased;
        }
        
        // 冲刺输入
        if (_sprintEvent != null)
        {
            _inputActions.Player.Sprint.performed -= OnSprintInput;
            _inputActions.Player.Sprint.canceled -= OnSprintInputReleased;
        }
        
        // 交互输入
        if (_interactEvent != null)
        {
            _inputActions.Player.Interact.performed -= OnInteractInput;
            _inputActions.Player.Interact.canceled -= OnInteractInputReleased;
        }
    }
    
    #region Input Handlers
    
    // 移动输入处理
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 moveVector = context.ReadValue<Vector2>();
        if (_debugMode) Debug.Log($"移动: ({moveVector.x}, {moveVector.y})");
        _moveEvent.TriggerVector(moveVector, this);
    }
    
    private void OnMoveInputCanceled(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("移动停止");
        _moveEvent.TriggerVector(Vector2.zero, this);
    }
    
    // 攻击输入处理
    private void OnAttackInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("攻击按下");
        _attackEvent.TriggerPressed(this);
    }
    
    private void OnAttackInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("攻击释放");
        _attackEvent.TriggerReleased(this);
    }
    
    // 跳跃输入处理
    private void OnJumpInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("跳跃按下");
        _jumpEvent.TriggerPressed(this);
    }
    
    private void OnJumpInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("跳跃释放");
        _jumpEvent.TriggerReleased(this);
    }
    
    // 冲刺输入处理
    private void OnSprintInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("冲刺按下");
        _sprintEvent.TriggerPressed(this);
    }
    
    private void OnSprintInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("冲刺释放");
        _sprintEvent.TriggerReleased(this);
    }
    
    // 交互输入处理
    private void OnInteractInput(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("交互按下");
        _interactEvent.TriggerPressed(this);
    }
    
    private void OnInteractInputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("交互释放");
        _interactEvent.TriggerReleased(this);
    }
    
    #endregion
    
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
    
    // 禁用所有输入（比如在游戏暂停或对话期间）
    public void DisableAllInput()
    {
        _inputActions.Disable();
    }
    
    // 启用所有输入
    public void EnableAllInput()
    {
        _inputActions.Enable();
    }
} 