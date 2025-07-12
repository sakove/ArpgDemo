using UnityEngine;

public class PlayerJumpingState : PlayerState
{
    private bool isJumpPerformed;
    
    public PlayerJumpingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 设置动画参数
        animator?.SetBool("IsJumping", true);
        animator?.SetBool("IsGrounded", false);
        
        // 执行跳跃
        playerController.PerformJump();
        isJumpPerformed = true;
        
        // 重置土狼时间
        playerController.ResetCoyoteTime();
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 重置动画参数
        animator?.SetBool("IsJumping", false);
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 获取移动输入
        Vector2 moveInput = playerController.GetMoveInput();
        
        // 设置动画参数
        animator?.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        
        // 检查是否已经开始下落
        if (rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(stateMachine.FallingState);
            return;
        }
        
        // 检查是否松开跳跃键（实现短跳）
        if (!playerController.JumpHeld && isJumpPerformed)
        {
            // 减小上升速度，实现短跳
            playerController.CutJump();
        }
        
        // 检查是否按下攻击键（允许空中攻击）
        if (playerController.AttackInput)
        {
            stateMachine.ChangeState(stateMachine.AttackingState);
            return;
        }
        
        // 允许在空中冲刺
        if (playerController.SprintInput && playerController.CanSprint)
        {
            stateMachine.ChangeState(stateMachine.SprintingState);
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 获取移动输入并应用水平移动（空中控制）
        Vector2 moveInput = playerController.GetMoveInput();
        playerController.Move(moveInput.x, playerController.AirControlFactor);
    }
    
    public override bool CanBeInterrupted()
    {
        // 跳跃状态可以被攻击和冲刺中断，但不能被普通移动中断
        return true;
    }
} 