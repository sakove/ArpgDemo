using UnityEngine;

/// <summary>
/// 玩家状态机，管理玩家的各种状态和状态转换
/// </summary>
public class PlayerStateMachine : MonoBehaviour
{
    // 当前状态
    private PlayerState currentState;
    
    // 玩家控制器引用
    private PlayerController playerController;
    private Animator animator;
    
    // 状态引用
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMovingState MovingState { get; private set; }
    public PlayerJumpingState JumpingState { get; private set; }
    public PlayerFallingState FallingState { get; private set; }
    public PlayerAttackingState AttackingState { get; private set; }
    public PlayerSprintingState SprintingState { get; private set; }
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        
        // 初始化所有状态
        IdleState = new PlayerIdleState(this, playerController);
        MovingState = new PlayerMovingState(this, playerController);
        JumpingState = new PlayerJumpingState(this, playerController);
        FallingState = new PlayerFallingState(this, playerController);
        AttackingState = new PlayerAttackingState(this, playerController);
        SprintingState = new PlayerSprintingState(this, playerController);
    }
    
    private void Start()
    {
        // 设置初始状态
        currentState = IdleState;
        currentState.Enter();
    }
    
    private void Update()
    {
        // 更新当前状态的逻辑
        currentState?.LogicUpdate();
        
        // 持续将玩家的地面状态同步到Animator
        if (animator != null)
        {
            animator.SetBool("IsGrounded", playerController.IsGrounded);
        }
    }
    
    private void FixedUpdate()
    {
        // 更新当前状态的物理
        currentState?.PhysicsUpdate();
    }
    
    /// <summary>
    /// 切换到新状态
    /// </summary>
    /// <param name="newState">要切换到的新状态</param>
    public void ChangeState(PlayerState newState)
    {
        // 退出当前状态
        currentState?.Exit();
        
        // 进入新状态
        currentState = newState;
        currentState.Enter();
        
        // 调试输出
        Debug.Log($"Changed to state: {newState.GetType().Name}");
    }
    
    /// <summary>
    /// 获取当前状态
    /// </summary>
    /// <returns>当前状态</returns>
    public PlayerState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 检查当前状态是否可以被中断
    /// </summary>
    /// <returns>如果当前状态可以被中断，则返回true</returns>
    public bool CanCurrentStateBeInterrupted()
    {
        return currentState?.CanBeInterrupted() ?? true;
    }
} 