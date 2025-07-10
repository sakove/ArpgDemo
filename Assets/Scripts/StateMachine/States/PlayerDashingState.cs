using UnityEngine;

public class PlayerDashingState : PlayerState
{
    private float dashStartTime;
    private float dashDuration;
    private Vector2 dashDirection;
    private bool isDashFinished;
    
    public PlayerDashingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 设置冲刺开始时间
        dashStartTime = Time.time;
        
        // 获取冲刺持续时间
        dashDuration = playerController.DashDuration;
        
        // 确定冲刺方向
        dashDirection = DetermineDashDirection();
        
        // 重置冲刺完成标志
        isDashFinished = false;
        
        // 执行冲刺
        playerController.PerformDash(dashDirection);
        
        // 设置动画参数
        animator?.SetTrigger("Dash");
        animator?.SetBool("IsDashing", true);
        
        // 重置冲刺冷却
        playerController.StartDashCooldown();
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 重置动画参数
        animator?.SetBool("IsDashing", false);
        animator?.ResetTrigger("Dash");
        
        // 恢复正常重力
        playerController.RestoreGravity();
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 检查冲刺是否完成
        if (Time.time >= dashStartTime + dashDuration)
        {
            isDashFinished = true;
        }
        
        // 如果冲刺完成，切换回适当的状态
        if (isDashFinished)
        {
            // 根据当前情况决定下一个状态
            DecideNextState();
            return;
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 冲刺过程中持续应用冲刺速度
        playerController.ApplyDashVelocity(dashDirection);
    }
    
    private Vector2 DetermineDashDirection()
    {
        // 获取移动输入
        Vector2 moveInput = playerController.GetMoveInput();
        
        // 如果有移动输入，使用该方向冲刺
        if (moveInput.sqrMagnitude > 0.1f)
        {
            return moveInput.normalized;
        }
        
        // 如果没有移动输入，使用玩家面朝的方向
        return new Vector2(playerController.IsFacingRight ? 1f : -1f, 0f);
    }
    
    private void DecideNextState()
    {
        // 检查是否在地面上
        if (playerController.IsGrounded)
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
        else
        {
            // 如果在空中，切换到下落状态
            stateMachine.ChangeState(stateMachine.FallingState);
        }
    }
    
    public override bool CanBeInterrupted()
    {
        // 冲刺通常不能被中断，除非已经接近完成
        float dashProgress = (Time.time - dashStartTime) / dashDuration;
        
        // 如果冲刺进度超过阈值（如0.8），则可以被中断
        return dashProgress > 0.8f || isDashFinished;
    }
} 