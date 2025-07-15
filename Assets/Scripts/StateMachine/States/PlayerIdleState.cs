using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
        // combatController现在由基类初始化，这里不再需要
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
        
        // 1. 优先检查状态特有的转换：移动
        if (playerController.GetMoveInput().x != 0)
        {
            stateMachine.ChangeState(stateMachine.MovingState);
            return; // 状态已切换，结束本次更新
        }
        
        // 2. 检查地面专属动作：跳跃
        if (playerController.JumpInput)
        {
            playerController.UseJumpInput();
            stateMachine.ChangeState(stateMachine.JumpingState);
            return; // 状态已切换，结束本次更新
        }
        
        // 3. 检查地面专属动作：冲刺
        if (playerController.SprintInput && playerController.CanSprint)
        {
            playerController.UseSprintInput();
            stateMachine.ChangeState(stateMachine.SprintingState);
            return; // 状态已切换，结束本次更新
        }
        
        // 4. 如果以上都不是，则调用基类的方法检查通用的攻击/技能动作
        CheckForAttackAndSkillInputs();
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