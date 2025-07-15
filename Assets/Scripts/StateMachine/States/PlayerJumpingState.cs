using UnityEngine;

public class PlayerJumpingState : PlayerState
{
    private bool isJumpPerformed;
    private CombatController combatController;
    
    public PlayerJumpingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
        combatController = playerController.GetComponent<CombatController>();
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 触发跳跃动画
        playerController.TriggerJumpAnimation();
        
        // 执行跳跃
        playerController.PerformJump();
        isJumpPerformed = true;
    }
    
    public override void Exit()
    {
        base.Exit();
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 获取移动输入
        Vector2 moveInput = playerController.GetMoveInput();
        
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
            playerController.UseAttackInput(); // <<-- 消耗攻击输入
            stateMachine.ChangeState(stateMachine.AttackingState);
            return;
        }
        
        // 允许在空中冲刺
        if (playerController.SprintInput && playerController.CanSprint)
        {
            playerController.UseSprintInput(); // <<-- 消耗冲刺输入
            stateMachine.ChangeState(stateMachine.SprintingState);
            return;
        }
        
        // 检查技能1输入（允许空中使用技能）
        if (playerController.Skill1Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(0);
            if (skill != null && combatController.CanUseSkill(0))
            {
                playerController.UseSkillInput(1); // <<-- 消耗技能1输入
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
                playerController.UseSkillInput(2); // <<-- 消耗技能2输入
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
                playerController.UseSkillInput(3); // <<-- 消耗技能3输入
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
                playerController.UseSkillInput(4); // <<-- 消耗技能4输入
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
        // 检查技能5输入
        else if (playerController.Skill5Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(4);
            if (skill != null && combatController.CanUseSkill(4))
            {
                playerController.UseSkillInput(5); // <<-- 消耗技能5输入
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
        // 检查技能6输入
        else if (playerController.Skill6Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(5);
            if (skill != null && combatController.CanUseSkill(5))
            {
                playerController.UseSkillInput(6); // <<-- 消耗技能6输入
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
        // 检查技能7输入
        else if (playerController.Skill7Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(6);
            if (skill != null && combatController.CanUseSkill(6))
            {
                playerController.UseSkillInput(7); // <<-- 消耗技能7输入
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
    }
    
    public override bool CanBeInterrupted()
    {
        // 跳跃状态可以被攻击和冲刺中断，但不能被普通移动中断
        return true;
    }
} 