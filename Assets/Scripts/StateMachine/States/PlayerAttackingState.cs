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
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 设置攻击开始时间
        attackStartTime = Time.time;
        
        // 获取当前攻击的持续时间
        attackDuration = combatController.GetCurrentAttackDuration();
        
        // 重置攻击完成标志
        attackFinished = false;
        
        // 执行攻击
        combatController.PerformAttack();
        
        // 设置动画参数
        animator?.SetTrigger("Attack");
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 重置动画参数（如果需要）
        animator?.ResetTrigger("Attack");
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 检查攻击是否完成
        if (Time.time >= attackStartTime + attackDuration)
        {
            attackFinished = true;
        }
        
        // 如果攻击完成，切换回适当的状态
        if (attackFinished)
        {
            // 根据当前情况决定下一个状态
            DecideNextState();
            return;
        }
        
        // 检查是否按下冲刺键（冲刺可以打断攻击）
        if (playerController.DashInput && playerController.CanDash)
        {
            stateMachine.ChangeState(stateMachine.DashingState);
            return;
        }
        
        // 攻击过程中可以接受下一个攻击输入（连击系统）
        if (playerController.AttackInput && combatController.CanPerformNextCombo())
        {
            // 设置下一个连击
            combatController.SetNextComboReady();
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 攻击时可以有轻微的水平移动（如果需要）
        Vector2 moveInput = playerController.GetMoveInput();
        
        // 如果配置允许攻击时移动，则应用移动
        if (combatController.CanMoveWhileAttacking())
        {
            playerController.Move(moveInput.x, combatController.AttackMovementFactor);
        }
    }
    
    private void DecideNextState()
    {
        // 检查是否准备好下一个连击
        if (combatController.isNextComboReady)
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