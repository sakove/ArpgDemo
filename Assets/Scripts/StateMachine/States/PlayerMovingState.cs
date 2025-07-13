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