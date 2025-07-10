using UnityEngine;

public class PlayerMovingState : PlayerState
{
    public PlayerMovingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 播放移动动画或设置动画参数
        animator?.SetBool("IsMoving", true);
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 重置动画参数
        animator?.SetBool("IsMoving", false);
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 获取移动输入
        Vector2 moveInput = playerController.GetMoveInput();
        
        // 如果没有移动输入，切换回闲置状态
        if (Mathf.Abs(moveInput.x) < 0.1f)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }
        
        // 设置动画参数
        animator?.SetFloat("Speed", Mathf.Abs(moveInput.x));
        
        // 检查是否按下跳跃键
        if (playerController.JumpInput)
        {
            // 检查是否在地面上或在土狼时间内
            if (playerController.IsGrounded || playerController.CoyoteTimeCounter > 0)
            {
                stateMachine.ChangeState(stateMachine.JumpingState);
                return;
            }
        }
        
        // 检查是否按下攻击键
        if (playerController.AttackInput)
        {
            stateMachine.ChangeState(stateMachine.AttackingState);
            return;
        }
        
        // 检查是否按下冲刺键
        if (playerController.DashInput && playerController.CanDash)
        {
            stateMachine.ChangeState(stateMachine.DashingState);
            return;
        }
        
        // 检查是否不在地面上且下落
        if (!playerController.IsGrounded && rb.linearVelocity.y < -0.1f)
        {
            stateMachine.ChangeState(stateMachine.FallingState);
            return;
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 获取移动输入
        Vector2 moveInput = playerController.GetMoveInput();
        
        // 移动玩家
        playerController.Move(moveInput.x);
    }
    
    public override bool CanBeInterrupted()
    {
        // 移动状态可以被任何操作中断
        return true;
    }
} 