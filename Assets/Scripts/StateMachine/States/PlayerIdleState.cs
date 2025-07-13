using UnityEngine;

public class PlayerIdleState : PlayerState
{
    private CombatController combatController;
    
    public PlayerIdleState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
        combatController = playerController.GetComponent<CombatController>();
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
        // 检查技能1输入
        else if (playerController.Skill1Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(0);
            if (skill != null && combatController.CanUseSkill(0))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
        // 检查技能2输入
        else if (playerController.Skill2Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(1);
            if (skill != null && combatController.CanUseSkill(1))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
            }
        }
        // 检查技能3输入
        else if (playerController.Skill3Input && combatController != null)
        {
            Skill skill = combatController.GetEquippedSkill(2);
            if (skill != null && combatController.CanUseSkill(2))
            {
                stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
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