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
    public PlayerUsingSkillState UsingSkillState { get; private set; }
    public PlayerSpecialAnimationState SpecialAnimationState { get; private set; }
    
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
        UsingSkillState = new PlayerUsingSkillState(this, playerController);
        SpecialAnimationState = new PlayerSpecialAnimationState(this, playerController);
    }
    
    private void Start()
    {
        // 设置初始状态
        currentState = IdleState;
        currentState.Enter();
        
        // 确保交互层的初始权重为0
        playerController.SetInteractionLayerWeight(0f);
    }
    
    private void Update()
    {
        // 更新当前状态的逻辑
        currentState?.LogicUpdate();
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
    /// 切换到技能使用状态
    /// </summary>
    /// <param name="newState">要切换到的新状态（必须是UsingSkillState）</param>
    /// <param name="skill">要使用的技能</param>
    public void ChangeState(PlayerState newState, Skill skill)
    {
        // 确保新状态是技能使用状态
        if (newState is PlayerUsingSkillState skillState)
        {
            // 设置技能
            skillState.SetActiveSkill(skill);
            
            // 退出当前状态
            currentState?.Exit();
            
            // 进入新状态
            currentState = newState;
            currentState.Enter();
            
            // 调试输出
            Debug.Log($"Changed to state: {newState.GetType().Name} with skill: {skill.skillName}");
        }
        else
        {
            // 如果不是技能使用状态，则使用普通的状态切换
            ChangeState(newState);
        }
    }
    
    /// <summary>
    /// 切换到特殊动画状态
    /// </summary>
    /// <param name="trigger">动画触发器名称</param>
    /// <param name="duration">动画持续时间</param>
    /// <param name="lockInput">是否锁定输入</param>
    public void ChangeToSpecialAnimationState(string trigger, float duration, bool lockInput = true)
    {
        // 设置特殊动画参数
        SpecialAnimationState.SetAnimationParameters(trigger, duration, lockInput);
        
        // 切换到特殊动画状态
        ChangeState(SpecialAnimationState);
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