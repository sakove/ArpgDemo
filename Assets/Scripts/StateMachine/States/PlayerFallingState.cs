using UnityEngine;

public class PlayerFallingState : PlayerState
{
    public PlayerFallingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 设置动画参数
        animator?.SetBool("IsFalling", true);
        animator?.SetBool("IsGrounded", false);
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 重置动画参数
        animator?.SetBool("IsFalling", false);
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 设置动画参数
        animator?.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        
        // 检查是否接触地面
        if (playerController.IsGrounded)
        {
            // 根据是否有水平移动输入决定切换到闲置还是移动状态
            Vector2 moveInput = playerController.GetMoveInput();
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                stateMachine.ChangeState(stateMachine.MovingState);
            }
            else
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
            return;
        }
        
        // 检查是否按下跳跃键（为了实现跳跃缓冲）
        if (playerController.JumpInput)
        {
            // 设置跳跃缓冲
            playerController.SetJumpBuffer();
        }
        
        // 检查是否按下攻击键（允许空中攻击）
        if (playerController.AttackInput)
        {
            stateMachine.ChangeState(stateMachine.AttackingState);
            return;
        }
        
        // 检查是否按下冲刺键（允许空中冲刺）
        if (playerController.DashInput && playerController.CanDash)
        {
            stateMachine.ChangeState(stateMachine.DashingState);
            return;
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 获取移动输入并应用水平移动（空中控制）
        Vector2 moveInput = playerController.GetMoveInput();
        playerController.Move(moveInput.x, playerController.AirControlFactor);
        
        // 应用下落加速度（更快的下落感觉更好）
        playerController.ApplyFallMultiplier();
    }
    
    public override bool CanBeInterrupted()
    {
        // 下落状态可以被攻击和冲刺中断，但不能被普通移动中断
        return true;
    }
} 