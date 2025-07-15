using System.Collections;
using UnityEngine;
// 添加正确的命名空间
using UnityEngine.Animations;

/// <summary>
/// 战斗控制器，负责处理玩家的战斗相关逻辑
/// </summary>
public class CombatController : MonoBehaviour
{
    [Header("攻击类型设置")]
    [Tooltip("地面普通攻击的技能数组，每个元素代表连击的一段")]
    [SerializeField] private Skill[] groundAttacks;

    [Tooltip("地面向上攻击的技能数组，每个元素代表连击的一段。为未来设计（如对空技）保留")]
    [SerializeField] private Skill[] upGroundAttacks;

    [Tooltip("地面向下攻击的技能数组，每个元素代表连击的一段。为未来设计（如地面波）保留")]
    [SerializeField] private Skill[] downGroundAttacks;
    
    [Tooltip("空中普通攻击的技能数组，每个元素代表连击的一段")]
    [SerializeField] private Skill[] airAttacks;
    
    [Tooltip("空中向上攻击的技能数组，每个元素代表连击的一段。为未来设计（如二段跳攻击、升龙等）保留")]
    [SerializeField] private Skill[] upAirAttacks;
    
    [Tooltip("空中向下攻击的技能数组，每个元素代表连击的一段。为未来设计（如踩踏、下劈等）保留")]
    [SerializeField] private Skill[] downAirAttacks;
    
    [Header("技能槽")]
    [SerializeField] private Skill[] equippedSkills = new Skill[7];
    
    [Header("连击设置")]
    [Tooltip("在最后一次攻击后，超过此时间没有新的攻击，连击将会重置")]
    [SerializeField] private float comboResetTime = 1.5f;
    
    [Header("攻击移动设置")]
    [SerializeField] private bool canMoveWhileAttacking = false;
    
    [Header("动画设置")]
    [Tooltip("基础的Animator Controller，包含所有的状态和转换")]
    [SerializeField] private RuntimeAnimatorController baseAnimatorController;
    
    [Tooltip("地面普通攻击的动画状态名称")]
    [SerializeField] private string groundAttackStateName = "Ground_Attack";

    [Tooltip("地面向上攻击的动画状态名称")]
    [SerializeField] private string upGroundAttackStateName = "Up_Ground_Attack";
    
    [Tooltip("地面向下攻击的动画状态名称")]
    [SerializeField] private string downGroundAttackStateName = "Down_Ground_Attack";
    
    [Tooltip("空中普通攻击的动画状态名称")]
    [SerializeField] private string airAttackStateName = "Air_Attack";
    
    [Tooltip("空中向上攻击的动画状态名称")]
    [SerializeField] private string upAirAttackStateName = "Up_Air_Attack";
    
    [Tooltip("空中向下攻击的动画状态名称")]
    [SerializeField] private string downAirAttackStateName = "Down_Air_Attack";
    
    [Tooltip("技能使用的动画状态名称")]
    [SerializeField] private string skillStateName = "Skill";
    
    // 连击状态
    private int groundComboIndex = 0;
    private int upGroundComboIndex = 0;
    private int downGroundComboIndex = 0;
    private int airComboIndex = 0;
    private int upAirComboIndex = 0;
    private int downAirComboIndex = 0;
    private float lastAttackTime;
    
    // 冷却状态
    private float[] skillCooldowns = new float[7];
    
    // 组件引用
    private PlayerController playerController;
    private Animator animator;
    private AnimatorOverrideController overrideController;
    
    // 当前使用的技能，用于跟踪并在适当的时候调用OnSkillEnd
    private Skill currentActiveSkill;
    private float currentSkillEndTime;
    
    // 属性
    public int CurrentGroundComboIndex => groundComboIndex;
    public int CurrentUpGroundComboIndex => upGroundComboIndex;
    public int CurrentDownGroundComboIndex => downGroundComboIndex;
    public int CurrentAirComboIndex => airComboIndex;
    public int CurrentUpAirComboIndex => upAirComboIndex;
    public int CurrentDownAirComboIndex => downAirComboIndex;
    public float AttackMovementFactor => 0.3f; // 为了向后兼容保留
    
    private void Awake()
    {
        // 获取组件
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        
        // 初始化动画覆盖控制器
        InitializeAnimationOverrideController();
    }
    
    /// <summary>
    /// 初始化动画覆盖控制器
    /// </summary>
    private void InitializeAnimationOverrideController()
    {
        if (animator != null && baseAnimatorController != null)
        {
            // 创建一个基于基础控制器的覆盖控制器
            overrideController = new AnimatorOverrideController(baseAnimatorController);
            
            // 将覆盖控制器应用到Animator
            animator.runtimeAnimatorController = overrideController;
            
            Debug.Log("AnimationOverrideController初始化成功");
        }
        else
        {
            Debug.LogError("无法初始化AnimationOverrideController：animator或baseAnimatorController为空");
        }
    }
    
    private void Update()
    {
        // 更新技能冷却
        for (int i = 0; i < skillCooldowns.Length; i++)
        {
            if (skillCooldowns[i] > 0)
            {
                skillCooldowns[i] -= Time.deltaTime;
            }
        }
        
        // 检查连击重置
        if (Time.time > lastAttackTime + comboResetTime)
        {
            ResetAllCombos();
        }
        
        // 检查当前技能是否结束
        if (currentActiveSkill != null && Time.time >= currentSkillEndTime)
        {
            // 调用技能结束方法以清理
            currentActiveSkill.OnSkillEnd(gameObject);
            currentActiveSkill = null;
        }
    }
    
    /// <summary>
    /// 执行地面普通攻击
    /// </summary>
    /// <param name="isMoving">玩家是否在移动</param>
    /// <param name="performedAttack">【输出】本次实际执行的攻击技能</param>
    /// <returns>攻击的持续时间</returns>
    public float PerformGroundAttack(bool isMoving, out Skill performedAttack)
    {
        // 核心改动：在执行此攻击前，重置所有其他类型的连击计数
        ResetOtherCombos("Ground");
        
        performedAttack = null;
        // 检查攻击数组是否有效
        if (groundAttacks == null || groundAttacks.Length == 0)
        {
            Debug.LogWarning("地面攻击数组未设置");
            return 0f;
        }
        
        // 获取当前要执行的攻击
        Skill currentAttack = groundAttacks[groundComboIndex];
        
        if (currentAttack != null)
        {
            performedAttack = currentAttack; // <<-- 核心改动：输出当前技能
            // 如果有其他活动技能，先结束它
            EndCurrentSkill();
            
            // 获取正确的动画剪辑
            AnimationClip attackClip = currentAttack.skillAnimation;
            
            if (attackClip != null)
            {
                // 使用AnimationOverrideController替换动画
                if (overrideController != null)
                {
                    overrideController[groundAttackStateName] = attackClip;
                }
                
                // 设置动画参数
                animator?.SetInteger("ComboIndex", groundComboIndex);
                
                // 触发攻击动画
                animator?.SetTrigger("Attack");
            }
            
            // 激活技能（造成伤害、播放特效等）
            currentAttack.Activate(gameObject);
            
            // 设置当前活动技能和结束时间
            currentActiveSkill = currentAttack;
            currentSkillEndTime = Time.time + currentAttack.GetDuration();
            
            // 更新最后攻击时间
            lastAttackTime = Time.time;
            
            // 递增连击索引，如果到达末尾则循环回开头
            groundComboIndex = (groundComboIndex + 1) % groundAttacks.Length;
            
            // 返回攻击持续时间
            return currentAttack.GetDuration();
        }
        
        return 0f;
    }
    
    /// <summary>
    /// 执行地面向上攻击
    /// </summary>
    /// <param name="performedAttack">【输出】本次实际执行的攻击技能</param>
    /// <returns>攻击的持续时间</returns>
    public float PerformUpGroundAttack(out Skill performedAttack)
    {
        // 核心改动：在执行此攻击前，重置所有其他类型的连击计数
        ResetOtherCombos("UpGround");

        performedAttack = null;
        if (upGroundAttacks == null || upGroundAttacks.Length == 0)
        {
            Debug.LogWarning("地面向上攻击数组未设置");
            return 0f;
        }
        
        Skill currentAttack = upGroundAttacks[upGroundComboIndex];
        
        if (currentAttack != null)
        {
            performedAttack = currentAttack; // <<-- 核心改动：输出当前技能
            EndCurrentSkill();
            AnimationClip attackClip = currentAttack.skillAnimation;
            if (attackClip != null)
            {
                if (overrideController != null)
                {
                    overrideController[upGroundAttackStateName] = attackClip;
                }
                animator?.SetInteger("ComboIndex", upGroundComboIndex);
                animator?.SetTrigger("Attack");
            }
            
            currentAttack.Activate(gameObject);
            currentActiveSkill = currentAttack;
            currentSkillEndTime = Time.time + currentAttack.GetDuration();
            lastAttackTime = Time.time;
            upGroundComboIndex = (upGroundComboIndex + 1) % upGroundAttacks.Length;
            return currentAttack.GetDuration();
        }
        return 0f;
    }

    /// <summary>
    /// 执行地面向下攻击
    /// </summary>
    /// <param name="performedAttack">【输出】本次实际执行的攻击技能</param>
    /// <returns>攻击的持续时间</returns>
    public float PerformDownGroundAttack(out Skill performedAttack)
    {
        // 核心改动：在执行此攻击前，重置所有其他类型的连击计数
        ResetOtherCombos("DownGround");

        performedAttack = null;
        if (downGroundAttacks == null || downGroundAttacks.Length == 0)
        {
            Debug.LogWarning("地面向下攻击数组未设置");
            return 0f;
        }
        
        Skill currentAttack = downGroundAttacks[downGroundComboIndex];
        
        if (currentAttack != null)
        {
            performedAttack = currentAttack; // <<-- 核心改动：输出当前技能
            EndCurrentSkill();
            AnimationClip attackClip = currentAttack.skillAnimation;
            if (attackClip != null)
            {
                if (overrideController != null)
                {
                    overrideController[downGroundAttackStateName] = attackClip;
                }
                animator?.SetInteger("ComboIndex", downGroundComboIndex);
                animator?.SetTrigger("Attack");
            }
            
            currentAttack.Activate(gameObject);
            currentActiveSkill = currentAttack;
            currentSkillEndTime = Time.time + currentAttack.GetDuration();
            lastAttackTime = Time.time;
            downGroundComboIndex = (downGroundComboIndex + 1) % downGroundAttacks.Length;
            return currentAttack.GetDuration();
        }
        return 0f;
    }

    /// <summary>
    /// 执行空中普通攻击
    /// </summary>
    /// <param name="performedAttack">【输出】本次实际执行的攻击技能</param>
    /// <returns>攻击的持续时间</returns>
    public float PerformAirAttack(out Skill performedAttack)
    {
        // 核心改动：在执行此攻击前，重置所有其他类型的连击计数
        ResetOtherCombos("Air");

        performedAttack = null;
        // 检查攻击数组是否有效
        if (airAttacks == null || airAttacks.Length == 0)
        {
            Debug.LogWarning("空中攻击数组未设置");
            return 0f;
        }
        
        // 获取当前要执行的攻击
        Skill currentAttack = airAttacks[airComboIndex];
        
        if (currentAttack != null)
        {
            performedAttack = currentAttack; // <<-- 核心改动：输出当前技能
            // 如果有其他活动技能，先结束它
            EndCurrentSkill();
            
            // 获取正确的动画剪辑
            AnimationClip attackClip = currentAttack.skillAnimation;
            
            if (attackClip != null)
            {
                // 使用AnimationOverrideController替换动画
                if (overrideController != null)
                {
                    overrideController[airAttackStateName] = attackClip;
                }
                
                // 设置动画参数
                animator?.SetInteger("ComboIndex", airComboIndex);
                
                // 触发攻击动画
                animator?.SetTrigger("Attack");
            }
            
            // 激活技能（造成伤害、播放特效等）
            currentAttack.Activate(gameObject);
            
            // 设置当前活动技能和结束时间
            currentActiveSkill = currentAttack;
            currentSkillEndTime = Time.time + currentAttack.GetDuration();
            
            // 更新最后攻击时间
            lastAttackTime = Time.time;
            
            // 递增连击索引，如果到达末尾则循环回开头
            airComboIndex = (airComboIndex + 1) % airAttacks.Length;
            
            // 返回攻击持续时间
            return currentAttack.GetDuration();
        }
        
        return 0f;
    }
    
    /// <summary>
    /// 执行空中向上攻击
    /// </summary>
    /// <param name="performedAttack">【输出】本次实际执行的攻击技能</param>
    /// <returns>攻击的持续时间</returns>
    public float PerformUpAirAttack(out Skill performedAttack)
    {
        // 核心改动：在执行此攻击前，重置所有其他类型的连击计数
        ResetOtherCombos("UpAir");

        performedAttack = null;
        // 检查攻击数组是否有效
        if (upAirAttacks == null || upAirAttacks.Length == 0)
        {
            Debug.LogWarning("空中向上攻击数组未设置");
            return 0f;
        }
        
        // 获取当前要执行的攻击
        Skill currentAttack = upAirAttacks[upAirComboIndex];
        
        if (currentAttack != null)
        {
            performedAttack = currentAttack; // <<-- 核心改动：输出当前技能
            // 如果有其他活动技能，先结束它
            EndCurrentSkill();
            
            // 获取正确的动画剪辑
            AnimationClip attackClip = currentAttack.skillAnimation;
            
            if (attackClip != null)
            {
                // 使用AnimationOverrideController替换动画
                if (overrideController != null)
                {
                    overrideController[upAirAttackStateName] = attackClip;
                }
                
                // 设置动画参数
                animator?.SetInteger("ComboIndex", upAirComboIndex);
                
                // 触发攻击动画
                animator?.SetTrigger("Attack");
            }
            
            // 激活技能（造成伤害、播放特效等）
            currentAttack.Activate(gameObject);
            
            // 设置当前活动技能和结束时间
            currentActiveSkill = currentAttack;
            currentSkillEndTime = Time.time + currentAttack.GetDuration();
            
            // 更新最后攻击时间
            lastAttackTime = Time.time;
            
            // 递增连击索引，如果到达末尾则循环回开头
            upAirComboIndex = (upAirComboIndex + 1) % upAirAttacks.Length;
            
            // 返回攻击持续时间
            return currentAttack.GetDuration();
        }
        
        return 0f;
    }
    
    /// <summary>
    /// 执行空中向下攻击
    /// </summary>
    /// <param name="performedAttack">【输出】本次实际执行的攻击技能</param>
    /// <returns>攻击的持续时间</returns>
    public float PerformDownAirAttack(out Skill performedAttack)
    {
        // 核心改动：在执行此攻击前，重置所有其他类型的连击计数
        ResetOtherCombos("DownAir");

        performedAttack = null;
        // 检查攻击数组是否有效
        if (downAirAttacks == null || downAirAttacks.Length == 0)
        {
            Debug.LogWarning("空中向下攻击数组未设置");
            return 0f;
        }
        
        // 获取当前要执行的攻击
        Skill currentAttack = downAirAttacks[downAirComboIndex];
        
        if (currentAttack != null)
        {
            performedAttack = currentAttack; // <<-- 核心改动：输出当前技能
            // 如果有其他活动技能，先结束它
            EndCurrentSkill();
            
            // 获取正确的动画剪辑
            AnimationClip attackClip = currentAttack.skillAnimation;
            
            if (attackClip != null)
            {
                // 使用AnimationOverrideController替换动画
                if (overrideController != null)
                {
                    overrideController[downAirAttackStateName] = attackClip;
                }
                
                // 设置动画参数
                animator?.SetInteger("ComboIndex", downAirComboIndex);
                
                // 触发攻击动画
                animator?.SetTrigger("Attack");
            }
            
            // 激活技能（造成伤害、播放特效等）
            currentAttack.Activate(gameObject);
            
            // 设置当前活动技能和结束时间
            currentActiveSkill = currentAttack;
            currentSkillEndTime = Time.time + currentAttack.GetDuration();
            
            // 更新最后攻击时间
            lastAttackTime = Time.time;
            
            // 递增连击索引，如果到达末尾则循环回开头
            downAirComboIndex = (downAirComboIndex + 1) % downAirAttacks.Length;
            
            // 返回攻击持续时间
            return currentAttack.GetDuration();
        }
        
        return 0f;
    }
    
    /// <summary>
    /// 使用指定槽位的技能
    /// </summary>
    /// <param name="slotIndex">技能槽位索引</param>
    /// <returns>技能的持续时间</returns>
    public float UseSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length)
            return 0f;
            
        Skill skill = equippedSkills[slotIndex];
        
        // 检查技能是否存在且可用
        if (skill != null && CanUseSkill(slotIndex))
        {
            // 如果有活动技能，先结束它
            EndCurrentSkill();
            
            // 获取正确的动画剪辑
            AnimationClip skillClip = skill.skillAnimation;
            
            if (skillClip != null)
            {
                // 使用AnimationOverrideController替换动画
                if (overrideController != null)
                {
                    overrideController[skillStateName] = skillClip;
                }
                
                // 触发技能动画
                animator?.SetTrigger("UseSkill");
            }
            
            // 激活技能
            skill.Activate(gameObject);
            
            // 设置当前活动技能和结束时间
            currentActiveSkill = skill;
            currentSkillEndTime = Time.time + skill.GetDuration();
            
            // 设置冷却
            skillCooldowns[slotIndex] = skill.cooldown;
            
            // 重置连击
            ResetAllCombos();
            
            // 返回技能持续时间
            return skill.GetDuration();
        }
        
        return 0f;
    }
    
    /// <summary>
    /// 辅助方法，用于重置除当前攻击类型之外的所有连击计数器。
    /// </summary>
    /// <param name="currentAttackType">当前正在执行的攻击类型，其连击不应被重置。</param>
    private void ResetOtherCombos(string currentAttackType)
    {
        if (currentAttackType != "Ground") groundComboIndex = 0;
        if (currentAttackType != "UpGround") upGroundComboIndex = 0;
        if (currentAttackType != "DownGround") downGroundComboIndex = 0;
        if (currentAttackType != "Air") airComboIndex = 0;
        if (currentAttackType != "UpAir") upAirComboIndex = 0;
        if (currentAttackType != "DownAir") downAirComboIndex = 0;
    }

    /// <summary>
    /// 结束当前活动的技能
    /// </summary>
    private void EndCurrentSkill()
    {
        if (currentActiveSkill != null)
        {
            currentActiveSkill.OnSkillEnd(gameObject);
            currentActiveSkill = null;
        }
    }
    
    /// <summary>
    /// 检查指定槽位的技能是否可用
    /// </summary>
    /// <param name="slotIndex">技能槽位索引</param>
    /// <returns>如果技能可用，则返回true</returns>
    public bool CanUseSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length)
            return false;
            
        Skill skill = equippedSkills[slotIndex];
        
        // 检查技能是否存在且冷却完成
        if (skill != null && skillCooldowns[slotIndex] <= 0)
        {
            return skill.CanUse(gameObject);
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取当前攻击的持续时间
    /// </summary>
    /// <returns>攻击持续时间</returns>
    public float GetCurrentAttackDuration()
    {
        // 如果有活动技能，返回其剩余持续时间
        if (currentActiveSkill != null)
        {
            return currentSkillEndTime - Time.time;
        }
        
        // 默认持续时间
        return 0.5f;
    }
    
    /// <summary>
    /// 重置所有连击状态
    /// </summary>
    public void ResetAllCombos()
    {
        groundComboIndex = 0;
        upGroundComboIndex = 0;
        downGroundComboIndex = 0;
        airComboIndex = 0;
        upAirComboIndex = 0;
        downAirComboIndex = 0;
        animator?.SetInteger("ComboIndex", 0);
    }
    
    /// <summary>
    /// 检查是否可以在攻击时移动
    /// </summary>
    /// <returns>如果可以在攻击时移动，则返回true</returns>
    public bool CanMoveWhileAttacking()
    {
        return canMoveWhileAttacking;
    }
    
    /// <summary>
    /// 装备技能到指定槽位
    /// </summary>
    /// <param name="skill">要装备的技能</param>
    /// <param name="slotIndex">槽位索引</param>
    public void EquipSkill(Skill skill, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedSkills.Length)
        {
            equippedSkills[slotIndex] = skill;
            skillCooldowns[slotIndex] = 0f;
        }
    }
    
    /// <summary>
    /// 获取指定技能在装备槽中的索引
    /// </summary>
    /// <param name="skill">要查找的技能</param>
    /// <returns>技能在装备槽中的索引，如果未找到则返回-1</returns>
    public int GetSkillIndex(Skill skill)
    {
        if (skill == null)
            return -1;
            
        // 在所有攻击数组中检查
        if (IsSkillInAttackArray(skill, groundAttacks)) return -1;
        if (IsSkillInAttackArray(skill, upGroundAttacks)) return -1;
        if (IsSkillInAttackArray(skill, downGroundAttacks)) return -1;
        if (IsSkillInAttackArray(skill, airAttacks)) return -1;
        if (IsSkillInAttackArray(skill, upAirAttacks)) return -1;
        if (IsSkillInAttackArray(skill, downAirAttacks)) return -1;
            
        // 在装备槽中查找技能
        for (int i = 0; i < equippedSkills.Length; i++)
        {
            if (equippedSkills[i] == skill)
                return i;
        }
        
        return -1; // 未找到技能
    }
    
    /// <summary>
    /// 辅助方法，检查技能是否存在于给定的攻击数组中
    /// </summary>
    private bool IsSkillInAttackArray(Skill skill, Skill[] attackArray)
    {
        if (attackArray == null) return false;
        foreach (var attack in attackArray)
        {
            if (attack == skill) return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取装备在指定槽位的技能
    /// </summary>
    /// <param name="slotIndex">技能槽位索引</param>
    /// <returns>装备的技能，如果槽位无效则返回null</returns>
    public Skill GetEquippedSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length)
            return null;
            
        return equippedSkills[slotIndex];
    }
    
    /// <summary>
    /// 获取基础攻击技能
    /// </summary>
    /// <returns>基础攻击技能</returns>
    public Skill GetBasicAttack()
    {
        if (groundAttacks != null && groundAttacks.Length > 0)
        {
            return groundAttacks[0];
        }
        return null;
    }
} 