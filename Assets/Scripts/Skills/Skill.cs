using UnityEngine;

/// <summary>
/// 技能基类，所有技能都继承自此类
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Basic Skill")]
public class Skill : ScriptableObject
{
    [Header("基本信息")]
    public string skillName = "New Skill";
    public Sprite skillIcon;
    public string description = "Skill description";
    
    [Header("技能参数")]
    public float cooldown = 1f;
    public float skillDuration = 0.5f;
    public bool canMoveWhileUsing = false;
    public float movementSpeedModifier = 0.5f; // 使用技能时的移动速度修正

    [Header("连击扩展接口")]
    [Tooltip("【未来扩展】如果这个技能是可连击的，这里可以指定连击中的下一个技能。")]
    [ContextMenuItem("Clear Next Skill", "ClearNextSkillInCombo")]
    public Skill nextSkillInCombo;

    [Header("动画设置")]
    [Tooltip("技能动画片段，用于覆盖默认动画")]
    public AnimationClip skillAnimation;
    [Tooltip("是否为特殊动画（如面向摄像机的动画，将使用交互层）")]
    public bool isSpecialAnimation = false;
    
    [Header("音效设置")]
    public AudioClip skillSound;
    public float volume = 1f;
    
    [Header("视觉效果")]
    public GameObject visualEffect;
    public Vector3 effectOffset = Vector3.zero;

    [Header("物理行为覆盖")]
    [Tooltip("是否在地面攻击时清除玩家的水平惯性，实现“原地急停”效果。")]
    public bool haltMomentumOnGround = true;

    [Tooltip("是否在空中攻击时减缓玩家的动量，实现“空中停滞”效果。")]
    public bool stallInAir = true;

    [Tooltip("【仅当Stall In Air为true时】在空中停滞时的垂直速度。可以为负数（缓慢下落）或正数（轻微上浮）。")]
    public float airStallVerticalVelocity = -2f;

    [Tooltip("【仅当Stall In Air为true时】在空中停滞时，玩家的水平控制力削减系数（0为无控制，1为完全控制）。")]
    [Range(0f, 1f)]
    public float airStallControlDampening = 0.5f;

    /// <summary>
    /// 激活技能
    /// </summary>
    /// <param name="user">使用技能的游戏对象</param>
    public virtual void Activate(GameObject user)
    {
        // 基本的激活逻辑，派生类可以重写此方法
        Debug.Log($"使用了技能: {skillName}");
        
        // 播放动画
        Animator animator = user.GetComponent<Animator>();
        if (animator != null)
        {
            PlayAnimation(animator);
        }
        
        // 播放音效
        if (skillSound != null)
        {
            AudioSource.PlayClipAtPoint(skillSound, user.transform.position, volume);
        }
        
        // 生成视觉效果
        if (visualEffect != null)
        {
            Instantiate(visualEffect, user.transform.position + effectOffset, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// 播放技能动画
    /// </summary>
    /// <param name="animator">动画控制器</param>
    protected virtual void PlayAnimation(Animator animator)
    {
        if (animator == null || skillAnimation == null) return;
        
        // 尝试获取动画覆盖控制器
        AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
        if (overrideController == null)
        {
            // 如果没有覆盖控制器，创建一个
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }
        
        // 覆盖"Skill"动画片段
        overrideController["Skill"] = skillAnimation;
        
        // 触发通用技能触发器
        animator.SetTrigger("UseSkill");
    }
    
    /// <summary>
    /// 获取技能的持续时间
    /// </summary>
    /// <returns>技能持续时间</returns>
    public virtual float GetDuration()
    {
        return skillDuration;
    }
    
    /// <summary>
    /// 检查技能是否可以使用
    /// </summary>
    /// <param name="user">使用技能的游戏对象</param>
    /// <returns>如果可以使用，则返回true</returns>
    public virtual bool CanUse(GameObject user)
    {
        // 默认实现，派生类可以添加额外的条件检查
        return true;
    }
    
    /// <summary>
    /// 技能结束时调用
    /// </summary>
    /// <param name="user">使用技能的游戏对象</param>
    public virtual void OnSkillEnd(GameObject user)
    {
        // 技能结束时的清理工作，派生类可以重写此方法
        
        // 重置触发器以确保动画状态机正确工作
        Animator animator = user.GetComponent<Animator>();
        if (animator != null)
        {
            // 重置通用技能触发器
            animator.ResetTrigger("UseSkill");
        }
    }

    /// <summary>
    /// 上下文菜单方法，用于在Inspector中快速清除下一个连击技能的引用。
    /// </summary>
    private void ClearNextSkillInCombo()
    {
        nextSkillInCombo = null;
    }
} 