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
        animator?.SetFloat("VerticalSpeed", 0f); // 确保在地面时垂直速度为0
        
        // 确保玩家停止水平移动
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 检查移动输入
        if (playerController.GetMoveInput().x != 0)
        {
            stateMachine.ChangeState(stateMachine.MovingState);
        }
        // 检查跳跃输入
        else if (playerController.JumpInput)
        {
            stateMachine.ChangeState(stateMachine.JumpingState);
        }
        // 检查攻击输入
        else if (playerController.AttackInput) // 暂时移除 CanAttack 检查
        {
            stateMachine.ChangeState(stateMachine.AttackingState);
        }
        // 检查冲刺输入
        else if (playerController.SprintInput && playerController.CanSprint)
        {
            stateMachine.ChangeState(stateMachine.SprintingState);
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