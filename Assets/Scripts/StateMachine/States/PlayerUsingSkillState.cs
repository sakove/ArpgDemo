using UnityEngine;

/// <summary>
/// 玩家使用技能状态
/// 这是一个通用的状态，可以处理各种技能的使用
/// </summary>
public class PlayerUsingSkillState : PlayerState
{
    private float skillStartTime;
    private float skillDuration;
    private bool skillFinished;
    private CombatController combatController;
    private Skill activeSkill;
    private AnimatorOverrideController originalOverrideController;
    
    public PlayerUsingSkillState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
        combatController = playerController.GetComponent<CombatController>();
        if (combatController == null)
        {
            Debug.LogError("PlayerUsingSkillState: CombatController is not found on the player object!");
        }
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 如果没有传入技能，则直接结束状态
        if (activeSkill == null)
        {
            Debug.LogError("PlayerUsingSkillState: No active skill provided!");
            skillFinished = true;
            return;
        }
        
        // 设置技能开始时间
        skillStartTime = Time.time;
        
        // 获取技能持续时间
        skillDuration = activeSkill.GetDuration();
        
        // 重置技能完成标志
        skillFinished = false;
        
        // 根据技能设置是否可以转向
        playerController.CanFlip = activeSkill.canMoveWhileUsing;
        
        // 保存原始的动画控制器（如果需要）
        if (animator != null && animator.runtimeAnimatorController is AnimatorOverrideController)
        {
            originalOverrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
        }
        
        // 激活技能
        if (combatController != null)
        {
            // 这里不直接调用activeSkill.Activate，而是通过CombatController来管理技能激活
            // 这样可以确保技能冷却、连击等逻辑由CombatController统一处理
            int skillIndex = GetSkillIndex();
            if (skillIndex >= 0)
            {
                combatController.UseSkill(skillIndex);
            }
            else
            {
                // 如果找不到对应的技能槽，直接激活技能
                activeSkill.Activate(playerController.gameObject);
            }
        }
        else
        {
            // 如果没有CombatController，直接激活技能
            activeSkill.Activate(playerController.gameObject);
        }
        
        // 设置动画 - 使用分层动画系统
        if (animator != null)
        {
            // 判断技能类型，决定使用哪个动画层
            if (activeSkill.isSpecialAnimation)
            {
                // 特殊动画使用交互层
                SetupSpecialAnimation();
            }
            else
            {
                // 普通技能使用战斗层
                SetupCombatAnimation();
            }
        }
    }
    
    private void SetupCombatAnimation()
    {
        // 确保战斗层权重为1
        playerController.SetCombatLayerWeight(1f);
        
        // 使用动画覆盖方式播放技能动画
        if (activeSkill.skillAnimation != null)
        {
            // 创建一个新的覆盖控制器或使用现有的
            AnimatorOverrideController overrideController;
            
            if (animator.runtimeAnimatorController is AnimatorOverrideController)
            {
                overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            }
            else
            {
                overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                animator.runtimeAnimatorController = overrideController;
            }
            
            // 覆盖"Skill"动画片段
            overrideController["Skill"] = activeSkill.skillAnimation;
            
            // 触发技能动画
            playerController.TriggerSkillAnimation();
        }
        else
        {
            Debug.LogWarning($"技能 {activeSkill.skillName} 没有提供动画片段！");
        }
    }
    
    private void SetupSpecialAnimation()
    {
        // 使用交互层播放特殊动画
        if (activeSkill.skillAnimation != null)
        {
            // 创建一个新的覆盖控制器或使用现有的
            AnimatorOverrideController overrideController;
            
            if (animator.runtimeAnimatorController is AnimatorOverrideController)
            {
                overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            }
            else
            {
                overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                animator.runtimeAnimatorController = overrideController;
            }
            
            // 覆盖"Special"动画片段
            overrideController["Special"] = activeSkill.skillAnimation;
            
            // 触发特殊动画
            playerController.TriggerSpecialAnimation("SpecialAction");
        }
        else
        {
            Debug.LogWarning($"特殊技能 {activeSkill.skillName} 没有提供动画片段！");
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 如果是特殊动画，结束交互层动画
        if (activeSkill != null && activeSkill.isSpecialAnimation)
        {
            playerController.EndSpecialAnimation();
        }
        
        // 恢复玩家的转向能力
        playerController.CanFlip = true;
        
        // 通知技能结束
        if (activeSkill != null)
        {
            activeSkill.OnSkillEnd(playerController.gameObject);
            activeSkill = null;
        }
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 检查技能是否已完成
        if (Time.time >= skillStartTime + skillDuration)
        {
            skillFinished = true;
        }
        
        // 如果技能已完成，切换到适当的状态
        if (skillFinished)
        {
            if (playerController.IsGrounded)
            {
                // 根据是否有移动输入决定切换到闲置还是移动状态
                Vector2 moveInput = playerController.GetMoveInput();
                if (Mathf.Abs(moveInput.x) > 0.1f)
                {
                    stateMachine.ChangeState(stateMachine.MovingState);
                }
                else
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                }
            }
            else
            {
                stateMachine.ChangeState(stateMachine.FallingState);
            }
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 如果技能允许移动，则应用移动
        if (activeSkill != null && activeSkill.canMoveWhileUsing)
        {
            Vector2 moveInput = playerController.GetMoveInput();
            playerController.Move(moveInput.x, activeSkill.movementSpeedModifier);
            
            // 手动处理转向
            // 即使在使用技能时，也应该能够根据移动方向转向
            if (moveInput.x > 0.1f && !playerController.IsFacingRight)
            {
                // 只有当技能允许转向时才调用Flip
                if (playerController.CanFlip)
                {
                    // 设置朝向为右
                    playerController.transform.rotation = Quaternion.Euler(0, 0, 0);
                    playerController.SetFacingRight(true);
                }
            }
            else if (moveInput.x < -0.1f && playerController.IsFacingRight)
            {
                // 只有当技能允许转向时才调用Flip
                if (playerController.CanFlip)
                {
                    // 设置朝向为左
                    playerController.transform.rotation = Quaternion.Euler(0, 180, 0);
                    playerController.SetFacingRight(false);
                }
            }
        }
        
        // 应用下落重力修正
        playerController.ApplyFallMultiplier();
    }
    
    public override bool CanBeInterrupted()
    {
        // 默认情况下，技能状态不能被中断
        // 但可以根据具体技能的属性来决定
        return activeSkill != null && activeSkill.canMoveWhileUsing;
    }
    
    /// <summary>
    /// 设置当前激活的技能
    /// </summary>
    /// <param name="skill">要使用的技能</param>
    public void SetActiveSkill(Skill skill)
    {
        activeSkill = skill;
    }
    
    /// <summary>
    /// 获取技能在CombatController中的索引
    /// </summary>
    /// <returns>技能索引，如果找不到则返回-1</returns>
    private int GetSkillIndex()
    {
        if (activeSkill == null || combatController == null)
            return -1;
            
        return combatController.GetSkillIndex(activeSkill);
    }
} 