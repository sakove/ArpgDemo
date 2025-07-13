using UnityEngine;

public class PlayerAttackingState : PlayerState
{
    private float attackStartTime;
    private float attackDuration;
    private bool attackFinished;
    private CombatController combatController;
    
    public PlayerAttackingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
        combatController = playerController.GetComponent<CombatController>();
        if (combatController == null)
        {
            Debug.LogError("PlayerAttackingState: CombatController is not found on the player object!");
        }
    }
    
    public override void Enter()
    {
        base.Enter();

        // 核心改动：进入攻击状态时，禁止玩家转向，以确保攻击动作的稳定性。
        playerController.CanFlip = false;
        
        // 设置攻击开始时间
        attackStartTime = Time.time;
        
        // 获取当前攻击的持续时间
        if (combatController != null)
        {
            attackDuration = combatController.GetCurrentAttackDuration();
        }
        else
        {
            // 如果没有CombatController，提供一个默认的短时间，并标记攻击立即完成，以避免卡死
            attackDuration = 0.1f;
            attackFinished = true;
            return; // 提前退出Enter方法，因为没有CombatController无法继续
        }
        
        // 重置攻击完成标志
        attackFinished = false;
        
        // 执行攻击
        if (combatController != null)
        {
            combatController.PerformAttack();
        }
        
        // 设置动画参数
        animator?.SetTrigger("Attack"); // 如果你仍想用触发器立即响应，可以保留
        animator?.SetBool("IsAttacking", true);
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 核心改动：退出攻击状态时，恢复玩家的转向能力。
        playerController.CanFlip = true;
        
        // 重置动画参数（如果需要）
        animator?.ResetTrigger("Attack"); // 重置攻击触发器
        // 不再重置"Skill"触发器，因为我们现在只使用"UseSkill"触发器
        animator?.SetBool("IsAttacking", false);
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
            stateMachine.ChangeState(stateMachine.SprintingState);
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 攻击时可以有轻微的水平移动（如果需要）
        Vector2 moveInput = playerController.GetMoveInput();
        
        // 如果配置允许攻击时移动，则应用移动
        if (combatController != null && combatController.CanMoveWhileAttacking())
        {
            playerController.Move(moveInput.x, combatController.AttackMovementFactor);
        }
    }
    
    private void DecideNextState()
    {
        // 检查是否准备好下一个连击
        if (combatController != null && combatController.isNextComboReady && playerController.AttackInput)
        {
            // 重新进入攻击状态，执行下一个连击
            stateMachine.ChangeState(stateMachine.AttackingState);
            return;
        }
        
        // 如果没有下一个连击，根据当前情况切换状态
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