using UnityEngine;

public class PlayerFallingState : PlayerState
{
    private CombatController combatController;
    
    public PlayerFallingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
        combatController = playerController.GetComponent<CombatController>();
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
        
        // 允许在空中冲刺
        if (playerController.SprintInput && playerController.CanSprint)
        {
            stateMachine.ChangeState(stateMachine.SprintingState);
            return;
        }
        
        // 检查技能1输入（允许空中使用技能）
        if (playerController.Skill1Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(0);
            if (skill != null && combatController.CanUseSkill(0))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
                return;
            }
        }
        
        // 检查技能2输入
        if (playerController.Skill2Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(1);
            if (skill != null && combatController.CanUseSkill(1))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
                return;
            }
        }
        
        // 检查技能3输入
        if (playerController.Skill3Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(2);
            if (skill != null && combatController.CanUseSkill(2))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
                return;
            }
        }
        // 检查技能4输入
        else if (playerController.Skill4Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(3);
            if (skill != null && combatController.CanUseSkill(3))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
        // 检查技能5输入
        else if (playerController.Skill5Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(4);
            if (skill != null && combatController.CanUseSkill(4))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
        // 检查技能6输入
        else if (playerController.Skill6Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(5);
            if (skill != null && combatController.CanUseSkill(5))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
        // 检查技能7输入
        else if (playerController.Skill7Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(6);
            if (skill != null && combatController.CanUseSkill(6))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 获取移动输入并应用水平移动（空中控制）
        Vector2 moveInput = playerController.GetMoveInput();
        playerController.Move(moveInput.x, playerController.AirControlFactor);
        
        // 应用下落加速度
        playerController.ApplyFallMultiplier();
    }
    
    public override bool CanBeInterrupted()
    {
        // 下落状态可以被攻击和冲刺中断，但不能被普通移动中断
        return true;
    }
} 