using UnityEngine;

public class PlayerMovingState : PlayerState
{
    private CombatController combatController;
    
    public PlayerMovingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
        combatController = playerController.GetComponent<CombatController>();
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 播放移动动画或设置动画参数
        animator?.SetFloat("VerticalSpeed", 0f); // 确保在地面时垂直速度为0
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 重置动画参数
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 更新动画参数
        animator?.SetFloat("Speed", Mathf.Abs(playerController.GetMoveInput().x));

        // 1. 优先检查状态特有的转换：停止移动
        if (playerController.GetMoveInput().x == 0)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
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