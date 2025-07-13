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
    
    [Header("技能事件")]
    [SerializeField] private InputEventSO _skill1Event;
    [SerializeField] private InputEventSO _skill2Event;
    [SerializeField] private InputEventSO _skill3Event;
    [SerializeField] private InputEventSO _skill4Event;
    [SerializeField] private InputEventSO _skill5Event;
    [SerializeField] private InputEventSO _skill6Event;
    [SerializeField] private InputEventSO _skill7Event;
    
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
            
        // 技能事件类型设置
        if (_skill1Event != null && _skill1Event.inputType != InputEventType.Skill1)
            _skill1Event.inputType = InputEventType.Skill1;
            
        if (_skill2Event != null && _skill2Event.inputType != InputEventType.Skill2)
            _skill2Event.inputType = InputEventType.Skill2;
            
        if (_skill3Event != null && _skill3Event.inputType != InputEventType.Skill3)
            _skill3Event.inputType = InputEventType.Skill3;
            
        if (_skill4Event != null && _skill4Event.inputType != InputEventType.Skill4)
            _skill4Event.inputType = InputEventType.Skill4;
            
        if (_skill5Event != null && _skill5Event.inputType != InputEventType.Skill5)
            _skill5Event.inputType = InputEventType.Skill5;
            
        if (_skill6Event != null && _skill6Event.inputType != InputEventType.Skill6)
            _skill6Event.inputType = InputEventType.Skill6;
            
        if (_skill7Event != null && _skill7Event.inputType != InputEventType.Skill7)
            _skill7Event.inputType = InputEventType.Skill7;
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
        
        // 技能输入
        if (_skill1Event != null)
        {
            _inputActions.Player.Skill1.performed += OnSkill1Input;
            _inputActions.Player.Skill1.canceled += OnSkill1InputReleased;
        }

        if (_skill2Event != null)
        {
            _inputActions.Player.Skill2.performed += OnSkill2Input;
            _inputActions.Player.Skill2.canceled += OnSkill2InputReleased;
        }

        if (_skill3Event != null)
        {
            _inputActions.Player.Skill3.performed += OnSkill3Input;
            _inputActions.Player.Skill3.canceled += OnSkill3InputReleased;
        }

        if (_skill4Event != null)
        {
            _inputActions.Player.Skill4.performed += OnSkill4Input;
            _inputActions.Player.Skill4.canceled += OnSkill4InputReleased;
        }

        if (_skill5Event != null)
        {
            _inputActions.Player.Skill5.performed += OnSkill5Input;
            _inputActions.Player.Skill5.canceled += OnSkill5InputReleased;
        }

        if (_skill6Event != null)
        {
            _inputActions.Player.Skill6.performed += OnSkill6Input;
            _inputActions.Player.Skill6.canceled += OnSkill6InputReleased;
        }

        if (_skill7Event != null)
        {
            _inputActions.Player.Skill7.performed += OnSkill7Input;
            _inputActions.Player.Skill7.canceled += OnSkill7InputReleased;
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
        
        // 技能输入
        if (_skill1Event != null)
        {
            _inputActions.Player.Skill1.performed -= OnSkill1Input;
            _inputActions.Player.Skill1.canceled -= OnSkill1InputReleased;
        }
        
        if (_skill2Event != null)
        {
            _inputActions.Player.Skill2.performed -= OnSkill2Input;
            _inputActions.Player.Skill2.canceled -= OnSkill2InputReleased;
        }
        
        if (_skill3Event != null)
        {
            _inputActions.Player.Skill3.performed -= OnSkill3Input;
            _inputActions.Player.Skill3.canceled -= OnSkill3InputReleased;
        }

        if (_skill4Event != null)
        {
            _inputActions.Player.Skill4.performed -= OnSkill4Input;
            _inputActions.Player.Skill4.canceled -= OnSkill4InputReleased;
        }

        if (_skill5Event != null)
        {
            _inputActions.Player.Skill5.performed -= OnSkill5Input;
            _inputActions.Player.Skill5.canceled -= OnSkill5InputReleased;
        }

        if (_skill6Event != null)
        {
            _inputActions.Player.Skill6.performed -= OnSkill6Input;
            _inputActions.Player.Skill6.canceled -= OnSkill6InputReleased;
        }

        if (_skill7Event != null)
        {
            _inputActions.Player.Skill7.performed -= OnSkill7Input;
            _inputActions.Player.Skill7.canceled -= OnSkill7InputReleased;
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
    
    // 技能1输入处理
    private void OnSkill1Input(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能1按下");
        _skill1Event.TriggerPressed(this);
    }
    
    private void OnSkill1InputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能1释放");
        _skill1Event.TriggerReleased(this);
    }
    
    // 技能2输入处理
    private void OnSkill2Input(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能2按下");
        _skill2Event.TriggerPressed(this);
    }
    
    private void OnSkill2InputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能2释放");
        _skill2Event.TriggerReleased(this);
    }
    
    // 技能3输入处理
    private void OnSkill3Input(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能3按下");
        _skill3Event.TriggerPressed(this);
    }
    
    private void OnSkill3InputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能3释放");
        _skill3Event.TriggerReleased(this);
    }

    // 技能4输入处理
    private void OnSkill4Input(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能4按下");
        _skill4Event.TriggerPressed(this);
    }

    private void OnSkill4InputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能4释放");
        _skill4Event.TriggerReleased(this);
    }

    // 技能5输入处理
    private void OnSkill5Input(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能5按下");
        _skill5Event.TriggerPressed(this);
    }

    private void OnSkill5InputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能5释放");
        _skill5Event.TriggerReleased(this);
    }

    // 技能6输入处理
    private void OnSkill6Input(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能6按下");
        _skill6Event.TriggerPressed(this);
    }

    private void OnSkill6InputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能6释放");
        _skill6Event.TriggerReleased(this);
    }

    // 技能7输入处理
    private void OnSkill7Input(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能7按下");
        _skill7Event.TriggerPressed(this);
    }

    private void OnSkill7InputReleased(InputAction.CallbackContext context)
    {
        if (_debugMode) Debug.Log("技能7释放");
        _skill7Event.TriggerReleased(this);
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