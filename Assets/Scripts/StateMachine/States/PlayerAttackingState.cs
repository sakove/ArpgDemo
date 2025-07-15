using UnityEngine;

public class PlayerAttackingState : PlayerState
{
    private float attackStartTime;
    private float attackDuration;
    private bool attackFinished;
    private CombatController combatController;
    private AnimationStateHelper stateHelper;
    private Vector2 inputAtStart; // 记录进入状态时的输入向量
    
    // 核心改动：缓存当前正在执行的攻击Skill，以便在整个状态中访问其配置
    private Skill activeAttack;

    public PlayerAttackingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
        combatController = playerController.GetComponent<CombatController>();
        stateHelper = playerController.GetComponent<AnimationStateHelper>();
        
        if (combatController == null)
        {
            Debug.LogError("PlayerAttackingState: CombatController is not found on the player object!");
        }
        
        if (stateHelper == null)
        {
            Debug.LogWarning("PlayerAttackingState: AnimationStateHelper is not found on the player object. Combined attack animations might not work correctly.");
        }
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 核心改动：进入攻击状态时，禁止玩家转向，以确保攻击动作的稳定性。
        playerController.CanFlip = false;
        
        // 设置攻击开始时间
        attackStartTime = Time.time;
        
        // 记录当前的输入向量，用于决定攻击类型
        inputAtStart = playerController.GetMoveInput();
        
        // 执行攻击，获取持续时间，并缓存当前的攻击Skill
        attackDuration = PerformAppropriateAttack(out activeAttack);
        
        // 根据当前攻击的配置，决定是否清除水平速度
        if (playerController.IsGrounded && activeAttack != null && activeAttack.haltMomentumOnGround)
        {
            // 实现“原地急停攻击”
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        
        // 重置攻击完成标志
        attackFinished = false;
        
        // 确保战斗层权重为1
        playerController.SetCombatLayerWeight(1f);
    }
    
    private float PerformAppropriateAttack(out Skill performedAttack)
    {
        performedAttack = null;
        if (combatController == null)
        {
            return 0.1f; // 默认短持续时间
        }
        
        // 获取当前的移动状态
        AnimationStateHelper.MovementState currentState = AnimationStateHelper.MovementState.Idle;
        if (stateHelper != null)
        {
            currentState = stateHelper.GetCurrentMovementState();
        }
        
        // 根据当前状态和输入决定使用哪种攻击
        if (playerController.IsGrounded)
        {
            // 地面攻击
            float verticalInput = inputAtStart.y;

            if (verticalInput > 0.5f)
            {
                // 地面向上攻击
                return combatController.PerformUpGroundAttack(out performedAttack);
            }
            else if (verticalInput < -0.5f)
            {
                // 地面向下攻击
                return combatController.PerformDownGroundAttack(out performedAttack);
            }
            else
            {
                // 地面普通攻击
                bool isMoving = currentState == AnimationStateHelper.MovementState.Moving;
                return combatController.PerformGroundAttack(isMoving, out performedAttack);
            }
        }
        else
        {
            // 空中攻击 - 根据垂直输入决定攻击类型
            float verticalInput = inputAtStart.y;
            
            if (verticalInput > 0.5f)
            {
                // 空中向上攻击
                return combatController.PerformUpAirAttack(out performedAttack);
            }
            else if (verticalInput < -0.5f)
            {
                // 空中向下攻击
                return combatController.PerformDownAirAttack(out performedAttack);
            }
            else
            {
                // 普通空中攻击
                return combatController.PerformAirAttack(out performedAttack);
            }
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 核心改动：退出攻击状态时，恢复玩家的转向能力。
        playerController.CanFlip = true;
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 检查动画是否播放完毕
        if (Time.time >= attackStartTime + attackDuration)
        {
            attackFinished = true; // 标记攻击已完成
            DecideNextState();
        }
        // 在攻击动画期间，允许冲刺来取消攻击
        else if (playerController.SprintInput && playerController.CanSprint)
        {
            playerController.UseSprintInput(); // <<-- 消耗冲刺输入
            stateMachine.ChangeState(stateMachine.SprintingState);
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        if (activeAttack == null) return;

        // --- 核心改动：物理行为现在完全由Skill资产驱动 ---

        // 1. 处理空中攻击
        if (!playerController.IsGrounded)
        {
            // 如果技能配置为“空中停滞”
            if (activeAttack.stallInAir)
            {
                // 设置一个固定的垂直速度（可以为负，实现缓慢下落）
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, activeAttack.airStallVerticalVelocity);

                // 在空中停滞时，仍然允许轻微的水平控制，控制程度由技能定义
                Vector2 moveInput = playerController.GetMoveInput();
                playerController.Move(moveInput.x, playerController.AirControlFactor * activeAttack.airStallControlDampening);
            }
            // 如果技能没有配置为“空中停滞”，则不干预物理，实现自由移动的空中攻击
        }
        // 2. 处理地面攻击
        else
        {
            // 这个全局开关可以保留，用于一些特殊情况，但主要控制权在haltMomentumOnGround
            if (combatController != null && combatController.CanMoveWhileAttacking())
            {
                Vector2 moveInput = playerController.GetMoveInput();
                playerController.Move(moveInput.x, combatController.AttackMovementFactor);
            }
        }
    }
    
    private void DecideNextState()
    {
        // 检查是否有新的攻击输入
        if (playerController.AttackInput)
        {
            playerController.UseAttackInput(); // <<-- 消耗攻击输入
            // 重新进入攻击状态，执行新的攻击
            stateMachine.ChangeState(stateMachine.AttackingState);
            return;
        }
        
        // 如果没有新的攻击输入，根据当前情况切换状态
        if (!playerController.IsGrounded)
        {
            // 如果在空中，切换到下落状态
            stateMachine.ChangeState(stateMachine.FallingState);
        }
        else
        {
            // 如果在地面上，根据移动输入切换到闲置或移动状态
            Vector2 moveInput = playerController.GetMoveInput();
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                stateMachine.ChangeState(stateMachine.MovingState);
            }
            else
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
        }
    }
    
    public override bool CanBeInterrupted()
    {
        // 攻击状态通常只能被冲刺中断，或者在攻击接近完成时才能被中断
        float attackProgress = (Time.time - attackStartTime) / attackDuration;
        
        // 如果攻击进度超过阈值（如0.7），则可以被中断
        return attackProgress > 0.7f || attackFinished;
    }
} 