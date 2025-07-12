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
        animator?.SetFloat("VerticalSpeed", 0f); // 确保在地面时垂直速度为0
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

        // 更新动画参数
        animator?.SetFloat("Speed", Mathf.Abs(playerController.GetMoveInput().x));

        // 检查是否停止移动
        if (playerController.GetMoveInput().x == 0)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
        // 检查跳跃输入
        else if (playerController.JumpInput)
        {
            stateMachine.ChangeState(stateMachine.JumpingState);
        }
        // 检查攻击输入
        else if (playerController.AttackInput)
        {
            stateMachine.ChangeState(stateMachine.AttackingState);
        }
        // 检查冲刺输入
        else if (playerController.SprintInput && playerController.CanSprint)
        {
            stateMachine.ChangeState(stateMachine.SprintingState);
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