using UnityEngine;

/// <summary>
/// 玩家状态的基类，所有具体状态都继承自此类
/// </summary>
public abstract class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected PlayerController playerController;
    protected Rigidbody2D rb;
    protected Animator animator; // 这个字段可能为空
    protected CombatController combatController; // 添加对CombatController的引用
    
    protected float startTime;
    
    public PlayerState(PlayerStateMachine stateMachine, PlayerController playerController)
    {
        this.stateMachine = stateMachine;
        this.playerController = playerController;
        this.rb = playerController.GetComponent<Rigidbody2D>();
        // 尝试获取Animator和CombatController组件，但如果不存在也不会报错
        this.animator = playerController.GetComponent<Animator>();
        this.combatController = playerController.GetComponent<CombatController>();
    }
    
    public virtual void Enter()
    {
        startTime = Time.time;
        DoChecks();
    }
    
    public virtual void Exit()
    {
        // 退出状态时的清理工作
    }
    
    public virtual void LogicUpdate()
    {
        // 处理状态逻辑
    }
    
    public virtual void PhysicsUpdate()
    {
        // 处理物理更新
        DoChecks();
    }
    
    public virtual void DoChecks()
    {
        // 进行状态检查，如是否接触地面等
    }

    /// <summary>
    /// 检查通用的攻击和技能输入。
    /// 这是一个旨在被所有子类（地面和空中）调用的共享方法，以避免代码重复。
    /// </summary>
    /// <returns>如果触发了攻击或技能并切换了状态，则返回true。</returns>
    protected virtual bool CheckForAttackAndSkillInputs()
    {
        // 检查攻击输入
        if (playerController.AttackInput)
        {
            playerController.UseAttackInput();
            stateMachine.ChangeState(stateMachine.AttackingState);
            return true;
        }

        if (combatController == null) return false;

        // 循环检查所有技能槽位
        for (int i = 0; i < 7; i++)
        {
            bool skillInput = false;
            switch (i)
            {
                case 0: skillInput = playerController.Skill1Input; break;
                case 1: skillInput = playerController.Skill2Input; break;
                case 2: skillInput = playerController.Skill3Input; break;
                case 3: skillInput = playerController.Skill4Input; break;
                case 4: skillInput = playerController.Skill5Input; break;
                case 5: skillInput = playerController.Skill6Input; break;
                case 6: skillInput = playerController.Skill7Input; break;
            }

            if (skillInput && combatController.CanUseSkill(i))
            {
                Skill skill = combatController.GetEquippedSkill(i);
                if (skill != null)
                {
                    playerController.UseSkillInput(i + 1); // 消耗输入
                    stateMachine.ChangeState(stateMachine.UsingSkillState, skill);
                    return true; // 成功触发技能，中断检查
                }
            }
        }
        return false; // 未触发任何技能
    }
    
    public virtual bool CanBeInterrupted()
    {
        // 默认所有状态都可以被中断
        return true;
    }
} 