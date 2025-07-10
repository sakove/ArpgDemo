using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 设置动画参数
        animator?.SetFloat("Speed", 0f);
        
        // 确保玩家停止水平移动
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 获取移动输入
        Vector2 moveInput = playerController.GetMoveInput();
        
        // 检查是否有移动输入
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            // 切换到移动状态
            stateMachine.ChangeState(stateMachine.MovingState);
            return;
        }
        
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
    
    public override void DoChecks()
    {
        base.DoChecks();
        
        // 在这里可以进行额外的检查
    }
    
    public override bool CanBeInterrupted()
    {
        // 闲置状态可以被任何操作中断
        return true;
    }
} 